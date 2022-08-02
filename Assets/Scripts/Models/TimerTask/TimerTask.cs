using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;

namespace Model
{
    public class TimerTask : SingletonMono<TimerTask>
    {
        private TaskCompletionSource<bool> waitAction;

        public Player player { get; private set; }
        public TimerType timerType { get; private set; }
        public int maxCard { get; set; } = 0;
        public int minCard { get; set; } = 0;
        public int maxDest { get; set; } = 0;
        public int minDest { get; set; } = 0;
        public Func<List<Card>, int> MaxDest { get; set; }
        public Func<List<Card>, int> MinDest { get; set; }
        public Func<Card, bool> ValidCard { get; set; } = (card) => !card.IsConvert;
        public Func<Player, Card, Player, bool> ValidDest { get; set; } = (player, card, fstPlayer) => true;
        public bool isPerformPhase { get; set; } = false;

        public string Hint { get; set; }
        public string GivenSkill { get; set; }

        public int second
        {
            get
            {
                switch (timerType)
                {
                    case TimerType.PerformPhase: return 15;
                    case TimerType.SelectHandCard: return 5 + maxCard;
                    case TimerType.无懈可击: return 5;
                    default: return 15;
                }
            }
        }

        public List<Card> Cards { get; private set; }
        public List<Player> Dests { get; private set; }
        public string Skill { get; private set; }

        /// <summary>
        /// 暂停主线程，并通过服务器或view开始计时
        /// </summary>
        /// <returns>是否有操作</returns>
        public async Task<bool> Run(Player player, TimerType timerType)
        {
            this.player = player;
            this.timerType = timerType;

            Cards = new List<Card>();
            Dests = new List<Player>();
            Skill = "";

            if (SgsMain.Instance.GameIsOver) return true;

            if (player.isSelf) moveSeat(player);
            startTimerView?.Invoke(this);

            if (Room.Instance.IsSingle) waitAction = new TaskCompletionSource<bool>();
            var result = Room.Instance.IsSingle ? await waitAction.Task : await WaitResult();

            stopTimerView(this);

            Reset();

            if (Skill == "丈八蛇矛" || Skill != "" && player.skills[Skill] is Converted)
            {
                var skill = Skill == "丈八蛇矛" ? (player.weapon as 丈八蛇矛).skill : player.skills[Skill] as Converted;
                Cards = new List<Card> { skill.Execute(Cards) };
                Skill = "";
            }

            return result;
        }

        public async Task<bool> Run(Player player, TimerType timerType, int cardCount, int destCount)
        {
            return await Run(player, timerType, cardCount, cardCount, destCount, destCount);
        }

        public async Task<bool> Run(Player player, TimerType timerType, int maxCard, int minCard, int maxDest, int minDest)
        {
            this.maxCard = maxCard;
            this.minCard = minCard;
            this.maxDest = maxDest;
            this.minDest = minDest;
            return await Run(player, timerType);
        }

        private void Reset()
        {
            Hint = "";
            maxCard = 0;
            minCard = 0;
            maxDest = 0;
            minDest = 0;
            MaxDest = null;
            MinDest = null;
            ValidCard = (card) => !card.IsConvert;
            ValidDest = (player, card, fstPlayer) => true;
            GivenSkill = "";
        }

        /// <summary>
        /// 传入已选中的卡牌与目标，通过设置TaskCompletionSource返回值，继续主线程
        /// </summary>
        public void SetResult(List<int> cards, List<int> dests, string skill)
        {
            foreach (var id in cards) Cards.Add(CardPile.Instance.cards[id]);
            foreach (var id in dests) Dests.Add(SgsMain.Instance.players[id]);
            Skill = skill;
        }

        public void SetResult()
        {
            if (Room.Instance.IsSingle) waitAction.TrySetResult(false);
        }

