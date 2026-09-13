using SmartX.Shared.Models;

//ST10445500 - PROG7312 - SmartX POE
//DeploymentTreeOperations

//.....................................o0oSTART OF FILEo0o........................................//

// The service keeps business rules and validation away from the controller.

namespace SmartX.Web.Services
{
    //the walks the deployment editor needs over a tree of any depth
    //every one of these calls itself on the children, since a tree has no fixed number of levels
    public static class DeploymentTreeOperations
    {
        //..............................................................................//

        //builds a copy of a node and everything underneath it
        public static DeploymentNode Clone(DeploymentNode node)
        {
            // Editing works on a copy so cancelling can simply throw it away rather
            // than trying to undo whatever was changed.
            return new DeploymentNode
            {
                Name = node.Name,
                SensorMacAddress = node.SensorMacAddress,
                Children = node.Children.Select(Clone).ToList()
            };
        }

        //..............................................................................//

        //removes a node from whichever parent holds it, returning false if it was not found
        public static bool Detach(DeploymentNode root, DeploymentNode node)
        {
            if (root.Children.Remove(node))
            {
                return true;
            }

            return root.Children.Any(child => Detach(child, node));
        }

        //..............................................................................//

        //retrieves every node that can hold children, paired with the path that reaches it
        public static List<DeploymentTarget> ZonePaths(DeploymentNode root)
        {
            var targets = new List<DeploymentTarget>();
            Collect(root, root.Name, targets);

            return targets;
        }

        //..............................................................................//

        //adds this node to the list if it can hold children, then asks its children
        private static void Collect(DeploymentNode node, string path, List<DeploymentTarget> targets)
        {
            // A sensor is a leaf, so it is never somewhere another node can move to.
            if (node.IsSensor)
            {
                return;
            }

            targets.Add(new DeploymentTarget { Path = path, Node = node });

            foreach (var child in node.Children)
            {
                Collect(child, $"{path} / {child.Name}", targets);
            }
        }

        //..............................................................................//

        //checks whether a node is this node or sits somewhere underneath it
        public static bool Contains(DeploymentNode node, DeploymentNode candidate)
        {
            if (ReferenceEquals(node, candidate))
            {
                return true;
            }

            return node.Children.Any(child => Contains(child, candidate));
        }

        //..............................................................................//

        //retrieves every mac address the tree currently places somewhere
        public static HashSet<string> MacAddressesIn(DeploymentNode root)
        {
            var found = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            CollectMacAddresses(root, found);

            return found;
        }

        //..............................................................................//

        //adds this node's mac address if it has one, then asks its children
        private static void CollectMacAddresses(DeploymentNode node, HashSet<string> found)
        {
            if (node.IsSensor)
            {
                found.Add(node.SensorMacAddress!);
            }

            foreach (var child in node.Children)
            {
                CollectMacAddresses(child, found);
            }
        }

        //..............................................................................//

        //puts a sensor back under the zone and room it registered with
        public static void PlaceAtRegisteredLocation(DeploymentNode root, SensorRegistration sensor)
        {
            // The same placement the gateway does when a sensor first registers, so a
            // sensor taken out of the tree goes back where it started rather than
            // landing at the root for someone to move by hand.
            var zone = ChildNamed(root, sensor.Location.Zone);
            var room = ChildNamed(zone, sensor.Location.Room);

            room.Children.Add(new DeploymentNode
            {
                Name = sensor.Location.NodeId,
                SensorMacAddress = sensor.MacAddress
            });
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
    }

    //..............................................................................//

    //holds one place a node can be moved to, with the path that reaches it
    public class DeploymentTarget
    {
        //the full path down to this node, such as Greenhouse A / Bay 2
        public string Path { get; set; } = string.Empty;

        //the node itself, which is what a move actually attaches to
        public DeploymentNode Node { get; set; } = null!;
    }

    //..............................................................................//

    //holds a request to move one node somewhere else in the tree
    public class DeploymentMove
    {
        //the node being moved, along with everything underneath it
        public DeploymentNode Node { get; set; } = null!;

        //the node it should end up under
        public DeploymentNode Target { get; set; } = null!;
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
