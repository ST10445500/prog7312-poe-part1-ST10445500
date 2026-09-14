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

        //moves a node inside a zone, at the end of whatever is already there
        public static bool MoveInto(DeploymentNode root, DeploymentNode node, DeploymentNode zone)
        {
            if (!CanMove(node, zone))
            {
                return false;
            }

            // Detaching is allowed to find nothing. A sensor dragged in from the
            // unplaced list is a brand new leaf that was never in the tree, so this is
            // "put it here" rather than strictly "move it".
            Detach(root, node);

            zone.Children.Add(node);
            return true;
        }

        //..............................................................................//

        //moves a node in among a zone's children, just before the one given
        public static bool MoveBefore(DeploymentNode root, DeploymentNode node, DeploymentNode zone, DeploymentNode? before)
        {
            if (!CanMove(node, zone))
            {
                return false;
            }

            Detach(root, node);

            // The index is looked up after the detach on purpose. Taking the node out
            // first shifts everything after it along, so a position worked out
            // beforehand would be one place off whenever the node came from earlier
            // in this same list.
            var index = before == null ? -1 : zone.Children.IndexOf(before);

            if (index < 0)
            {
                zone.Children.Add(node);
            }
            else
            {
                zone.Children.Insert(index, node);
            }

            return true;
        }

        //..............................................................................//

        //checks whether a node is allowed to end up under this zone
        private static bool CanMove(DeploymentNode node, DeploymentNode zone)
        {
            // A sensor is a leaf, and dropping a branch inside itself would take the
            // whole branch out of the tree along with it.
            return !zone.IsSensor && !Contains(node, zone);
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

    }

    //..............................................................................//

    //holds where a node was dropped in among a zone's children
    public class DeploymentDrop
    {
        //the zone whose children the node is landing in
        public DeploymentNode Zone { get; set; } = null!;

        //the child it should sit just before, or null to land at the end
        public DeploymentNode? Before { get; set; }
    }

}

//.....................................o0oEND OF FILEo0o..........................................//
