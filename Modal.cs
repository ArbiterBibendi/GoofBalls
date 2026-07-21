using Godot;
using System;

public partial class Modal : Panel
{
	// Called when the node enters the scene tree for the first time.
	Label _label = null;
	public override void _Ready()
	{
		_label = GetNode<Label>("Label");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public void SetText(String text) {
		_label.Text = text;
	}
}
