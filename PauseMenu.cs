using Godot;
using System;

public partial class PauseMenu : Panel
{
    Button _disconnect = null;
	Button _quit = null;
    public override void _Ready()
    {
        _disconnect = (Button)FindChild("Disconnect");
        _quit = (Button)FindChild("Quit");


        _quit.Pressed += QuitGame;
        _disconnect.Pressed += Disconnect;
    }
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
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
}
