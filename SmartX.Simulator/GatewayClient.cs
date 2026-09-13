using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SmartX.Shared.Models;

//ST10445500 - PROG7312 - SmartX POE
//GatewayClient

//.....................................o0oSTART OF FILEo0o........................................//

// The client talks to the gateway over http, the same way a real device would.

namespace SmartX.Simulator
{
    //sends readings to the gateway and reads back the fleet it should be reporting for
    public class GatewayClient
    {
        //how many readings the gateway accepts in a single batch
        public const int MaxBatchSize = 500;

        // The gateway sends category names as strings rather than numbers, so this
        // client needs its own converter. Nothing is shared between the api, the
        // blazor app and this, so each one has to say it for itself.
        private static readonly JsonSerializerOptions JsonOptions = CreateJsonOptions();

        private readonly HttpClient _http;

        public GatewayClient(string baseUrl)
        {
            _http = new HttpClient { BaseAddress = new Uri(baseUrl) };
        }

        //..............................................................................//

        //retrieves every sensor the gateway has registered
        public async Task<List<SensorRegistration>> GetFleetAsync()
        {
            var fleet = await _http.GetFromJsonAsync<List<SensorRegistration>>("api/sensors", JsonOptions);
            return fleet ?? new List<SensorRegistration>();
        }

        //..............................................................................//

        //builds the json settings every call from this client uses
        private static JsonSerializerOptions CreateJsonOptions()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
