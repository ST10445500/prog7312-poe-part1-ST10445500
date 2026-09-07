using SmartX.Api.Collections;
using SmartX.Api.Models.Telemetry;

//ST10445500 - PROG7312 - SmartX POE
//TelemetryBatchesTests

//.....................................o0oSTART OF FILEo0o........................................//

// The unit tests check the data structures without needing the API to be running.

namespace SmartX.Tests
{
    //checks that the batch grid transfers into the list at the right point
    public class TelemetryBatchesTests
    {
        [Fact]
        public void Add_FillingTheWholeGrid_TransfersEveryReadingAndResets()
        {
            // Arrange
            var batches = new TelemetryBatches<PowerReading>();
            var readingCount = TelemetryBatches<PowerReading>.BatchSize * TelemetryBatches<PowerReading>.MaxBatches;

            // Act
            for (var i = 0; i < readingCount; i++)
            {
                batches.Add(new TelemetryPacket<PowerReading>("AA:BB:CC:DD:EE:01", new PowerReading(i), DateTime.UtcNow));
            }

            // Assert
            Assert.Equal(readingCount, batches.TransferredCount);
            Assert.Equal(0, batches.FinishedBatchCount);
            Assert.Equal(0, batches.ReadingsInCurrentBatch);
            Assert.Equal(readingCount, batches.ToList().Count);
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
