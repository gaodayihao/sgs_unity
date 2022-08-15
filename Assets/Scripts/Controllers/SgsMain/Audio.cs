using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Controller
{
    public class Audio : MonoBehaviour
    {
        private View.Audio view => GetComponent<View.Audio>();

        void Start()
        {
            // view = GetComponent<View.Audio>();

            Model.Card.UseCardView += view.CardVoice;
            Model.UpdateHp.ActionView += view.OnDamage;
            Model.Skill.UseSkillView += view.SkillVoice;
            Model.SetLock.ActionView += view.OnLock;
        }

        private void OnDestroy()
        {
            Model.Card.UseCardView -= view.CardVoice;
            Model.UpdateHp.ActionView -= view.OnDamage;
            Model.Skill.UseSkillView -= view.SkillVoice;
            Model.SetLock.ActionView -= view.OnLock;
        }
    }
}