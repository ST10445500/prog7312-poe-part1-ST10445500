using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7312 - SmartX POE
//SensorRegistration

//.....................................o0oSTART OF FILEo0o........................................//

// This model represents data that the gateway stores and works with in memory.

namespace SmartX.Shared.Models
{
    //a single sensor registered with the gateway
    public class SensorRegistration : IValidatableObject
    {
        //identity of the device as it reports itself
        [Required]
        [RegularExpression(@"^([0-9A-Fa-f]{2}[:-]){5}[0-9A-Fa-f]{2}$",
            ErrorMessage = "Mac address must look like AA:BB:CC:DD:EE:FF.")]
        public string MacAddress { get; set; } = string.Empty;

        //where the sensor is physically installed
        [Required]
        public DeploymentLocation Location { get; set; } = new DeploymentLocation();

        //which of the three sensor kinds this device is
        [Required]
        public SensorCategory Category { get; set; }

        //when the gateway accepted this registration
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

        //..............................................................................//

        //checks the location fields, since blazor does not validate nested objects on its own
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Blazor's DataAnnotationsValidator only checks the properties on this
            // class, not the ones inside Location, so those checks have to happen here.
            // Zone and room are not asked for. A sensor is registered with a name and
            // then dragged into the deployment tree, and saving that tree is what
            // fills the two of them in.
            if (string.IsNullOrWhiteSpace(Location.NodeId))
            {
                yield return new ValidationResult("Name is required.", new[] { nameof(Location) });
            }
        }
    }

    //..............................................................................//

    //the zone, room and node a sensor is deployed at
    //the node id is the sensor's name, and is the only one given at registration
    public class DeploymentLocation
    {
        // Zone and room are worked out from where the sensor sits in the deployment
        // tree, so they are blank until it has been placed there.
        public string Zone { get; set; } = string.Empty;

        public string Room { get; set; } = string.Empty;

        [Required]
        public string NodeId { get; set; } = string.Empty;
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
