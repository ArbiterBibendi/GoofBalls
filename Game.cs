using System;
using System.Collections.Generic;
using Godot;

public partial class Game : Node3D
{
    private const float DEADZONE = -1f;
    private int _playersDead = 0;

    public static Game Instance = null;
    Dictionary<long, Player> _players = new Dictionary<long, Player>();
    Panel _pauseMenu = null;
    SpawnManager _spawnManager = null;
    Timer _roundRestartTimer = null;

    public enum GameState
    {
        MainMenu,
        Connecting,
        Playing
    }
    public event EventHandler<GameState> StateChange;
    public Game()
    {
        Instance = this;
    }
    public override void _Ready()
    {
        base._Ready();
        _pauseMenu = GetNode<Panel>("PauseMenu");
        _spawnManager = GetNode<SpawnManager>("Map/SpawnManager");
        Multiplayer.ServerDisconnected += Disconnect;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.ConnectedToServer += OnConnectionSucceeded;
        Multiplayer.PeerConnected += OnClientJoined;
        StateChange?.Invoke(this, GameState.MainMenu);
        _roundRestartTimer = GetNode<Timer>("RoundRestartTimer");
        _roundRestartTimer.Timeout += BroadcastRestartRound;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        foreach (var kvp in _players)
        {
            Player player = kvp.Value;
            if (player.Transform.Origin.Y <= DEADZONE)
            {
                if (!player.Dead) {
                    player.Die();
                    _playersDead++;
                    if (_playersDead >= _players.Count) {
                        _roundRestartTimer.Start();
                    }
                }
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        if (@event is InputEventKey inputEventKey)
        {
            if (inputEventKey.Pressed && inputEventKey.Keycode == Key.Escape)
            {
                _pauseMenu.Visible = !_pauseMenu.Visible;
            }
        }
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
        GD.Print("Join", Multiplayer.GetUniqueId());
    }

    private void OnClientJoined(long id)
    {
        GD.Print("Client Joined");

        if ((bool)(Multiplayer?.IsServer()))
        {
            foreach (var kvp in _players)
            {
                RpcId(id, MethodName.AddPlayer, kvp.Key);
            }
            Rpc(MethodName.AddPlayer, id);
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
    private void RestartRound()
    {
        foreach (var kvp in _players)
        {
            kvp.Value.Reset();
            GD.Print("Resetting Player: ", kvp.Value.GetId());
        }
        _playersDead = 0;
    }
    private void BroadcastRestartRound() {
        if (Multiplayer.IsServer()) {
            Rpc(MethodName.RestartRound);
        }
    }
    public void BroadcastDisconnect()
    {
        GD.Print("Broadcast disconnect", Multiplayer.MultiplayerPeer);
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
        GD.Print("Restart RPC Called", Multiplayer?.GetUniqueId());

        foreach (var kvp in _players)
        {
            kvp.Value.QueueFree();
        }

        _players = [];

        CallDeferred(MethodName.ClosePeerAndDisconnect);

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