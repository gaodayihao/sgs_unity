using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class PlayerEquip : MonoBehaviour
    {
        public int Id { get; private set; }
        public Image cardImage;
        public Image suit;
        public Image weight;

        private Sprites sprites => Sprites.Instance;

        public void Init(Model.Equipage card)
        {
            Id = card.Id;
            name = card.Name;

            var sprites = Sprites.Instance;
            cardImage.sprite = sprites.seat_equip[name];
            suit.sprite = sprites.seat_suit[card.Suit];
            if (card.Suit == "黑桃" || card.Suit == "草花") weight.sprite = sprites.seat_blackWeight[card.Weight];
            else weight.sprite = sprites.seat_redWeight[card.Weight];
        }
    }
}