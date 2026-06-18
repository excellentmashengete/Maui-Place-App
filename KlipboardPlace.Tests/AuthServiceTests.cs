using System.Net;
using KlipboardPlace.Core.Services;
using KlipboardPlace.Core.Settings;
using Microsoft.Extensions.Options;
using Xunit;

namespace KlipboardPlace.Tests;

public class AuthServiceTests
{
    // AuthService posts to "/connect/token" on the named auth client, so the client needs a base address.
    private const string IdentityBaseUrl = "https://identity.example";

    private static IOptions<KlipboardSetting> Settings() => Options.Create(new KlipboardSetting
    {
        ClientId = "client",
        ClientSecret = "secret",
        GrantType = "client_credentials",
        Scope = "eos.common.fullaccess",
        TokenEndpoint = IdentityBaseUrl,
    });

    [Fact]
    public async Task GetAccessTokenAsync_ReturnsParsedToken()
    {
        var handler = new StubHttpMessageHandler(
            "{\"access_token\":\"abc123\",\"token_type\":\"Bearer\",\"expires_in\":3600,\"scope\":\"eos.common.fullaccess\"}");
        var sut = new AuthService(new StubHttpClientFactory(handler, IdentityBaseUrl), Settings());

        var token = await sut.GetAccessTokenAsync();

        Assert.Equal("abc123", token.token);
        Assert.Equal("Bearer", token.tokenType);
    }

    [Fact]
    public async Task GetAccessTokenAsync_CachesToken_AcrossCalls()
    {
        // AuthService treats a token as valid until (Timestamp + expires_in), so the response must
        // carry a current timestamp for the in-memory cache to engage.
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var handler = new StubHttpMessageHandler(
            $"{{\"access_token\":\"abc123\",\"token_type\":\"Bearer\",\"expires_in\":3600,\"timestamp\":{nowMs}}}");
        var sut = new AuthService(new StubHttpClientFactory(handler, IdentityBaseUrl), Settings());

        var first = await sut.GetAccessTokenAsync();
        var second = await sut.GetAccessTokenAsync();

        Assert.Equal(first, second);
        Assert.Equal(1, handler.CallCount); // the identity service was hit only once
    }

    [Fact]
    public async Task GetAccessTokenAsync_PostsClientCredentialsForm()
    {
        var handler = new StubHttpMessageHandler(
            "{\"access_token\":\"abc123\",\"expires_in\":3600}");
        var sut = new AuthService(new StubHttpClientFactory(handler, IdentityBaseUrl), Settings());

        await sut.GetAccessTokenAsync();

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal("https://identity.example/connect/token", request.RequestUri!.ToString());

        var body = Assert.Single(handler.RequestBodies);
        Assert.NotNull(body);
        Assert.Contains("grant_type=client_credentials", body);
        Assert.Contains("client_id=client", body);
        Assert.Contains("client_secret=secret", body);
        Assert.Contains("scope=eos.common.fullaccess", body);
    }

    [Fact]
    public async Task GetAccessTokenAsync_Throws_OnErrorStatus()
    {
        var handler = new StubHttpMessageHandler("{\"error\":\"invalid_client\"}", HttpStatusCode.Unauthorized);
        var sut = new AuthService(new StubHttpClientFactory(handler, IdentityBaseUrl), Settings());

        await Assert.ThrowsAsync<HttpRequestException>(() => sut.GetAccessTokenAsync());
    }
}
