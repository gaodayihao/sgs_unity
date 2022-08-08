using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace View
{
    public class Drag<T> : SingletonMono<T>, IDragHandler where T : MonoBehaviour
    {
        private Canvas canvas;

        void Start()
        {
            canvas = FindObjectOfType<Canvas>();
        }

        public void OnDrag(PointerEventData eventData)
        {
            // 移动拖拽框的位置
            GetComponent<RectTransform>().anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }
}
