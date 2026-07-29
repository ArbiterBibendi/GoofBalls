using Godot;
using System;

public partial class PauseMenu : Panel

{

    Button _disconnect = null;
    Button _quit = null;
    bool _enabled = false;
    public override void _Ready()
    {
        _disconnect = (Button)FindChild("Disconnect");
        _quit = (Button)FindChild("Quit");


        _quit.Pressed += QuitGame;
        _disconnect.Pressed += Disconnect;
        Game.StateChange += OnStateChange;
    }
    public override void _ExitTree()
    {
        _quit.Pressed -= QuitGame;
        _disconnect.Pressed -= Disconnect;
        Game.StateChange -= OnStateChange;
    }
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (!_enabled)
        {
            return;
        }
        if (@event is InputEventKey inputEventKey)
        {
            if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Escape)
            {
                Visible = !Visible;
            }
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
    private void QuitGame()
    {
        GetTree().Quit();
    }
    private void Disconnect()
    {
        Game.Instance.BroadcastDisconnect();
    }
    private void OnStateChange(object sender, Game.GameState state)
    {
        switch (state)
        {
            case Game.GameState.MainMenu:
                _enabled = false;
                break;
            default:
                _enabled = true;
                break;
        }
    }
}
