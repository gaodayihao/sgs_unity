using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class SkillArea : SingletonMono<SkillArea>
    {
        // 技能表
        public List<Skill> Skills { get; private set; } = new List<Skill>();
        // 已选技能
        public Model.Skill SelectedSkill { get; set; }
        private Player self => SgsMain.Instance.self;
        private Model.TimerTask timerTask => Model.TimerTask.Instance;

        public Transform Long;
        public Transform Short;

        /// <summary>
        /// 初始化技能区
        /// </summary>
        public void InitSkill()
        {
            foreach (var i in Model.SgsMain.Instance.players)
            {
                if (!i.isSelf) continue;
                int c = 0;
                // 实例化预制件，添加到技能区
                foreach (var j in i.skills)
                {
                    string str;
                    if (j.Value.Passive) str = "锁定技";
                    else str = "主动技";
                    var prefab = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>(str);
                    var instance = Instantiate(prefab).GetComponent<Skill>();
                    instance.name = j.Key;
                    instance.text.text = j.Key;
                    instance.model = j.Value;

                    if (i.skills.Count % 2 == 1 && c == 0)
                    {
                        instance.transform.SetParent(Long, false);
                        instance.transform.SetAsFirstSibling();
                        c++;
                    }
                    else instance.transform.SetParent(Short, false);

                    Skills.Add(instance);
                }

                MoveSeat(i);
            }
        }

        public void MoveSeat(Model.Player model)
        {
            foreach (var i in Skills)
            {
                i.gameObject.SetActive(i.model.Src == model);
            }
        }

        /// <summary>
        /// 显示进度条时更新技能区
        /// </summary>
        public void InitSkillArea()
        {
            if (Model.TimerTask.Instance.GivenSkill != "")
            {
                foreach (var i in Skills)
                {
                    if (i.name == Model.TimerTask.Instance.GivenSkill)
                    {
                        i.Select();
                        break;
                    }
                }
            }
            if (SelectedSkill != null)
            {
                foreach (var i in Skills) i.button.interactable = i.model == SelectedSkill;
            }
            else
            {
                foreach (var i in Skills) i.button.interactable = !(i.model is Model.Triggered) && i.model.IsValid;
            }
        }

        /// <summary>
        /// 重置技能区
        /// </summary>
        public void ResetSkillArea(Model.TimerTask timerTask)
        {
            if (!timerTask.isWxkj && self.model != timerTask.player) return;

            foreach (var i in Skills) i.ResetSkill();
        }
    }
}
