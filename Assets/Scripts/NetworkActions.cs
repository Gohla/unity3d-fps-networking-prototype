using UnityEngine;
using System.Collections;
using System;
using Lidgren.Network;

public class NetworkActions : NetworkState {

    GameObject target;

    public override void Receive(NetIncomingMessage msg)
    {

    }

    public override void Send(NetOutgoingMessage msg)
    {

    }

    public override void Init()
    {
        target = (GameObject)GameObject.Instantiate(Resources.Load("Target"), Vector3.zero, Quaternion.identity);
        target.renderer.enabled = false;
    }

    public override void NetworkFixedUpdateClient()
    {
        if (!net_actor.is_owner && (UserInput.cmd.keystate & UserInput.FIRE) != 0)
        {
            SetTargetPosition();
        }
    }

    public override void NetworkFixedUpdateServer()
    {
        if (net_actor.owner.cmd_queue.Count > 0)
        {
            var cmd = net_actor.owner.cmd_queue.Peek();

            if ((cmd.keystate & UserInput.FIRE) != 0)
            {
                foreach (var actor in net_actor.owner.proximity_set)
                {
                    if (actor != net_actor)
                    {
                        var f = actor.gameObject.GetComponent<NetworkActions>();
                        var p = actor.gameObject.GetComponent<NetworkPosition>();

                        p.SetPosition(cmd.client_time - 0.1f);
                        f.SetTargetPosition();
                        p.SetPosition();
                    }
                }
            }
        }
    }

    void SetTargetPosition()
    {
        target.transform.position = transform.position;
        target.transform.Translate(Vector3.up);
        target.renderer.enabled = true;
    }
}
