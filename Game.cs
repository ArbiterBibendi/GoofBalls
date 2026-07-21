using System;
using System.Collections.Generic;
using Godot;

public partial class Game : Node3D
{

    public static Game Instance = null;
    Dictionary<long, Player> _players = new Dictionary<long, Player>();
    Panel _mainMenu = null;
    Panel _pauseMenu = null;
    Modal _modal = null;
    Node3D _spawnPoint = null;

    public override void _Ready()
    {
        base._Ready();
        _mainMenu = GetNode<Panel>("MainMenu");
        _pauseMenu = GetNode<Panel>("PauseMenu");
        _spawnPoint = GetNode<Node3D>("Map/SpawnPoint");
        _modal = GetNode<Modal>("Modal");
        Instance = this;
        Multiplayer.ServerDisconnected += RestartGame;
        Multiplayer.ConnectionFailed += OnConnectionFailed;
        Multiplayer.ConnectedToServer += OnConnectionSucceeded;
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
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        Error e = peer.CreateServer(1337);
        if (e != Error.Ok)
        {
            return;
        }
        Multiplayer.PeerConnected += OnClientJoined;
        Multiplayer.MultiplayerPeer = peer;

        _mainMenu.Visible = false;
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
        GD.Print(e);
        Multiplayer.MultiplayerPeer = peer;

        _mainMenu.Visible = false;
        _modal.Visible = true;
        _modal.SetText("Attempting to connect...");
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
        player.Transform = _spawnPoint.Transform;
        GD.Print("Adding Player: ", id);
        AddChild(player);
        _players[id] = player;
    }

    public void BroadcastRestartGame()
    {
        GD.Print("Broadcast", Multiplayer.MultiplayerPeer);
        if (Multiplayer.IsServer())
        {
            Rpc(MethodName.RestartGame);
        }
        else {
            RestartGame();
        }
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true)]
    private void RestartGame()
    {
        GD.Print("Restart RPC Called", Multiplayer?.GetUniqueId());

        foreach (var kvp in _players)
        {
            kvp.Value.QueueFree();
        }

        _players = [];
        
        GD.Print("CallDeffered ", Multiplayer?.GetUniqueId());
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
    }
    private void OnConnectionFailed()
    {
        Multiplayer.MultiplayerPeer = null;
        _mainMenu.Visible = true;
        _modal.Visible = false;
        GD.Print("Connection failed");
    }
    private void OnConnectionSucceeded() {
        _modal.Visible = false;
        _mainMenu.Visible = false;
    }
}