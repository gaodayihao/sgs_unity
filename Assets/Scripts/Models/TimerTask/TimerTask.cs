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
        private TaskCompletionSource<bool> waitAction;

        public Player player { get; private set; }
        public int maxCard { get; set; } = 0;
        public int minCard { get; set; } = 0;
        public int maxDest { get; set; } = 0;
        public int minDest { get; set; } = 0;
        public Func<List<Card>, int> MaxDest { get; set; }
        public Func<List<Card>, int> MinDest { get; set; }
        public Func<Card, bool> ValidCard { get; set; } = (card) => !card.IsConvert;
        public Func<Player, Card, Player, bool> ValidDest { get; set; } = (player, card, fstPlayer) => true;
        public bool isPerformPhase { get; set; } = false;
        public bool isWxkj { get; private set; } = false;
        public bool Refusable { get; set; } = true;

        public string Hint { get; set; }
        public string GivenSkill { get; set; }

        public int second;

        public List<Card> Cards { get; private set; }
        public List<Player> Dests { get; private set; }
        public string Skill { get; private set; }

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

            if (SgsMain.Instance.GameIsOver) return true;

            if (player.isSelf) moveSeat(player);
            startTimerView?.Invoke(this);

            if (Room.Instance.IsSingle) waitAction = new TaskCompletionSource<bool>();
           if(player.isSelf) StartCoroutine(SelfAutoResult());
            StartCoroutine(AIAutoResult());
            var result = Room.Instance.IsSingle ? await waitAction.Task : await WaitResult();

            stopTimerView(this);

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
            Refusable = true;

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
        public void SetResult(List<int> cards, List<int> dests, string skill)
        {
            foreach (var id in cards) Cards.Add(CardPile.Instance.cards[id]);
            foreach (var id in dests) Dests.Add(SgsMain.Instance.players[id]);
            Skill = skill;
        }

        public void SendResult(List<int> cards, List<int> dests, string skill, bool result = true)
        {
            StopAllCoroutines();
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

            if (isWxkj)
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
                }
                StopAllCoroutines();
                return json.result;
            }

            if (json.result) SetResult(json.cards, json.dests, json.skill);
            return json.result;
        }

        private Dictionary<int, bool> wxkjDone;

        public async Task<bool> RunWxkj()
        {
            maxCard = 1;
            minCard = 1;
            isWxkj = true;
            ValidCard = (card) => card is 无懈可击;

            Cards = new List<Card>();
            Dests = new List<Player>();

            wxkjDone = new Dictionary<int, bool>();
            foreach (var i in SgsMain.Instance.players)
            {
                if (i.IsAlive) wxkjDone.Add(i.Position, false);
            }

            waitAction = new TaskCompletionSource<bool>();
            startTimerView?.Invoke(this);
            StartCoroutine(WxkjAutoResult());
            StartCoroutine(AIAutoResult());
            bool result = Room.Instance.IsSingle ? await waitAction.Task : await WaitResult();

            stopTimerView?.Invoke(this);

            maxCard = 0;
            minCard = 0;
            isWxkj = false;
            ValidCard = (card) => !card.IsConvert;

            return result;
        }

        public void SetWxkjResult(int src, bool result, List<int> cards, string skill)
        {
            if (cards is null) cards = new List<int>();
            if (result)
            {
                player = SgsMain.Instance.players[src];
                SetResult(cards, new List<int>(), "");
                StopAllCoroutines();
                waitAction.TrySetResult(true);
            }
            else
            {
                wxkjDone[src] = true;
                foreach (var i in wxkjDone.Values)
                {
                    if (!i) return;
                }

                if (Room.Instance.IsSingle)
                {
                    StopAllCoroutines();
                    waitAction.TrySetResult(false);
                }
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

        private IEnumerator AIAutoResult()
        {
            yield return new WaitForSeconds(1);
            if (!isWxkj)
            {
                if (player.isAI) SendResult();
            }
            else
            {
                foreach (var i in wxkjDone.Keys.ToList())
                {
                    if (SgsMain.Instance.players[i].isAI) SetWxkjResult(i, false, null, "");
                }
            }
        }

        private IEnumerator SelfAutoResult()
        {
            yield return new WaitForSeconds(second);
            SendResult();

        }

        private IEnumerator WxkjAutoResult()
        {
            yield return new WaitForSeconds(second);

            foreach (var i in wxkjDone)
            {
                if (!i.Value && SgsMain.Instance.players[i.Key].isSelf) SendWxkjResult(i.Key, false);
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