using UnityEngine;
using System.Collections;

public class NetworkController : MonoBehaviour
{
    bool has_spawned = false;

    NetworkServerBehaviour srv;
    NetworkClientBehaviour clt;

    void Start()
    {
        srv = (NetworkServerBehaviour)gameObject.GetComponent(typeof(NetworkServerBehaviour));
        clt = (NetworkClientBehaviour)gameObject.GetComponent(typeof(NetworkClientBehaviour));
    }

    void OnGUI()
    {
        if (SystemInfo.graphicsDeviceID == 0)
        {
            srv.enabled = true;
        }

        if (!srv.is_running && !clt.is_running)
        {
            if (GUI.Button(new Rect(10, 10, 100, 25), "Start Server"))
            {
                srv.enabled = true;
            }

            if (GUI.Button(new Rect(10, 45, 100, 25), "Start Client"))
            {
                clt.enabled = true;
            }
        }
        else
        {
            GUI.TextArea(new Rect(300, 10, 150, 45), "Gametime: " + NetworkTime.gameTime);
        }

        if (srv.is_running)
        {
            GUI.TextArea(new Rect(10, 10, 125, 25), "Running As Server");
            GUI.TextArea(new Rect(10, 45, 125, 25), "Clients: " + srv.server.connected_clients.Count);
        }

        if (clt.is_running)
        {
            GUI.TextArea(new Rect(10, 10, 225, 25), "Running As Client (ping: " + Mathf.Round(clt.client.rtt * 1000.0f) + "ms)");
            GUI.TextArea(new Rect(10, 45, 225, 25), "Client Id: " + clt.client.host_id);

            if (!has_spawned && GUI.Button(new Rect(10, 80, 125, 25), "Spawn"))
            {
                has_spawned = true;
                var respawn = GameObject.FindGameObjectWithTag("Respawn");
                NetworkRemoteCall.CallOnServer("RequestSpawn", "Player", respawn.transform.position, Quaternion.identity);
            }

            if (has_spawned)
            {
				// Auto turn left
                if (GUI.Button(new Rect(10, 80, 25, 25), "Q"))
                {
                    UserInput.auto_keys = (byte)(UserInput.auto_keys ^ UserInput.TURNLEFT);
                }
				
                // Auto walk forward
                if (GUI.Button(new Rect(40, 80, 25, 25), "W"))
                {
                    UserInput.auto_keys = (byte)(UserInput.auto_keys ^ UserInput.FORWARD);
                }
				
				// Auto turn right
                if (GUI.Button(new Rect(70, 80, 25, 25), "E"))
                {
                    UserInput.auto_keys = (byte)(UserInput.auto_keys ^ UserInput.TURNRIGHT);
                }

                // Auto walk backward
                if (GUI.Button(new Rect(40, 110, 25, 25), "S"))
                {
                    UserInput.auto_keys = (byte)(UserInput.auto_keys ^ UserInput.BACKWARD);
                }

                // Auto walk left
                if (GUI.Button(new Rect(10, 110, 25, 25), "A"))
                {
                    UserInput.auto_keys = (byte)(UserInput.auto_keys ^ UserInput.LEFT);
                }

                // Auto walk right
                if (GUI.Button(new Rect(70, 110, 25, 25), "D"))
                {
                    UserInput.auto_keys = (byte)(UserInput.auto_keys ^ UserInput.RIGHT);
                }

                // Auto action 1
                if (GUI.Button(new Rect(10, 140, 25, 25), "1"))
                {
                    UserInput.auto_actions = (byte)(UserInput.auto_actions ^ UserInput.ACTION1);
                }

                // Auto action 2
                if (GUI.Button(new Rect(40, 140, 25, 25), "2"))
                {
                    UserInput.auto_actions = (byte)(UserInput.auto_actions ^ UserInput.ACTION2);
                }

                // Auto action 3
                if (GUI.Button(new Rect(70, 140, 25, 25), "3"))
                {
                    UserInput.auto_actions = (byte)(UserInput.auto_actions ^ UserInput.ACTION3);
                }

                // Auto action 4
                if (GUI.Button(new Rect(100, 140, 25, 25), "4"))
                {
                    UserInput.auto_actions = (byte)(UserInput.auto_actions ^ UserInput.ACTION4);
                }
            }
        }
    }
}
