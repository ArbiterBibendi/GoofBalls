using Godot;
using System;

public partial class Modal : Panel
{
	// Called when the node enters the scene tree for the first time.
	Label _label = null;

	public override void _Ready()
	{
		_label = GetNode<Label>("Label");
        Game.StateChange += OnGameStateChange;
	}
    public override void _ExitTree()
    {
        Game.StateChange -= OnGameStateChange;
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
        Game game = (Game)sender;
        switch (state)
        {
            case Game.GameState.Connecting:
                Visible = true;
        		SetText("Attempting to connect...");
                break;
            case Game.GameState.RoundEnding:
                Visible = true;
                SetText($"{game.Winner.GetId()} won! Starting next round...");
                break;
            default:
                Visible = false;
                break;
        }
    }
}
