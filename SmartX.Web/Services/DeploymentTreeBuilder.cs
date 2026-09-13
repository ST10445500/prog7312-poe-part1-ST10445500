using SmartX.Shared.Models;

//ST10445500 - PROG7312 - SmartX POE
//DeploymentTreeBuilder

//.....................................o0oSTART OF FILEo0o........................................//

// The service keeps business rules and validation away from the controller.

namespace SmartX.Web.Services
{
    //builds example trees for trying the validator out
    //the deployment the gateway is running comes from the api, not from here
    public static class DeploymentTreeBuilder
    {
        //..............................................................................//

        //builds a tree with a duplicate sibling name and a sensor that also has a child
        public static DeploymentNode BuildBrokenExample()
        {
            return new DeploymentNode
            {
                Name = "Facility A",
                Children =
                [
                    new DeploymentNode
                    {
                        Name = "Zone 1",
                        Children = [new DeploymentNode { Name = "Probe", SensorMacAddress = "11:22:33:44:55:66" }]
                    },
                    new DeploymentNode
                    {
                        Name = "Zone 1",
                        Children =
                        [
                            new DeploymentNode
                            {
                                Name = "Valve A",
                                SensorMacAddress = "AA:BB:CC:DD:EE:FF",
                                Children = [new DeploymentNode { Name = "Should not be here" }]
                            }
                        ]
                    }
                ]
            };
        }

        //..............................................................................//

        //builds a straight chain nested past the validators depth limit
        public static DeploymentNode BuildDeepExample(int levels = 40)
        {
            var root = new DeploymentNode { Name = "Level 1" };
            var current = root;

            for (var level = 2; level <= levels; level++)
            {
                var child = new DeploymentNode { Name = $"Level {level}" };
                current.Children.Add(child);
                current = child;
            }

            return root;
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
