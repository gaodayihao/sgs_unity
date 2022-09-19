using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;

namespace Model
{
    /// <summary>
    /// 单机回合系统
    /// </summary>
    public class TurnSystem : SingletonMono<TurnSystem>
    {
        private void Init()
        {
            // 初始化被跳过阶段,设置为否
            SkipPhase = new Dictionary<Player, Dictionary<Phase, bool>>();
            foreach (Player player in SgsMain.Instance.players)
            {
                SkipPhase.Add(player, new Dictionary<Phase, bool>());
                foreach (Phase phase in System.Enum.GetValues(typeof(Phase)))
                {
                    SkipPhase[player].Add(phase, false);
                }
            }
        }

        // 当前执行回合的玩家
        public Player CurrentPlayer { get; private set; }

        // 当前阶段
        public Phase CurrentPhase { get; private set; }

        // 被跳过阶段
        public Dictionary<Player, Dictionary<Phase, bool>> SkipPhase { get; set; }

        public int Round { get; private set; } = 1;

        public async Task StartGame()
        {
            Init();

            CurrentPlayer = SgsMain.Instance.players[0];
            while (true)
            {
                // 执行回合
                startTurnView?.Invoke(this);
                BeforeTurn?.Invoke();
                for (CurrentPhase = Phase.Prepare; CurrentPhase <= Phase.End; CurrentPhase++)
                {
                    // if (!Room.Instance.IsSingle) await Sync();
                    await ExecutePhase();
                    if (ExtraPhase != Phase.Null) await ExeuteExtraPhase();
                    if (SgsMain.Instance.GameIsOver) return;
                }
                finishTurnView?.Invoke(this);
                AfterTurn?.Invoke();

                // 额外回合
                if (ExtraTurn != null) await ExecuteExtraTurn();

                CurrentPlayer = CurrentPlayer.Next;
                Round++;
            }
        }

        private async Task Sync()
        {
            PhaseJson json = new PhaseJson();
            json.eventname = "execute_phase";
            json.id = Wss.Instance.Count + 1;
            json.username = User.Instance.Username;
            json.phase = CurrentPhase;

            Wss.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            var msg = await Wss.Instance.PopSgsMsg();
            Debug.Log(JsonUtility.FromJson<SgsJson>(msg).eventname);
        }

        private async Task ExecutePhase()
        {
            if (!CurrentPlayer.IsAlive) return;
            // 执行阶段开始时view事件
            startPhaseView?.Invoke(this);

            // #if UNITY_EDITOR
            //             await Task.Delay(300);
            // #endif
            await SgsMain.Instance.Delay(0.3f);

            var playerEvents = CurrentPlayer.playerEvents;

            // 阶段开始时判断是否跳过
            if (SkipPhase[CurrentPlayer][CurrentPhase])
            {
                SkipPhase[CurrentPlayer][CurrentPhase] = false;
                return;
            }
            // 执行阶段开始时事件
            await playerEvents.startPhaseEvents[CurrentPhase].Execute();

            // 阶段中判断是否跳过
            if (SkipPhase[CurrentPlayer][CurrentPhase])
            {
                SkipPhase[CurrentPlayer][CurrentPhase] = false;
                return;
            }
            // await playerEvents.phaseEvents[CurrentPhase].Execute();

            switch (CurrentPhase)
            {
                case Phase.Judge:
                    while (CurrentPlayer.JudgeArea.Count != 0)
                    {
                        await CurrentPlayer.JudgeArea[0].Judge();
                    }
                    break;
                // 执行摸牌阶段
                case Phase.Get:

                    var act = new GetCardFromPile(CurrentPlayer, 2);
                    act.InGetCardPhase = true;
                    await act.Execute();
                    break;

                // 执行出牌阶段
                case Phase.Perform:
                    await Perform();
                    break;

                // 执行弃牌阶段
                case Phase.Discard:

                    var count = CurrentPlayer.HandCardCount - CurrentPlayer.HandCardLimit;
                    if (count > 0) await TimerAction.DiscardFromHand(CurrentPlayer, count);
                    break;
            }

            if (!CurrentPlayer.IsAlive) return;

            // 执行阶段结束时事件
            await playerEvents.finishPhaseEvents[CurrentPhase].Execute();
            finishPhaseView?.Invoke(this);
        }

