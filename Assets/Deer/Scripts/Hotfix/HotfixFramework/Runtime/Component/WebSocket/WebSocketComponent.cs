using System;
using System.Collections.Generic;
using System.IO;
using Google.Protobuf;
using UnityEngine;
using UnityGameFramework.Runtime;
using UnityWebSocket;

public delegate void OnRevicePacketMsg(byte[] msg);

[DisallowMultipleComponent]
[AddComponentMenu("Deer/WebSocket")]
public class WebSocketComponent : GameFrameworkComponent
{
    private const int PACKETHEADLEN = 8;
    private readonly MemoryStream m_stream = new();
    private WebSocket m_WebSocket;
    private readonly Dictionary<MID, OnRevicePacketMsg> m_vHandles = new();
    private readonly float m_HeartBeatInterval = 10.0f;
    private float m_Interval = 0.0f;

    public void Update()
    {
        if (GetWebSocketState() == WebSocketState.Open)
        {
            m_Interval += Time.deltaTime;
            if (m_Interval > m_HeartBeatInterval)
            {
                HeartBeat();
            }
        }
    }

    public WebSocketState GetWebSocketState()
    {
        return m_WebSocket == null ? WebSocketState.Closed : m_WebSocket.ReadyState;
    }

    /// <summary>
    /// 注册指令返回事件
    /// </summary>
    /// <param name="msgID"></param>
    /// <param name="handler"></param>
    public void RegisterEvent(MID msgID, OnRevicePacketMsg handler)
    {
        if (m_vHandles.ContainsKey(msgID)) return;
        m_vHandles.Add(msgID, handler);
    }

    public void UnRegisterEvent(MID msgID)
    {
        if (!m_vHandles.ContainsKey(msgID)) return;
        m_vHandles.Remove(msgID);
    }

    /// <summary>
    /// 连接网络
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    public void ConnectNet(string host, int port)
    {
        string address = "wss://" + host + ":" + port;
        if (m_WebSocket == null)
        {
            try
            {
                m_WebSocket = new WebSocket(address);
                m_WebSocket.OnOpen += OnOpen;
                m_WebSocket.OnMessage += OnMessage;
                m_WebSocket.OnClose += OnClose;
                m_WebSocket.OnError += OnError;
                m_WebSocket.ConnectAsync();
            }
            catch (Exception) { }
        }
        else
        {
            Debug.LogError("Socket 已连接或正在连接中");
        }
    }

    /// <summary>
    /// 请求服务端数据
    /// </summary>
    /// <param name="msgID">指令头</param>
    /// <param name="msg">指令内容</param>
    public void Request(MID msgID, IMessage msg)
    {
        if (GetWebSocketState() == WebSocketState.Open)
        {
            int len = msg.CalculateSize();
            m_stream.Position = 0;
            m_stream.Write(BigEndian(len + 4), 0, 4);   //protobuff长度  
            m_stream.Write(BigEndian((int)msgID << 16), 0, 4);   //指令头 服务端约定左移16位
            m_stream.Write(msg.ToByteArray(), 0, len);  //指令内容
            m_WebSocket.SendAsync(m_stream.ToArray());
            Log.Info("======sendPack======== mid: " + msgID + "  mName = " + Enum.GetName(typeof(MID), msgID) + "  msg = " + msg.ToString());
        }
        else
        {
            //todo
            Log.Warning("重新连接服务器，待完成");
        }
    }

    private void Response(byte[] bytes)
    {
        m_Interval = 0;
        if (bytes.Length >= PACKETHEADLEN)
        {
            int protoLength = BitConverter.ToInt32(bytes, 0);   //前四位pb长度
            int combine = BitConverter.ToInt32(bytes, 4);       //指令头
            int msgID = combine >> 16;

            byte[] m_pReciveBuff = new byte[protoLength];
            Buffer.BlockCopy(bytes, PACKETHEADLEN, m_pReciveBuff, 0, protoLength);
            if (m_vHandles.TryGetValue((MID)msgID, out OnRevicePacketMsg onHandler))
            {
                onHandler?.Invoke(m_pReciveBuff);
            }
        }
        else
        {
            Log.Error("指令长度有问题");
        }
    }

    private void OnError(object sender, UnityWebSocket.ErrorEventArgs e)
    {
        CloseSocket();
    }

    private void OnClose(object sender, CloseEventArgs e)
    {
        CloseSocket();
    }

    private void OnMessage(object sender, MessageEventArgs e)
    {
        if (e.IsBinary)
        {
            Response(e.RawData);
        }
    }

    private void OnOpen(object sender, OpenEventArgs e)
    {
        //socket连接成功，广播成功事件
        GameEntry.Event.Fire(this, ServerConnectSuccessEventArgs.Create());
    }

    /// <summary>
    /// 小端转大端
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    private byte[] BigEndian(int value)
    {
        byte[] result = BitConverter.GetBytes(value);
        if (BitConverter.IsLittleEndian) //小端转大端
            Array.Reverse(result);
        return result;
    }

    private void CloseSocket()
    {
        m_WebSocket = null;
    }

    /// <summary>
    /// 发送心跳包
    /// </summary>
    private void HeartBeat()
    {
        //todo
        m_Interval = 0;
        Log.Warning("发送心跳包，待完成");
    }
}