        public void SendResult(List<int> cards, List<int> dests, string skill, bool result = true)
        {
            if (Room.Instance.IsSingle)
            {
                if (result) SetResult(cards, dests, skill);
                waitAction.TrySetResult(result);
            }
            // 多人模式
            else
            {
                var json = new TimerJson();
                json.eventname = "set_result";
                json.id = Wss.Instance.Count + 1;
                json.result = result;
                json.cards = cards;
                json.dests = dests;
                json.skill = skill;

                Wss.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            }
        }

        public void SendResult()
        {
            SendResult(null, null, null, false);
        }

        public async Task<bool> WaitResult()
        {
            var message = await Wss.Instance.PopSgsMsg();
            var json = JsonUtility.FromJson<TimerJson>(message);

            if (timerType == TimerType.无懈可击)
            {
                if (json.result)
                {
                    player = SgsMain.Instance.players[json.src];
                    SetResult(json.cards, new List<int>(), "");
                }
                else
                {
                    wxkjDone[json.src] = true;
                    foreach (var i in wxkjDone.Values)
                    {
                        if (!i)
                        {
                            Wss.Instance.Count--;
                            return await WaitResult();
                        }
                    }

                    // if (!Room.Instance.isSingle) Connection.Instance.Count++;
                    // SetResult();
                    // return false;
                }
                return json.result;
            }

            if (json.result) SetResult(json.cards, json.dests, json.skill);
            return json.result;
        }

        private Dictionary<int, bool> wxkjDone;

        public async Task<bool> RunWxkj()
        {
            this.timerType = TimerType.无懈可击;
            this.maxCard = 1;
            this.minCard = 1;
            // GivenCard = new List<string> { "无懈可击" };

            Cards = new List<Card>();
            // Equipages = new List<Card>();
            Dests = new List<Player>();

            wxkjDone = new Dictionary<int, bool>();
            foreach (var i in SgsMain.Instance.players)
            {
                if (i.IsAlive) wxkjDone.Add(i.Position, false);
            }

            waitAction = new TaskCompletionSource<bool>();

            startTimerView?.Invoke(this);
            // if (!Room.Instance.isSingle) Connection.Instance.IsRunning = false;

            bool result = Room.Instance.IsSingle ? await waitAction.Task : await WaitResult();

            stopTimerView?.Invoke(this);

            Hint = "";
            // GivenCard = null;

            return result;
        }

        public void SetWxkjResult(int src, bool result, List<int> cards, string skill)
        {
            if (cards is null) cards = new List<int>();
            if (result)
            {
                player = SgsMain.Instance.players[src];
                SetResult(cards, new List<int>(), "");
                waitAction.TrySetResult(true);
            }
            else
            {
                wxkjDone[src] = true;
                foreach (var i in wxkjDone.Values)
                {
                    if (!i) return;
                }

                if (Room.Instance.IsSingle) waitAction.TrySetResult(false);
            }
        }

        public void SendWxkjResult(int src, bool result, List<int> cards = null, string skill = "")
        {
            if (Room.Instance.IsSingle) SetWxkjResult(src, result, cards, skill);
            // 多人模式
            else
            {
                var json = new TimerJson();
                json.eventname = "set_result";
                json.id = Wss.Instance.Count + 1;
                json.result = result;
                json.cards = cards;
                json.src = src;

                Wss.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            }
        }


        private UnityAction<TimerTask> startTimerView;
        private UnityAction<TimerTask> stopTimerView;
        private UnityAction<Player> moveSeat;

        /// <summary>
        /// 开始计时触发事件
        /// </summary>
        public event UnityAction<TimerTask> StartTimerView
        {
            add => startTimerView += value;
            remove => startTimerView -= value;
        }
        /// <summary>
        /// 结束计时触发事件
        /// </summary>
        public event UnityAction<TimerTask> StopTimerView
        {
            add => stopTimerView += value;
            remove => stopTimerView -= value;
        }
        public event UnityAction<Player> MoveSeat
        {
            add => moveSeat += value;
            remove => moveSeat -= value;
        }
    }
}