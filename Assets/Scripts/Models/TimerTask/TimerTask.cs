using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace Model
{
    public class TimerTask : SingletonMono<TimerTask>
    {
        private TaskCompletionSource<TimerJson> waitAction;

        public Player player { get; private set; }
        public int maxCard { get; set; } = 0;
        public int minCard { get; set; } = 0;
        public int maxDest { get; set; } = 0;
        public int minDest { get; set; } = 0;
        public Func<List<Card>, int> MaxDest { get; set; }
        public Func<List<Card>, int> MinDest { get; set; }
        public Func<Card, bool> ValidCard { get; set; } = card => !card.IsConvert;
        public Func<Player, Card, Player, bool> ValidDest { get; set; } = (dest, card, first) => true;
        public bool isPerformPhase { get; set; } = false;
        public bool isWxkj { get; private set; } = false;
        public bool isCompete { get; private set; } = false;
        public bool Refusable { get; set; } = true;
        public List<Card> MultiConvert { get; set; } = new List<Card>();

        public string Hint { get; set; }
        public string GivenSkill { get; set; }

        public int second;

        public List<Card> Cards { get; private set; }
        public List<Player> Dests { get; private set; }
        public string Skill { get; private set; }
        public string Other { get; private set; }

        /// <summary>
        /// 暂停主线程，并通过服务器或view开始计时
        /// </summary>
        /// <returns>是否有操作</returns>
        public async Task<bool> Run(Player player)
        {
            this.player = player;
            second = minCard > 1 ? 10 + minCard : 15;

            Cards = new List<Card>();
            Dests = new List<Player>();
            Skill = "";

            if (SgsMain.Instance.GameIsOver) return false;

            if (player.isSelf)
            {
                moveSeat(player);
                StartCoroutine(SelfAutoResult());
            }
            else if (Room.Instance.IsSingle) StartCoroutine(AIAutoResult());
            startTimerView?.Invoke(this);
            bool result = await WaitResult();

            stopTimerView(this);

            Hint = "";
            maxCard = 0;
            minCard = 0;
            maxDest = 0;
            minDest = 0;
            MaxDest = null;
            MinDest = null;
            ValidCard = card => !card.IsConvert;
            ValidDest = (dest, card, first) => true;
            GivenSkill = "";
            Refusable = true;
            MultiConvert.Clear();

            if (Skill == "丈八蛇矛" || Skill != "" && player.skills[Skill] is Converted)
            {
                var skill = Skill == "丈八蛇矛" ? (player.weapon as 丈八蛇矛).skill : player.skills[Skill] as Converted;
                skill.Execute();
                Cards = new List<Card> { skill.Execute(Cards) };
                Skill = "";
            }

            return result;
        }

        public async Task<bool> Run(Player player, int cardCount, int destCount)
        {
            return await Run(player, cardCount, cardCount, destCount, destCount);
        }

        public async Task<bool> Run(Player player, int maxCard, int minCard, int maxDest, int minDest)
        {
            this.maxCard = maxCard;
            this.minCard = minCard;
            this.maxDest = maxDest;
            this.minDest = minDest;
            return await Run(player);
        }

        /// <summary>
        /// 传入已选中的卡牌与目标，通过设置TaskCompletionSource返回值，继续主线程
        /// </summary>
        public void SetResult(List<int> cards, List<int> dests, string skill, string other)
        {
            foreach (var id in cards) Cards.Add(CardPile.Instance.cards[id]);
            foreach (var id in dests) Dests.Add(SgsMain.Instance.players[id]);
            Skill = skill;
            Other = other;
        }

        public void SendResult(List<int> cards, List<int> dests, string skill, string other, bool result = true)
        {
            StopAllCoroutines();
            var json = new TimerJson();
            json.eventname = "set_result";
            json.id = Wss.Instance.Count + 1;
            json.result = result;
            json.cards = cards;
            json.dests = dests;
            json.skill = skill;
            json.other = other;

            if (Room.Instance.IsSingle) waitAction?.TrySetResult(json);
            else Wss.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
        }

        public void SendResult()
        {
            SendResult(null, null, null, null, false);
        }

        public async Task<bool> WaitResult()
        {
            TimerJson json;
            if (Room.Instance.IsSingle)
            {
                waitAction = new TaskCompletionSource<TimerJson>();
                json = await waitAction.Task;
            }
            else
            {
                var message = await Wss.Instance.PopSgsMsg();
                json = JsonUtility.FromJson<TimerJson>(message);
            }

            if (SgsMain.Instance.GameIsOver) return false;

            if (isWxkj)
            {
                if (json.result)
                {
                    player = SgsMain.Instance.players[json.src];
                    SetResult(json.cards, new List<int>(), "", "");
                }
                else
                {
                    wxkjDone++;
                    if (wxkjDone < SgsMain.Instance.AlivePlayers.Count)
                    {
                        Wss.Instance.Count--;
                        return await WaitResult();
                    }
                }
            }
            else if (isCompete)
            {
                if (json.cards is null) json.cards = new List<int>();
                if (json.src == player0.Position) card0 = json.cards.Count > 0 ? json.cards[0] : player0.HandCards[0].Id;
                else card1 = json.cards.Count > 0 ? json.cards[0] : player1.HandCards[0].Id;
                if (card0 == 0 || card1 == 0)
                {
                    Wss.Instance.Count--;
                    await WaitResult();
                }
            }
            else if (json.result) SetResult(json.cards, json.dests, json.skill, json.other);

            StopAllCoroutines();
            return json.result;
        }


        private int wxkjDone;
        public async Task<bool> RunWxkj()
        {
            maxCard = 1;
            minCard = 1;
            isWxkj = true;
            ValidCard = card => card is 无懈可击;

            Cards = new List<Card>();
            Dests = new List<Player>();

            wxkjDone = 0;

            startTimerView?.Invoke(this);
            StartCoroutine(WxkjAutoResult());
            if (Room.Instance.IsSingle) StartCoroutine(AIAutoResult());
            bool result = await WaitResult();

            stopTimerView?.Invoke(this);

            maxCard = 0;
            minCard = 0;
            isWxkj = false;
            ValidCard = card => !card.IsConvert;

            return result;
        }

        public void SendResult(int src, bool result, List<int> cards = null, string skill = "")
        {
            var json = new TimerJson();
            json.eventname = "set_result";
            json.id = Wss.Instance.Count + 1;
            json.result = result;
            json.cards = cards;
            json.src = src;

            if (Room.Instance.IsSingle) waitAction.TrySetResult(json);
            else
            {
                StopAllCoroutines();
                Wss.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            }
        }

        public Player player0;
        public Player player1;
        public int card0;
        public int card1;
        public async Task Compete(Player player0, Player player1)
        {
            maxCard = 1;
            minCard = 1;
            isCompete = true;
            ValidCard = card => player0.HandCards.Contains(card) || player1.HandCards.Contains(card);
            this.player0 = player0;
            this.player1 = player1;
            card0 = 0;
            card1 = 0;

            Cards = new List<Card>(2);
            Dests = new List<Player>();

            if (player0.isSelf) moveSeat(player0);
            else if (player1.isSelf) moveSeat(player1);

            startTimerView?.Invoke(this);
            StartCoroutine(CompeteAutoResult());
            if (Room.Instance.IsSingle) StartCoroutine(AIAutoResult());
            await WaitResult();

            stopTimerView?.Invoke(this);

            maxCard = 0;
            minCard = 0;
            isCompete = false;
            ValidCard = card => !card.IsConvert;
        }

        private IEnumerator AIAutoResult()
        {
            yield return new WaitForSeconds(1);
            if (isWxkj)
            {
                foreach (var i in SgsMain.Instance.AlivePlayers)
                {
                    if (i.isAI) SendResult(i.Position, false, null, "");
                }
            }
            else if (isCompete)
            {
                if (player0.isAI) SendResult(player0.Position, false);
                if (player1.isAI) SendResult(player1.Position, false);
            }
            else SendResult();
        }

        private IEnumerator SelfAutoResult()
        {
            yield return new WaitForSeconds(second);
            // int s = second;
            // while (s > 0)
            // {
            //     Debug.Log(s--);
            //     yield return new WaitForSeconds(1);
            // }
            SendResult();
        }

        private IEnumerator WxkjAutoResult()
        {
            yield return new WaitForSeconds(second);

            foreach (var i in SgsMain.Instance.AlivePlayers)
            {
                if (i.isSelf) SendResult(i.Position, false);
            }
        }

        private IEnumerator CompeteAutoResult()
        {
            yield return new WaitForSeconds(second);

            if (card0 == 0) SendResult(player0.Position, false);
            if (card1 == 0) SendResult(player1.Position, false);
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