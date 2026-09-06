//ST10445500 - PROG7312 - SmartX POE
//TelemetryPacket

//.....................................o0oSTART OF FILEo0o........................................//

// The wrapper carries readings of different types without boxing them.

namespace SmartX.Api.Models.Telemetry
{
    //wraps one reading from a device along with who sent it and when
    //the struct constraint keeps the value inline so it never gets boxed into an object
    public readonly struct TelemetryPacket<T> where T : struct
    {
        //identity of the device the reading came from
        public string MacAddress { get; }

        //the reading itself, such as a moisture percentage or a wattage
        public T Value { get; }

        //when the gateway recorded the reading
        public DateTime RecordedAt { get; }

        //..............................................................................//

        public TelemetryPacket(string macAddress, T value, DateTime recordedAt)
        {
            MacAddress = macAddress;
            Value = value;
            RecordedAt = recordedAt;
        }

        //..............................................................................//

        //retrieves a short readable version of the packet
        public override string ToString()
        {
            return $"{MacAddress} {Value} at {RecordedAt:HH:mm:ss}";
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
