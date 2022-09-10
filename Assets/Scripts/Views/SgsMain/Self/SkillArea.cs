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
        private Model.Skill SelectedSkill => Model.Operation.Instance.skill;
        private Player self => SgsMain.Instance.self;
        private Model.TimerTask timerTask => Model.TimerTask.Instance;

        public Transform Long;
        public Transform Short;

        /// <summary>
        /// 初始化技能区
        /// </summary>
        public void InitSkill()
        {
            Skills.Clear();
            foreach (Transform i in Long) if (i.name != "Short") Destroy(i.gameObject);
            foreach (Transform i in Short) Destroy(i.gameObject);

            foreach (var i in Model.SgsMain.Instance.AlivePlayers)
            {
                if (!i.isSelf) continue;
                int c = 0;
                // 实例化预制件，添加到技能区
                foreach (var j in i.skills)
                {
                    string str = j.Passive ? "锁定技" : "主动技";
                    var prefab = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>(str);
                    var instance = Instantiate(prefab).GetComponent<Skill>();
                    instance.name = j.Name;
                    instance.text.text = j.Name;
                    instance.model = j;

                    if (i.skills.Count % 2 == 1 && c == 0)
                    {
                        instance.transform.SetParent(Long, false);
                        instance.transform.SetAsFirstSibling();
                        c++;
                    }
                    else instance.transform.SetParent(Short, false);

                    Skills.Add(instance);
                }

                // MoveSeat(i);
            }
            MoveSeat(Model.SgsMain.Instance.AlivePlayers.Find(x => x.isSelf));
        }

        public void InitSkill(Model.UpdateSkill model)
        {
            Debug.Log("a");
            if (model.player.team == self.model.team) InitSkill();
        }

        public void MoveSeat(Model.Player model)
        {
            foreach (var i in Skills) i.gameObject.SetActive(i.model.Src == model);
        }

        /// <summary>
        /// 显示进度条时更新技能区
        /// </summary>
        public void InitSkillArea()
        {
            if (timerTask.GivenSkill != "") Skills.Find(x => x.name == timerTask.GivenSkill).Select();

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
        public void ResetSkillArea()
        {
            if (!timerTask.isWxkj && self.model != timerTask.player) return;

            foreach (var i in Skills) i.ResetSkill();
        }
    }
}
