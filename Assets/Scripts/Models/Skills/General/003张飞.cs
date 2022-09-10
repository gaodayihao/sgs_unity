using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Model
{
    public class 咆哮 : Triggered
    {
        public 咆哮(Player src) : base(src, "咆哮", true) { }

        public bool Effect(Card card) => card is 杀;

        public override void OnEnabled()
        {
            Src.unlimitedCard += Effect;
            Src.playerEvents.whenUseCard.AddEvent(Src, Execute);
        }

        public override void OnDisabled()
        {
            Src.unlimitedCard -= Effect;
            Src.playerEvents.whenUseCard.RemoveEvent(Src, Execute);
        }

        public async Task Execute(Card card)
        {
            await Task.Yield();
            if (card is 杀 && Src.ShaCount > 0) Execute();
        }
    }
}