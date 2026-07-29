using Godot;
using System;

public partial class MainMenu : Panel
{
    // Called when the node enters the scene tree for the first time.
    Button _create = null;
    Button _join = null;
    Button _quit = null;
    public override void _Ready()
    {
        _create = (Button)FindChild("Create");
        _join = (Button)FindChild("Join");
        _quit = (Button)FindChild("Quit");
        
        _create.Pressed += OnCreateClicked;
        _join.Pressed += OnJoinClicked;
        _quit.Pressed += QuitGame;

        Game.StateChange += OnGameStateChange;
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        _create.Pressed -= OnCreateClicked;
        _join.Pressed -= OnJoinClicked;
        _quit.Pressed -= QuitGame;
        Game.StateChange -= OnGameStateChange;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void OnCreateClicked()
    {
        Game.Instance?.CreateGame();
    }
    public void OnJoinClicked()
    {
        Game.Instance?.JoinGame();
    }
    private void OnGameStateChange(object sender, Game.GameState state)
    {
        switch (state)
        {
            case Game.GameState.MainMenu:
                Visible = true;
                break;
            default:
                Visible = false;
                break;
        }
    }
    private void QuitGame()
    {
        GetTree().Quit();
    }
}
