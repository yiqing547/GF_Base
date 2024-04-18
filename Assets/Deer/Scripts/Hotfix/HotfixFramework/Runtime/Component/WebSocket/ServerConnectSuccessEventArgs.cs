using System.Collections.Generic;
using GameFramework;
using GameFramework.Event;

public class ServerConnectSuccessEventArgs : GameEventArgs
{
    public static readonly int EventId = typeof(ServerConnectSuccessEventArgs).GetHashCode();

    public override int Id
    {
        get
        {
            return EventId;
        }
    }

    public static ServerConnectSuccessEventArgs Create()
    {
        ServerConnectSuccessEventArgs eventArgs = ReferencePool.Acquire<ServerConnectSuccessEventArgs>();

        return eventArgs;
    }

    public override void Clear()
    {
    }
}