using System.Collections.Generic;
using Godot;

public partial class Game : Node3D
{

    public static Game Instance = null;
    Dictionary<long, Player> _players = new Dictionary<long, Player>();
    Panel _menu = null;
    public Game()
    {
        Instance = this;
    }
    public override void _Ready()
    {
        base._Ready();
        _menu = GetNode<Panel>("Menu");
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

        _menu.Visible = false;
        GD.Print("Create", Multiplayer.GetUniqueId());
        Rpc(MethodName.AddPlayer, true, Multiplayer.GetUniqueId());
    }
    public void JoinGame()
    {
        ENetMultiplayerPeer peer = new ENetMultiplayerPeer();
        Error e = peer.CreateClient("127.0.0.1", 1337);
        if (e != Error.Ok)
        {
            return;
        }
        Multiplayer.MultiplayerPeer = peer;

        _menu.Visible = false;
        GD.Print("Join", Multiplayer.GetUniqueId());
    }

    private void OnClientJoined(long id)
    {
        GD.Print("Client Joined");

        if (Multiplayer.IsServer())
        {
            foreach (var kvp in _players)
            {
                RpcId(id, MethodName.AddPlayer, false, kvp.Key);
            }
            Rpc(MethodName.AddPlayer, id == Multiplayer.GetUniqueId(), id);
        }
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void AddPlayer(bool isLocal, long id)
    {
        PackedScene ballScene = GD.Load<PackedScene>("res://Player.tscn");
        Player player = ballScene.Instantiate<Player>();
        player.SetId(id);
        player.Name = id.ToString();
        GD.Print("Adding Player: ", id);
        AddChild(player);
        _players[id] = player;
    }
}