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
    public class TurnSystem : Singleton<TurnSystem>
    {

        // 
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

            // PerformTimer = SgsMain.Instance.mode.performTimer;
        }

        // 当前执行回合的玩家
        public Player CurrentPlayer { get; private set; }

        // 当前阶段
        public Phase CurrentPhase { get; private set; }

        // 被跳过阶段
        public Dictionary<Player, Dictionary<Phase, bool>> SkipPhase { get; set; }

        public async Task StartGame()
        {
            Init();

            CurrentPlayer = SgsMain.Instance.players[0];
            // int turnCount = 0;

            // 单机模式
            // if (Room.Instance.isSingle)
            // {
                while (SgsMain.Instance.players[0].IsAlive)
                {
                    // Debug.Log("CurrentPlayer is " + CurrentPlayer.Position.ToString());

                    // 执行回合
                    startTurnView?.Invoke(this);
                    for (CurrentPhase = Phase.Prepare; CurrentPhase <= Phase.End; CurrentPhase++)
                    {
                        if(!Room.Instance.isSingle) await Sync();
                        BeforeTurn?.Invoke();
                        await ExecutePhase();
                        AfterTurn?.Invoke();
                    }
                    finishTurnView?.Invoke(this);

                    CurrentPlayer = CurrentPlayer.Next;
                    // turnCount++;
                }
            // }
            // 多人模式
            // else
            // {
            //     SendPhase(CurrentPlayer, Phase.Prepare);
            //     Connection.Instance.IsRunning = false;
            // }
        }

        private async Task Sync()
        {
            PhaseJson json = new PhaseJson();
            json.eventname = "execute_phase";
            json.id = Connection.Instance.Count + 1;
            json.player = CurrentPlayer.Position;
            json.phase = CurrentPhase;

            Connection.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
            var msg = await Connection.Instance.PopSgsMsg();
            Debug.Log(JsonUtility.FromJson<SgsJson>(msg).eventname);
        }

        // private void SendPhase(Player player, Phase phase)
        // {
        //     PhaseJson json = new PhaseJson();
        //     json.eventname = "execute_phase";
        //     json.id = Connection.Instance.Count + 1;
        //     json.player = player.Position;
        //     json.phase = phase;

        //     Connection.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
        // }

        // private void SendNextPhase()
        // {
        //     if (CurrentPhase != Phase.End) SendPhase(CurrentPlayer, CurrentPhase + 1);
        //     else SendPhase(CurrentPlayer.Next, Phase.Prepare);
        // }

        // public async void ReceivePhase(PhaseJson json)
        // {
        //     CurrentPlayer = SgsMain.Instance.players[json.player];
        //     CurrentPhase = json.phase;

        //     if (CurrentPhase == Phase.Prepare)
        //     {
        //         BeforeTurn?.Invoke();
        //         startPhaseView(this);
        //     }

        //     await ExecutePhase();

        //     if (CurrentPhase == Phase.End)
        //     {
        //         finishTurnView(this);
        //         AfterTurn?.Invoke();
        //     }

        //     SendNextPhase();
        //     Connection.Instance.IsRunning = false;
        // }

        // public int PerformTimer { get; private set; }

        private async Task ExecutePhase()
        {
            // 执行阶段开始时view事件
            startPhaseView?.Invoke(this);

#if UNITY_EDITOR
            await Task.Delay(300);
#endif

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

                    await new GetCardFromPile(CurrentPlayer, 2).Execute();
                    break;

                // 执行出牌阶段
                case Phase.Perform:
                    await Perform();
                    break;

                // 执行弃牌阶段
                case Phase.Discard:

                    var count = CurrentPlayer.HandCardCount - CurrentPlayer.HandCardLimit;
                    if (count > 0) await Discard.DiscardFromHand(CurrentPlayer, count);
                    // {
                    //     await TimerTask.Instance.Run(CurrentPlayer, TimerType.Discard, quantity);
                    // }
                    break;
            }

            // 执行阶段结束时事件
            await playerEvents.finishPhaseEvents[CurrentPhase].Execute();
            finishPhaseView?.Invoke(this);
        }

        private async Task Perform()
        {
            // 重置出杀次数
            CurrentPlayer.ShaCount = 0;
            // 重置使用技能次数
            foreach (var i in CurrentPlayer.skills.Values) if (i is Active) (i as Active).Time = 0;

            bool performIsDone = false;
            var timerTask = TimerTask.Instance;

            while (!performIsDone)
            {
                // 暂停线程,显示进度条
                timerTask.Hint = "出牌阶段，请选择一张牌。";
                performIsDone = !await timerTask.Run(CurrentPlayer, TimerType.PerformPhase);

                if (!performIsDone)
                {
                    if (timerTask.Skill != "" && CurrentPlayer.skills[timerTask.Skill] is Active)
                    {
                        var skill = CurrentPlayer.skills[timerTask.Skill] as Active;
                        await skill.Execute(timerTask.Dests, timerTask.Cards, "");
                    }
                    else
                    {
                        var card = timerTask.Cards[0];
                        await card.UseCard(CurrentPlayer, timerTask.Dests);
                    }
                }

                else if (CurrentPlayer.isAI && CardArea.UseSha(CurrentPlayer))
                {
                    foreach (var card in CurrentPlayer.HandCards)
                    {
                        if (card is 杀)
                        {
                            var dest = CurrentPlayer.Next;
                            do
                            {
                                if (DestArea.UseSha(CurrentPlayer, dest))
                                {
                                    await card.UseCard(CurrentPlayer, new List<Player> { dest });
                                    performIsDone = false;
                                    goto done;
                                    // break;
                                }
                                dest = dest.Next;
                            } while (dest != CurrentPlayer);

                            // foreach (var dest in SgsMain.Instance.players)
                            //     if (DestArea.UseSha(CurrentPlayer, dest))
                            //     {
                            //         await card.UseCard(CurrentPlayer, new List<Player> { dest });
                            //         performIsDone = false;
                            //         goto done;
                            //         // break;
                            //     }
                        }
                        if (card is Equipage)
                        {
                            await card.UseCard(CurrentPlayer);
                            performIsDone = false;
                            goto done;
                        }
                    }
                }
            done:
                finishPerformView?.Invoke(this);
            }
        }

        public Action BeforeTurn;
        public Action AfterTurn;

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