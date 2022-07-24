using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class Room : Singleton<Room>
    {
        public bool isSingle = true;
        public Mode mode;
        public List<Player> players;

        public async Task StartRank()
        {
            // while (connection.websocket == null || connection.websocket.State != NativeWebSocket.WebSocketState.Open)
            // {
            //     await Task.Yield();
            // }

            // await connection.websocket.SendText(
            //     $"{{\"eventname\": \"create_player\", \"username\": \"{User.Instance.Username}\"}}"
            // );
            var json = new CreatePlayerJson();
            json.eventname = "create_player";
            json.username = User.Instance.Username;

            var connection = Connection.Instance;
            while (connection.websocket == null || connection.websocket.State != NativeWebSocket.WebSocketState.Open)
            {
                await Task.Yield();
            }

            Connection.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));

            var message = await Connection.Instance.PopMsg();
            while (JsonUtility.FromJson<WebsocketJson>(message).eventname != "start_game")
            {
                message = await Connection.Instance.PopMsg(); ;
            }
            var startGameJson = JsonUtility.FromJson<StartGameJson>(message);
            InitPlayers(startGameJson);
            // await Connection.Instance.SendWebSocketMessage
            // (
            //     $"{{\"eventname\": \"create_player\", \"username\": \"{User.Instance.Username}\"}}"
            // );
        }

        public void CreatePlayer(WebsocketJson json)
        {
            // Player player = new Player();
            // player.Username = json.username;
            // players.Add(player);
            // Debug.Log("add " + player.Username);
        }

        public void InitPlayers(StartGameJson json)
        {
            players = new List<Player>();
            RandomGeneral = json.generals;
            Debug.Log(json.generals[0]);
            foreach (string username in json.players)
            {
                Player player = new Player();
                player.Username = username;
                players.Add(player);
            }
        }

        public List<int> RandomGeneral { get; set; }
    }
}