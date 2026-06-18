using KlipboardPlace.Core.Interfaces;
using KlipboardPlace.Core.Models;
using KlipboardPlace.Core.Services;
using Moq;
using Xunit;

namespace KlipboardPlace.Tests;

public class PlaceServiceTests
{
    private readonly Mock<IApiClient> _apiClient = new();

    private PlaceService CreateSut() => new(_apiClient.Object);

    [Fact]
    public async Task SearchPlacesAsync_ReturnsMappedPlaces_FromEnvelope()
    {
        var envelope = new ApiResponse<List<Place>>
        {
            Data = new List<Place>
            {
                new() { PlaceId = "p1", MainText = "Cape Town", SecondaryText = "South Africa" },
                new() { PlaceId = "p2", MainText = "Cape Coral", SecondaryText = "FL, USA" },
            },
        };

        _apiClient
            .Setup(c => c.GetAsync<ApiResponse<List<Place>>>(
                It.Is<string>(u => u.Contains("criteria=Cape")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);

        var result = await CreateSut().SearchPlacesAsync("Cape");

        Assert.Equal(2, result.Count);
        Assert.Equal("Cape Town", result[0].MainText);
        Assert.Equal("South Africa", result[0].SecondaryText);
        Assert.Equal("p2", result[1].PlaceId);
    }

    [Fact]
    public async Task SearchPlacesAsync_UrlEncodesCriteria()
    {
        string? capturedUri = null;
        _apiClient
            .Setup(c => c.GetAsync<ApiResponse<List<Place>>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((uri, _) => capturedUri = uri)
            .ReturnsAsync(new ApiResponse<List<Place>> { Data = new List<Place>() });

        await CreateSut().SearchPlacesAsync("New York");

        Assert.NotNull(capturedUri);
        Assert.Contains("criteria=New%20York", capturedUri);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SearchPlacesAsync_BlankCriteria_ReturnsEmpty_WithoutCallingApi(string criteria)
    {
        var result = await CreateSut().SearchPlacesAsync(criteria);

        Assert.Empty(result);
        _apiClient.Verify(
            c => c.GetAsync<ApiResponse<List<Place>>>(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task SearchPlacesAsync_NullEnvelopeOrData_ReturnsEmpty()
    {
        _apiClient
            .Setup(c => c.GetAsync<ApiResponse<List<Place>>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiResponse<List<Place>>?)null);

        var result = await CreateSut().SearchPlacesAsync("anything");

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPlaceDetailsAsync_ReturnsDetails_FromEnvelope()
    {
        var envelope = new ApiResponse<PlaceDetails>
        {
            Data = new PlaceDetails
            {
                PlaceId = "p1",
                Name = "Table Mountain",
                FormattedAddress = "Cape Town, 8001",
                Vicinity = "Cape Town",
                Url = "https://maps.example/p1",
                Latitude = -33.9628,
                Longitude = 18.4098,
                UtcOffset = 120,
            },
        };

        _apiClient
            .Setup(c => c.GetAsync<ApiResponse<PlaceDetails>>(
                It.Is<string>(u => u.Contains("/p1")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(envelope);

        var result = await CreateSut().GetPlaceDetailsAsync("p1");

        Assert.NotNull(result);
        Assert.Equal("Table Mountain", result!.Name);
        Assert.Equal("Cape Town, 8001", result.FormattedAddress);
        Assert.Equal(-33.9628, result.Latitude);
        Assert.Equal(18.4098, result.Longitude);
        Assert.Equal(120, result.UtcOffset);
    }

    [Fact]
    public async Task GetPlaceDetailsAsync_BlankId_Throws()
    {
        await Assert.ThrowsAsync<ArgumentException>(() => CreateSut().GetPlaceDetailsAsync(" "));
    }
}
