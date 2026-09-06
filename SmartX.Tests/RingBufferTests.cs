using SmartX.Api.Collections;

//ST10445500 - PROG7312 - SmartX POE
//RingBufferTests

//.....................................o0oSTART OF FILEo0o........................................//

// The unit tests check the data structures without needing the API to be running.

namespace SmartX.Tests
{
    //checks that the ring buffer wraps around correctly and stays a fixed size
    public class RingBufferTests
    {
        [Fact]
        public void Add_WhenBufferIsFull_OverwritesOldestItem()
        {
            // Arrange
            var buffer = new RingBuffer<int>(3);
            buffer.Add(1);
            buffer.Add(2);
            buffer.Add(3);

            // Act
            buffer.Add(4);

            // Assert
            Assert.Equal(new[] { 2, 3, 4 }, buffer.ToList());
            Assert.Equal(3, buffer.Count);
        }

        [Fact]
        public void Add_AfterSeveralLaps_KeepsOnlyTheNewestItems()
        {
            // Arrange
            var buffer = new RingBuffer<int>(3);

            // Act
            for (var i = 1; i <= 10; i++)
            {
                buffer.Add(i);
            }

            // Assert
            Assert.Equal(new[] { 8, 9, 10 }, buffer.ToList());
            Assert.Equal(3, buffer.Count);
        }

        [Fact]
        public void GetEnumerator_AfterWrapping_WalksOldestToNewest()
        {
            // Arrange
            var buffer = new RingBuffer<int>(3);
            for (var i = 1; i <= 5; i++)
            {
                buffer.Add(i);
            }

            // Act
            var walked = new List<int>();
            foreach (var item in buffer)
            {
                walked.Add(item);
            }

            // Assert
            Assert.Equal(new[] { 3, 4, 5 }, walked);
        }

        [Fact]
        public void Newest_OnAnEmptyBuffer_Throws()
        {
            // Arrange
            var buffer = new RingBuffer<int>(2);

            // Act and Assert
            Assert.Throws<InvalidOperationException>(() => buffer.Newest());
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
