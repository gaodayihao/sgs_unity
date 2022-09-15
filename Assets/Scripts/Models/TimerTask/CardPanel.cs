using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;

namespace Model
{
    public class CardPanel : SingletonMono<CardPanel>
    {
        private TaskCompletionSource<TimerJson> waitAction;

        public Player player { get; private set; }
        public Player dest { get; private set; }
        public TimerType timerType { get; private set; }
        public bool judgeArea { get; private set; }
        public bool display { get; set; } = false;

        public string Title { get; set; }
        public string Hint { get; set; }
        public int second { get; private set; } = 10;

        public List<Card> Cards { get; private set; }

        public async Task<bool> Run(Player player, Player dest, TimerType timerType, bool judgeArea = false)
        {
            this.player = player;
            this.dest = dest;
            this.timerType = timerType;
            this.judgeArea = judgeArea;

            // if (!Room.Instance.isSingle) Connection.Instance.IsRunning = false;

            startTimerView?.Invoke(this);
            if (player.isSelf) StartCoroutine(SelfAutoResult());
            StartCoroutine(AIAutoResult());
            var result = await WaitAction();
            stopTimerView?.Invoke(this);

            Hint = "";
            Title = "";
            display = false;

            return result;
        }

        public void SetResult(List<int> cards)
        {
            Cards = new List<Card>();
            foreach (var i in cards) Cards.Add(CardPile.Instance.cards[i]);
        }

        public void SendResult(List<int> cards, bool result)
        {
            StopAllCoroutines();

            var json = new TimerJson();
            json.eventname = "card_panel_result";
            json.id = Wss.Instance.Count + 1;
            json.result = result;
            json.cards = cards;

            if (Room.Instance.IsSingle) waitAction.TrySetResult(json);
            else Wss.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
        }

        public void SendResult()
        {
            SendResult(null, false);
        }

        public async Task<bool> WaitAction()
        {
            TimerJson json;
            if (Room.Instance.IsSingle)
            {
                waitAction = new TaskCompletionSource<TimerJson>();
                json = await waitAction.Task;
            }
            else
            {
                var msg = await Wss.Instance.PopSgsMsg();
                json = JsonUtility.FromJson<TimerJson>(msg);
            }

            if (SgsMain.Instance.GameIsOver) return false;

            if (json.result) SetResult(json.cards);
            // else SendResult();
            return json.result;
        }

        private IEnumerator AIAutoResult()
        {
            yield return new WaitForSeconds(1);
            if (player.isAI) SendResult();
        }

        private IEnumerator SelfAutoResult()
        {
            yield return new WaitForSeconds(second);
            SendResult();
        }

        public async Task<Card> SelectCard(Player player, Player dest, bool judgeArea = false)
        {
            if (player.Teammate == dest) display = true;
            bool result = await Run(player, dest, TimerType.区域内, judgeArea);
            Card card;
            if (!result)
            {
                if (dest.armor != null) card = dest.armor;
                else if (dest.plusHorse != null) card = dest.plusHorse;
                else if (dest.weapon != null) card = dest.weapon;
                else if (dest.subHorse != null) card = dest.subHorse;
                else if (dest.HandCardCount != 0) card = dest.HandCards[0];
                else card = dest.JudgeArea[0];
            }
            else card = CardPanel.Instance.Cards[0];
            return card;
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
