# Smart-X

A hybrid IoT telemetry gateway for monitoring distributed environments such as
hydroponic farms, utility trackers and smart grids. Thousands of ESP32-class
devices publish multi-typed telemetry to a central gateway that validates,
stores and optimises the stream.

This is a rebuild of the project on a different stack. Part 1 is being
implemented again from scratch here.

- **Backend** — ASP.NET Core Web API on .NET 10 (`SmartX.Api`)
- **Frontend** — Blazor WebAssembly on .NET 10 (`SmartX.Web`)

The two run as separate apps for now, with the API allowing the client's
origin through CORS rather than the client being hosted inside the API.

---

## Prerequisites

| Requirement | Version | Check with |
|---|---|---|
| .NET SDK | 10.0 or later | `dotnet --version` |

The client vendors Bootstrap 5.3.3 CSS directly into `wwwroot/lib/bootstrap/`
rather than loading it from a CDN, so the app still looks right with no
internet connection.

---

## Running it

Two terminals, from the repository root:

```bash
dotnet run --project SmartX.Api --launch-profile https
dotnet run --project SmartX.Web --launch-profile https
```

- API: `https://localhost:7002`
- Client: `https://localhost:7267`

The first time the API is hit over https on this machine, the ASP.NET dev
certificate needs to be trusted:

```bash
dotnet dev-certs https --trust
```

---

## More to come

This README grows alongside the rebuild — this is a stub until there is a
real Part 1 to describe.
