using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class EquipArea : SingletonMono<EquipArea>
    {
        private Dictionary<string, Equipage> equipages;
        public List<Equipage> SelectedCard { get; private set; } = new List<Equipage>();
        private Model.TimerTask timerTask { get => Model.TimerTask.Instance; }
        private Model.Skill skill { get => SkillArea.Instance.SelectedSkill; }

        private Player self { get => SgsMain.Instance.self; }

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

        public void InitEquipArea()
        {
            if (Model.TimerTask.Instance.GivenSkill != null)
            {
                foreach (var i in equipages.Values)
                {
                    if (i.name == Model.TimerTask.Instance.GivenSkill)
                    {
                        i.Use();
                        break;
                    }
                }
            }

            if (CardArea.Instance.MaxCount == 0)
            {
                foreach (var i in equipages.Values) i.button.interactable = false;
            }

            else if (skill != null)
            {
                foreach (var i in equipages.Values) i.button.interactable = skill.IsValidCard(i.model);
            }

            else
            {
                foreach (var i in equipages.Values) i.button.interactable = timerTask.ValidCard(i.model);

                if (equipages["武器"].name == "丈八蛇矛" && timerTask.ValidCard(Model.Card.Convert<Model.杀>()))
                {
                    equipages["武器"].button.interactable = true;
                }
            }
        }

        public void ResetEquipArea()
        {
            // 重置装备牌状态
            foreach (var card in equipages.Values) card.ResetCard();
        }

        public void ResetEquipArea(Model.TimerTask timerTask)
        {
            if (self.model != timerTask.player) return;

            ResetEquipArea();
        }

        public void MoveSeat(Model.Player model)
        {
            foreach (var i in model.Equipages)
            {
                var equip = equipages[i.Key];
                if (i.Value is null) equip.gameObject.SetActive(false);
                else
                {
                    equip.gameObject.SetActive(true);
                    equip.Init(i.Value);
                }
            }
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