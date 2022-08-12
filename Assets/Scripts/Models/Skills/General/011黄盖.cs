using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 苦肉 : Active
    {
        public 苦肉(Player src) : base(src, "苦肉", 1) { }

        public override int MaxCard()
        {
            return 1;
        }

        public override int MinCard()
        {
            return 1;
        }

        public override async Task Execute(List<Player> dests, List<Card> cards, string additional)
        {
            await base.Execute(dests, cards, additional);

            await new Discard(Src, cards).Execute();
            await new UpdateHp(Src, -1).Execute();
        }
    }

    public class 诈降 : Triggered
    {
        public 诈降(Player src) : base(src, "诈降", true) { }

        public override void OnEnabled()
        {
            Src.playerEvents.afterLoseHp.AddEvent(Src, Execute);
        }

        public override void OnDisabled()
        {
            Src.playerEvents.afterLoseHp.RemoveEvent(Src, Execute);
        }

        public async Task Execute(UpdateHp updataHp)
        {
            Execute();

            await new GetCardFromPile(Src, -3 * updataHp.Value).Execute();

            if (TurnSystem.Instance.CurrentPlayer != Src || TurnSystem.Instance.CurrentPhase != Phase.Perform)
            {
                return;
            }

            // 出杀次数加1
            Src.ShaCount--;
            // 红杀无距离限制
            Src.unlimitedDst += IsUnlimited;
            // 红杀不可闪避
            Src.playerEvents.afterUseCard.AddEvent(Src, WhenUseSha);

            TurnSystem.Instance.AfterTurn += ResetEffect;
        }

        private bool IsUnlimited(Card card, Player dest)
        {
            return card is 杀 && (card.Suit == "红桃" || card.Suit == "方片");
        }

        private async Task WhenUseSha(Card card)
        {
            if (card is 杀 && (card.Suit == "红桃" || card.Suit == "方片" || card.Suit == "红色"))
            {
                await Task.Yield();
                (card as 杀).ShanCount = 0;
            }
        }

        private void ResetEffect()
        {
            Src.unlimitedDst -= IsUnlimited;
            Src.playerEvents.afterUseCard.RemoveEvent(Src, WhenUseSha);
            TurnSystem.Instance.AfterTurn -= ResetEffect;
        }
    }
}