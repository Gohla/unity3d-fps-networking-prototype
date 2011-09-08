using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;

class NetworkClient : NetworkPeer
{
    public const int SEND_RATE = 2;
    public const float SEND_TIME = 0.030f;
    public readonly NetClient net_client;
	public bool has_spawned;

    public float rtt
    {
        get
        {
            if (net_client == null || net_client.ServerConnection == null)
                return -1.0f;

            return net_client.ServerConnection.AverageRoundtripTime;
        }
    }

    public NetworkClient()
        : base(SEND_RATE)
    {
        instance = this;
        is_server = false;
        is_client = true;

        net_peer = net_client = new NetClient(CreateConfig());
        net_client.Start();
        net_client.Connect("127.0.0.1", NetworkPeer.APP_PORT);
    }

    [RPC]
    public void Hello(NetIncomingMessage msg, byte host_id)
    {
        this.host_id = host_id;
        NetworkRemoteCall.CallOnServer("RequestObjects");
    }

    [RPC]
    public void Spawn(NetIncomingMessage msg, byte hostId, int objectId, string prefabName, Vector3 pos, Quaternion rot)
    {
        var game_object = (GameObject)GameObject.Instantiate(Resources.Load(prefabName), pos, rot);
        var net_actor = game_object.GetComponent<NetworkActor>();
        net_actor.host_id = hostId;
        net_actor.actor_id = objectId;
        net_actor.is_owner = NetworkPeer.instance.host_id == hostId;
        net_actor.prefab_name = prefabName;

        NetworkRemoteCall.CallOnServer("RequestObjectRegistration", net_actor.actor_id);
        NetworkActorRegistry.RegisterActor(net_actor);
    }

    protected override void OnDataMessage(NetIncomingMessage msg)
    {
        // Update estimated local time
        NetworkTime.step = msg.ReadInt32();
        NetworkTime.SetOffset(NetworkTime.step / 66.66666666f, rtt);

        Debug.Log("local: " + NetworkTime.gameTime);
        Debug.Log("remote: " + (NetworkTime.step / 66.66666666f));

        // Read message flag
        switch (msg.ReadByte())
        {
            case NetworkPeer.REMOTE_CALL_FLAG:
                NetworkRemoteCallReceiver.ReceiveRemoteCall(msg);
                break;

            case NetworkPeer.ACTOR_EVENT_FLAG:
                ReceiveObjectState(msg);
                break;
        }
    }

    private void ReceiveObjectState(NetIncomingMessage msg)
    {
        while (msg.Position < msg.LengthBits)
        {
            var objectId = msg.ReadInt32();
            NetworkActorRegistry.GetById(objectId).Receive(msg);
        }
    }

    protected override void AfterSimulate()
    {
        if (has_spawned)
        {
            UserInput.QueueCurrent();
        }
    }

    protected override void BeforePump()
    {
        NetworkTime.Update();

        if (has_spawned)
        {
            UserInput.Read();

            if (ticks == send_rate)
            {
                SendCommands();
            }
        }
    }

    void SendCommands()
    {
        var msg = CreateMessage();
        msg.Write(NetworkPeer.USER_COMMAND_FLAG);
        msg.Write((byte)UserInput.unsent_cmds);

        for (var i = (UserInput.unsent_cmds - 1); i >= 0; --i)
        {
            var cmd = UserInput.cmd_buffer[i];

            msg.Write(cmd.commandid);
            msg.Write(cmd.keystate);
            msg.Write(cmd.actionstate);
            msg.Write(cmd.client_time);
        }

        UserInput.unsent_cmds = 0;
        net_client.SendMessage(msg, NetDeliveryMethod.UnreliableSequenced);
    }
}
