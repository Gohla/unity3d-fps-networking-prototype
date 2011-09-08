using System;
using System.Collections;
using System.Collections.Generic;
using Lidgren.Network;
using UnityEngine;

public class NetworkActor : MonoBehaviour
{
    public const int MaxId = UInt16.MaxValue;

    [HideInInspector]
    public byte host_id = 0;

    [HideInInspector]
    public int actor_id = 0;

    [HideInInspector]
    public NetworkClientInfo owner = null;

    [HideInInspector]
    public bool is_owner = false;

    [HideInInspector]
    public string prefab_name = null;

    List<NetworkState> net_states = new List<NetworkState>();

    public void Send(NetOutgoingMessage msg)
    {
        msg.Write(actor_id);

        for (var i = 0; i < net_states.Count; ++i)
        {
            net_states[i].Send(msg);
        }
    }

    public void Receive(NetIncomingMessage msg)
    {
        for (var i = 0; i < net_states.Count; ++i)
        {
            net_states[i].Receive(msg);
        }
    }

    public void NetworkFixedUpdate()
    {
        for (var i = 0; i < net_states.Count; ++i)
        {
            if (NetworkPeer.is_server)
            {
                net_states[i].NetworkFixedUpdateServer();
            }
            else
            {
                net_states[i].NetworkFixedUpdateClient();
            }
        }
    }

    public override int GetHashCode()
    {
        return actor_id;
    }

    public virtual void Init()
    {

    }

    void LocateAndSortScripts()
    {
        var components = GetComponents<NetworkState>();

        for (var i = 0; i < components.Length; ++i)
        {
            var state = components[i];
            state.net_actor = this;
            state.Init();
            net_states.Add(state);
        }

        net_states.Sort(SortNetworkStates);
    }

    int SortNetworkStates(NetworkState x, NetworkState y)
    {
        if (System.Object.ReferenceEquals(x, y))
            return 0;

        var cmp = x.GetType().FullName.CompareTo(y.GetType().FullName);
        if (cmp == 0)
        {
            throw new Exception("Two instances of the same network state are attached to the same actor (" + x.GetType().FullName + " and " + y.GetType().FullName + ")");
        }

        return cmp;
    }

    void Start()
    {
        LocateAndSortScripts();
        NetworkActorRegistry.RegisterActor(this);
        Init();
    }

    void Update()
    {
        NetworkTime.Update();

        for (var i = 0; i < net_states.Count; ++i)
        {
            if (NetworkPeer.is_server)
            {
                net_states[i].NetworkUpdateServer();
            }
            else
            {
                net_states[i].NetworkUpdateClient();
            }
        }
    }
}
