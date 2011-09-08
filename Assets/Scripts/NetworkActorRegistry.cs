using System;
using System.Collections;
using System.Collections.Generic;
using Lidgren.Network;
using UnityEngine;

public static class NetworkActorRegistry
{
    public static NetworkActor[] Objects = new NetworkActor[256];
    public static int[] DeliveryMethods = new int[256];
    public static int MaxIndex = -1;
    public static int Count = 0;

    public static void RegisterActor(NetworkActor obj)
    {
        var index = obj.actor_id;

        if (index >= Objects.Length)
        {
            var size = Math.Min(Objects.Length * 2, NetworkActor.MaxId);
            var newObjects = new NetworkActor[size];
            Array.Copy(Objects, newObjects, Objects.Length);
            Objects = newObjects;
        }

        MaxIndex = Math.Max(MaxIndex, index);
        Objects[index] = obj;
        ++Count;
    }

    public static NetworkActor GetById(int id)
    {
        if (id < Objects.Length)
        {
            return Objects[id];
        }

        return null;
    }

    public static void UnregisterActor(NetworkActor obj)
    {
        Objects[obj.actor_id] = null;
        --Count;
    }

    public static void RegisterDeliveryMethod(NetDeliveryMethod deliveryMethod)
    {
        DeliveryMethods[(int)deliveryMethod]++;
    }

    public static void UnregisterDeliveryMethod(NetDeliveryMethod deliveryMethod)
    {
        DeliveryMethods[(int)deliveryMethod]--;
    }

    public static bool HasDeliveryMethod(NetDeliveryMethod deliveryMethod)
    {
        return DeliveryMethods[(int)deliveryMethod] > 0;
    }
}