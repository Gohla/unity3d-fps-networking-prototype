using UnityEngine;
using Lidgren.Network;
using System;
using System.Collections;
using System.Collections.Generic;

public class NetworkPosition : NetworkState
{
    public struct State
    {
        public Vector3 pos;
		public Quaternion rot;
        public float timestamp;
    }

    const int STATE_BUFFER_SIZE = 20;

    Vector3 pos_to;
    Vector3 pos_from;
    Vector3 verified_pos;
	
	Quaternion rot_to;
    Quaternion rot_from;
    Quaternion verified_rot;

    float interp_start;
    byte last_cmd_id;

    int state_count;
    State[] state_buffer;

    void FreeStateSlot()
    {
        for (var i = (STATE_BUFFER_SIZE - 2); i >= 0; --i)
        {
            state_buffer[i + 1] = state_buffer[i];
        }

        state_count = Math.Min(state_count + 1, STATE_BUFFER_SIZE);
    }

    public override void Send(NetOutgoingMessage msg)
    {
        msg.Write(last_cmd_id);
        NetworkUtils.Write(msg, transform.position);
		NetworkUtils.Write(msg, transform.rotation);

        FreeStateSlot();
        state_buffer[0].pos = transform.position;
		state_buffer[0].rot = transform.rotation;
        state_buffer[0].timestamp = NetworkTime.step / 66.6666666f;
    }


    public override void Receive(NetIncomingMessage msg)
    {
        var command_id = msg.ReadByte();
        var position = NetworkUtils.ReadVector3(msg);
		var rotation = NetworkUtils.ReadQuaternion(msg);

        if (net_actor.is_owner)
        {
            verified_pos = position;
			verified_rot = rotation;
            last_cmd_id = command_id;
        }
        else
        {
            FreeStateSlot();
            state_buffer[0].pos = position;
			state_buffer[0].rot = rotation;
            state_buffer[0].timestamp = NetworkTime.step / 66.66666666f;
        }
    }

    public override void NetworkFixedUpdateServer()
    {
        if (net_actor.owner.cmd_queue.Count > 0)
        {
            var cmd = net_actor.owner.cmd_queue.Peek();
            last_cmd_id = cmd.commandid;
			rigidbody.MoveRotation(rigidbody.rotation * UserInput.KeyStateToRotation(cmd.keystate));
			Vector3 pos = rigidbody.position + (rigidbody.rotation * UserInput.KeyStateToVelocity(cmd.keystate));
			pos.y = 1.0f;
			rigidbody.MovePosition(pos);
        }
    }
    public override void NetworkFixedUpdateClient()
    {
        // Owning client
        if (net_actor.is_owner)
        {
            ((NetworkClient)NetworkPeer.instance).has_spawned = true;

            interp_start = Time.time;
            pos_from = pos_to;
            pos_to = verified_pos;
			rot_from = rot_to;
			rot_to = verified_rot;

            for (var i = 0; i < UserInput.cmd_buffer.Length; ++i)
            {
                var cmd = UserInput.cmd_buffer[i];
                if (cmd.commandid != 0 && cmd.commandid == last_cmd_id)
                {
                    for (var j = (i - 1); j >= 0; --j)
                    {
						rot_to *= UserInput.KeyStateToRotation(UserInput.cmd_buffer[j].keystate);
                        pos_to += rot_to * UserInput.KeyStateToVelocity(UserInput.cmd_buffer[j].keystate);
                    }

                    break;
                }
            }
        
			rot_to *= UserInput.KeyStateToRotation(UserInput.cmd.keystate);
            pos_to += rot_to * UserInput.KeyStateToVelocity(UserInput.cmd.keystate);
        }
    }

    public override void Init()
    {
        state_buffer = new State[STATE_BUFFER_SIZE];
        pos_to = pos_from = verified_pos = transform.position;
		rot_to = rot_from = verified_rot = transform.rotation;
    }

    public bool SetPosition(float time)
    {
        for (var i = 0; i < state_count; ++i)
        {
            if (state_buffer[i].timestamp <= time || i == state_count - 1)
            {
                var rhs = state_buffer[Math.Max(i - 1, 0)];
                var lhs = state_buffer[i];
                var length = rhs.timestamp - lhs.timestamp;
                var t = 0.0f;

                if (length > 0.0001f)
                {
                    t = (float)((time - lhs.timestamp) / length);
                }
				
				rigidbody.MoveRotation(Quaternion.Slerp(lhs.rot, rhs.rot, t));
				rhs.pos.y = 1.0f;
                rigidbody.MovePosition(Vector3.Lerp(lhs.pos, rhs.pos, t));
                return true;
            }
        }

        return false;
    }

    public bool SetPosition()
    {
        return SetPosition(NetworkTime.gameTime - 0.1f);
    }

    public override void NetworkUpdateClient()
    {
        if (NetworkPeer.is_client)
        {
            if (!net_actor.is_owner)
            {
                if (!SetPosition())
                {
                    // If we failed to set position, we need to extrapolate...
                }
            }
            else
            {
				var t = (Time.time - interp_start) / NetworkPeer.TICK_TIME;
				rigidbody.MoveRotation(Quaternion.Slerp(rot_from, rot_to, Mathf.Min(t, 1.0f)));
				pos_to.y = 1.0f;
				rigidbody.MovePosition(Vector3.Lerp(pos_from, pos_to, Mathf.Min(t, 1.0f)));
            }
        }
    }
}
