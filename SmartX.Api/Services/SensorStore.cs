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

        //retrieves every sensor registered with the gateway
        public List<SensorRegistration> GetAll()
        {
            return _sensors.Values.ToList();
        }

        //..............................................................................//

        //retrieves a sensor by mac address, or null if it is not registered
        public SensorRegistration? Find(string macAddress)
        {
            _sensors.TryGetValue(macAddress.ToUpperInvariant(), out var sensor);
            return sensor;
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
