using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class SkillArea : MonoBehaviour
    {
        // 技能表
        public Dictionary<string, Skill> Skills { get; private set; } = new Dictionary<string, Skill>();
        // 已选技能
        public Skill SelectedSkill { get; set; }
        private Player self { get => GameObject.FindObjectOfType<SgsMain>().self; }

        public Transform Long;
        public Transform Short;

        /// <summary>
        /// 初始化技能区
        /// </summary>
        public void InitSkill(Dictionary<string, Model.Skill> model)
        {
            Debug.Log("init skill");
            // 实例化预制件，添加到技能区
            foreach (var i in model)
            {
                string str;
                if (i.Value.Passive) str = "锁定技";
                else str = "主动技";
                var prefab = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>(str);
                var instance = Instantiate(prefab).GetComponent<Skill>();
                instance.name = i.Key;
                instance.text.text = i.Key;
                instance.model = i.Value;

                if (model.Count % 2 == 1 && Long.childCount == 1)
                {
                    instance.transform.SetParent(Long, false);
                    instance.transform.SetAsFirstSibling();
                }
                else instance.transform.SetParent(Short, false);

                Skills.Add(i.Key, instance);
            }
        }

        /// <summary>
        /// 显示进度条时更新技能区
        /// </summary>
        public void InitSkillArea(TimerType timerType)
        {
            switch (timerType)
            {
                case TimerType.CallSkill:
                    foreach (var i in Skills.Values)
                    {
                        if (i != SelectedSkill) i.button.interactable = false;
                        if(i.name==Model.TimerTask.Instance.GivenSkill) i.Select();
                    }
                    break;

                default:
                    foreach (var i in Skills.Values)
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
            if (timerTask.timerType != TimerType.UseWxkj && self.model != timerTask.player) return;

            foreach (var i in Skills.Values) i.ResetSkill();
        }
    }
}
