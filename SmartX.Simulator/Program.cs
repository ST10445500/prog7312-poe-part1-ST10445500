using SmartX.Simulator;

//ST10445500 - PROG7312 - SmartX POE
//Program

//.....................................o0oSTART OF FILEo0o........................................//

// Program starts the simulator and reports for whatever the gateway has registered.

// The gateway's https profile, the same address the blazor client is pointed at.
const string DefaultBaseUrl = "https://localhost:7002/";

var baseUrl = args.Length > 0 ? args[0] : DefaultBaseUrl;
var gateway = new GatewayClient(baseUrl);

Console.WriteLine($"Smart-X simulator, talking to {baseUrl}");

List<SmartX.Shared.Models.SensorRegistration> fleet;

try
{
	fleet = await gateway.GetFleetAsync();
}
catch (HttpRequestException problem)
{
	// The api not being up is the usual reason this fails, and the raw socket
	// error on its own does not say that.
	Console.WriteLine($"Could not reach the gateway at {baseUrl}. Is SmartX.Api running?");
	Console.WriteLine(problem.Message);
	return;
}

if (fleet.Count == 0)
{
	Console.WriteLine("The gateway has no sensors registered, so there is nothing to report for.");
	return;
}

Console.WriteLine($"Reporting for {fleet.Count} registered sensors:");

foreach (var sensor in fleet)
{
	Console.WriteLine($"  {sensor.MacAddress}  {sensor.Category}  {sensor.Location.Zone} / {sensor.Location.Room} / {sensor.Location.NodeId}");
}

//.....................................o0oEND OF FILEo0o..........................................//
