using KlipboardPlace.App.ViewModels;

namespace KlipboardPlace.App.Views;

public partial class PlaceDetailsPage : ContentPage
{
    public PlaceDetailsPage(PlaceDetailsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
