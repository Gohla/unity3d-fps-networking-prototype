using System;
using System.IO;
using UnityEngine;
using System.Runtime.InteropServices;

public static class NetworkTime
{
    static float lastRemoteTime;

	public static int step = 0;
    public static float offset = 0.0f;
    public static float gameTime = 0.0f;

    public static void Update()
    {
        if (NetworkPeer.is_server)
        {
            gameTime = step / 66.66666666f;
            return;
        }

        gameTime = Time.time + offset;
    }
	
	public static void UpdateServerStep()
	{
		var newStep = Mathf.FloorToInt(Time.time * 66.66666666f);
		
		if(newStep-1 != step) 
		{
			step += 1;
		}
		else
		{
			step = newStep;
		}
	}

    public static void SetOffset(float remoteTime, float rtt)
    {
        if (lastRemoteTime != remoteTime)
        {
            var newOffset = remoteTime - Time.time;

            if (offset == 0.0f)
            {
                offset = newOffset;
            }
            else
            {
                offset = (offset * 0.95f) + (newOffset * 0.05f);
            }

            lastRemoteTime = remoteTime;
            Update();
        }
    }

}