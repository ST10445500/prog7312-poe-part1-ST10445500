using System.Text.Json;
using System.Text.Json.Serialization;

//ST10445500 - PROG7312 - SmartX POE
//ApiJsonOptions

//.....................................o0oSTART OF FILEo0o........................................//

// The service keeps business rules and validation away from the controller.

namespace SmartX.Web.Services
{
    //shared json settings for every call to the gateway api
    public static class ApiJsonOptions
    {
        // The gateway always sends and expects enum values as strings, so the
        // client needs the same converter or a response like "category":"Environmental"
        // fails to deserialize.
        public static readonly JsonSerializerOptions Default = CreateOptions();

        //..............................................................................//

        private static JsonSerializerOptions CreateOptions()
        {
            var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
