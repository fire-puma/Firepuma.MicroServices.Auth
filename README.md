# Firepuma.MicroServices.Auth
An authentication library for secure communication with microservices

## Quick Start

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...

    services.AddOpenIdConnectTokenProvider(Configuration.GetSection("TokenProvider"));

    ...
}
```

Use options `AutoRefreshEnabled` and `AutoRefreshInterval` to register a hosted service that will automatically refresh the token and use a singleton `CachedMicroServiceTokenProvider`.