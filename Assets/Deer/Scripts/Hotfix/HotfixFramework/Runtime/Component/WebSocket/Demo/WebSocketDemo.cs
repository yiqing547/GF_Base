using System;
using System.Collections;
using System.Collections.Generic;
using DPLogin;
using GameFramework.Event;
using UnityEngine;
using UnityEngine.Networking;
using UnityGameFramework.Runtime;

public class WebSocketDemo : MonoBehaviour
{
    private int connectCount = 0;
    private readonly int maxReconnectCount = 3;
    private ServerInfo serverInfo;

    void Start()
    {
        GameEntry.WebSocket.RegisterEvent((int)MID.LoginFinishRes, OnLoginFinish);
        GameEntry.WebSocket.RegisterEvent((int)MID.LoginRes, OnLoginRes);

        GameEntry.Event.Subscribe(ServerConnectSuccessEventArgs.EventId, OnServerConnectSuccess);
    }

    private void OnServerConnectSuccess(object sender, GameEventArgs e)
    {
        
    }

    private void OnDestroy()
    {
        GameEntry.WebSocket.UnRegisterEvent((int)MID.LoginFinishRes);
        GameEntry.WebSocket.UnRegisterEvent((int)MID.LoginRes);
    }

    private void OnLoginRes(byte[] msg)
    {
        LoginResponse loginResponse = ProtobufUtils.Deserialize<LoginResponse>(msg);
        if (loginResponse != null)
        {

        }
    }

    private void OnLoginFinish(byte[] msg)
    {
        LoginFinishResponse loginFinishResponse = ProtobufUtils.Deserialize<LoginFinishResponse>(msg);
        if (loginFinishResponse != null)
        {

        }
    }

    /// <summary>
    /// 请求服务器列表
    /// </summary>
    /// <returns></returns>
    private IEnumerator IRequestSeverList()
    {
        connectCount = 0;
        bool connectFinish = false;

        while (!connectFinish)
        {
            using UnityWebRequest webRequest = UnityWebRequest.Get("https://rpg-wx.fjbgwl.com:35002/");
            yield return webRequest.SendWebRequest();
            if (!string.IsNullOrEmpty(webRequest.error))
            {
                Log.Error("获取服务器列表失败 error = " + webRequest.error);
                connectCount++;
                if (connectCount >= maxReconnectCount)
                {
                    connectFinish = true;
                }
                yield return new WaitForSeconds(0.5f);
            }
            else
            {
                connectFinish = true;
                Servers servers = JsonUtility.FromJson<Servers>(webRequest.downloadHandler.text);

                if (servers.gateServers.Count > 0)
                {
                    serverInfo = servers.gateServers[0];
                    GameEntry.WebSocket.ConnectNet(serverInfo.ip, serverInfo.port);
                }
            }
        }
    }

    private void Login()
    {
        string account = "yiqing";
        LoginRequest request = new()
        {
            Account = account,
            CellId = serverInfo.cellId,
            Udid = SystemInfo.deviceUniqueIdentifier,
            Version = Application.version,
            Channel = "google",
            OsInfo = SystemInfo.operatingSystem,
#if EnableAnalytics
            DistinctId = TDAnalytics.GetDistinctId() == null ? account : TDAnalytics.GetDistinctId(),
#else
            DistinctId = account,
#endif
        };

#if UNITY_ANDROID
        request.OsVersion = "Android";
#elif UNITY_IOS
        request.OsVersion = "iOS";
#else
        request.OsVersion = "other";
#endif

        GameEntry.WebSocket.Request((int)MID.LoginReq, request);
    }
}

[Serializable]
public class ServerInfo
{
    public int cellId;
    public int id;
    public string ip;
    public string name;
    public int port;
}

[Serializable]
public class Servers
{
    public List<ServerInfo> gateServers;
}
