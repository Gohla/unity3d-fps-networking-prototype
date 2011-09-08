using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Lidgren.Network;
using UnityEngine;

static class UserInput
{
    public const byte FORWARD = 1;
    public const byte BACKWARD = 2;
    public const byte LEFT = 4;
    public const byte RIGHT = 8;
    public const byte FIRE = 16;
	public const byte TURNLEFT = 32;
	public const byte TURNRIGHT = 64;

    public const byte ACTION1 = 1;
    public const byte ACTION2 = 2;
    public const byte ACTION3 = 4;
    public const byte ACTION4 = 8;

    static byte cmd_id = 0;

    public static byte auto_keys;
    public static byte auto_actions;

    public static int unsent_cmds;
    public static UserCommand cmd;
    public static UserCommand[] cmd_buffer = new UserCommand[256];

    public static void Read()
    {
        cmd.commandid = ++cmd_id;
        cmd.actionstate = GetActionState();
        cmd.keystate = GetKeyState();

        if (cmd.commandid == 0)
        {
            cmd.commandid = cmd_id = 1;
        }
    }

    public static void QueueCurrent()
    {
        ++unsent_cmds;

        for (var i = 254; i >= 0; --i)
        {
            cmd_buffer[i + 1] = cmd_buffer[i];
        }

        cmd_buffer[0] = cmd;
        cmd_buffer[0].client_time = NetworkTime.gameTime;
    }

    public static Vector3 KeyStateToVelocity(byte state)
    {
        var vector = Vector3.zero;

        if ((state & FORWARD) > 0) vector += Vector3.forward;
        if ((state & BACKWARD) > 0) vector += Vector3.back;
        if ((state & LEFT) > 0) vector += Vector3.left;
        if ((state & RIGHT) > 0) vector += Vector3.right;

        return vector.normalized * 0.1f;
    }
	
	public static Quaternion KeyStateToRotation(byte state) 
	{
		var quaternion = Quaternion.identity;
		
		if ((state & TURNLEFT) > 0) quaternion *= Quaternion.Euler(0, -2, 0);
		if ((state & TURNRIGHT) > 0) quaternion *= Quaternion.Euler(0, 2, 0);
		
		return quaternion;
	}

    static byte GetKeyState()
    {
        byte state = auto_keys;

        if (Input.GetKey(KeyCode.W)) state |= FORWARD;
        if (Input.GetKey(KeyCode.S)) state |= BACKWARD;
        if (Input.GetKey(KeyCode.A)) state |= LEFT;
        if (Input.GetKey(KeyCode.D)) state |= RIGHT;
		if (Input.GetKey(KeyCode.Q)) state |= TURNLEFT;
        if (Input.GetKey(KeyCode.E)) state |= TURNRIGHT;
        if (Input.GetMouseButton(0)) state |= FIRE;

        return state;
    }

    static byte GetActionState()
    {
        byte state = auto_actions;

        if (Input.GetKey(KeyCode.Alpha1)) state |= ACTION1;
        if (Input.GetKey(KeyCode.Alpha2)) state |= ACTION2;
        if (Input.GetKey(KeyCode.Alpha3)) state |= ACTION3;
        if (Input.GetKey(KeyCode.Alpha4)) state |= ACTION4;

        return state;
    }

}
