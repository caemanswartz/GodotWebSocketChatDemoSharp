
using System;
using Godot;
using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;


public class ClientUI : Control
{
	public Utils _utils;
	public Client _client;
	public RichTextLabel _logDest;
	public LineEdit _lineEdit;
	public LineEdit _host;
	public CheckBox _multiplayer;
	public OptionButton _writeMode;
	public OptionButton _destination;
	public override void _Ready()
	{
		_utils=GetNode<Utils>("/root/Utils");
		_client = GetNode<Client>("Client");
		_logDest = GetNode<RichTextLabel>("Panel/VBoxContainer/RichTextLabel");
		_lineEdit = GetNode<LineEdit>("Panel/VBoxContainer/Send/LineEdit");
		_host = GetNode<LineEdit>("Panel/VBoxContainer/Connect/Host");
		_multiplayer = GetNode<CheckBox>("Panel/VBoxContainer/Settings/Multiplayer");
		_writeMode = GetNode<OptionButton>("Panel/VBoxContainer/Settings/Mode");
		_destination = GetNode<OptionButton>("Panel/VBoxContainer/Settings/Destination");
		_writeMode.Clear();
		_writeMode.AddItem("BINARY");
		_writeMode.SetItemMetadata(0, WebSocketPeer.WriteMode.Binary);
		_writeMode.AddItem("TEXT");
		_writeMode.SetItemMetadata(1, WebSocketPeer.WriteMode.Text);
		_destination.AddItem("Broadcast");
		_destination.SetItemMetadata(0, 0);
		_destination.AddItem("Last connected");
		_destination.SetItemMetadata(1, 1);
		_destination.AddItem("All But last connected");
		_destination.SetItemMetadata(2, -1);
		_destination.Select(0);
	}
	public void _OnModeItemSelected(int _id)
	{  
		_client.SetWriteMode((WebSocketPeer.WriteMode)_writeMode.GetSelectedMetadata());
	}
	public void _OnSendPressed()
	{  
		if(_lineEdit.Text == "")
		{
			return;
		}
		int dest = (int)_destination.GetSelectedMetadata();
		if(dest > 0)
		{
			dest = _client.lastConnectedClient;
		}
		else if(dest < 0)
		{
			dest = -_client.lastConnectedClient;
		}
		_utils._Log(_logDest, $"Sending data {_lineEdit.Text} to {dest}");
		_client.SendData(_lineEdit.Text, dest);
		_lineEdit.Text = "";
	}
	
	public void _OnConnectToggled(bool pressed )
	{  
		if(pressed)
		{
			var multiplayer = _multiplayer.Pressed;
			if(multiplayer)
			{
				_writeMode.Disabled = true;
			}
			else
			{
				_destination.Disabled = true;
			}
			_multiplayer.Disabled = true;
			if(_host.Text != "")
			{
				_utils._Log(_logDest, $"Connecting to host: {_host.Text}");
				string[] supportedProtocols = {"my-protocol2", "my-protocol", "binary"};
				_client.ConnectToUrl(_host.Text, supportedProtocols, multiplayer);
			}
		}
		else
		{
			_destination.Disabled = false;
			_writeMode.Disabled = false;
			_multiplayer.Disabled = false;
			_client.DisconnectFromHost();
		}
	}
}
