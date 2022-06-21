
using System;
using Godot;
using Dictionary = System.Collections.Generic.Dictionary<int,Godot.WebSocketPeer>;
using Array = Godot.Collections.Array;


public class Server : Node
{
	public RichTextLabel _logDest;
	private Utils _utils;
	private WebSocketServer _server;
	private Dictionary _clients;
	private WebSocketPeer.WriteMode _writeMode;
	private bool _useMultiplayer;
	public int lastConnectedClient;
	
	public Server()
	{
		_server = new WebSocketServer();
		_server.Connect("client_connected", this, nameof(_ClientConnected));
		_server.Connect("client_disconnected", this, nameof(_ClientDisconnected));
		_server.Connect("client_close_request", this, nameof(_ClientCloseRequest));
		_server.Connect("data_received", this, nameof(_ClientReceive));
		_server.Connect("peer_packet", this, nameof(_ClientReceive));
		_server.Connect("peer_connected", this, nameof(_ClientConnected), new Array(){"multiplayer_protocol"});
		_server.Connect("peer_disconnected", this, nameof(_ClientDisconnected));
	}
	public override void _Ready()
	{
		_logDest = GetParent().GetNode<RichTextLabel>("Panel/VBoxContainer/RichTextLabel");
		_utils=GetNode<Utils>("/root/Utils");
		_clients = new Dictionary(){};
		_writeMode = WebSocketPeer.WriteMode.Binary;
		_useMultiplayer = true;
		lastConnectedClient = 0;
	}
	public override void _ExitTree()
	{  
		_clients.Clear();
		_server.Stop();
	}
	public override void _Process(float _delta)
	{  
		if(_server.IsListening())
		{
			_server.Poll();
		}
	}
	public void _ClientCloseRequest(int id, string code, string reason)
	{  
		GD.Print(reason == "Bye bye!");
		_utils._Log(_logDest, $"Client {id} close code: {code}, reason: {reason}");
	}
	public void _ClientConnected(int id, string protocol)
	{  
		_clients.Add(id,_server.GetPeer(id));
		_clients[id].SetWriteMode(_writeMode);
		lastConnectedClient = id;
		_utils._Log(_logDest, $"Client {id} connected with protocol {protocol}");
	}
	public void _ClientDisconnected(int id, bool clean = true)
	{  
		_utils._Log(_logDest, $"Client {id} disconnected. Was clean: {clean}");
		if(_clients.ContainsKey(id))
		{
			_clients.Remove(id);
		}
	}
	public void _ClientReceive(int id)
	{  
		if(_useMultiplayer)
		{
			var peerId = _server.GetPacketPeer();
			var packet = _server.GetPacket();
			_utils._Log(_logDest, $"From MPAPI {peerId} data: {_utils.DecodeData(packet, false)}");
		}
		else
		{
			var packet = _server.GetPeer(id).GetPacket();
			var isString = _server.GetPeer(id).WasStringPacket();
			_utils._Log(_logDest, $"Data from {id} BINARY: {!isString}: {_utils.DecodeData(packet, isString)}");
		}
	}
	public void SendData(string data, int dest)
	{  
		if(_useMultiplayer)
		{
			_server.SetTargetPeer(dest);
			_server.PutPacket(_utils.EncodeData(data, _writeMode));
		}
		else
		{
			foreach(int id in _clients.Keys)
			{
				_server.GetPeer(id).PutPacket(_utils.EncodeData(data, _writeMode));
			}
		}
	}
	public Error Listen(int port, string[] supportedProtocols, bool multiplayer)
	{  
		_useMultiplayer = multiplayer;
		if(_useMultiplayer)
		{
			SetWriteMode(WebSocketPeer.WriteMode.Binary);
		}
		return _server.Listen(port, supportedProtocols, multiplayer);
	}
	public void Stop()
	{  
		_server.Stop();
	}
	public void SetWriteMode(WebSocketPeer.WriteMode mode)
	{  
		_writeMode = mode;
		foreach(int id in _clients.Keys)
		{
			_clients[id].SetWriteMode(_writeMode);
		}
	}
}
