using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class OperationArea : SingletonMono<OperationArea>
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

        private Player self { get => GameObject.FindObjectOfType<SgsMain>().self; }
        private CardArea cardArea { get => CardArea.Instance; }
        private DestArea destArea { get => DestArea.Instance; }
        private EquipArea equipArea { get => EquipArea.Instance; }
        private SkillArea skillArea { get => SkillArea.Instance; }

        private Model.TimerTask timerTask;
        public TimerType timerType { get; private set; }

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

            List<int> cards = new List<int>();
            foreach (var card in cardArea.SelectedCard) cards.Add(card.Id);
            foreach (var card in equipArea.SelectedCard) cards.Add(card.Id);

            List<int> players = new List<int>();
            foreach (var player in destArea.SelectedPlayer) players.Add(player.model.Position);

            // List<int> equips = new List<int>();
            // foreach (var card in equipArea.SelectedCard) equips.Add(card.Id);

            string skill = "";
            if (skillArea.SelectedSkill != null) skill = skillArea.SelectedSkill.Name;

            if (timerTask.timerType != TimerType.无懈可击)
            {
                timerTask.SendResult(cards, players, skill);
            }
            else
            {

                bool isSelf = self.model.HandCards.Contains(Model.CardPile.Instance.cards[cards[0]]);
                timerTask.SendWxkjResult((isSelf ? self.model : self.model.Teammate).Position, true, cards);
            }
        }

        /// <summary>
        /// 点击取消键
        /// </summary>
        private void ClickCancel()
        {
            StopAllCoroutines();
            HideTimer();
            if (timerTask.timerType != TimerType.无懈可击) timerTask.SendResult();
            else
            {
                timerTask.SendWxkjResult(self.model.Position, false);
                timerTask.SendWxkjResult(self.model.Teammate.Position, false);
            }
        }

        /// <summary>
        /// 点击回合结束键
        /// </summary>
        private void ClickFinishPhase()
        {
            StopAllCoroutines();
            timerTask.SendResult();
        }

        /// <summary>
        /// 显示倒计时进度条
        /// </summary>
        public void ShowTimer(Model.TimerTask timerTask)
        {
            if (timerTask.timerType != TimerType.无懈可击 && self.model != timerTask.player) return;

            this.timerTask = timerTask;
            timerType = timerTask.timerType;
            operationArea.SetActive(true);
            hint.text = timerTask.Hint;

            // 根据进度条类型初始化进度条和按键
            switch (timerType)
            {
                // 确定 + 回合结束
                case TimerType.PerformPhase:
                    confirm.gameObject.SetActive(true);
                    finishPhase.gameObject.SetActive(true);
                    break;

                // 确定
                case TimerType.SelectHandCard:
                    confirm.gameObject.SetActive(true);
                    break;

                // 确定 + 取消
                default:
                    confirm.gameObject.SetActive(true);
                    cancel.gameObject.SetActive(true);
                    break;
            }

            skillArea.InitSkillArea();
            cardArea.InitCardArea();
            destArea.InitDestArea();
            equipArea.InitEquipArea();

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
            if (timerTask.timerType != TimerType.无懈可击 && self.model != timerTask.player) return;
            HideTimer();
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
            confirm.interactable = cardArea.IsSettled && destArea.IsSettled;
        }

        public void UseSkill()
        {
            cardArea.ResetCardArea();
            destArea.ResetDestArea();
            equipArea.ResetEquipArea();

            this.timerType = timerType;

            skillArea.InitSkillArea();
            cardArea.InitCardArea();
            destArea.InitDestArea();
            equipArea.InitEquipArea();

            UpdateButtonArea();
        }

        // public void ChangeType()
        // {
        //     ChangeType(timerTask.timerType);
        // }

    }
}