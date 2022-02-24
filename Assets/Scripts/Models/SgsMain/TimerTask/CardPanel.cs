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

            if (!Room.Instance.isSingle) Connection.Instance.IsRunning = false;

            waitAction = new TaskCompletionSource<bool>();

            startTimerView?.Invoke(this);
            var result = await waitAction.Task;
            stopTimerView?.Invoke(this);

            Hint = "";

            return result;
        }

        public void SetResult(List<int> cards)
        {
            Debug.Log("SetResult(List<int> cards)");
            Cards = new List<Card>();
            foreach (var i in cards) Cards.Add(CardPile.Instance.cards[i]);
            waitAction.TrySetResult(true);
        }

        public void SetResult()
        {
            waitAction.TrySetResult(false);
        }

        public void SendSetResult(List<int> cards)
        {
            if (Room.Instance.isSingle) SetResult(cards);
            // 多人模式
            else
            {
                var json = new TimerJson();
                json.eventname = "card_panel_result";
                json.id = Connection.Instance.Count + 1;
                json.result = true;
                json.cards = cards;

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
                json.eventname = "card_panel_result";
                json.id = Connection.Instance.Count + 1;
                json.result = false;

                Connection.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            }
        }

        public void ReceiveSetResult(TimerJson json)
        {
            Debug.Log("ReceiveSetResult");
            if (json.result) SetResult(json.cards);
            else SetResult();
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
