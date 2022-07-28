using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class SkillArea : MonoBehaviour
    {
        // 技能表
        public List<Skill> Skills { get; private set; } = new List<Skill>();
        // 已选技能
        public Skill SelectedSkill { get; set; }
        private Player self { get => GameObject.FindObjectOfType<SgsMain>().self; }

        public Transform Long;
        public Transform Short;

        /// <summary>
        /// 初始化技能区
        /// </summary>
        public void InitSkill(Model.Player model)
        {
            int c = 0;
            // 实例化预制件，添加到技能区
            foreach (var i in model.skills)
            {
                string str;
                if (i.Value.Passive) str = "锁定技";
                else str = "主动技";
                var prefab = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>(str);
                var instance = Instantiate(prefab).GetComponent<Skill>();
                instance.name = i.Key;
                instance.text.text = i.Key;
                instance.model = i.Value;

                if (model.skills.Count % 2 == 1 && c == 0)
                {
                    instance.transform.SetParent(Long, false);
                    instance.transform.SetAsFirstSibling();
                    c++;
                }
                else instance.transform.SetParent(Short, false);

                Skills.Add(instance);
            }

            MoveSeat(model);
        }

        public void MoveSeat(Model.Player model)
        {
            foreach (var i in Skills)
            {
                i.gameObject.SetActive(i.model.Src == model);
            }
        }

        public void Clear()
        {

        }

        /// <summary>
        /// 显示进度条时更新技能区
        /// </summary>
        public void InitSkillArea(TimerType timerType)
        {
            switch (timerType)
            {
                case TimerType.CallSkill:
                    foreach (var i in Skills)
                    {
                        if (i != SelectedSkill) i.button.interactable = false;
                        if (i.name == Model.TimerTask.Instance.GivenSkill) i.Select();
                    }
                    break;

                default:
                    foreach (var i in Skills)
                    {
                        i.button.interactable = i.model.IsValid() &&
                            (i.model is Model.Active || i.model is Model.Converted);
                    }
                    break;
            }
        }

        /// <summary>
        /// 重置技能区
        /// </summary>
        public void ResetSkillArea(Model.TimerTask timerTask)
        {
            if (timerTask.timerType != TimerType.无懈可击 && self.model != timerTask.player) return;

            foreach (var i in Skills) i.ResetSkill();
        }
    }
}
