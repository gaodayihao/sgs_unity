using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    public class CardPanel : Singleton<CardPanel>
    {
        private TaskCompletionSource<bool> waitAction;

        public Player player { get; private set; }
        public Player dest { get; private set; }
        public TimerType timerType { get; private set; }

        public string Title { get; set; }
        public string Hint { get; set; }
        public int second { get; private set; } = 10;

        public List<Card> Cards { get; private set; }

        public async Task<bool> Run(Player player, Player dest, TimerType timerType)
        {
            this.player = player;
            this.dest = dest;
            this.timerType = timerType;

            // if (!Room.Instance.isSingle) Connection.Instance.IsRunning = false;

            waitAction = new TaskCompletionSource<bool>();

            startTimerView?.Invoke(this);
            var result = Room.Instance.isSingle ? await waitAction.Task : await ReceiveResult();
            stopTimerView?.Invoke(this);

            Hint = "";

            return result;
        }

        public void SetResult(List<int> cards)
        {
            Cards = new List<Card>();
            foreach (var i in cards) Cards.Add(CardPile.Instance.cards[i]);
        }

        public void SendResult(List<int> cards, bool result)
        {
            if (Room.Instance.isSingle)
            {
                if (result) SetResult(cards);
                waitAction.TrySetResult(result);
            }
            // 多人模式
            else
            {
                var json = new TimerJson();
                json.eventname = "card_panel_result";
                json.id = Connection.Instance.Count + 1;
                json.result = result;
                json.cards = cards;

                Connection.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            }
        }

        public void SendResult()
        {
            SendResult(null, false);
            // if (Room.Instance.isSingle) SetResult();
            // // 多人模式
            // else
            // {
            //     var json = new TimerJson();
            //     json.eventname = "card_panel_result";
            //     json.id = Connection.Instance.Count + 1;
            //     json.result = false;

            //     Connection.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            // }
        }

        public async Task<bool> ReceiveResult()
        {
            Debug.Log("ReceiveSetResult");
            var msg = await Connection.Instance.PopSgsMsg();
            var json = JsonUtility.FromJson<TimerJson>(msg);
            if (json.result) SetResult(json.cards);
            else SendResult();
            return json.result;
        }

        private UnityAction<CardPanel> startTimerView;
        private UnityAction<CardPanel> stopTimerView;

        /// <summary>
        /// 开始计时触发事件
        /// </summary>
        public event UnityAction<CardPanel> StartTimerView
        {
            add => startTimerView += value;
            remove => startTimerView -= value;
        }
        /// <summary>
        /// 结束计时触发事件
        /// </summary>
        public event UnityAction<CardPanel> StopTimerView
        {
            add => stopTimerView += value;
            remove => stopTimerView -= value;
        }
    }
}
