using KlipboardPlace.App.Constants;
using KlipboardPlace.App.Views;

namespace KlipboardPlace.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        // Routes that are navigated to (rather than tabs) must be registered here so that
        // Shell can resolve them — and their view models — from the DI container.
        Routing.RegisterRoute(Routes.PlaceDetails, typeof(PlaceDetailsPage));
    }
}
