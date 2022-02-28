using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Model
{
    [Serializable]
    public class General
    {
        // 编号
        public int id;
        // 势力
        public string nation;
        // 姓名
        public string name;
        // 性别
        public bool gender;
        // 体力上限
        public int hp_limit;
        // 技能
        // 皮肤
        public List<Skin> skin;
    }

    [Serializable]
    public class Skin
    {
        public string name;
        public int id;
    }
}