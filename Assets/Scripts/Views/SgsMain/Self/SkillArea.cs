using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class SkillArea : MonoBehaviour
    {
        public Dictionary<string, Skill> Skills { get; private set; } = new Dictionary<string, Skill>();
        public Skill SelectedSkill { get; set; }
        public Transform Long;
        public Transform Short;

        // Start is called before the first frame update
        void Start()
        {

        }

        public void InitSkill(Dictionary<string, Model.Skill> model)
        {
            // 从assetbundle中加载卡牌预制件
            // while (!ABManager.Instance.ABMap.ContainsKey("sgsasset")) await Task.Yield();
            // var card = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("Card");

            // 实例化新卡牌，添加到手牌区，并根据卡牌id初始化
            foreach (var i in model)
            {
                string str;
                if (i.Value.Passive) str = "锁定技";
                else str = "主动技";
                var prefab = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>(str);
                var instance = Instantiate(prefab);
                instance.name = i.Key;
                instance.GetComponent<Skill>().model = i.Value;

                if (model.Count % 2 == 1 || Long.childCount == 1)
                {
                    // Long.gameObject.SetActive(true);
                    instance.transform.SetParent(Long, false);
                    instance.transform.SetAsFirstSibling();
                }
                else instance.transform.SetParent(Short, false);

                Skills.Add(i.Key, instance.GetComponent<Skill>());
            }
        }

        public void InitSkillArea(TimerType timerType)
        {
            switch (timerType)
            {
                case TimerType.PerformPhase:
                    foreach (var i in Skills.Values)
                    {
                        if (i.model is Model.Active)
                        {
                            var skill = i.model as Model.Active;
                            if (skill.Time < skill.TimeLimit) i.button.interactable = true;
                        }
                    }
                    break;

                case TimerType.CallSkill:
                    foreach (var i in Skills.Values)
                    {
                        if (i != SelectedSkill) i.button.interactable = false;
                    }
                    break;
            }
        }

        public void ResetSkillArea(Model.TimerTask timerTask)
        {
            if (timerTask.timerType != TimerType.UseWxkj && GetComponent<Self>().model != timerTask.player) return;

            foreach (var i in Skills.Values) i.ResetSkill();
        }
    }
}
