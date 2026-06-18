using KlipboardPlace.Core.Models;

namespace KlipboardPlace.Core.Interfaces;

/// <summary>
/// High-level operations for searching places and retrieving their details.
/// </summary>
public interface IPlaceService
{
    /// The full list of matching places (empty if none).</returns>
    Task<IReadOnlyList<Place>> SearchPlacesAsync(string criteria, CancellationToken cancellationToken = default);
    
    /// Retrieves detailed information for a single place by its unique id.
    Task<PlaceDetails?> GetPlaceDetailsAsync(string placeId, CancellationToken cancellationToken = default);
}
