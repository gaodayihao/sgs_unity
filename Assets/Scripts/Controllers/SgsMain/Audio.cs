using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class Audio : MonoBehaviour
    {
        private View.Audio view;

        void Start()
        {
            view = GetComponent<View.Audio>();

            Model.Card.UseCardView += view.CardVoice;
            Model.UpdateHp.ActionView += view.Damage;
            Model.Skill.UseSkillView += view.SkillVoice;
        }

        private void OnDestroy()
        {
            Model.Card.UseCardView -= view.CardVoice;
            Model.UpdateHp.ActionView -= view.Damage;
            Model.Skill.UseSkillView -= view.SkillVoice;
        }
    }
}