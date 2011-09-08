using UnityEngine;

class NetworkServerBehaviour : MonoBehaviour
{
    public NetworkServer server;

    void Start()
    {
        server = new NetworkServer();
    }

    void FixedUpdate()
    {
        server.MessagePump();
    }

    public bool is_running
    {
        get
        {
            return enabled && server != null;
        }
    }
}
