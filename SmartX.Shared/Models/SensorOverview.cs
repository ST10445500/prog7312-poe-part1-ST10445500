//ST10445500 - PROG7312 - SmartX POE
//SensorOverview

//.....................................o0oSTART OF FILEo0o........................................//

// This shape keeps API input and output simple instead of exposing internal types.

namespace SmartX.Shared.Models
{
    //holds one sensor's latest reading, for showing the whole fleet in one table
    public class SensorOverview
    {
        //identity of the device
        public string MacAddress { get; set; } = string.Empty;

        //what kind of readings this sensor sends
        public SensorCategory Category { get; set; }

        //where the sensor is physically installed
        public DeploymentLocation Location { get; set; } = new DeploymentLocation();

        //the newest reading with its unit, or null if the sensor has not reported yet
        public string? LatestReading { get; set; }

        //when that reading was recorded
        public DateTime? LatestRecordedAt { get; set; }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
