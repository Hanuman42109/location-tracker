using HeatMapTracker.ViewModels;

namespace HeatMapTracker.Views;

/// <summary>
/// Main page: shows the heat map and the tracking controls. Logic lives in <see cref="MainViewModel"/>.
/// </summary>
public partial class MainView : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainView(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    // async void is acceptable here: it is a UI lifecycle override.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
}
