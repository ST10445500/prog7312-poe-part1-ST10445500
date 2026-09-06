using SmartX.Api.Services;
using SmartX.Shared.Models;

//ST10445500 - PROG7312 - SmartX POE
//DeploymentTreeValidatorTests

//.....................................o0oSTART OF FILEo0o........................................//

// The unit tests check the data structures without needing the API to be running.

namespace SmartX.Tests
{
    //checks that the recursive walk gets the domain rules right and handles deep trees
    public class DeploymentTreeValidatorTests
    {
        private const string KnownMac = "AA:BB:CC:DD:EE:01";

        //builds a validator with one sensor already registered
        private static DeploymentTreeValidator BuildValidator()
        {
            var store = new SensorStore();

            store.TryRegister(new SensorRegistration
            {
                MacAddress = KnownMac,
                Category = SensorCategory.Environmental,
                Location = new DeploymentLocation { Zone = "Zone 1", Room = "Sub-Zone B", NodeId = "N01" }
            });

            return new DeploymentTreeValidator(store);
        }

        //builds a chain of nodes nested to the depth asked for
        private static DeploymentNode BuildChain(int depth)
        {
            var root = new DeploymentNode { Name = "Level 1" };
            var current = root;

            for (var level = 2; level <= depth; level++)
            {
                var child = new DeploymentNode { Name = $"Level {level}" };
                current.Children.Add(child);
                current = child;
            }

            return root;
        }

        [Fact]
        public void Validate_OnATreeWithARegisteredSensor_PassesWithNoErrors()
        {
            // Arrange
            var validator = BuildValidator();

            var tree = new DeploymentNode
            {
                Name = "Zone 1",
                Children =
                [
                    new DeploymentNode { Name = "Sub-Zone B", SensorMacAddress = KnownMac }
                ]
            };

            // Act
            var result = validator.Validate(tree);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(1, result.SensorsFound);
        }

        [Fact]
        public void Validate_WhenASensorIsNotRegistered_ReportsIt()
        {
            // Arrange
            var validator = BuildValidator();

            var tree = new DeploymentNode
            {
                Name = "Zone 1",
                Children =
                [
                    new DeploymentNode { Name = "Ghost Sensor", SensorMacAddress = "FF:FF:FF:FF:FF:FF" }
                ]
            };

            // Act
            var result = validator.Validate(tree);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Contains("not registered"));
        }

        [Fact]
        public void Validate_OnATreeAtTheDepthLimit_StillPasses()
        {
            // Arrange
            var validator = BuildValidator();
            var tree = BuildChain(DeploymentTreeValidator.MaxDepth);

            // Act
            var result = validator.Validate(tree);

            // Assert
            Assert.True(result.IsValid);
            Assert.Equal(DeploymentTreeValidator.MaxDepth, result.DeepestLevel);
        }

        [Fact]
        public void Validate_OnATreeDeeperThanTheLimit_StopsInsteadOfOverflowing()
        {
            // Arrange
            var validator = BuildValidator();
            var tree = BuildChain(DeploymentTreeValidator.MaxDepth + 500);

            // Act
            var result = validator.Validate(tree);

            // Assert
            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, error => error.Contains("nested deeper than"));
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
