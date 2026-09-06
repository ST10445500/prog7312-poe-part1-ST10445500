using SmartX.Shared.Models;

//ST10445500 - PROG7312 - SmartX POE
//DeploymentTreeBuilder

//.....................................o0oSTART OF FILEo0o........................................//

// The service keeps business rules and validation away from the controller.

namespace SmartX.Web.Services
{
    //turns the registered fleet into a deployment tree, and builds example trees for testing the validator
    public static class DeploymentTreeBuilder
    {
        private const string RootName = "Smart-X Gateway";

        //..............................................................................//

        //groups the fleet by zone then room, with each sensor as a leaf named by its node id
        public static DeploymentNode BuildFromSensors(IEnumerable<SensorRegistration> sensors)
        {
            var zoneNodes = sensors
                .GroupBy(sensor => sensor.Location.Zone)
                .Select(zoneGroup =>
                {
                    var roomNodes = zoneGroup
                        .GroupBy(sensor => sensor.Location.Room)
                        .Select(roomGroup => new DeploymentNode
                        {
                            Name = roomGroup.Key,
                            Children = roomGroup.Select(SensorLeaf).ToList()
                        })
                        .ToList();

                    return new DeploymentNode { Name = zoneGroup.Key, Children = roomNodes };
                })
                .ToList();

            return new DeploymentNode { Name = RootName, Children = zoneNodes };
        }

        //..............................................................................//

        //builds a leaf node for one sensor, named by its node id
        private static DeploymentNode SensorLeaf(SensorRegistration sensor)
        {
            return new DeploymentNode
            {
                Name = sensor.Location.NodeId,
                SensorMacAddress = sensor.MacAddress
            };
        }

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
