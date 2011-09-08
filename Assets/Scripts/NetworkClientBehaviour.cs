using UnityEngine;

class NetworkClientBehaviour : MonoBehaviour
{
    public NetworkClient client;

    void Start()
    {
        client = new NetworkClient();
    }

    void FixedUpdate()
    {
        client.MessagePump();
    }

    public bool is_running
    {
        get
        {
            return enabled && client != null;
        }
    }
}
