
using System;
using Godot;
using Dictionary = Godot.Collections.Dictionary;
using Array = Godot.Collections.Array;


public class Utils : Node
{
	public byte[] EncodeData(string data, WebSocketPeer.WriteMode mode)
	{  
		if(mode == WebSocketPeer.WriteMode.Text)
		{
			return System.Text.Encoding.UTF8.GetBytes(data);
		}
		return GD.Var2Bytes(data);
	}
	public object DecodeData(byte[] data, bool isString)
	{  
		if(isString)
		{
			return System.Text.Encoding.UTF8.GetString(data);
		}
		return GD.Bytes2Var(data);
	}
	public void _Log(RichTextLabel node, string msg)
	{  
		GD.Print(msg);
		node.AddText(GD.Str(msg) + "\n");
	}
}
