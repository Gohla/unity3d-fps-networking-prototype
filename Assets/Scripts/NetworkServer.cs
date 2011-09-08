using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using UnityEngine;

class NetworkServer : NetworkPeer
{
    public const int SEND_RATE = 3;
    public const float SEND_TIME = 0.045f;

    public readonly NetServer netServer;
    public readonly HashSet<NetworkClientInfo> connected_clients = new HashSet<NetworkClientInfo>();

    byte host_id_counter;
    int actor_id_counter;

    NetOutgoingMessage out_msg;

    public NetworkServer()
        : base(SEND_RATE)
    {
        instance = this;
        is_server = true;
        is_client = false;

        host_id = 0;
        host_id_counter = 0;
        actor_id_counter = 0;

        var config = CreateConfig();
        config.Port = NetworkPeer.APP_PORT;

        net_peer = netServer = new NetServer(config);
        netServer.Start();
    }

    public NetworkClientInfo GetClientInfo(NetIncomingMessage msg)
    {
        var connection = msg.SenderConnection;

        if (connection.Tag == null)
        {
            connection.Tag = new NetworkClientInfo(++host_id_counter, connection);
        }

        return ((NetworkClientInfo)connection.Tag);
    }

    protected override void OnStatusChanged(NetIncomingMessage msg)
    {
        switch (msg.SenderConnection.Status)
        {
            case NetConnectionStatus.Connected:
                var new_client = GetClientInfo(msg);
                connected_clients.Add(new_client);
                NetworkRemoteCall.CallOnClient(new_client, "Hello", new_client.host_id);
                break;

            case NetConnectionStatus.Disconnecting:
            case NetConnectionStatus.Disconnected:
                connected_clients.Remove(GetClientInfo(msg));
                break;
        }
    }

    protected override void OnDataMessage(NetIncomingMessage msg)
    {
        while (msg.Position < msg.LengthBits)
        {
            // Read message flag
            switch (msg.ReadByte())
            {
                case NetworkPeer.USER_COMMAND_FLAG:
                    ReceiveUserCommand(msg);
                    break;

                case NetworkPeer.REMOTE_CALL_FLAG:
                    NetworkRemoteCallReceiver.ReceiveRemoteCall(msg);
                    break;
            }
        }
    }

    private void ReceiveUserCommand(NetIncomingMessage msg)
    {
        var client = GetClientInfo(msg);
        var commandCount = (int)msg.ReadByte();

        for (var i = 0; i < commandCount; ++i)
        {
            UserCommand cmd = new UserCommand();

            cmd.commandid = msg.ReadByte();
            cmd.keystate = msg.ReadByte();
            cmd.actionstate = msg.ReadByte();
            cmd.client_time = msg.ReadFloat();
			
			if(client.has_spawned) {
            	client.cmd_queue.Enqueue(cmd);
			}
        }
    }

    protected override void AfterSimulate()
    {
        foreach (var client in connected_clients)
        {
            // Make sure we remove the current command, if any exist
            if (client.cmd_queue.Count > 0)
            {
                client.cmd_queue.Dequeue();
            }
        }
    }
	
	protected override void BeforePump ()
	{
		NetworkTime.Update();
		NetworkTime.UpdateServerStep();
	}

    public override NetOutgoingMessage CreateMessage()
    {
        var msg = base.CreateMessage();
        msg.Write(NetworkTime.step);
        return msg;
    }

    protected override void OnSend()
    {
        foreach (var client in connected_clients)
        {
            if (client.proximity_set.Count > 0)
            {
                var msg = CreateMessage();
                msg.Write(NetworkPeer.ACTOR_EVENT_FLAG);

                foreach (var obj in client.proximity_set)
                {
                    obj.Send(msg);
                }

                netServer.SendMessage(msg, client.connection, NetDeliveryMethod.UnreliableSequenced, 0);
            }
        }
    }

    [RPC]
    public void RequestObjects(NetIncomingMessage msg)
    {
        var client = GetClientInfo(msg);

        foreach (var obj in NetworkActorRegistry.Objects)
        {
            if (obj != null)
            {
                NetworkRemoteCall.CallOnClient(
                    client, "Spawn",
                    obj.host_id, obj.actor_id, obj.prefab_name,
                    obj.transform.position, obj.transform.rotation
                );
            }
        }
    }

    [RPC]
    public void RequestObjectRegistration(NetIncomingMessage msg, int actor_id)
    {
        var client = GetClientInfo(msg);
        var obj = NetworkActorRegistry.GetById(actor_id);
        client.proximity_set.Add(obj);
    }

    [RPC]
    public void RequestSpawn(NetIncomingMessage msg, string prefab_name, Vector3 pos, Quaternion rot)
    {
        var client_info = GetClientInfo(msg);
        var game_object = (GameObject)GameObject.Instantiate(Resources.Load(prefab_name), pos, rot);
        var obj = game_object.GetComponent<NetworkActor>();
		
        obj.host_id = client_info.host_id;
        obj.actor_id = actor_id_counter++;
        obj.is_owner = false;
        obj.owner = client_info;
        obj.prefab_name = prefab_name;
		
		client_info.has_spawned = true;
		
        NetworkRemoteCall.CallOnAllClients(
            "Spawn",
            obj.host_id, obj.actor_id, obj.prefab_name,
            obj.transform.position, obj.transform.rotation
        );
    }
}