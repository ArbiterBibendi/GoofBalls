using System;
using System.Collections.Generic;
using Godot;

public partial class Game : Node3D
{
    public static float DEADZONE = -1f;
    private int _playersDead = 0;
    public Player Winner = null;
    public static Game Instance = null;
    Dictionary<long, Player> _players = new Dictionary<long, Player>();
    Panel _pauseMenu = null;
    SpawnManager _spawnManager = null;
    Timer _roundRestartTimer = null;

    public enum GameState
    {
        MainMenu,
        Connecting,
        Playing,
        RoundEnding
    }
    public static event EventHandler<GameState> StateChange;
    public Game()
    {
        Instance = this;
    }
    public override void _Ready()
    {
        base._Ready();
        _pauseMenu = GetNode<Panel>("PauseMenu");
        _spawnManager = GetNode<SpawnManager>("Map/SpawnManager");
        _roundRestartTimer = GetNode<Timer>("RoundRestartTimer");
        Multiplayer.ServerDisconnected += Disconnect;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.ConnectedToServer += OnConnectionSucceeded;
        Multiplayer.PeerConnected += OnClientJoined;
        Multiplayer.PeerDisconnected += OnClientDisconnected;
        StateChange?.Invoke(this, GameState.MainMenu);
        _roundRestartTimer.Timeout += BroadcastRestartRound;
        Player.Died += OnPlayerDied;
    }
    public override void _ExitTree()
    {
        base._ExitTree();
        Multiplayer.ServerDisconnected -= Disconnect;
        Multiplayer.ConnectionFailed -= OnConnectionFailed;
        Multiplayer.ConnectedToServer -= OnConnectionSucceeded;
        Multiplayer.PeerConnected -= OnClientJoined;
        Multiplayer.PeerDisconnected -= OnClientDisconnected;
        _roundRestartTimer.Timeout -= BroadcastRestartRound;
        Player.Died -= OnPlayerDied;
    }

    private void OnPlayerDied(object sender, EventArgs e)
    {
        if (++_playersDead >= _players.Count - 1)
        {
            _roundRestartTimer.Start();
            foreach (var kvp in _players)
            {
                if (!kvp.Value.Dead)
                {
                    Winner = kvp.Value;
                }
            }
            StateChange?.Invoke(this, GameState.RoundEnding);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
    }

    public void CreateGame()
    {
        ENetMultiplayerPeer peer = new();
        Error e = peer.CreateServer(1337);
        if (e != Error.Ok)
        {
            return;
        }
        Multiplayer.MultiplayerPeer = peer;

        StateChange?.Invoke(this, GameState.Playing);
        GD.Print("Create", Multiplayer.GetUniqueId());
        Rpc(MethodName.AddPlayer, Multiplayer.GetUniqueId());
    }
    public void JoinGame()
    {
        ENetMultiplayerPeer peer = new();
        Error e = peer.CreateClient("127.0.0.1", 1337);
        if (e != Error.Ok)
        {
            return;
        }
        Multiplayer.MultiplayerPeer = peer;

        StateChange?.Invoke(this, GameState.Connecting);
    }

    private void OnClientJoined(long id)
    {
        GD.Print("Client Joined");

        if (Multiplayer.IsServer())
        {
            foreach (var kvp in _players)
            {
                RpcId(id, MethodName.AddPlayer, kvp.Key);
            }
            Rpc(MethodName.AddPlayer, id);
        }
    }
    private void OnClientDisconnected(long id)
    {
        if (Multiplayer.IsServer())
        {
            Rpc(MethodName.RemovePlayer, id);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void AddPlayer(long id)
    {
        PackedScene ballScene = GD.Load<PackedScene>("res://Player.tscn");
        Player player = ballScene.Instantiate<Player>();
        player.SetId(id);
        player.Name = id.ToString();
        player.Transform = _spawnManager.GetSpawnTransform();
        GD.Print("Adding Player: ", id);
        AddChild(player);
        _players[id] = player;
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void RemovePlayer(long id)
    {
        _players[id].QueueFree();
        _players.Remove(id);
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void RestartRound()
    {
        foreach (var kvp in _players)
        {
            kvp.Value.Reset();
            GD.Print("Resetting Player: ", kvp.Value.GetId());
        }
        _playersDead = 0;
        Winner = null;
        StateChange?.Invoke(this, GameState.Playing);

    }
    private void BroadcastRestartRound()
    {
        if (Multiplayer.IsServer())
        {
            Rpc(MethodName.RestartRound);
        }
    }
    public void BroadcastDisconnect()
    {
        if (Multiplayer.IsServer())
        {
            Rpc(MethodName.Disconnect);
        }
        else
        {
            Disconnect();
        }
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void Disconnect()
    {
        foreach (var kvp in _players)
        {
            kvp.Value.QueueFree();
        }

        _players = [];
        CallDeferred(MethodName.ClosePeerAndDisconnect);
        StateChange?.Invoke(this, GameState.MainMenu);

    }
    private void ClosePeerAndDisconnect()
    {
        if (Multiplayer.MultiplayerPeer != null)
        {
            Multiplayer.MultiplayerPeer.Close();
            Multiplayer.MultiplayerPeer = null;
        }

        GetTree().ReloadCurrentScene();
        StateChange?.Invoke(this, GameState.MainMenu);
    }
    private void OnConnectionFailed()
    {
        Multiplayer.MultiplayerPeer = null;
        GD.Print("Connection failed");
        StateChange?.Invoke(this, GameState.MainMenu);

    }
    private void OnConnectionSucceeded()
    {
        StateChange?.Invoke(this, GameState.Playing);
    }
}