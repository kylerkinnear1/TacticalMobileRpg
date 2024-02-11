namespace Rpg.Mobile.App;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void GameView_OnStartInteraction(object? sender, TouchEventArgs e) => GameView.OnClickDown(e);

    private void GameView_OnEndInteraction(object? sender, TouchEventArgs e) => GameView.OnClickUp(e);
}

