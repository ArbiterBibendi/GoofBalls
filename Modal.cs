using Godot;
using System;

public partial class Modal : Panel
{
	// Called when the node enters the scene tree for the first time.
	Label _label = null;
    private Game _game = null;

	public override void _Ready()
	{
		_label = GetNode<Label>("Label");
		if ((_game = Game.Instance) == null)
        {
            GD.PrintErr("Modal: Game instance is null");
        }
        _game.StateChange += OnGameStateChange;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void SetText(String text) {
		_label.Text = text;
	}
	private void OnGameStateChange(object sender, Game.GameState state)
    {
        switch (state)
        {
            case Game.GameState.MainMenu:
                Visible = false;
                break;
            case Game.GameState.Connecting:
                Visible = true;
        		SetText("Attempting to connect...");
                break;
            case Game.GameState.Playing:
                Visible = false;
                break;
        }
    }
}
