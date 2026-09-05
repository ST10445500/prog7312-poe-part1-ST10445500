using System.ComponentModel.DataAnnotations;

//ST10445500 - PROG7312 - SmartX POE
//DeploymentNode

//.....................................o0oSTART OF FILEo0o........................................//

// This model represents data that the gateway stores and works with in memory.

namespace SmartX.Shared.Models
{
    //holds one level of the deployment hierarchy, such as a zone, a room or a sensor
    //children are a list rather than fixed levels so the tree can be any depth
    public class DeploymentNode
    {
        //name of this level, for example Zone 1 or Sub-Zone B
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;

        //set only on nodes that are an actual sensor rather than a grouping level
        public string? SensorMacAddress { get; set; }

        //levels nested underneath this one
        public List<DeploymentNode> Children { get; set; } = new List<DeploymentNode>();

        //checks whether this node is a sensor instead of a grouping level
        public bool IsSensor => !string.IsNullOrWhiteSpace(SensorMacAddress);
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
