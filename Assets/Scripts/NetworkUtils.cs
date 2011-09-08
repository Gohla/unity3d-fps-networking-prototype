using UnityEngine;
using System.Collections;
using Lidgren.Network;

public class NetworkUtils
{
    public static void Write(NetOutgoingMessage msg, Vector3 data)
    {
        msg.Write(data.x);
        msg.Write(data.y);
        msg.Write(data.z);
    }

    public static Vector3 ReadVector3(NetIncomingMessage msg)
    {
        Vector3 data;

        data.x = msg.ReadFloat();
        data.y = msg.ReadFloat();
        data.z = msg.ReadFloat();

        return data;
    }

    public static void Write(NetOutgoingMessage msg, Quaternion data)
    {
        msg.Write(data.x);
        msg.Write(data.y);
        msg.Write(data.z);
        msg.Write(data.w);
    }

    public static Quaternion ReadQuaternion(NetIncomingMessage msg)
    {
        Quaternion data;

        data.x = msg.ReadFloat();
        data.y = msg.ReadFloat();
        data.z = msg.ReadFloat();
        data.w = msg.ReadFloat();

        return data;
    }

    public static void WritePositionRotation(NetOutgoingMessage msg, Transform transform)
    {
        Write(msg, transform.position);
        Write(msg, transform.rotation);
    }
}