using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class OperationArea : MonoBehaviour
    {
        // 操作区
        public GameObject operationArea;
        // 倒计时读条
        public Slider timer;
        // 提示词
        public Text hint;
        // 按键栏
        public GameObject buttonBar;
        // 确定键
        public Button confirm;
        // 取消键
        public Button cancel;
        // 回合结束键
        public Button finishPhase;

        private Self self { get => GetComponent<Self>(); }
        private CardArea cardArea { get => GetComponent<CardArea>(); }
        private DestArea destArea { get => GetComponent<DestArea>(); }
        private EquipArea equipArea { get => GetComponent<EquipArea>(); }

        private Model.TimerTask timerTask;

        void Start()
        {
            confirm.onClick.AddListener(ClickConfirm);
            cancel.onClick.AddListener(ClickCancel);
            finishPhase.onClick.AddListener(ClickFinishPhase);

            HideTimer();
        }

        private Model.Player player { get => self.model; }
        private List<Model.Card> cards { get => Model.CardPile.Instance.cards; }

        /// <summary>
        /// 点击确定键
        /// </summary>
        private void ClickConfirm()
        {
            StopAllCoroutines();

            List<int> cards = null;
            // if (cardArea.SelectedCard.Count != 0)
            // {
                cards = new List<int>();
                foreach (var card in cardArea.SelectedCard) cards.Add(card.Id);
            // }

            List<int> players = null;
            // if (destArea.SelectedPlayer.Count != 0)
            // {
                players = new List<int>();
                foreach (var player in destArea.SelectedPlayer) players.Add(player.model.Position);
            // }

            List<int> equips = null;
            // if (equipArea.SelectedCard.Count != 0)
            // {
                equips = new List<int>();
                foreach (var card in equipArea.SelectedCard) equips.Add(card.Id);
            // }

            if (timerTask.timerType == TimerType.UseWxkj) timerTask.SendSetWxkjResult(self.model.Position, true, cards);
            else timerTask.SendSetResult(cards, players, equips);
        }

        /// <summary>
        /// 点击取消键
        /// </summary>
        private void ClickCancel()
        {
            StopAllCoroutines();
            HideTimer();
            if (timerTask.timerType == TimerType.UseWxkj) timerTask.SendSetWxkjResult(self.model.Position, false);
            else timerTask.SendSetResult();
        }

        /// <summary>
        /// 点击回合结束键
        /// </summary>
        private void ClickFinishPhase()
        {
            StopAllCoroutines();
            timerTask.SendSetResult();
        }

        /// <summary>
        /// 显示倒计时进度条
        /// </summary>
        public void ShowTimer(Model.TimerTask timerTask)
        {
            if (timerTask.timerType != TimerType.UseWxkj && self.model != timerTask.player) return;

            this.timerTask = timerTask;
            operationArea.SetActive(true);
            // hint.text = GetHint();
            hint.text = timerTask.Hint;

            // 根据进度条类型初始化进度条和按键
            switch (timerTask.timerType)
            {
                // 确定 + 回合结束
                case TimerType.PerformPhase:
                    confirm.gameObject.SetActive(true);
                    finishPhase.gameObject.SetActive(true);
                    // confirm.interactable = false;
                    break;

                // 确定
                case TimerType.DiscardFromHand:
                    confirm.gameObject.SetActive(true);
                    // confirm.interactable = false;
                    break;

                // 确定 + 取消
                default:
                    confirm.gameObject.SetActive(true);
                    cancel.gameObject.SetActive(true);
                    // confirm.interactable = false;
                    break;
            }

            cardArea.InitCardArea(timerTask);
            destArea.InitDestArea(timerTask);
            equipArea.InitEquipArea(timerTask);
            UpdateButtonArea();
            StartTimer(timerTask.second);
        }

        /// <summary>
        /// 隐藏进度条
        /// </summary>
        public void HideTimer()
        {
            // 隐藏所有按键
            StopAllCoroutines();
            confirm.gameObject.SetActive(false);
            cancel.gameObject.SetActive(false);
            finishPhase.gameObject.SetActive(false);
            operationArea.SetActive(false);
        }

        public void HideTimer(Model.TimerTask timerTask)
        {
            if (timerTask.timerType != TimerType.UseWxkj && self.model != timerTask.player) return;
            HideTimer();
        }

        /// <summary>
        /// 显示出牌阶段操作区
        /// </summary>
        private void ShowPerformPhase(int second)
        {
            // 显示按键
            confirm.gameObject.SetActive(true);
            finishPhase.gameObject.SetActive(true);
            confirm.interactable = false;

            // 开始倒计时
            StartTimer(second);
        }

        /// <summary>
        /// 仅显示确定键
        /// </summary>
        /// <param name="second"></param>
        private void ShowConfirm(int second)
        {
            // 显示按键
            confirm.gameObject.SetActive(true);
            confirm.interactable = false;

            // 设置按键位置
            confirm.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            buttonBar.GetComponent<RectTransform>().anchoredPosition = new Vector2(-75, 0);

            // 开始倒计时
            StartTimer(second);
        }

        /// <summary>
        /// 开始倒计时
        /// </summary>
        private void StartTimer(int second)
        {
            timer.value = 1;
            StartCoroutine(UpdateTimer(second));
        }

        private IEnumerator UpdateTimer(int second)
        {
            while (timer.value > 0)
            {
                timer.value -= 0.1f / (second - 0.5f);
                yield return new WaitForSeconds(0.1f);
            }
            ClickCancel();
        }

        /// <summary>
        /// 更新按键区
        /// </summary>
        public void UpdateButtonArea()
        {
            // 启用确定键
            confirm.interactable = cardArea.IsSettled && destArea.IsSettled ? true : false;
        }

        private string GetHint()
        {
            switch (timerTask.timerType)
            {
                case TimerType.PerformPhase:
                    return "出牌阶段，请选择一张卡牌";

                case TimerType.DiscardFromHand:
                    return "请弃置" + timerTask.minCount + "张手牌";


                // case TimerType.AskForShan:
                //     return "请使用一张闪";

                // case TimerType.AskForTao:
                //     return "请使用一张桃";

                // case TimerType.QLYYD:
                //     return "发动青龙偃月刀";

                // case TimerType.CallSkill:
                //     return "是否发动" + timerTask.SkillName + "？";

                default:
                    return "请选择一张牌";
            }
        }

    }
}