        private async Task Perform()
        {
            // 重置出杀次数
            CurrentPlayer.ShaCount = 0;
            CurrentPlayer.酒Count = 0;
            CurrentPlayer.Use酒 = false;
            // 重置使用技能次数
            // foreach (var i in CurrentPlayer.skills.Values) if (i is Active) (i as Active).Time = 0;

            bool result = true;
            var timerTask = TimerTask.Instance;

            while (result && !SgsMain.Instance.GameIsOver && CurrentPlayer.IsAlive)
            {
                // 暂停线程,显示进度条
                timerTask.Hint = "出牌阶段，请选择一张牌。";
                timerTask.MaxDest = DestArea.Instance.MaxDest;
                timerTask.MinDest = DestArea.Instance.MinDest;
                timerTask.IsValidCard = CardArea.Instance.ValidCard;
                timerTask.IsValidDest = DestArea.Instance.ValidDest;
                timerTask.isPerformPhase = true;
                result = await timerTask.Run(CurrentPlayer, 1, 0);
                timerTask.isPerformPhase = false;

                if (CurrentPlayer.isAI)
                {
                    result = AI.Instance.Perform();
                }

                if (result)
                {
                    if (timerTask.Skill != "")
                    {
                        var skill = CurrentPlayer.FindSkill(timerTask.Skill) as Active;
                        await skill.Execute(timerTask.Dests, timerTask.Cards, "");
                    }
                    else
                    {
                        var card = timerTask.Cards[0];
                        if (card is 杀) CurrentPlayer.ShaCount++;
                        await card.UseCard(CurrentPlayer, timerTask.Dests);
                    }
                }
                finishPerformView?.Invoke(this);
            }


            // 重置出杀次数
            CurrentPlayer.ShaCount = 0;
            CurrentPlayer.Use酒 = false;

            AfterPerform?.Invoke();
        }

        public Action BeforeTurn;
        public Action AfterTurn;
        public Action AfterPerform;

        public void SortDest(List<Player> dests)
        {
            dests.Sort((x, y) =>
            {
                Player i = CurrentPlayer;
                while (true)
                {
                    if (x == i) return -1;
                    else if (y == i) return 1;
                    i = i.Next;
                }
            });
        }

        public Player ExtraTurn { get; set; }
        private async Task ExecuteExtraTurn()
        {
            var t = CurrentPlayer;
            CurrentPlayer = ExtraTurn;
            ExtraTurn = null;

            // 执行回合
            startTurnView?.Invoke(this);
            BeforeTurn?.Invoke();
            for (CurrentPhase = Phase.Prepare; CurrentPhase <= Phase.End; CurrentPhase++)
            {
                // if (!Room.Instance.IsSingle) await Sync();
                await ExecutePhase();
                if (ExtraPhase != Phase.Null) await ExeuteExtraPhase();
                if (SgsMain.Instance.GameIsOver) return;
            }
            finishTurnView?.Invoke(this);
            AfterTurn?.Invoke();

            if (ExtraTurn != null) await ExecuteExtraTurn();

            CurrentPlayer = t;
        }

        public Phase ExtraPhase = Phase.Null;
        private async Task ExeuteExtraPhase()
        {
            var t = CurrentPhase;
            CurrentPhase = ExtraPhase;
            ExtraPhase = Phase.Null;

            // 执行回合
            await ExecutePhase();

            CurrentPhase = t;
        }

        private UnityAction<TurnSystem> startTurnView;
        private UnityAction<TurnSystem> finishTurnView;
        private UnityAction<TurnSystem> startPhaseView;
        private UnityAction<TurnSystem> finishPhaseView;
        private UnityAction<TurnSystem> finishPerformView;

        /// <summary>
        /// 回合开始时view事件
        /// </summary>
        public event UnityAction<TurnSystem> StartTurnView
        {
            add => startTurnView += value;
            remove => startTurnView -= value;
        }
        /// <summary>
        /// 回合结束后view事件
        /// </summary>
        public event UnityAction<TurnSystem> FinishPhaseView
        {
            add => finishPhaseView += value;
            remove => finishPhaseView -= value;
        }
        /// <summary>
        /// 阶段开始时view事件
        /// </summary>
        public event UnityAction<TurnSystem> StartPhaseView
        {
            add => startPhaseView += value;
            remove => startPhaseView -= value;
        }
        /// <summary>
        /// 阶段结束时view事件
        /// </summary>
        public event UnityAction<TurnSystem> FinishTurnView
        {
            add => finishTurnView += value;
            remove => finishTurnView -= value;
        }
        /// <summary>
        /// 完成一次出牌时view事件(清空弃牌区)
        /// </summary>
        public event UnityAction<TurnSystem> FinishPerformView
        {
            add => finishPerformView += value;
            remove => finishPerformView -= value;
        }

    }
}