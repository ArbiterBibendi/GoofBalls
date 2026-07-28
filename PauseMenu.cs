using Godot;
using System;

public partial class PauseMenu : Panel
{
    bool _enabled = false;
    public override void _Ready()
    {
        Button disconnect = (Button)FindChild("Disconnect");
        Button quit = (Button)FindChild("Quit");


        quit.Pressed += QuitGame;
        disconnect.Pressed += Disconnect;
        Game.Instance.StateChange += OnStateChange;
    }
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (!_enabled) {
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
        if (state == Game.GameState.Playing)
        {
            _enabled = true;
        }
        else
        {
            _enabled = false;
        }
    }
}
