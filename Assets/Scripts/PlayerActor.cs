using UnityEngine;
using System.Collections;
using Lidgren.Network;

public class PlayerActor : NetworkActor
{
	public override void Init ()
	{
        if (is_owner)
        {
            Camera.main.GetComponent<LocalCamera>().Target = transform;
        }
	}
}
