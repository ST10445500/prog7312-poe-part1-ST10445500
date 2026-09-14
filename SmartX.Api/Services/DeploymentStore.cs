using SmartX.Shared.Models;

//ST10445500 - PROG7312 - SmartX POE
//DeploymentStore

//.....................................o0oSTART OF FILEo0o........................................//

// The store keeps gateway data in memory instead of using a database.

namespace SmartX.Api.Services
{
    //keeps the one deployment tree the gateway is running
    //the tree can be any depth, so every walk over it calls itself on the children
    public class DeploymentStore
    {
        //what the root of a freshly seeded tree is called
        private const string RootName = "Smart-X Gateway";

        // A tree is a whole graph of objects rather than a single value, so the
        // lock guards every read and write of it. Handing the live tree out would
        // let a caller change it, or read it half written, without ever taking
        // this lock.
        private readonly object _lock = new object();

        private DeploymentNode _root = new DeploymentNode { Name = RootName };

        //..............................................................................//

        //retrieves a copy of the tree the gateway is running
        public DeploymentNode Get()
        {
            lock (_lock)
            {
                return Clone(_root);
            }
        }

        //..............................................................................//

        //replaces the tree with a copy of the one supplied
        public void Replace(DeploymentNode root)
        {
            lock (_lock)
            {
                _root = Clone(root);
            }
        }

        //..............................................................................//

        //builds the starting tree from the fleet, grouped by zone then room
        public void SeedFromFleet(IEnumerable<SensorRegistration> sensors)
        {
            var zones = sensors
                // A sensor registered with only a name has no zone to group under,
                // and grouping on a blank one would build a nameless node.
                .Where(sensor => !string.IsNullOrWhiteSpace(sensor.Location.Zone))
                .GroupBy(sensor => sensor.Location.Zone)
                .Select(zone => new DeploymentNode
                {
                    Name = zone.Key,
                    Children = zone
                        .GroupBy(sensor => sensor.Location.Room)
                        .Select(room => new DeploymentNode
                        {
                            Name = room.Key,
                            Children = room.Select(SensorLeaf).ToList()
                        })
                        .ToList()
                })
                .ToList();

            lock (_lock)
            {
                _root = new DeploymentNode { Name = RootName, Children = zones };
            }
        }

        //..............................................................................//

        //adds a newly registered sensor to the tree under its zone and room
        public void Place(SensorRegistration sensor)
        {
            lock (_lock)
            {
                // A sensor that is already somewhere in the tree stays where it is.
                // Otherwise anything moved by hand would be dragged back to the spot
                // it was first registered at.
                if (Holds(_root, sensor.MacAddress))
                {
                    return;
                }

                var zone = ChildNamed(_root, sensor.Location.Zone);
                var room = ChildNamed(zone, sensor.Location.Room);

                room.Children.Add(SensorLeaf(sensor));
            }
        }

        //..............................................................................//

        //records against each placed sensor the zone and room the tree has it under
        public void ApplyLocationsTo(SensorStore sensors)
        {
            lock (_lock)
            {
                // The tree is what says where a sensor is. Its registration carries a
                // zone and room because the brief asks a registration to, so they are
                // worked out from the tree rather than typed and left to go stale.
                // The walk starts at the root's children, because the root is the
                // gateway itself rather than a level of the building.
                foreach (var child in _root.Children)
                {
                    ApplyLocations(child, new List<string>(), sensors);
                }
            }
        }

        //..............................................................................//

        //walks down to each sensor carrying the names of the levels above it
        private static void ApplyLocations(DeploymentNode node, List<string> ancestors, SensorStore sensors)
        {
            if (node.IsSensor)
            {
                // The first level under the root is the zone and the one directly above
                // the sensor is the room. Anything in between is dropped, which loses a
                // label but never structure, since the tree keeps that.
                var zone = ancestors.Count > 0 ? ancestors[0] : string.Empty;
                var room = ancestors.Count > 0 ? ancestors[^1] : string.Empty;

                sensors.SetLocation(node.SensorMacAddress!, zone, room);
                return;
            }

            var deeper = new List<string>(ancestors) { node.Name };

            foreach (var child in node.Children)
            {
                ApplyLocations(child, deeper, sensors);
            }
        }

        //..............................................................................//

        //retrieves the child with this name, adding it if the tree does not have one yet
        private static DeploymentNode ChildNamed(DeploymentNode parent, string name)
        {
            var existing = parent.Children
                .FirstOrDefault(child => string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                return existing;
            }

            var added = new DeploymentNode { Name = name };
            parent.Children.Add(added);

            return added;
        }

        //..............................................................................//

        //checks whether this mac address appears anywhere in the tree
        private static bool Holds(DeploymentNode node, string macAddress)
        {
            if (string.Equals(node.SensorMacAddress, macAddress, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            // Each child is asked the same question, which is what lets a sensor be
            // found however deep it has been moved.
            return node.Children.Any(child => Holds(child, macAddress));
        }

        //..............................................................................//

        //builds a copy of a node and everything underneath it
        private static DeploymentNode Clone(DeploymentNode node)
        {
            return new DeploymentNode
            {
                Name = node.Name,
                SensorMacAddress = node.SensorMacAddress,
                Children = node.Children.Select(Clone).ToList()
            };
        }

        //..............................................................................//

        //builds the leaf node that stands for one sensor, named by its node id
        private static DeploymentNode SensorLeaf(SensorRegistration sensor)
        {
            return new DeploymentNode
            {
                Name = sensor.Location.NodeId,
                SensorMacAddress = sensor.MacAddress
            };
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
