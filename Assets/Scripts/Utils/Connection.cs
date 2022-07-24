using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using NativeWebSocket;

public class Connection : MonoBehaviour
{
    private static Connection instance;
    public static Connection Instance
    {
        get
        {
            if (instance is null)
            {
                GameObject obj = new GameObject("Connection");
                obj = Instantiate(obj);
                DontDestroyOnLoad(obj);
                instance = obj.AddComponent<Connection>();
            }
            return instance;
        }
    }

    public WebSocket websocket { get; private set; }
    // public bool IsRunning { get; set; } = true;
    public int Count { get; set; } = 0;

    // Start is called before the first frame update
    async void Start()
    {
        websocket = new WebSocket("wss://app931.acapp.acwing.com.cn/wss/multiplayer/");

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("OnMessage! " + message);

            messages.Add(message);
        };

        // Keep sending messages at every 0.3s
        // InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

        // waiting for messages
        await websocket.Connect();
    }

    private List<string> messages = new List<string>();

    public async Task<string> PopMsg()
    {
        while (messages.Count == 0) await Task.Yield();
        var msg = messages[0];
        messages.RemoveAt(0);
        return msg;
    }

    public async Task<string> PopSgsMsg()
    {
        var msg = await PopMsg();
        var id = JsonUtility.FromJson<SgsJson>(msg).id;
        if (id <= Count) return await PopSgsMsg();
        else
        {
            Count++;
            return msg;
        }
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }

    public async void SendWebSocketMessage(string json)
    {
        Debug.Log("send:" + json);
        // Sending plain text
        if (websocket.State == WebSocketState.Open) await websocket.SendText(json);
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }
}