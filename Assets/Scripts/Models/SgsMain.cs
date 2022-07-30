using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace Model
{
    public class SgsMain : SingletonMono<SgsMain>
    {
        public async void Start()
        {
            // Connection.Instance.Start();
            // this.mode = mode;
            // 初始化玩家
            PlayersCount = 4;
            players = new Player[PlayersCount];
            // Debug.Log("players init");

            if (Room.Instance.IsSingle)
            {
                // 初始化位置
                for (int i = 0; i < PlayersCount; i++)
                {
                    players[i] = new Player();
                    players[i].isAI = true;
                    AlivePlayers.Add(players[i]);
                }

                // self
                players[0].isSelf = true;
                players[0].isAI = false;
                players[3].isSelf = true;
                players[3].isAI = false;
            }
            else
            {
                players = Room.Instance.players.ToArray();
                foreach (var i in players)
                {
                    if (i.Username == User.Instance.Username) i.isSelf = true;
                    i.isAI = i.Username == "AI";
                    AlivePlayers.Add(i);
                }
            }

            players[0].Teammate = players[3];
            players[1].Teammate = players[2];
            players[2].Teammate = players[1];
            players[3].Teammate = players[0];

            for (int i = 0; i < PlayersCount; i++) players[i].Position = i;
            for (int i = 1; i < PlayersCount; i++) players[i].Last = players[i - 1];
            for (int i = 0; i < PlayersCount - 1; i++) players[i].Next = players[i + 1];

            players[PlayersCount - 1].Next = players[0];
            players[0].Last = players[PlayersCount - 1];

            await Task.Yield();
            positionView(players);

            // 初始化武将
            await InitGeneral();
            // foreach (var player in players) generalView?.Invoke(player);
            generalView();

            // 初始化牌堆
            await CardPile.Instance.Init();

            if (Room.Instance.IsSingle) await DebugCard();

            foreach (var player in players) await new GetCardFromPile(player, 4).Execute();

            // debug
            // await new Damaged(players[0], null).Execute();
            // await new Damaged(players[1], null).Execute();
            // await new Damaged(players[0], null, 2).Execute();

            // 开始第一个回合
            await TurnSystem.Instance.StartGame();
        }


        // 模式
        // public Mode mode { get; private set; }
        // 玩家人数
        public int PlayersCount { get; private set; }
        // 玩家
        public Player[] players;
        public List<Player> AlivePlayers { get; private set; } = new List<Player>();
        public bool GameIsOver { get; private set; } = false;
        public void GameOver()
        {
            GameIsOver = true;
            gameOverView();
        }

        private async Task DebugCard()
        {
            List<string> list = new List<string>
            {
                "决斗","无中生有","诸葛连弩","爪黄飞电","丈八蛇矛"
            };

            while (list.Count > 0)
            {
                await new GetCardFromPile(players[0], 1).Execute();
                var newCard = players[0].HandCards[players[0].HandCardCount - 1];
                if (!list.Contains(newCard.Name)) await new Discard(players[0], new List<Card> { newCard }).Execute();
                else list.Remove(newCard.Name);
            }
        }

        /// <summary>
        /// 初始化武将
        /// </summary>
        private async Task InitGeneral()
        {
            // UnityWebRequest www = UnityWebRequest.Get(Urls.JSON_URL + "general.json");
            // www.SendWebRequest();

            // while (!www.isDone) await Task.Yield();

            // if (www.result != UnityWebRequest.Result.Success)
            // {
            //     Debug.Log(www.error);
            //     return;
            // }

            // List<General> json = JsonList<General>.FromJson(www.downloadHandler.text);

            string url = Urls.JSON_URL + "general.json";
            List<General> json = JsonList<General>.FromJson(await WebRequest.GetString(url));
            // for (int i = 0; i < json.Count; i++) Debug.Log(i.ToString() + json[i].name);


            if (Room.Instance.IsSingle)
            {
                // debug
                General self = json[0];
                json.Remove(self);

                foreach (var i in players)
                {
                    General general;

                    // debug
                    if (i.Position == 0) general = self;

                    else general = json[Random.Range(0, json.Count)];
                    json.Remove(general);

                    i.InitGeneral(general);
                }
            }
            else
                foreach (var player in players)
                {
                    var general = json[Room.Instance.RandomGeneral[player.Position]];
                    player.InitGeneral(general);
                }
        }

        private UnityAction<Player[]> positionView;
        public event UnityAction<Player[]> PositionView
        {
            add => positionView += value;
            remove => positionView -= value;
        }

        private UnityAction generalView;
        public event UnityAction GeneralView
        {
            add => generalView += value;
            remove => generalView -= value;
        }

        private UnityAction gameOverView;
        public event UnityAction GameOverView
        {
            add => gameOverView += value;
            remove => gameOverView -= value;
        }
    }
}