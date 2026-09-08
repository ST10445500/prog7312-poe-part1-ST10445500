//ST10445500 - PROG7312 - SmartX POE
//TelemetryStatus

//.....................................o0oSTART OF FILEo0o........................................//

// This enum lists the fixed set of values the gateway accepts.

namespace SmartX.Shared.Models
{
    //lists the states a stretch of telemetry can be in
    public enum TelemetryStatus
    {
        //readings arrived and none of them changed sharply
        Normal,

        //readings arrived and one of them changed sharply enough to be worth looking at
        Spike,

        //nothing arrived at all
        Silent
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
