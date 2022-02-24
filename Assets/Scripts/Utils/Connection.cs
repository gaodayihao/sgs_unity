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
    public bool IsRunning { get; set; } = true;
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

        websocket.OnMessage += async (bytes) =>
        {
            // getting the message as a string
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("OnMessage! " + message);

            WebsocketJson json = JsonUtility.FromJson<WebsocketJson>(message);

            switch (json.eventname)
            {
                // case "create_player":
                //     Model.Room.Instance.CreatePlayer(json);
                //     break;

                case "start_game":
                    var startGameJson = JsonUtility.FromJson<StartGameJson>(message);

                    Model.Room.Instance.InitPlayers(startGameJson);
                    StartCoroutine(SceneManager.Instance.LoadSceneFromAB("SgsMain"));
                    break;

                case "set_result":
                    var timerJson = JsonUtility.FromJson<TimerJson>(message);
                    if (!await Verify(timerJson.id)) break;
                    if (Model.TimerTask.Instance.timerType == TimerType.UseWxkj && !timerJson.result) Count--;

                    Model.TimerTask.Instance.ReceiveSetResult(timerJson);
                    break;

                case "card_panel_result":
                    var cardPanelJson = JsonUtility.FromJson<TimerJson>(message);
                    if (!await Verify(cardPanelJson.id)) break;

                    Model.CardPanel.Instance.ReceiveSetResult(cardPanelJson);
                    break;

                case "execute_phase":
                    var phaseJson = JsonUtility.FromJson<PhaseJson>(message);
                    if (!await Verify(phaseJson.id)) break;

                    Model.TurnSystem.Instance.ReceivePhase(phaseJson);
                    break;

                case "shuffle":
                    var cardPileJson = JsonUtility.FromJson<CardPileJson>(message);
                    if (!await Verify(cardPileJson.id)) break;

                    Model.CardPile.Instance.ReceiveShuffle(cardPileJson);
                    break;
            }
        };

        // Keep sending messages at every 0.3s
        // InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

        // waiting for messages
        await websocket.Connect();
    }

    private async Task<bool> Verify(int id)
    {
        Debug.Log("id=" + id.ToString() + " count=" + Count.ToString());
        if (id <= Count) return false;
        while (IsRunning || id != Count + 1) await Task.Yield();

        IsRunning = true;
        Count = id;
        return true;
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