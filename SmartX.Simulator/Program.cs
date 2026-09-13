using SmartX.Shared.Models;
using SmartX.Simulator;

//ST10445500 - PROG7312 - SmartX POE
//Program

//.....................................o0oSTART OF FILEo0o........................................//

// Program starts the simulator and reports for whatever the gateway has registered.

// The gateway's https profile, the same address the blazor client is pointed at.
const string DefaultBaseUrl = "https://localhost:7002/";

// The demo fleet includes a spare unit that was registered but never powered up,
// so it is skipped here and shows on the dashboard as a silent sensor.
const string SpareNodeId = "spare-valve";

// Twelve hundred readings per sensor is past the ring buffer's hundred and twenty
// and past the thousand the batch grid holds before it transfers into the list, so
// both are exercised straight away.
const int BackfillReadings = 1200;

// Backfilled readings are spaced the same as live ones. At thirty seconds apart the
// health windows are only ten seconds wide, so two windows in every three held no
// reading and the older half of the ribbon came out striped with silent blocks.
const int BackfillIntervalSeconds = 3;

// Live readings come in far quicker than the backfill's thirty seconds, because the
// signal trace ribbon buckets into ten second windows and would otherwise show a
// gap between most of them.
const double MinIntervalSeconds = 2;
const double MaxIntervalSeconds = 5;

var baseUrl = args.Length > 0 ? args[0] : DefaultBaseUrl;
var gateway = new GatewayClient(baseUrl);

Console.WriteLine($"Smart-X simulator, talking to {baseUrl}");

List<SensorRegistration> fleet;

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

var reporting = fleet
	.Where(sensor => sensor.Location.NodeId != SpareNodeId)
	.ToList();

if (reporting.Count == 0)
{
	Console.WriteLine("The gateway has no sensors to report for.");
	return;
}

Console.WriteLine($"Reporting for {reporting.Count} of {fleet.Count} registered sensors.");

// One generator per sensor, seeded differently so the fleet does not move in step.
var generators = new Dictionary<string, ReadingGenerator>();

for (var index = 0; index < reporting.Count; index++)
{
	generators[reporting[index].MacAddress] = new ReadingGenerator(reporting[index].Category, index + 1);
}

Console.WriteLine($"Backfilling {BackfillReadings} readings per sensor...");

var backfillStart = DateTime.UtcNow.AddSeconds(-BackfillReadings * BackfillIntervalSeconds);

foreach (var sensor in reporting)
{
	var generator = generators[sensor.MacAddress];
	var batch = new List<DeviceReading>();
	var accepted = 0;
	var rejected = 0;
	var firstReason = string.Empty;

	for (var index = 0; index < BackfillReadings; index++)
	{
		var recordedAt = backfillStart.AddSeconds(index * BackfillIntervalSeconds);
		batch.Add(generator.Next(sensor.MacAddress, recordedAt));

		// Readings go up in the order they were taken, so the newest one the
		// gateway holds is genuinely the newest.
		if (batch.Count < GatewayClient.MaxBatchSize && index < BackfillReadings - 1)
		{
			continue;
		}

		var result = await gateway.PostBatchAsync(batch);

		accepted += result.Accepted;
		rejected += result.Rejected.Count;

		if (firstReason.Length == 0 && result.Rejected.Count > 0)
		{
			firstReason = result.Rejected[0].Reason;
		}

		batch.Clear();
	}

	// A rejection here means the generator produced something the gateway will
	// not take, which is worth seeing rather than passing over quietly.
	var note = rejected > 0 ? $", {rejected} rejected - {firstReason}" : string.Empty;
	Console.WriteLine($"  {sensor.MacAddress}  {accepted} accepted{note}");
}

Console.WriteLine("Backfill done.");

// Ctrl-C asks the sensor tasks to stop rather than killing the process mid-post.
using var stopping = new CancellationTokenSource();

Console.CancelKeyPress += (_, pressed) =>
{
	pressed.Cancel = true;
	stopping.Cancel();
};

Console.WriteLine($"Streaming live readings every {MinIntervalSeconds} to {MaxIntervalSeconds} seconds. Ctrl-C to stop.");

// A task per sensor rather than one loop through them all, so readings genuinely
// arrive at the same time. That is what puts the per sensor lock in
// SensorTelemetry and the concurrent dictionary in TelemetryStore under real load
// instead of only under a unit test.
var streams = reporting
	.Select(sensor => StreamSensorAsync(sensor, stopping.Token))
	.ToList();

await Task.WhenAll(streams);

Console.WriteLine("Simulator stopped.");

//..............................................................................//

//keeps one sensor reporting until the simulator is asked to stop
async Task StreamSensorAsync(SensorRegistration sensor, CancellationToken token)
{
	var generator = generators[sensor.MacAddress];
	var jitter = new Random(sensor.MacAddress.GetHashCode());

	while (!token.IsCancellationRequested)
	{
		// Each sensor waits a slightly different length of time, so the fleet does
		// not fall into posting all at once on the same beat.
		var seconds = MinIntervalSeconds + (jitter.NextDouble() * (MaxIntervalSeconds - MinIntervalSeconds));

		try
		{
			await Task.Delay(TimeSpan.FromSeconds(seconds), token);
			await gateway.PostReadingAsync(generator.Next(sensor.MacAddress, DateTime.UtcNow), token);
		}
		catch (OperationCanceledException)
		{
			return;
		}
		catch (HttpRequestException problem)
		{
			// One device losing the gateway should not take the rest of the fleet
			// down with it, so it reports the problem and keeps trying.
			Console.WriteLine($"  {sensor.MacAddress} could not post: {problem.Message}");
		}
	}
}

//.....................................o0oEND OF FILEo0o..........................................//
