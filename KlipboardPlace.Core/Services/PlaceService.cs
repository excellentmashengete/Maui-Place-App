using KlipboardPlace.Core.Interfaces;
using KlipboardPlace.Core.Models;

namespace KlipboardPlace.Core.Services;

/// <summary>
/// Searches places and retrieves place details via the Location API, unwrapping the
/// <see cref="ApiResponse{T}"/> envelope.
/// </summary>
public class PlaceService : IPlaceService
{
    private readonly IApiClient _apiClient;

    public PlaceService(IApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<IReadOnlyList<Place>> SearchPlacesAsync(string criteria, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(criteria))
        {
            return Array.Empty<Place>();
        } var response = await _apiClient
            .GetAsync<ApiResponse<List<Place>>>(
                $"/location/api/v1/locations/places?criteria={Uri.EscapeDataString(criteria)}", 
                cancellationToken
                )
            .ConfigureAwait(false);

        return response?.Data ?? new List<Place>();
    }

    public async Task<PlaceDetails?> GetPlaceDetailsAsync(string placeId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(placeId))
        {
            throw new ArgumentException("A place id is required.", nameof(placeId));
        }
        
        var response = await _apiClient
            .GetAsync<ApiResponse<PlaceDetails>>(
                $"/location/api/v1/locations/places/{Uri.EscapeDataString(placeId)}", 
                cancellationToken)
            .ConfigureAwait(false);

        return response?.Data;
    }
}
