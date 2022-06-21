
using System;
using Godot;
using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;


public class Client : Node
{
	 
	public RichTextLabel _logDest;
	private WebSocketClient _client;
	private Utils _utils;
	private WebSocketPeer.WriteMode _writeMode;
	private bool _useMultiplayer;
	public int lastConnectedClient;
	public override void _Ready()
	{
		_logDest=GetParent().GetNode<RichTextLabel>("Panel/VBoxContainer/RichTextLabel");
		_utils=GetNode<Utils>("/root/Utils");
		_writeMode = WebSocketPeer.WriteMode.Binary;
		_useMultiplayer = true;
		lastConnectedClient = 0;
	}
	public Client()
	{  
		_client = new WebSocketClient();
		_client.Connect("connection_established", this, nameof(_ClientConnected));
		_client.Connect("connection_error", this, nameof(_ClientDisconnected));
		_client.Connect("connection_closed", this, nameof(_ClientDisconnected));
		_client.Connect("server_close_request", this, nameof(_ClientCloseRequest));
		_client.Connect("data_received", this, nameof(_ClientReceived));
	
		_client.Connect("peer_packet", this, nameof(_ClientReceived));
		_client.Connect("peer_connected", this, nameof(_PeerConnected));
		_client.Connect("connection_succeeded", this, nameof(_ClientConnected), new Array(){"multiplayer_protocol"});
		_client.Connect("connection_failed", this, nameof(_ClientDisconnected));
	}
	public void _ClientCloseRequest(string code, string reason)
	{  
		_utils._Log(_logDest, $"Close code: {code}, reason: {reason}");
	}
	public void _PeerConnected(int id)
	{  
		_utils._Log(_logDest, $"Client {id} just connected");
		lastConnectedClient = id;
	}
	public override void _ExitTree()
	{  
		_client.DisconnectFromHost(1001, "Bye bye!");
	}
	
	public override void _Process(float _delta)
	{  
		if(_client.GetConnectionStatus() == NetworkedMultiplayerPeer.ConnectionStatus.Disconnected)
		{
			return;
		}
		_client.Poll();
	}
	public void _ClientConnected(string protocol)
	{  
		_utils._Log(_logDest, $"Client just connected with protocol: {protocol}");
		_client.GetPeer(1).SetWriteMode(_writeMode);
	}
	public void _ClientDisconnected(bool clean=true)
	{  
		_utils._Log(_logDest, $"Client just disconnected. Was clean: {clean.ToString()}");
	}
	public void _ClientReceived(int _pId = 1)
	{  
		if(_useMultiplayer)
		{
			var peerId = _client.GetPacketPeer();
			var packet = _client.GetPacket();
			_utils._Log(_logDest, $"From MPAPI {GD.Str(peerId)} Data: {_utils.DecodeData(packet, false)}");
		}
		else
		{
			var packet = _client.GetPeer(1).GetPacket();
			var isString = _client.GetPeer(1).WasStringPacket();
			_utils._Log(_logDest, $"Received data. BINARY: {!isString}: {_utils.DecodeData(packet, isString)}");
		}
	}
	public Error ConnectToUrl(string host, string[] protocols, bool multiplayer)
	{  
		_useMultiplayer = multiplayer;
		if(_useMultiplayer)
		{
			_writeMode = WebSocketPeer.WriteMode.Binary;
		}
		return _client.ConnectToUrl(host, protocols, multiplayer);
	}
	public void DisconnectFromHost()
	{  
		_client.DisconnectFromHost(1000, "Bye bye!");
	}
	public void SendData(string data, int dest)
	{  
		_client.GetPeer(1).SetWriteMode(_writeMode);
		if(_useMultiplayer)
		{
			_client.SetTargetPeer(dest);
			_client.PutPacket(_utils.EncodeData(data, _writeMode));
		}
		else
		{
			_client.GetPeer(1).PutPacket(_utils.EncodeData(data, _writeMode));
		}
	}
	public void SetWriteMode(WebSocketPeer.WriteMode mode)
	{  
		_writeMode = mode;
	}
}
