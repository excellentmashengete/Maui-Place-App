using System.Collections.ObjectModel;
using System.Collections.Specialized;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KlipboardPlace.App.Constants;
using KlipboardPlace.Core.Interfaces;
using KlipboardPlace.Core.Models;

namespace KlipboardPlace.App.ViewModels;

public partial class SearchViewModel : ObservableObject
{
    private const int DebounceMilliseconds = 450;

    private readonly IPlaceService _placeService;
    private CancellationTokenSource? _debounceCts;

    public SearchViewModel(IPlaceService placeService)
    {
        _placeService = placeService;
        Places.CollectionChanged += OnPlacesCollectionChanged;
    }
    
    public ObservableCollection<Place> Places { get; } = new();

    [ObservableProperty]
    private string _criteria = string.Empty;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    private bool _hasInput;
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))] 
    private string? _errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    [NotifyPropertyChangedFor(nameof(IsLoading))]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    private bool _isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmptyState))]
    private bool _hasSearched;

    public bool IsNotBusy => !IsBusy;
    
    public bool IsLoading => IsBusy && HasInput;

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool ShowEmptyState => HasSearched && Places.Count == 0 && !IsBusy && !HasError;
    
    partial void OnCriteriaChanged(string value)
    {
        HasInput = !string.IsNullOrWhiteSpace(value) || !string.IsNullOrEmpty(value);
        _ = DebounceSearchAsync(value);
    }
    
    private void OnPlacesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    { 
        OnPropertyChanged(nameof(ShowEmptyState));
    }
     
    private async Task DebounceSearchAsync(string criteria)
    {
        _debounceCts?.Cancel();
        _debounceCts?.Dispose();

        var cts = new CancellationTokenSource();
        _debounceCts = cts;

        try
        {
            await Task.Delay(DebounceMilliseconds, cts.Token);

            if (string.IsNullOrWhiteSpace(criteria))
            {
                Places.Clear();
                HasSearched = false;
                ErrorMessage = null;
                IsBusy = false;
                return;
            }

            await RunSearchAsync(criteria.Trim(), cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Ignore cancelled searches
        }
    }

    private async Task RunSearchAsync(
        string criteria,
        CancellationToken cancellationToken)
    {
        IsBusy = true;
        ErrorMessage = null;

        try
        {
            var results = await _placeService.SearchPlacesAsync(
                criteria,
                cancellationToken);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (!string.Equals(
                    criteria,
                    Criteria.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Places.Clear();

            foreach (var place in results)
            {
                Places.Add(place);
            }

            HasSearched = true;
        }
        catch (OperationCanceledException)
        {
            // Ignore
        }
        catch (Exception ex)
        {
            ErrorMessage = string.Format(AppMessages.SearchFailedFormat, ex.Message);
            HasSearched = true;
        }
        finally
        {
            if (!cancellationToken.IsCancellationRequested)
            {
                IsBusy = false;
            }
        }
    }
    
    [RelayCommand]
    private async Task SelectPlaceAsync(Place? place)
    {
        if (place is null || string.IsNullOrEmpty(place.PlaceId))
        {
            return;
        }

        await Shell.Current.GoToAsync(
            $"{Routes.PlaceDetails}?{NavigationKeys.PlaceId}={Uri.EscapeDataString(place.PlaceId)}");
    }
}
