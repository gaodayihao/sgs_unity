using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class EquipArea : MonoBehaviour
    {
        private Dictionary<string, Equipage> equipages;
        public List<Equipage> SelectedCard { get; private set; } = new List<Equipage>();

        private Self self { get => GetComponent<Self>(); }

        void Start()
        {
            var parent = transform.Find("装备区");
            equipages = new Dictionary<string, Equipage>
            {
                {"武器", parent.transform.Find("武器").GetComponent<Equipage>()},
                {"防具", parent.transform.Find("防具").GetComponent<Equipage>()},
                {"加一马", parent.transform.Find("加一马").GetComponent<Equipage>()},
                {"减一马", parent.transform.Find("减一马").GetComponent<Equipage>()}
            };
        }

        public void InitEquipArea(Model.TimerTask timerTask)
        {
            switch (timerTask.timerType)
            {
                case TimerType.Discard:
                case TimerType.SelectCard:
                    foreach (var card in equipages.Values) card.button.interactable = true;
                    break;
                case TimerType.PerformPhase:
                    if (equipages["武器"].name == "丈八蛇矛") equipages["武器"].button.interactable = true;
                    break;
                case TimerType.UseCard:
                    if (timerTask.GivenCard.Contains("杀"))
                    {
                        if (equipages["武器"].name == "丈八蛇矛") equipages["武器"].button.interactable = true;
                    }
                    break;
            }
        }

        public void ResetEquipArea(Model.TimerTask timerTask)
        {
            if (self.model != timerTask.player) return;

            // 重置装备牌状态
            foreach (var card in equipages.Values) card.ResetCard();
        }

        public void ShowEquipage(Model.Equipage card)
        {
            if (card.Src != self.model) return;

            equipages[card.Type].gameObject.SetActive(true);
            equipages[card.Type].Init(card);
        }

        public void HideEquipage(Model.Equipage card)
        {
            if (card.Owner != self.model) return;
            if (card.Id != equipages[card.Type].Id) return;

            equipages[card.Type].gameObject.SetActive(false);
        }
    }
}