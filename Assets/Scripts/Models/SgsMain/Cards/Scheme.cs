using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Model
{
    public class WuXieKeJi : Card
    {
        public WuXieKeJi()
        {
            Type = "锦囊牌";
            Name = "无懈可击";
        }

        public static async Task<bool> Call(Card card, Player dest)
        {
            string hint = dest != null ? "对" + (dest.Position + 1).ToString() + "号位" : "";
            TimerTask.Instance.Hint = card.Name + "即将" + hint + "生效，是否使用无懈可击？";

            bool result = await TimerTask.Instance.RunWxkj();
            if (result)
            {
                Debug.Log(TimerTask.Instance.Cards[0].Name);
                var wxkj = (WuXieKeJi)TimerTask.Instance.Cards[0];
                await wxkj.UseCard(TimerTask.Instance.player);
                if (!wxkj.isCountered) return true;
            }

            return false;

        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            await base.UseCard(src, dests);

            isCountered = await Call(this, null);
        }

        private bool isCountered;
    }

    public class GuoHeChaiQiao : Card
    {
        public GuoHeChaiQiao()
        {
            Type = "锦囊牌";
            Name = "过河拆桥";
        }

        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);
            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) continue;

                CardPanel.Instance.Title = "过河拆桥";
                bool result = await CardPanel.Instance.Run(Src, dest, TimerType.RegionPanel);

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

                if (card is DelayScheme && dest.JudgeArea.Contains((DelayScheme)card))
                {
                    ((DelayScheme)card).RemoveToJudgeArea();
                    CardPile.Instance.AddToDiscard(card);
                }
                else await new Discard(dest, new List<Card> { card }).Execute();
            }
        }
    }

    public class ShunShouQianYang : Card
    {
        public ShunShouQianYang()
        {
            Type = "锦囊牌";
            Name = "顺手牵羊";
        }

        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);
            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) continue;

                CardPanel.Instance.Title = "顺手牵羊";
                bool result = await CardPanel.Instance.Run(Src, dest, TimerType.RegionPanel);

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

                if (card is DelayScheme && dest.JudgeArea.Contains((DelayScheme)card))
                {
                    ((DelayScheme)card).RemoveToJudgeArea();
                    await new GetCard(src, new List<Card> { card }).Execute();
                }
                else await new GetCardFromElse(src, dest, new List<Card> { card }).Execute();
            }
        }
    }

    public class JueDou : Card
    {
        public JueDou()
        {
            Type = "锦囊牌";
            Name = "决斗";
        }

        public override async Task UseCard(Player src, List<Player> dests)
        {
            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) continue;

                var player = dest;
                bool done = false;
                while (!done)
                {
                    done = !await Sha.Call(player);
                    if (done) await new Damaged(player, 1, player == dest ? src : dest, this).Execute();
                    else player = player == dest ? src : dest;
                }
            }
        }
    }

    public class NanManRuQin : Card
    {
        public NanManRuQin()
        {
            Type = "锦囊牌";
            Name = "南蛮入侵";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            foreach (var i in SgsMain.Instance.AlivePlayers) dests.Add(i);
            dests.Remove(src);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) continue;

                if (!await Sha.Call(dest)) await new Damaged(dest, 1, src, this).Execute();
            }
        }
    }

    public class WanJianQiFa : Card
    {
        public WanJianQiFa()
        {
            Type = "锦囊牌";
            Name = "万箭齐发";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            foreach (var i in SgsMain.Instance.AlivePlayers) dests.Add(i);
            dests.Remove(src);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) continue;

                if (!await Shan.Call(dest)) await new Damaged(dest, 1, src, this).Execute();
            }
        }
    }

    public class TaoYuanJieYi : Card
    {
        public TaoYuanJieYi()
        {
            Type = "锦囊牌";
            Name = "桃园结义";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            dests = new List<Player>();
            foreach (var i in SgsMain.Instance.AlivePlayers) dests.Add(i);

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (dest.Hp >= dest.HpLimit) continue;
                if (await WuXieKeJi.Call(this, dest)) continue;

                await new Recover(dest).Execute();
            }
        }
    }

    public class WuZhongShengYou : Card
    {
        public WuZhongShengYou()
        {
            Type = "锦囊牌";
            Name = "无中生有";
        }

        public override async Task UseCard(Player src, List<Player> dests = null)
        {
            // 默认将目标设为使用者
            if (dests is null || dests.Count == 0) dests = new List<Player> { src };

            await base.UseCard(src, dests);

            foreach (var dest in Dests)
            {
                if (await WuXieKeJi.Call(this, dest)) continue;

                await new GetCardFromPile(dest, 2).Execute();
            }
        }
    }
}
