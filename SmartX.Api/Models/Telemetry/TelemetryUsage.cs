//ST10445500 - PROG7312 - SmartX POE
//TelemetryUsage

//.....................................o0oSTART OF FILEo0o........................................//

// This shape keeps API input and output simple instead of exposing internal types.

namespace SmartX.Api.Models.Telemetry
{
    //shows how full each of the storage structures is for one sensor
    //the dashboard uses this to show that the structures cope under load
    public class TelemetryUsage
    {
        //how many readings are in the live ring buffer
        public int LiveReadings { get; set; }

        //how many the ring buffer can hold before it starts overwriting
        public int LiveCapacity { get; set; }

        //how many rows of the batch grid are completely full
        public int FinishedBatches { get; set; }

        //how many readings are in the row being filled
        public int ReadingsInCurrentBatch { get; set; }

        //how many readings have been transferred into the list
        public int TransferredToList { get; set; }

        //the newest reading the sensor sent, or null if it has sent nothing
        public string? LatestReading { get; set; }

        //when that newest reading was recorded
        public DateTime? LatestRecordedAt { get; set; }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
