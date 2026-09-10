using System.Security.Cryptography;

//ST10445500 - PROG7312 - SmartX POE
//AttachmentEncryption

//.....................................o0oSTART OF FILEo0o........................................//

// The service keeps business rules and validation away from the controller.

namespace SmartX.Api.Services
{
    //encrypts and decrypts attachment files as they stream past, without ever holding one in memory
    //aes needs a fresh iv per file, so it is written in front of the ciphertext and read back off it later
    public class AttachmentEncryption
    {
        //aes-256, so the key is 32 bytes and the iv is one 16 byte block
        private const int KeySizeInBytes = 32;
        private const int IvSizeInBytes = 16;

        //how much of a file is held in memory at once, whatever its size
        private const int BufferSizeInBytes = 64 * 1024;

        private readonly byte[] _key;

        //..............................................................................//

        public AttachmentEncryption(IConfiguration configuration)
        {
            var configured = configuration["Encryption:Key"];

            // Failing here rather than on the first upload means a missing or
            // malformed key is found when the gateway starts, not hours later when
            // someone tries to attach a file.
            if (string.IsNullOrWhiteSpace(configured))
            {
                throw new InvalidOperationException("Encryption:Key is not configured, so attachments cannot be stored.");
            }

            _key = Convert.FromBase64String(configured);

            if (_key.Length != KeySizeInBytes)
            {
                throw new InvalidOperationException($"Encryption:Key must be {KeySizeInBytes} bytes once decoded, but it is {_key.Length}.");
            }
        }

        //..............................................................................//

        //encrypts everything read from the source into the destination, and reports how much was read
        public async Task<long> EncryptToAsync(Stream source, Stream destination, CancellationToken token)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.GenerateIV();

            // The iv is not a secret, it only has to be different every time, so it
            // goes in front of the ciphertext where decryption can find it.
            await destination.WriteAsync(aes.IV, token);

            using var crypto = new CryptoStream(destination, aes.CreateEncryptor(), CryptoStreamMode.Write, leaveOpen: true);

            var buffer = new byte[BufferSizeInBytes];
            long copied = 0;
            int read;

            // Copying a buffer at a time is what keeps a large upload from being held
            // in memory. The same few kilobytes are reused for the whole file.
            while ((read = await source.ReadAsync(buffer, token)) > 0)
            {
                await crypto.WriteAsync(buffer.AsMemory(0, read), token);
                copied += read;
            }

            await crypto.FlushFinalBlockAsync(token);
            return copied;
        }

        //..............................................................................//

        //decrypts everything read from the source into the destination
        public async Task DecryptToAsync(Stream source, Stream destination, CancellationToken token)
        {
            var iv = new byte[IvSizeInBytes];
            await source.ReadExactlyAsync(iv, token);

            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = iv;

            using var crypto = new CryptoStream(source, aes.CreateDecryptor(), CryptoStreamMode.Read, leaveOpen: true);
            await crypto.CopyToAsync(destination, BufferSizeInBytes, token);
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
