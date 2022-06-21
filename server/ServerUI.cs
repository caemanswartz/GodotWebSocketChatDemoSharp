
using System;
using Godot;
using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;


public class ServerUI : Control
{
	public Utils _utils;
	public Server _server;
	public SpinBox _port;
	public LineEdit _lineEdit;
	public OptionButton _writeMode;
	public RichTextLabel _logDest;
	public CheckBox _multiplayer;
	public OptionButton _destination;
	
	public override void _Ready()
	{
		_utils=GetNode<Utils>("/root/Utils");
		_server = GetNode<Server>("Server");
		_port = GetNode<SpinBox>("Panel/VBoxContainer/HBoxContainer/Port");
		_lineEdit = GetNode<LineEdit>("Panel/VBoxContainer/HBoxContainer3/LineEdit");
		_writeMode = GetNode<OptionButton>("Panel/VBoxContainer/HBoxContainer2/WriteMode");
		_logDest = GetNode<RichTextLabel>("Panel/VBoxContainer/RichTextLabel");
		_multiplayer = GetNode<CheckBox>("Panel/VBoxContainer/HBoxContainer2/MPAPI");
		_destination = GetNode<OptionButton>("Panel/VBoxContainer/HBoxContainer2/Destination");
		
		_writeMode.Clear();
		_writeMode.AddItem("BINARY");
		_writeMode.SetItemMetadata(0, WebSocketPeer.WriteMode.Binary);
		_writeMode.AddItem("TEXT");
		_writeMode.SetItemMetadata(1, WebSocketPeer.WriteMode.Text);
		_writeMode.Select(0);
	
		_destination.AddItem("Broadcast");
		_destination.SetItemMetadata(0, 0);
		_destination.AddItem("Last connected");
		_destination.SetItemMetadata(1, 1);
		_destination.AddItem("All But last connected");
		_destination.SetItemMetadata(2, -1);
		_destination.Select(0);
	
	
	}
	
	public void _OnListenToggled(bool pressed)
	{  
		if(pressed)
		{
			var useMultiplayer = _multiplayer.Pressed;
			_multiplayer.Disabled = true;
			string[] supportedProtocols = {"my-protocol", "binary"};
			var port = (int)_port.Value;
			if(useMultiplayer)
			{
				_writeMode.Disabled = true;
				_writeMode.Select(0);
			}
			else
			{
				_destination.Disabled = true;
				_destination.Select(0);
			}
			if(_server.Listen(port, supportedProtocols, useMultiplayer) == Error.Ok)
			{
				_utils._Log(_logDest, $"Listing on port {port}");
				if(!useMultiplayer)
				{
					_utils._Log(_logDest, $"Supported protocols: {supportedProtocols}");
				}
			}
			else
			{
				_utils._Log(_logDest, $"Error  listening on port {port}");
			}
		}
		else
		{
			_server.Stop();
			_multiplayer.Disabled = false;
			_writeMode.Disabled = false;
			_destination.Disabled = false;
			_utils._Log(_logDest, "Server stopped");
		}
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
			dest = _server.lastConnectedClient;
		}
		else if(dest < 0)
		{
			dest = -_server.lastConnectedClient;
		}
		_utils._Log(_logDest, $"Sending data {_lineEdit.Text} to {dest}");
		_server.SendData(_lineEdit.Text, dest);
		_lineEdit.Text = "";
	}
	public void _OnWriteModeItemSelected(int _id)
	{  
		_server.SetWriteMode((WebSocketPeer.WriteMode)_writeMode.GetSelectedMetadata());
	}
}
