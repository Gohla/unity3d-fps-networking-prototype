using Lidgren.Network;
using UnityEngine;

public abstract class NetworkState : MonoBehaviour
{
    [HideInInspector]
    public NetworkActor net_actor = null;

    public NetworkClientInfo owner { get { return net_actor.owner; } }

    public virtual void Init() { }
    public abstract void Send(NetOutgoingMessage msg);
    public abstract void Receive(NetIncomingMessage msg);

    public virtual void NetworkUpdateClient() { }
    public virtual void NetworkUpdateServer() { }

    public virtual void NetworkFixedUpdateClient() { }
    public virtual void NetworkFixedUpdateServer() { }
}