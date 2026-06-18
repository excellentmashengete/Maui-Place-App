using KlipboardPlace.Core.Interfaces;
using KlipboardPlace.Core.Models;
using KlipboardPlace.Core.Services;
using Moq;
using Xunit;

namespace KlipboardPlace.Tests;

public class ApiClientTests
{
    private const string BaseUrl = "https://api.example/location/";

    [Fact]
    public async Task GetAsync_ResolvesRelativeUriAgainstBaseAddress()
    {
        // The bearer token is attached by AuthTokenHandler on the named client, not by ApiClient,
        // so here we only assert URI resolution. See AuthTokenHandlerTests for the auth header.
        var handler = new StubHttpMessageHandler("{\"data\":[],\"meta\":{}}");
        var sut = new ApiClient(new StubHttpClientFactory(handler, BaseUrl), Mock.Of<IAuthService>());

        await sut.GetAsync<ApiResponse<List<Place>>>("api/v1/locations/places?criteria=x");

        var request = Assert.Single(handler.Requests);
        Assert.Equal(HttpMethod.Get, request.Method);
        Assert.Equal(
            "https://api.example/location/api/v1/locations/places?criteria=x",
            request.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetAsync_DeserializesEnvelope()
    {
        const string json = "{\"data\":[{\"placeId\":\"p1\",\"mainText\":\"Cape Town\",\"secondaryText\":\"ZA\"}]}";
        var handler = new StubHttpMessageHandler(json);
        var sut = new ApiClient(new StubHttpClientFactory(handler, BaseUrl), Mock.Of<IAuthService>());

        var result = await sut.GetAsync<ApiResponse<List<Place>>>("api/v1/locations/places");

        Assert.NotNull(result);
        var place = Assert.Single(result!.Data!);
        Assert.Equal("p1", place.PlaceId);
        Assert.Equal("Cape Town", place.MainText);
        Assert.Equal("ZA", place.SecondaryText);
    }
}
