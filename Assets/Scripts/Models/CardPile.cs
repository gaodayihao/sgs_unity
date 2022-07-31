using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace Model
{
    public class CardPile : SingletonMono<CardPile>
    {
        public async Task Init()
        {
            string url = Urls.JSON_URL + "card.json";
            List<CardJson> cardJsons = JsonList<CardJson>.FromJson(await WebRequest.GetString(url));

            cards = new List<Card>();
            remainPile = new List<Card>();
            discardPile = new List<Card>();
            foreach (var cardJson in cardJsons)
            {
                Card card;
                switch (cardJson.name)
                {
                    case "杀": card = new 杀(); break;
                    case "闪": card = new 闪(); break;
                    case "桃": card = new 桃(); break;

                    case "绝影": card = new PlusHorse(); break;
                    case "大宛": card = new SubHorse(); break;
                    case "赤兔": card = new SubHorse(); break;
                    case "爪黄飞电": card = new PlusHorse(); break;
                    case "的卢": card = new PlusHorse(); break;
                    case "紫骍": card = new SubHorse(); break;

                    case "青龙偃月刀": card = new 青龙偃月刀(); break;
                    case "麒麟弓": card = new 麒麟弓(); break;
                    case "雌雄双股剑": card = new 雌雄双股剑(); break;
                    case "青釭剑": card = new 青缸剑(); break;
                    case "丈八蛇矛": card = new 丈八蛇矛(); break;
                    case "诸葛连弩": card = new 诸葛连弩(); break;
                    case "贯石斧": card = new 贯石斧(); break;
                    case "方天画戟": card = new 方天画戟(); break;

                    case "八卦阵": card = new 八卦阵(); break;

                    case "乐不思蜀": card = new 乐不思蜀(); break;

                    case "过河拆桥": card = new 过河拆桥(); break;
                    case "顺手牵羊": card = new 顺手牵羊(); break;
                    case "无懈可击": card = new 无懈可击(); break;
                    case "南蛮入侵": card = new 南蛮入侵(); break;
                    case "万箭齐发": card = new 万箭齐发(); break;
                    case "桃园结义": card = new 桃园结义(); break;
                    case "无中生有": card = new 无中生有(); break;
                    case "决斗": card = new 决斗(); break;

                    default: card = new 桃(); break;
                }
                card.Id = cardJson.id;
                card.Suit = cardJson.suit;
                card.Weight = cardJson.weight;
                card.Type = cardJson.type;
                card.Name = cardJson.name;

                cards.Add(card);
                remainPile.Add(card);
            }
            // View.Sprites.Instance.InitCard(cards);

            remainPile.RemoveAt(0);
            await Shuffle();
        }

        public List<Card> cards;

        // 牌堆
        private List<Card> remainPile;
        // 弃牌堆
        public List<Card> discardPile;

        // 牌堆数
        public int PileCount { get => remainPile.Count; }


        /// <summary>
        /// 弹出并返回牌堆顶的牌
        /// </summary>
        public async Task<Card> Pop()
        {
            Card T = remainPile[0];
            remainPile.RemoveAt(0);

            if (remainPile.Count == 0) await Refresh();
            pileCountView?.Invoke(this);

            return T;
        }

        /// <summary>
        /// 将一张牌添加到弃牌堆
        /// </summary>
        public void AddToDiscard(List<Card> cards)
        {
            discardView?.Invoke(cards);
            foreach (var card in cards) discardPile.Add(card);
        }

        public void AddToDiscard(Card card)
        {
            AddToDiscard(new List<Card> { card });
        }

        public void RemoveToDiscard(Card card)
        {
            discardPile.Remove(card);
        }

        /// <summary>
        /// 洗牌
        /// </summary>
        private async Task Shuffle()
        {
            if (Room.Instance.IsSingle)
            {
                // 随机取一个元素与第i个元素交换
                for (int i = 0; i < remainPile.Count; i++)
                {
                    int t = Random.Range(i, remainPile.Count);
                    var card = remainPile[i];
                    remainPile[i] = remainPile[t];
                    remainPile[t] = card;
                }
            }
            else
            {
                // 发送洗牌请求
                var json = new CardPileJson();
                json.eventname = "shuffle";
                json.id = Wss.Instance.Count + 1;
                json.cards = new List<int>();
                foreach (var i in remainPile) json.cards.Add(i.Id);
                Wss.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));

                // 等待消息
                // tcs = new TaskCompletionSource<CardPileJson>();
                // Connection.Instance.IsRunning = false;
                // var message = await tcs.Task;
                var msg = await Wss.Instance.PopSgsMsg();
                var newJson = JsonUtility.FromJson<CardPileJson>(msg);

                // 更新牌堆
                remainPile.Clear();
                foreach (int i in newJson.cards) remainPile.Add(cards[i]);
            }
        }

        private TaskCompletionSource<CardPileJson> tcs;
        public void ReceiveShuffle(CardPileJson json)
        {
            tcs.TrySetResult(json);
        }

        // public void SendShuffle()

        /// <summary>
        /// 刷新牌堆
        /// </summary>
        public async Task Refresh()
        {
            // 将弃牌堆的牌移到牌堆
            while (discardPile.Count > 0)
            {
                remainPile.Add(discardPile[0]);
                discardPile.RemoveAt(0);
            }
            // 洗牌
            await Shuffle();
        }

        private UnityAction<List<Card>> discardView;
        public event UnityAction<List<Card>> DiscardView
        {
            add => discardView += value;
            remove => discardView -= value;
        }

        private UnityAction<CardPile> pileCountView;
        public event UnityAction<CardPile> PileCountView
        {
            add => pileCountView += value;
            remove => pileCountView -= value;
        }
    }
}