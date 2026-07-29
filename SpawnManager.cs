using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class SpawnManager : Node3D
{
    Transform3D[] _transforms = new Transform3D[4];
    int index = 0;
    Dictionary<Player, Transform3D> _spawnDictionary = [];
    public override void _Ready()
    {
        // Must only contain Node3D
        Godot.Collections.Array<Node> spawns = (Godot.Collections.Array<Node>)GetChildren();
        for (int i = 0; i < _transforms.Length; i++)
        {
            _transforms[i] = ((Node3D)spawns[i]).GlobalTransform;
        }
    }

    public Transform3D GetSpawnTransform(Player player)
    {
        bool playerRegistered = _spawnDictionary.TryGetValue(player, out Transform3D spawn);
        if (playerRegistered)
        {
            return spawn;
        }
        GD.PrintErr("Player not registered. Falling back to default spawn");
        return _transforms[0]; // if player not registered fall back to default spawn
    }
    public void RegisterPlayer(Player player)
    {
        if (player == null || !IsInstanceValid(player) || _spawnDictionary.ContainsKey(player))
        {
            return;
        }

        _spawnDictionary.Add(player, GetNextUnregisteredSpawn());
    }
    public void UnregisterPlayer(Player player)
    {
        _spawnDictionary.Remove(player);
    }
    private Transform3D GetNextUnregisteredSpawn()
    {
        index = 0;
		for (;index < _transforms.Length; index++)
		{
			if (_spawnDictionary.ContainsValue(_transforms[index]))
			{
				continue;
			}
			else
			{
				return _transforms[index];
			}
		}
		return _transforms[0];
    }
}
