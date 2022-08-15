using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Model
{
    public class SgsMain : SingletonMono<SgsMain>
    {
        public async void Start()
        {
            // 初始化玩家
            players = new Player[4];
            List<string> users = Room.Instance.IsSingle ? new List<string> { "AI", "user" } : Room.Instance.Users;
            User.Instance.team = users[0] == User.Instance.Username ? Team.Blue : Team.Red;
            players[0] = new Player(Team.Blue);
            players[1] = new Player(Team.Red);
            players[2] = new Player(Team.Red);
            players[3] = new Player(Team.Blue);

            foreach (var i in players)
            {
                i.isSelf = i.team == User.Instance.team;
                i.isAI = Room.Instance.IsSingle && !i.isSelf;
                AlivePlayers.Add(i);
            }

            players[0].Teammate = players[3];
            players[1].Teammate = players[2];
            players[2].Teammate = players[1];
            players[3].Teammate = players[0];

            for (int i = 0; i < 4; i++) players[i].Position = i;
            for (int i = 1; i < 4; i++) players[i].Last = players[i - 1];
            for (int i = 0; i < 4 - 1; i++) players[i].Next = players[i + 1];

            players[3].Next = players[0];
            players[0].Last = players[3];

            await Task.Yield();
            positionView(players);

            await BanPick.Instance.Execute();

            // 初始化武将
            // await InitGeneral();
            await Task.Yield();
            generalView();

            // 初始化牌堆
            await CardPile.Instance.Init();

#if UNITY_EDITOR
            if (Room.Instance.IsSingle) await DebugCard();
#endif

            foreach (var player in players) await new GetCardFromPile(player, 4).Execute();

            // 开始第一个回合
            await TurnSystem.Instance.StartGame();
        }


        // 模式
        // public Mode mode { get; private set; }
        // 玩家人数
        // public int PlayersCount { get; private set; }
        // 玩家
        public Player[] players;
        public List<Player> AlivePlayers { get; private set; } = new List<Player>();
        public bool GameIsOver { get; private set; } = false;
        public async Task GameOver()
        {
            GameIsOver = true;
            await Delay(2);
            gameOverView();
        }

        private async Task DebugCard()
        {
            List<string> list = new List<string>
            {
                "火杀","无中生有","诸葛连弩","顺手牵羊","铁索连环","寒冰剑","闪电"
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
        /// 当前最大体力值
        /// </summary>
        public int MaxHp(Player exp = null)
        {
            int maxHp = 0;
            foreach (var i in AlivePlayers)
            {
                if (i == exp) continue;
                if (i.Hp > maxHp) maxHp = i.Hp;
            }
            return maxHp;
        }

        /// <summary>
        /// 当前最少手牌数
        /// </summary>
        public int MinHand(Player exp = null)
        {
            int minHand = int.MaxValue;
            foreach (var i in AlivePlayers)
            {
                if (i == exp) continue;
                if (i.HandCardCount < minHand) minHand = i.HandCardCount;
            }
            return minHand;
        }

        /// <summary>
        /// 初始化武将
        /// </summary>
        private async Task InitGeneral()
        {

            string url = Urls.JSON_URL + "general.json";
            List<General> json = JsonList<General>.FromJson(await WebRequest.GetString(url));

            if (Room.Instance.IsSingle)
            {
#if UNITY_EDITOR
                // debug
                General self = null;
                string name = "刘备";
                foreach (var i in json)
                {
                    if (i.name == name)
                    {
                        self = i;
                        break;
                    }
                }
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
#else
                foreach (var i in players)
                {
                    var general = json[Random.Range(0, json.Count)];
                    json.Remove(general);
                    i.InitGeneral(general);
                }
#endif
            }
            else
                foreach (var player in players)
                {
                    var general = json[Room.Instance.Generals[player.Position]];
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

        private bool isDone;
        public async Task Delay(float second)
        {
            isDone = false;
            StartCoroutine(CorDelay(second));
            while (!isDone) await Task.Yield();
        }

        public IEnumerator CorDelay(float second)
        {
            yield return new WaitForSeconds(second);
            isDone = true;
        }
    }
}