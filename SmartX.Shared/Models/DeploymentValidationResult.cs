//ST10445500 - PROG7312 - SmartX POE
//DeploymentValidationResult

//.....................................o0oSTART OF FILEo0o........................................//

// This shape keeps API input and output simple instead of exposing internal types.

namespace SmartX.Shared.Models
{
    //holds what came back from walking a deployment tree
    public class DeploymentValidationResult
    {
        //everything found wrong with the tree
        public List<string> Errors { get; set; } = new List<string>();

        //how many nodes the walk visited
        public int NodesChecked { get; set; }

        //how far down the walk went
        public int DeepestLevel { get; set; }

        //how many of the nodes were sensors rather than grouping levels
        public int SensorsFound { get; set; }

        //checks whether the tree can be accepted
        public bool IsValid => Errors.Count == 0;
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
