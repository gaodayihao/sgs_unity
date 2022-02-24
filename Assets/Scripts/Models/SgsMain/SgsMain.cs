using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Model
{
    public class SgsMain : Singleton<SgsMain>
    {
        public async void StartGame(Mode mode)
        {
            // Connection.Instance.Start();
            this.mode = mode;
            // 初始化玩家
            PlayersCount = mode.playerCount;
            players = new Player[PlayersCount];
            Debug.Log("players init");

            if (Room.Instance.isSingle)
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
            }
            else
            {
                players = Room.Instance.players.ToArray();
                foreach (var i in players)
                {
                    if (i.Username == User.Instance.Username) i.isSelf = true;
                    i.isAI = i.Username == "AI";
                }
            }

            for (int i = 0; i < PlayersCount; i++) players[i].Position = i;
            for (int i = 1; i < PlayersCount; i++) players[i].Last = players[i - 1];
            for (int i = 0; i < PlayersCount - 1; i++) players[i].Next = players[i + 1];

            players[PlayersCount - 1].Next = players[0];
            players[0].Last = players[PlayersCount - 1];

            positionView(players);

            // 初始化武将
            if (heroView is null) await Task.Yield();
            foreach (var player in players) heroView(player);

            // 初始化牌堆
            await CardPile.Instance.Init();

            await DebugCard();

            foreach (var player in players) await new GetCardFromPile(player, 4).Execute();

            // debug
            // await new Damaged(players[0], null).Execute();
            // await new Damaged(players[1], null).Execute();
            // await new Damaged(players[0], null, 2).Execute();

            // 开始第一个回合
            await TurnSystem.Instance.StartGame();
        }


        // 模式
        public Mode mode { get; private set; }
        // 玩家人数
        public int PlayersCount { get; private set; }
        // 玩家
        public Player[] players;
        public List<Player> AlivePlayers { get; private set; } = new List<Player>();

        private UnityAction<Player[]> positionView;
        public event UnityAction<Player[]> PositionView
        {
            add => positionView += value;
            remove => positionView -= value;
        }

        private UnityAction<Player> heroView;
        public event UnityAction<Player> HeroView
        {
            add => heroView += value;
            remove => heroView -= value;
        }

        private async Task DebugCard()
        {
            List<string> list = new List<string>
            {
                "决斗","桃园结义","无中生有","顺手牵羊","万箭齐发"
            };

            while (list.Count > 0)
            {
                await new GetCardFromPile(players[0], 1).Execute();
                var newCard = players[0].HandCards[players[0].HandCardCount - 1];
                if (!list.Contains(newCard.Name)) await new Discard(players[0], new List<Card> { newCard }).Execute();
                else list.Remove(newCard.Name);
            }
        }
    }
}