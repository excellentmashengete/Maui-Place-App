# KlipboardPlace

A .NET MAUI mobile app for the Klipboard **API Integration Test**: search for places by a single
input criteria, view the result list (main + secondary text), then drill into a place for full
details. Authentication uses OAuth2 **client credentials** against the Eos Identity service.

Type to search (results stream in as you type), tap a result to see its details, and tap the URL to
open the place on a map.

## Solution layout

```
KlipboardPlace/
├── KlipboardPlace.Core/         # Pure .NET 8 class library (no MAUI dependency)
│   ├── Constants/HttpClients.cs        # named clients: "klipboardApi", "klipboardAuthApi"
│   ├── Settings/KlipboardSetting.cs    # base urls, token endpoint, client id/secret, grant type, scope
│   ├── Models/                         # Place, PlaceDetails, TokenResponse, ApiResponse<T>
│   ├── Interfaces/                     # IAuthService, IApiClient, IPlaceService
│   ├── Services/                       # AuthService, ApiClient, PlaceService
│   ├── Handlers/AuthTokenHandler.cs    # attaches bearer token; retries once on 401
│   └── DependencyInjection.cs          # AddKlipboardCore(IConfiguration) registration helper
│
├── KlipboardPlace.App/          # .NET MAUI app (android / ios / maccatalyst)
│   ├── MauiProgram.cs                  # configuration + DI wiring
│   ├── appsettings.json                # embedded config (Klipboard section)
│   ├── Constants/                      # AppConstants (routes, nav keys, fonts), AppMessages
│   ├── Controls/                       # TextBody, TextTitle (reusable label controls)
│   ├── ViewModels/                     # SearchViewModel, PlaceDetailsViewModel (MVVM Toolkit)
│   └── Views/                          # SearchPage.xaml, PlaceDetailsPage.xaml
│
└── KlipboardPlace.Tests/        # xUnit tests (Moq + a stub HttpMessageHandler)
```

### How it fits together

`SearchViewModel` → `IPlaceService` (`PlaceService`) → `IApiClient` (`ApiClient`), which issues GETs
against the named **`klipboardApi`** client. An **`AuthTokenHandler`** (a `DelegatingHandler`) sits on
that client: it adds the `Bearer` token from `IAuthService` to every request, and on a `401` it calls
`IAuthService.Invalidate()`, refreshes the token, and retries once.

`AuthService` obtains the token via the **`klipboardAuthApi`** client (`POST {TokenEndpoint}/connect/token`,
client-credentials form), returns it as a `(token, tokenType)` tuple, and caches it in memory. The API
wraps payloads in a `{ "data": ... }` envelope, modelled by `ApiResponse<T>`.

| Concern  | Endpoint                                          |
|----------|---------------------------------------------------|
| Token    | `POST {TokenEndpoint}/connect/token`              |
| Search   | `GET  {ApiBaseUrl}/api/v1/locations/places?criteria=` |
| Details  | `GET  {ApiBaseUrl}/api/v1/locations/places/{id}`  |

Base URLs default to the staging environment and are configured per the section below.

## Prerequisites

- .NET 8 SDK + MAUI workloads (`dotnet workload install maui`)

## Configuration & secrets

Settings live in [`KlipboardPlace.App/appsettings.json`](KlipboardPlace.App/appsettings.json) under a
`Klipboard` section (`ApiBaseUrl`, `TokenEndpoint`, `GrantType`, `Scope`, `ClientId`, `ClientSecret`).
They bind to `KlipboardSetting` via `AddKlipboardCore`, and the options are validated on startup. The
file is embedded in the app at build time.

Sources are layered, **last wins**, so secrets can stay out of source control:

1. `appsettings.json` — non-sensitive defaults.

For simplicity and ease of assessment, configuration is stored in appsettings.json. 
In a production environment, secrets should be stored in a secure secret-management solution.


## Build & run

```bash
# Restore + build everything (Core, App for all target frameworks, Tests)
dotnet build

# Run the tests
dotnet test

# Run the app on a chosen platform
dotnet build KlipboardPlace.App/KlipboardPlace.App.csproj -t:Run -f net8.0-maccatalyst
# or net8.0-android  (emulator/device)  |  net8.0-ios (simulator)
```

In an IDE (Visual Studio / Rider / VS Code + MAUI), just select `KlipboardPlace.App`, pick a target
and press Run.

## App features

- **Search-as-you-type** — a debounced search runs as the user types; superseded queries are cancelled.
- **Animated search** — the search box starts centered and animates to the top-left on the first keystroke.
- **Loader gating** — the activity indicator shows only while a search with criteria is in flight; clearing
  the box hides it immediately.
- **Open in maps** — the place URL is a tappable link that launches the map via `Launcher`.
- **Reusable label controls** — `TextBody` / `TextTitle` standardise font size and colour so views don't
  repeat them.
- **Centralised strings** — navigation/config constants live in `AppConstants`, user-facing copy in
  `AppMessages`.

## Tests

`dotnet test` runs 15 xUnit tests covering:

- **PlaceService** — envelope unwrapping, URL-encoding of the criteria, blank-criteria short-circuit,
  null-response handling, details mapping, blank-id guard.
- **AuthService** — token parsing (returned as a tuple), in-memory caching (identity hit only once), the
  `client_credentials` form body, and error propagation on a non-success status.
- **ApiClient** — relative-URI resolution against the configured base address and envelope deserialization.
- **AuthTokenHandler** — attaches the `Bearer` token; on a `401` it invalidates the cached token and
  retries once.

No network access is required: HTTP is faked with a stub `HttpMessageHandler`, and `IApiClient` /
`IAuthService` are mocked with Moq.
