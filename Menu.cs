using Godot;
using System;

public partial class Menu : Panel
{

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Button create = (Button)FindChild("Create");
		Button join = (Button)FindChild("Join");
		create.Pressed += OnCreateClicked;
		join.Pressed += OnJoinClicked;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnCreateClicked() {
		Game.Instance.CreateGame();
	}
	public void OnJoinClicked() {
		Game.Instance.JoinGame();
	}
}
