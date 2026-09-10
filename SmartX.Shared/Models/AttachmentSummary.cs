//ST10445500 - PROG7312 - SmartX POE
//AttachmentSummary

//.....................................o0oSTART OF FILEo0o........................................//

// This shape keeps API input and output simple instead of exposing internal types.

namespace SmartX.Shared.Models
{
    //holds what the client is told about one attached file
    //the name of the encrypted copy on disk is deliberately left out, the client has no use for it
    public class AttachmentSummary
    {
        //identity used to ask for the file back
        public string Id { get; set; } = string.Empty;

        //what the file was called when it was uploaded
        public string FileName { get; set; } = string.Empty;

        //what kind of file the browser said it was
        public string ContentType { get; set; } = string.Empty;

        //how big the file is once decrypted
        public long SizeInBytes { get; set; }

        //how big the encrypted copy on disk is, so the client can show that the stored file is not the original
        public long StoredSizeInBytes { get; set; }

        //when the gateway accepted it
        public DateTime UploadedAt { get; set; }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
