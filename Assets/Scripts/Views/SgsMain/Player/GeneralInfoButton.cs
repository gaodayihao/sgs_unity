using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace View
{
    public class GeneralInfoButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private GeneralInfo generalInfo => SgsMain.Instance.transform.Find("武将信息").GetComponent<GeneralInfo>();


        public async void ShowInfo()
        {
            var player = GetComponent<Player>();
            if (player != null)
            {
                await generalInfo.Init(player.model.general, player.CurrentSkin.id.ToString(), player.CurrentSkin.name);
            }
            else
            {
                var general = GetComponent<General>();
                await generalInfo.Init(general.model, 1 + general.Id.ToString().PadLeft(3, '0') + "01", "经典形象");
            }
            generalInfo.gameObject.SetActive(true);
        }

        // bool clicking = false;
        // float totalDownTime = 0;

        private float holdTime = 1f;

        // 如果希望侦听点击事件，就把这些注释全掉取消掉
        //private bool held = false;
        //public UnityEvent onClick = new UnityEvent();

        // public UnityEvent onLongPress = new UnityEvent();

        public void OnPointerDown(PointerEventData eventData)
        {
            //held = false;
            Invoke("OnLongPress", holdTime);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CancelInvoke("OnLongPress");
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CancelInvoke("OnLongPress");
        }

        private void OnLongPress()
        {
            ShowInfo();
        }
    }
}