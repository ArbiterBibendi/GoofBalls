using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SpawnManager : Node3D
{
	Transform3D[] _transforms = new Transform3D[4];
	int index = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		// Must only contain Node3D
		Godot.Collections.Array<Node> spawns = (Godot.Collections.Array<Node>)GetChildren();
		for (int i = 0; i < _transforms.Length; i++) {
			_transforms[i] = ((Node3D)spawns[i]).GlobalTransform;
		}

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	public Transform3D GetSpawnTransform() {
		GD.Print(index);
		Transform3D transform = _transforms[index++];
		if (index >= _transforms.Length) {
			index = 0;
		}
		return transform;
	}
}
