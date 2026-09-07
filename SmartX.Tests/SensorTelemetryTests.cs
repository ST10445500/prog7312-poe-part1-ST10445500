using SmartX.Api.Collections;
using SmartX.Api.Models.Telemetry;

//ST10445500 - PROG7312 - SmartX POE
//SensorTelemetryTests

//.....................................o0oSTART OF FILEo0o........................................//

// The unit tests check the data structures without needing the API to be running.

namespace SmartX.Tests
{
    //checks that the per-sensor lock keeps readings from several threads intact
    public class SensorTelemetryTests
    {
        [Fact]
        public void Add_FromSeveralThreadsAtOnce_KeepsEveryReading()
        {
            // Arrange
            var telemetry = new SensorTelemetry<PowerReading>();
            const int threads = 8;
            const int perThread = 500;

            // Act
            Parallel.For(0, threads, _ =>
            {
                for (var i = 0; i < perThread; i++)
                {
                    telemetry.Add(new TelemetryPacket<PowerReading>("AA:BB:CC:DD:EE:01", new PowerReading(i), DateTime.UtcNow));
                }
            });

            // Assert
            Assert.Equal(threads * perThread, telemetry.GetHistory().Count);
            Assert.Equal(SensorTelemetry<PowerReading>.LiveWindowSize, telemetry.GetLive().Count);
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
