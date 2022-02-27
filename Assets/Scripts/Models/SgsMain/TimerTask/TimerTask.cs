using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    public class TimerTask : Singleton<TimerTask>
    {
        private TaskCompletionSource<bool> waitAction;

        public Player player { get; private set; }
        public TimerType timerType { get; private set; }
        public int maxCount { get; private set; }
        public int minCount { get; private set; }

        public string Hint { get; set; }
        public Player GivenDest { get; set; }
        public List<string> GivenCard { get; set; }
        public string SkillName { get; set; }

        public int second
        {
            get
            {
                switch (timerType)
                {
                    case TimerType.PerformPhase: return 10;
                    case TimerType.DiscardFromHand: return 5 + maxCount;
                    case TimerType.UseWxkj: return 5;
                    default: return 10;
                }
            }
        }

        public List<Card> Cards { get; set; }
        public List<Card> Equipages { get; private set; }
        public List<Player> Dests { get; private set; }

        /// <summary>
        /// 暂停主线程，并通过服务器或view开始计时
        /// </summary>
        /// <returns>是否有操作</returns>
        public async Task<bool> Run(Player player, TimerType timerType, int maxCount, int minCount)
        {
            this.player = player;
            this.timerType = timerType;
            this.maxCount = maxCount;
            this.minCount = maxCount;

            Cards = new List<Card>();
            Equipages = new List<Card>();
            Dests = new List<Player>();

            waitAction = new TaskCompletionSource<bool>();

            startTimerView?.Invoke(this);
            if (!Room.Instance.isSingle) Connection.Instance.IsRunning = false;

            var result = await waitAction.Task;

            stopTimerView?.Invoke(this);

            Hint = "";
            GivenDest = null;
            GivenCard = null;
            SkillName = null;

            if (Equipages.Count != 0 && Equipages[0] is ZhangBaSheMao)
            {
                Cards = new List<Card> { ((ZhangBaSheMao)Equipages[0]).ConvertSkill(Cards) };
            }

            return result;
        }

        public async Task<bool> Run(Player player, TimerType timerType, int count = 1)
        {
            return await Run(player, timerType, count, count);
        }

        /// <summary>
        /// 传入已选中的卡牌与目标，通过设置TaskCompletionSource返回值，继续主线程
        /// </summary>
        public void SetResult(List<int> cards, List<int> dests, List<int> equipages)
        {
            foreach (var id in cards) Cards.Add(CardPile.Instance.cards[id]);

            foreach (var id in dests) Dests.Add(SgsMain.Instance.players[id]);

            foreach (var id in equipages) Equipages.Add(CardPile.Instance.cards[id]);

            waitAction.TrySetResult(true);
        }

        public void SetResult()
        {
            waitAction.TrySetResult(false);
        }

        public void SendSetResult(List<int> cards, List<int> dests, List<int> equipages)
        {
            if (Room.Instance.isSingle) SetResult(cards, dests, equipages);
            // 多人模式
            else
            {
                var json = new TimerJson();
                json.eventname = "set_result";
                json.id = Connection.Instance.Count + 1;
                json.result = true;
                json.cards = cards;
                json.dests = dests;
                json.equipages = equipages;

                Connection.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            }
        }

        public void SendSetResult()
        {
            if (Room.Instance.isSingle) SetResult();
            // 多人模式
            else
            {
                var json = new TimerJson();
                json.eventname = "set_result";
                json.id = Connection.Instance.Count + 1;
                json.result = false;

                Connection.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            }
        }

        public void ReceiveSetResult(TimerJson json)
        {
            // while (Connection.Instance.IsRunning) await Task.Yield();

            if (timerType != TimerType.UseWxkj)
            {
                if (!json.result) SetResult();
                else SetResult(json.cards, json.dests, json.equipages);
            }
            else SetWxkjResult(json.src, json.result, json.cards);
        }

        private Dictionary<int, bool> wxkjDone;

        public async Task<bool> RunWxkj()
        {
            this.timerType = TimerType.UseWxkj;
            this.maxCount = 1;
            this.minCount = 1;
            GivenCard = new List<string> { "无懈可击" };

            Cards = new List<Card>();
            Equipages = new List<Card>();
            Dests = new List<Player>();

            wxkjDone = new Dictionary<int, bool>();
            foreach (var i in SgsMain.Instance.players)
            {
                if (i.IsAlive) wxkjDone.Add(i.Position, false);
            }

            waitAction = new TaskCompletionSource<bool>();

            startTimerView?.Invoke(this);
            if (!Room.Instance.isSingle) Connection.Instance.IsRunning = false;

            var result = await waitAction.Task;

            stopTimerView?.Invoke(this);

            Hint = "";
            GivenCard = null;

            return result;
        }

        public void SetWxkjResult(int src, bool result, List<int> cards)
        {
            if (cards is null) cards = new List<int>();
            if (result)
            {
                player = SgsMain.Instance.players[src];
                SetResult(cards, new List<int>(), new List<int>());
            }
            else
            {
                wxkjDone[src] = true;
                foreach (var i in wxkjDone.Values)
                {
                    if (!i)
                    {
                        if (!Room.Instance.isSingle) Connection.Instance.IsRunning = false;
                        return;
                    }
                }

                if (!Room.Instance.isSingle) Connection.Instance.Count++;
                SetResult();
            }
        }

        public void SendSetWxkjResult(int src, bool result, List<int> cards = null)
        {
            if (Room.Instance.isSingle) SetWxkjResult(src, result, cards);
            // 多人模式
            else
            {
                var json = new TimerJson();
                json.eventname = "set_result";
                json.id = Connection.Instance.Count + 1;
                json.result = result;
                json.cards = cards;
                json.src = src;

                Connection.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            }
        }


        private UnityAction<TimerTask> startTimerView;
        private UnityAction<TimerTask> stopTimerView;

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
    }
}