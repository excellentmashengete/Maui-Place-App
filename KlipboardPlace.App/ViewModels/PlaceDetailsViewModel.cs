using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KlipboardPlace.App.Constants;
using KlipboardPlace.Core.Interfaces;
using KlipboardPlace.Core.Models;

namespace KlipboardPlace.App.ViewModels;

/// <summary>
/// Backs <see cref="Views.PlaceDetailsPage"/>: loads the detailed information for the place id
/// supplied as a Shell navigation query parameter.
/// </summary>
[QueryProperty(nameof(PlaceId), NavigationKeys.PlaceId)]
public partial class PlaceDetailsViewModel : ObservableObject
{
    private readonly IPlaceService _placeService;

    public PlaceDetailsViewModel(IPlaceService placeService)
    {
        _placeService = placeService;
    }

    [ObservableProperty]
    private string? _placeId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Coordinates))]
    private PlaceDetails? _details;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    private bool _isBusy;

    public bool IsNotBusy => !IsBusy;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    
    /// <summary>Convenience binding for the "lat, long" display.</summary>
    public string Coordinates => Details is null
        ? string.Empty
        : $"{Details.Latitude}, {Details.Longitude}";
    partial void OnPlaceIdChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _ = LoadAsync();
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (string.IsNullOrEmpty(PlaceId) || IsBusy)
        {
            return;
        }

        IsBusy = true;
        ErrorMessage = null;

        try
        {
            Details = await _placeService.GetPlaceDetailsAsync(PlaceId);
            OnPropertyChanged(nameof(Coordinates));

            if (Details is null)
            {
                ErrorMessage = AppMessages.NoPlaceDetailsFound;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Format(AppMessages.PlaceDetailsLoadFailedFormat, ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task OpenUrlAsync()
    {
        var url = Details?.Url;
        if (string.IsNullOrWhiteSpace(url))
        {
            return;
        }

        try
        {
            await Launcher.Default.OpenAsync(new Uri(url));
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Format(AppMessages.OpenMapFailedFormat, ex.Message);
        }
    }
}
