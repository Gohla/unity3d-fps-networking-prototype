using Lidgren.Network;
using UnityEngine;

public abstract class NetworkPeer
{
    // Message flags
    public const byte REMOTE_CALL_FLAG = 0;
    public const byte USER_COMMAND_FLAG = 1;
    public const byte ACTOR_STATE_FLAG = 2;
    public const byte ACTOR_EVENT_FLAG = 3;

    // Tick options
    public const float TICK_TIME = 0.015f;

    // Configuration options
    public const int APP_PORT = 14000;
    public const string APP_IDENTIFIER = "mobax";
    public const float SIMULATED_LOSS = 0.0f;
    public const float SIMULATED_MIN_LATENCY = 0.0f;
    public const float SIMULATDE_RANDOM_LATENCY = 0.0f;

    // Static variables
    public static bool is_server;
    public static bool is_client;
    public static NetworkPeer instance;

    // Instance variables
    public byte host_id;
    public NetPeer net_peer;
    public int ticks;
    public readonly int send_rate;

    public NetworkPeer(int send_rate)
        : base()
    {
        this.send_rate = send_rate;
    }

    public void MessagePump()
    {
        NetIncomingMessage msg;

        ticks += 1;
        BeforePump();

        while ((msg = net_peer.ReadMessage()) != null)
        {
            switch (msg.MessageType)
            {
                case NetIncomingMessageType.Data:
                    OnDataMessage(msg);
                    break;

                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.ErrorMessage:
                    OnDebugMessage(msg);
                    break;

                case NetIncomingMessageType.StatusChanged:
                    OnStatusChanged(msg);
                    break;

                default:
                    OnUnknownMessage(msg);
                    break;
            }

            net_peer.Recycle(msg);
        }

        Simulate();

        if (ticks == send_rate)
        {
            OnSend();
            ticks = 0;
        }
    }

    public virtual NetOutgoingMessage CreateMessage()
    {
        return net_peer.CreateMessage();
    }

    protected NetPeerConfiguration CreateConfig()
    {
        var config = new NetPeerConfiguration(APP_IDENTIFIER);
        config.SimulatedMinimumLatency = SIMULATED_MIN_LATENCY;
        config.SimulatedRandomLatency = SIMULATDE_RANDOM_LATENCY;
        config.SimulatedLoss = SIMULATED_LOSS;
        return config;
    }

    protected virtual void OnDataMessage(NetIncomingMessage msg)
    {
        Debug.Log("Data message: " + msg.LengthBytes + " bytes");
    }

    protected virtual void OnDebugMessage(NetIncomingMessage msg)
    {
        Debug.Log("Debug message: " + msg.ReadString());
    }

    protected virtual void OnStatusChanged(NetIncomingMessage msg)
    {
        Debug.Log("Status changed: " + msg.SenderConnection.Status);
    }

    protected virtual void OnUnknownMessage(NetIncomingMessage msg)
    {
        Debug.Log("Unhandled message type: " + msg.MessageType);
    }

    protected virtual void BeforePump()
    {

    }

    protected virtual void AfterSimulate()
    {
		
    }

    protected virtual void OnSend()
    {

    }

    void Simulate()
    {
        NetworkActor obj;

        for (var i = 0; i <= NetworkActorRegistry.MaxIndex; ++i)
        {
            if ((obj = NetworkActorRegistry.Objects[i]) != null)
            {
                obj.NetworkFixedUpdate();
            }
        }

        AfterSimulate();
    }
}

