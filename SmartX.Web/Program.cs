using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SmartX.Web;

//ST10445500 - PROG7312 - SmartX POE
//Program

//.....................................o0oSTART OF FILEo0o........................................//

// Program starts the Blazor client, registers services and points it at the gateway API.

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Falls back to the client's own origin if the setting is missing, so a
// broken config shows up as a failed api call instead of quietly pointing
// nowhere useful.
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

await builder.Build().RunAsync();

//.....................................o0oEND OF FILEo0o..........................................//
