//ST10445500 - PROG7312 - SmartX POE
//BatchResult

//.....................................o0oSTART OF FILEo0o........................................//

// This shape keeps API input and output simple instead of exposing internal types.

namespace SmartX.Simulator
{
    //holds what the gateway did with a batch of readings
    public class BatchResult
    {
        //how many readings the gateway kept
        public int Accepted { get; set; }

        //the readings the gateway turned away, with the reason for each
        public List<RejectedReading> Rejected { get; set; } = new List<RejectedReading>();
    }

    //..............................................................................//

    //holds why the gateway turned one reading away
    public class RejectedReading
    {
        //what was wrong with it, in the gateway's own words
        public string Reason { get; set; } = string.Empty;
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
