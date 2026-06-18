using KlipboardPlace.App.ViewModels;

namespace KlipboardPlace.App.Views;

public partial class SearchPage : ContentPage
{
    public SearchPage(SearchViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
