using System.Collections.Concurrent;
using SmartX.Shared.Models;

//ST10445500 - PROG7312 - SmartX POE
//SensorStore

//.....................................o0oSTART OF FILEo0o........................................//

// The store keeps gateway data in memory instead of using a database.

namespace SmartX.Api.Services
{
    //keeps every registered sensor in memory
    //keyed on the mac address so a lookup does not have to scan a list
    public class SensorStore
    {
        private readonly ConcurrentDictionary<string, SensorRegistration> _sensors = new();

        //..............................................................................//

        //registers a sensor and returns false if the mac address is already taken
        public bool TryRegister(SensorRegistration sensor)
        {
            // Mac addresses are not case sensitive, so the same device typed in
            // upper or lower case must not be able to register twice.
            var key = sensor.MacAddress.ToUpperInvariant();
            sensor.MacAddress = key;

            return _sensors.TryAdd(key, sensor);
        }

        //..............................................................................//

        //retrieves every sensor registered with the gateway, in deployment order
        public List<SensorRegistration> GetAll()
        {
            // A ConcurrentDictionary hands its values back in whatever order it
            // happens to hold them, which left the fleet table, the dashboard cards
            // and every sensor dropdown in a jumbled order. Sorting here means each
            // caller does not have to remember to.
            // Zone, room and node read the way someone would walk the building. A
            // mac address sorts just as reliably but says nothing about where a
            // sensor is, so it is only the tie breaker.
            return _sensors.Values
                .OrderBy(sensor => sensor.Location.Zone, StringComparer.OrdinalIgnoreCase)
                .ThenBy(sensor => sensor.Location.Room, StringComparer.OrdinalIgnoreCase)
                .ThenBy(sensor => sensor.Location.NodeId, StringComparer.OrdinalIgnoreCase)
                .ThenBy(sensor => sensor.MacAddress, StringComparer.Ordinal)
                .ToList();
        }

        //..............................................................................//

        //retrieves a sensor by mac address, or null if it is not registered
        public SensorRegistration? Find(string macAddress)
        {
            _sensors.TryGetValue(macAddress.ToUpperInvariant(), out var sensor);
            return sensor;
        }

        //..............................................................................//

        //checks whether a mac address belongs to a registered sensor
        public bool IsRegistered(string macAddress)
        {
            return Find(macAddress) != null;
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
