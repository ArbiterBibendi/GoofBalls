using Godot;
using System;

public partial class MainMenu : Panel
{
    private Game _game = null;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Button create = (Button)FindChild("Create");
        Button join = (Button)FindChild("Join");
        Button quit = (Button)FindChild("Quit");
        
        create.Pressed += OnCreateClicked;
        join.Pressed += OnJoinClicked;
        quit.Pressed += QuitGame;

        if ((_game = Game.Instance) == null)
        {
            GD.PrintErr("MainMenu: Game instance is null");
        }
        _game.StateChange += OnGameStateChange;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnCreateClicked()
    {
        Game.Instance.CreateGame();
    }
    public void OnJoinClicked()
    {
        Game.Instance.JoinGame();
    }
    private void OnGameStateChange(object sender, Game.GameState state)
    {
        switch (state)
        {
            case Game.GameState.MainMenu:
                Visible = true;
                break;
            case Game.GameState.Connecting:
                Visible = false;
                break;
            case Game.GameState.Playing:
                Visible = false;
                break;
        }
    }
    private void QuitGame()
    {
        GetTree().Quit();
    }
}
