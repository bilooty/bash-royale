using LiteNetLib;
using LiteNetLib.Utils;

namespace bash_royale.Networking;

public class NetworkManager : INetEventListener
{
    private NetManager? _client;
    private NetPeer? _serverPeer;
    private readonly NetPacketProcessor _packetProcessor;
    private readonly NetDataWriter _writer = new NetDataWriter();
    public bool IsConnected => _serverPeer != null;
    public Dictionary<int, NetworkAction> RemoteInputs { get; private set; } = new();

    /// <summary>The opponent's deck, or null until their handshake packet arrives.</summary>
    public List<CardId>? RemoteDeck { get; private set; }

    public NetworkManager()
    {
        _packetProcessor = new NetPacketProcessor();
        _packetProcessor.Subscribe<NetworkAction>(OnPlayerActionReceived, () => new NetworkAction());
        _packetProcessor.SubscribeNetSerializable<DeckPacket>(OnDeckReceived, () => new DeckPacket());
    }
    public void StartClient(string ip, int port)
    {
        _client = new NetManager(this);
        _client.Start();
        _client.Connect(ip, port, "BashRoyaleKey");
    }
    public void StartHost(int port)
    {
        _client = new NetManager(this);
        _client.Start(port); 
    }
    public void PollEvents()
    {
        _client?.PollEvents();
    }

    public void SendAction(NetworkAction action)
    {
        if (_serverPeer != null)
        {
            _writer.Reset();
            _packetProcessor.Write(_writer, action);
            _serverPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
        }
    }

    public void SendDeck(List<CardId> deck)
    {
        if (_serverPeer == null) return;

        DeckPacket packet = new() { Cards = Decks.ToBytes(deck) };
        _writer.Reset();
        _packetProcessor.WriteNetSerializable(_writer, ref packet);
        _serverPeer.Send(_writer, DeliveryMethod.ReliableOrdered);
    }

    private void OnDeckReceived(DeckPacket packet)
    {
        RemoteDeck = Decks.FromBytes(packet.Cards);
    }

    private void OnPlayerActionReceived(NetworkAction action)
    {
        RemoteInputs[action.Tick] = action;
    }
    public void OnPeerConnected(NetPeer peer) { _serverPeer = peer; }
    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo) { _serverPeer = null; RemoteDeck = null; }
        
    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
    {
        // Pass the raw bytes to our packet processor
        _packetProcessor.ReadAllPackets(reader, peer);
    }
        
    public void OnNetworkError(System.Net.IPEndPoint endPoint, System.Net.Sockets.SocketError socketError) { }
    public void OnNetworkReceiveUnconnected(System.Net.IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType) { }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        request.AcceptIfKey("BashRoyaleKey"); 
    }
    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        // You can leave this empty for now, or save the 'latency' 
        // variable to display a "Ping: 45ms" indicator on your UI later!
    }
}