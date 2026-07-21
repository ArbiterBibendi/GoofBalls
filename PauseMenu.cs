using Godot;
using System;

public partial class PauseMenu : Panel
{
    Button _restart = null;
	Button _quit = null;
    public override void _Ready()
    {
        _restart = (Button)FindChild("Restart");
        _quit = (Button)FindChild("Quit");

		Multiplayer.ConnectedToServer += OnConnected;

        _quit.Pressed += QuitGame;
        _restart.Pressed += RestartGame;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    private void QuitGame()
    {
        GetTree().Quit();
    }
    private void RestartGame()
    {
        Game.Instance.BroadcastRestartGame();
    }
    private void OnConnected()
    {
        if (!Multiplayer.IsServer())
        {
            _restart.Flat = true;
        }
    }
}
