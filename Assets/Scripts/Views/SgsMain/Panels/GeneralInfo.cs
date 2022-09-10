using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace View
{
    public class GeneralInfo : SingletonMono<GeneralInfo>
    {
        public Image image;
        public Text generalName;
        public Transform skills;
        private GameObject skillObj;

        public async Task Init(Model.General model, string skinId, string skinName)
        {
            // imag
            // var model = player.model.general;
            generalName.text = "   " + skinName + "*" + model.name;
            // foreach(var i in player.model.skills)
            skillObj = ABManager.Instance.ABMap["sgsasset"].LoadAsset<GameObject>("技能信息");
            // await Task.Yield();
            for (int i = 0; i < model.skill.Count; i++)
            {
                // 
                var skill = Instantiate(skillObj).GetComponent<SkillInfo>();
                skill.transform.SetParent(skills, false);
                skill.title.text = model.skill[i];
                skill.discribe.text = model.discribe[i];
            }

            // int id = player.CurrentSkin.id;
            string url = Urls.GENERAL_IMAGE + "Window/" + skinId + ".png";
            var texture = await WebRequest.GetTexture(url);
            image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        void OnDisable()
        {
            foreach (Transform i in skills) if (i.name != "武将名" && i.name != "背景") Destroy(i.gameObject);
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0)) gameObject.SetActive(false);
        }
    }
}
