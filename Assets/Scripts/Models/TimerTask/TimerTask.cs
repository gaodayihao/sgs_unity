using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    public class TimerTask : SingletonMono<TimerTask>
    {
        private TaskCompletionSource<bool> waitAction;

        public Player player { get; private set; }
        public TimerType timerType { get; private set; }
        public int maxCount { get; private set; }
        public int minCount { get; private set; }

        public string Hint { get; set; }
        public Player GivenDest { get; set; }
        public List<string> GivenCard { get; set; }
        public string GivenSkill { get; set; }
        public string Extra { get; set; }

        public int second
        {
            get
            {
                switch (timerType)
                {
                    case TimerType.PerformPhase: return 15;
                    case TimerType.SelectHandCard: return 5 + maxCount;
                    case TimerType.无懈可击: return 5;
                    default: return 10;
                }
            }
        }

        // public List<Card> Cards { get; private set; } = new List<Card>();
        // public List<Card> Equipages { get; private set; } = new List<Card>();
        // public List<Player> Dests { get; private set; } = new List<Player>();
        public List<Card> Cards { get; private set; }
        // public List<Card> Equipages { get; private set; }
        public List<Player> Dests { get; private set; }
        public string Skill { get; private set; }

        /// <summary>
        /// 暂停主线程，并通过服务器或view开始计时
        /// </summary>
        /// <returns>是否有操作</returns>
        public async Task<bool> Run(Player player, TimerType timerType, int maxCount, int minCount)
        {
            this.player = player;
            this.timerType = timerType;
            this.maxCount = maxCount;
            this.minCount = minCount;

            // Cards.Clear();
            // Equipages.Clear();
            // Dests.Clear();
            Cards = new List<Card>();
            // Equipages = new List<Card>();
            Dests = new List<Player>();
            Skill = "";

            if (SgsMain.Instance.GameIsOver) return true;

            waitAction = new TaskCompletionSource<bool>();

            if (player.isSelf) moveSeat(player);
            startTimerView?.Invoke(this);
            // if (!Room.Instance.isSingle) Connection.Instance.IsRunning = false;

            // var result = await waitAction.Task;
            var result = Room.Instance.IsSingle ? await waitAction.Task : await WaitResult();

            stopTimerView(this);

            Hint = "";
            GivenDest = null;
            GivenCard = null;
            GivenSkill = "";
            Extra = "";

            // 转化技
            //    if(Skill!="") Debug.Log("timerTask.Skill="+Skill);
            if (Skill != "")
            {
                if (player.skills[Skill] is Converted)
                {
                    player.skills[Skill].Execute();
                    Cards = new List<Card> { (player.skills[Skill] as Converted).Execute(Cards) };
                }
            }
            // 转化装备
            else if ((timerType == TimerType.UseCard || timerType == TimerType.PerformPhase)
                && Cards.Count == 3)
            {
                foreach (var i in Cards)
                {
                    if (i is 丈八蛇矛)
                    {
                        Cards.Remove(i);
                        Cards = new List<Card> { (i as 丈八蛇矛).Execute(Cards) };
                        break;
                    }
                }
            }
            // else if (Equipages.Count != 0 && Equipages[0] is 丈八蛇矛
            //     && (timerType == TimerType.UseCard || timerType == TimerType.PerformPhase))
            // {
            //     Cards = new List<Card> { ((丈八蛇矛)Equipages[0]).Execute(Cards) };
            // }

            return result;
        }

        public async Task<bool> Run(Player player, TimerType timerType, int count = 1)
        {
            return await Run(player, timerType, count, count);
        }

        /// <summary>
        /// 传入已选中的卡牌与目标，通过设置TaskCompletionSource返回值，继续主线程
        /// </summary>
        public void SetResult(List<int> cards, List<int> dests, string skill)
        {
            foreach (var id in cards) Cards.Add(CardPile.Instance.cards[id]);

            foreach (var id in dests) Dests.Add(SgsMain.Instance.players[id]);

            // foreach (var id in equipages) Equipages.Add(CardPile.Instance.cards[id]);

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
            // if (Room.Instance.isSingle) SetResult();
            // // 多人模式
            // else
            // {
            //     var json = new TimerJson();
            //     json.eventname = "set_result";
            //     json.id = Connection.Instance.Count + 1;
            //     json.result = false;

            //     Connection.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            // }
        }

        public async Task<bool> WaitResult()
        {
            // while (Connection.Instance.IsRunning) await Task.Yield();

            var message = await Wss.Instance.PopSgsMsg();
            var json = JsonUtility.FromJson<TimerJson>(message);

            if (timerType == TimerType.无懈可击)
            {
                if (json.result)
                {
                    player = SgsMain.Instance.players[json.src];
                    SetResult(json.cards, new List<int>(), "");
                    // return true;
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
            this.maxCount = 1;
            this.minCount = 1;
            GivenCard = new List<string> { "无懈可击" };

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
            GivenCard = null;

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

                // if (!Room.Instance.isSingle) Connection.Instance.Count++;
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