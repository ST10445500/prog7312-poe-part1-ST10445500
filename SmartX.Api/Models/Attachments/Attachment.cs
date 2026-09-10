//ST10445500 - PROG7312 - SmartX POE
//Attachment

//.....................................o0oSTART OF FILEo0o........................................//

// This model represents data that the gateway stores and works with in memory.

namespace SmartX.Api.Models.Attachments
{
    //holds one file attached to a sensor, such as a config file, a photo or a hardware log
    //the file itself sits on disk encrypted, so this only describes it
    public class Attachment
    {
        //identity the client uses to ask for this file back
        public string Id { get; set; } = string.Empty;

        //the device this file was attached to
        public string MacAddress { get; set; } = string.Empty;

        //what the file was called when it was uploaded
        public string FileName { get; set; } = string.Empty;

        //what kind of file the browser said it was
        public string ContentType { get; set; } = string.Empty;

        //how big the file is before it was encrypted
        public long SizeInBytes { get; set; }

        //how big the encrypted copy on disk is, which is larger because of the iv and the padding
        public long StoredSizeInBytes { get; set; }

        //when the gateway accepted it
        public DateTime UploadedAt { get; set; }

        //what the encrypted copy is called on disk
        public string StoredFileName { get; set; } = string.Empty;
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
