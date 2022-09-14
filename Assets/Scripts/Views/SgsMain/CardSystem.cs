using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace View
{
    public class CardSystem : SingletonMono<CardSystem>
    {
        private GameObject[] players => SgsMain.Instance.players;

        public Dictionary<int, Card> Cards { get; private set; } = new Dictionary<int, Card>();
        private CardArea cardArea => CardArea.Instance;

        public void UpdateAll(float second)
        {
            foreach (var i in Cards.Values) i.Move(second);

            foreach (var i in CardArea.Instance.handcards.Values) i.Move(second);

            foreach (var i in DiscardArea.Instance.discards) i?.Move(second);
        }

        public Vector3 PlayerPos(Model.Player model) => IsViewSelf(model)
            ? Self.Instance.transform.position
            : players[model.Position].transform.position;

        public bool IsViewSelf(Model.Player model) => model == SgsMain.Instance.self.model;

        public async void GetCardFromPile(Model.GetCard model)
        {
            if (model is not Model.GetCardFromPile) return;

            var dest = PlayerPos(model.player);

            var cardAnime = Instantiate(ABManager.Instance.GetSgsAsset("CardAnime"), transform.parent).transform;
            cardAnime.position = Vector3.MoveTowards(dest, transform.position, 50);

            foreach (var i in model.Cards)
            {
                var card = Instantiate(ABManager.Instance.GetSgsAsset("Card")).GetComponent<Card>();
                card.SetParent(cardAnime);
                Cards.Add(i.Id, card);
                if (model.player.isSelf) card.Init(i);
                else
                {
                    card.InitInPanel(i, false);
                    card.button.interactable = false;
                }
            }

            int f = Time.frameCount;
            while (f == Time.frameCount) await Task.Yield();
            UpdateAll(0);

            if (IsViewSelf(model.player))
            {
                foreach (var i in model.Cards) cardArea.Add(Cards[i.Id]);
                cardArea.UpdateSpacing(false);
            }
            else
            {
                cardAnime.position = dest;
                foreach (var i in model.Cards) Destroy(Cards[i.Id].gameObject, 1f);
            }

            UpdateAll(0.3f);
            foreach (var i in model.Cards) Cards.Remove(i.Id);
            Destroy(cardAnime.gameObject, 0.6f);
        }

        private async void GetCardFromElse(List<Model.Card> cards, Model.Player src, Model.Player dest, bool known)
        {
            var cardAnime = Instantiate(
                ABManager.Instance.GetSgsAsset("CardAnime"),
                PlayerPos(src),
                Quaternion.identity,
                transform.parent).transform;

            if (IsViewSelf(src))
            {
                foreach (var i in cards)
                {
                    var card = Instantiate(ABManager.Instance.GetSgsAsset("Card")).GetComponent<Card>();
                    Cards.Add(i.Id, card);
                    card.transform.position = transform.InverseTransformPoint(
                        cardArea.handcards.ContainsKey(i.Id) ?
                        cardArea.handcards[i.Id].transform.position : Self.Instance.transform.position);

                    card.SetParent(cardAnime);
                }
            }
            else
            {
                foreach (var i in cards)
                {
                    var card = Instantiate(ABManager.Instance.GetSgsAsset("Card")).GetComponent<Card>();
                    Cards.Add(i.Id, card);
                    card.SetParent(cardAnime);
                }
                int f = Time.frameCount;
                while (f == Time.frameCount) await Task.Yield();
                UpdateAll(0);
            }

            foreach (var i in cards)
            {
                var card = Cards[i.Id];

                if (known) card.Init(i);
                else
                {
                    card.InitInPanel(i, false);
                    card.button.interactable = false;
                }

                if (IsViewSelf(dest)) CardArea.Instance.Add(card);
                else Destroy(card.gameObject, 1);
            }

            if (IsViewSelf(dest)) cardArea.UpdateSpacing(false);
            else cardAnime.position = PlayerPos(dest);

            UpdateAll(0.5f);
            Destroy(cardAnime.gameObject, 1f);
            foreach (var i in cards) Cards.Remove(i.Id);
        }

        public void GetCardFromElse(Model.GetCard model)
        {
            if (model is not Model.GetCardFromElse) return;
            var gfe = model as Model.GetCardFromElse;

            GetCardFromElse(model.Cards, gfe.Dest, gfe.player, gfe.Dest.isSelf || gfe.player.isSelf);
        }

        public void Exchange(Model.ExChange model)
        {
            bool known = model.Dest.isSelf || model.player.isSelf;
            GetCardFromElse(model.Dest.HandCards, model.Dest, model.player, known);
            GetCardFromElse(model.player.HandCards, model.player, model.Dest, known);
        }

        public void GetJudgeCard(Model.GetCard model)
        {
            if (model is not Model.GetJudgeCard) return;
            // var gjc = model as Model.GetJudgeCard;
            GetCardFromElse(model.Cards, (model.Cards[0] as Model.DelayScheme).Owner, model.player, true);
        }

        public async void GetDiscard(Model.GetCard model)
        {
            if (model is not Model.GetDisCard) return;

            var dest = PlayerPos(model.player);

            var cardAnime = Instantiate(ABManager.Instance.GetSgsAsset("CardAnime"), transform.parent).transform;
            cardAnime.position = transform.InverseTransformPoint(transform.position);

            foreach (var i in model.Cards)
            {
                var card = Instantiate(ABManager.Instance.GetSgsAsset("Card")).GetComponent<Card>();
                card.SetParent(cardAnime);
                Cards.Add(i.Id, card);
                card.Init(i);
                var d = DiscardArea.Instance.discards.Find(x => x.Id == i.Id);
                if (d != null) card.transform.position = transform.InverseTransformPoint(d.transform.position);
                else
                {

                    await SgsMain.Instance.WaitFrame();
                    UpdateAll(0);
                }
            }

            if (IsViewSelf(model.player))
            {
                foreach (var i in model.Cards) cardArea.Add(Cards[i.Id]);
                cardArea.UpdateSpacing(false);
            }
            else
            {
                cardAnime.position = dest;
                foreach (var i in model.Cards) Destroy(Cards[i.Id].gameObject, 1f);
            }

            UpdateAll(0.3f);
            foreach (var i in model.Cards) Cards.Remove(i.Id);
            Destroy(cardAnime.gameObject, 0.6f);
        }

        public void UseCard(Model.Card card)
        {
            if (card.Dests is null) return;

            Vector3 src = PlayerPos(card.Src);
            foreach (var i in card.Dests)
            {
                if (i == card.Src) continue;
                Vector3 dest = PlayerPos(i);
                StartCoroutine(DrawLine(src, dest));
            }
        }

        public void UseSkill(Model.Skill skill)
        {
            if (skill.Dests is null || skill is Model.Converted) return;

            Vector3 src = PlayerPos(skill.Src);
            foreach (var i in skill.Dests)
            {
                if (i == skill.Src) continue;
                Vector3 dest = PlayerPos(i);
                StartCoroutine(DrawLine(src, dest));
            }
        }

        private IEnumerator DrawLine(Vector3 src, Vector3 dest)
        {
            var line = Instantiate(ABManager.Instance.GetSgsAsset("Line"), transform.parent).GetComponent<LineRenderer>();
            Destroy(line.gameObject, 1.5f);

            src = transform.InverseTransformPoint(src);
            dest = transform.InverseTransformPoint(dest);

            line.SetPosition(0, src);
            line.SetPosition(1, src);

            var x = dest - src;
            // float s=
            while ((line.GetPosition(1)) != dest)
            {
                var dx = x / 0.3f * Time.deltaTime;

                if (dx.magnitude < (dest - line.GetPosition(1)).magnitude)
                    line.SetPosition(1, line.GetPosition(1) + dx);

                else
                {
                    line.SetPosition(1, dest);
                }
                yield return null;
            }

            yield return new WaitForSeconds(0.8f);

            while ((line.GetPosition(0)) != dest)
            {
                var dx = x / 0.3f * Time.deltaTime;

                if (dx.magnitude < (dest - line.GetPosition(0)).magnitude)
                    line.SetPosition(0, line.GetPosition(0) + dx);

                else
                {
                    line.SetPosition(0, dest);
                }
                yield return null;
            }
        }
    }
}
