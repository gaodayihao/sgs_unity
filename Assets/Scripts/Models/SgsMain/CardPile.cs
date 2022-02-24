using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;

namespace Model
{
    public class CardPile : Singleton<CardPile>
    {
        public async Task Init()
        {
            string url;
#if UNITY_EDITOR
            url = "file:///" + Application.dataPath + "/../Json/card.json";
#else
            url = Urls.STATIC_URL + "json/card.json";
#endif
            UnityWebRequest www = UnityWebRequest.Get(url);
            www.SendWebRequest();

            while (!www.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                return;
            }

            List<CardJson> cardJsons = JsonList<CardJson>.FromJson(www.downloadHandler.text);

            cards = new List<Card>();
            remainPile = new List<Card>();
            discardPile = new List<Card>();
            foreach (var cardJson in cardJsons)
            {
                Card card;
                switch (cardJson.name)
                {
                    case "杀": card = new Sha(); break;
                    case "闪": card = new Shan(); break;
                    case "桃": card = new Tao(); break;

                    case "绝影": card = new PlusHorse(); break;
                    case "大宛": card = new SubHorse(); break;
                    case "赤兔": card = new SubHorse(); break;
                    case "爪黄飞电": card = new PlusHorse(); break;
                    case "的卢": card = new PlusHorse(); break;
                    case "紫骍": card = new SubHorse(); break;

                    case "青龙偃月刀": card = new QingLongYanYueDao(); break;
                    case "麒麟弓": card = new QiLinGong(); break;
                    case "雌雄双股剑": card = new CiXiongShuangGuJian(); break;
                    case "青釭剑": card = new QingGangJian(); break;
                    case "丈八蛇矛": card = new ZhangBaSheMao(); break;
                    case "诸葛连弩": card = new ZhuGeLianNu(); break;
                    case "贯石斧": card = new GuanShiFu(); break;
                    case "方天画戟": card = new FangTianHuaJi(); break;

                    case "八卦阵": card = new BaGuaZhen(); break;

                    case "乐不思蜀": card = new LeBuSiShu(); break;

                    case "过河拆桥": card = new GuoHeChaiQiao(); break;
                    case "无懈可击": card = new WuXieKeJi(); break;

                    default: card = new Tao(); break;
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
        private List<Card> discardPile;

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
            if (Room.Instance.isSingle)
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
                json.id = Connection.Instance.Count + 1;
                json.cards = new List<int>();
                foreach (var i in remainPile) json.cards.Add(i.Id);
                Connection.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));

                // 等待消息
                tcs = new TaskCompletionSource<CardPileJson>();
                Connection.Instance.IsRunning = false;
                var message = await tcs.Task;

                // 更新牌堆
                remainPile.Clear();
                foreach (int i in message.cards) remainPile.Add(cards[i]);
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