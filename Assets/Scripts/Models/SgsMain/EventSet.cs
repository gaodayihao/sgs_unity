using System.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    /// <summary>
    /// 一个操作或阶段所触发的操作请求集合
    /// </summary>
    public class EventSet
    {
        /// <summary>
        /// 所属玩家
        /// </summary>
        // private Player player;

        /// <summary>
        /// 用字典维护每名玩家的操作请求
        /// </summary>
        protected Dictionary<Player, List<Func<Task<bool>>>> eventDic;

        // public EventSet(Player player)
        // {
        //     this.player = player;
        //     // eventDic = new Dictionary<Player, List<T>>();
        // }

        /// <summary>
        /// 添加操作请求
        /// </summary>
        public void AddEvent(Player player, Func<Task<bool>> request)
        {
            if (eventDic == null)
                eventDic = new Dictionary<Player, List<Func<Task<bool>>>>();

            if (!eventDic.ContainsKey(player))
                eventDic.Add(player, new List<Func<Task<bool>>>());

            eventDic[player].Add(request);
        }

        /// <summary>
        /// 删除操作请求
        /// </summary>
        public void RemoveEvent(Player player, Func<Task<bool>> request)
        {
            if (!eventDic.ContainsKey(player)) return;

            if (eventDic[player].Contains(request))
                eventDic[player].Remove(request);

            if (eventDic[player].Count == 0)
                eventDic.Remove(player);
        }
        // 询问请求

        public async Task Execute()
        {
            if (eventDic == null) return;

            Player player = TurnSystem.Instance.CurrentPlayer;
            do
            {
                if (eventDic.ContainsKey(player))
                {
                    if (eventDic[player].Count == 1)
                    {
                        var breakAsking = await eventDic[player][0]();
                        if (breakAsking) break;
                    }
                }
                player = player.Next;
            } while (player != TurnSystem.Instance.CurrentPlayer);
        }
    }

    public class EventSet<T>
    {
        /// <summary>
        /// 所属玩家
        /// </summary>
        // private Player player;

        /// <summary>
        /// 用字典维护每名玩家的操作请求
        /// </summary>
        protected Dictionary<Player, List<Func<T, Task<bool>>>> eventDic;

        // public EventSet(Player player)
        // {
        //     this.player = player;
        //     // eventDic = new Dictionary<Player, List<T>>();
        // }

        /// <summary>
        /// 添加操作请求
        /// </summary>
        public void AddEvent(Player player, Func<T, Task<bool>> request)
        {
            if (eventDic == null)
                eventDic = new Dictionary<Player, List<Func<T, Task<bool>>>>();

            if (!eventDic.ContainsKey(player))
                eventDic.Add(player, new List<Func<T, Task<bool>>>());

            eventDic[player].Add(request);
        }

        /// <summary>
        /// 删除操作请求
        /// </summary>
        public void RemoveEvent(Player player, Func<T, Task<bool>> request)
        {
            if (!eventDic.ContainsKey(player)) return;

            if (eventDic[player].Contains(request))
                eventDic[player].Remove(request);

            if (eventDic[player].Count == 0)
                eventDic.Remove(player);
        }
        // 询问请求

        public async Task Execute(T param)
        {
            if (eventDic == null) return;

            Player player = TurnSystem.Instance.CurrentPlayer;
            do
            {
                if (eventDic.ContainsKey(player))
                {
                    if (eventDic[player].Count == 1)
                    {
                        var Continue = await eventDic[player][0](param);
                        if (!Continue) break;
                    }
                }
                player = player.Next;
            } while (player != TurnSystem.Instance.CurrentPlayer);
        }
    }
}
