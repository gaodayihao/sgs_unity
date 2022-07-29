using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace View
{
    public class Sprites : Singleton<Sprites>
    {
        // 从assetbundle中加载的sprite数组
        public Sprite[] seat;

        public Sprite[] selfseat;

        public Sprite[] card;


        // seat
        public Sprite[] position;
        // 每阶段对应sprite
        public Dictionary<Phase, Sprite> phase;
        // 手牌数
        public Sprite[] handCardCount;
        // 阴阳鱼
        public Sprite[] yinYangYu;
        // 装备
        public Dictionary<string, Sprite> seat_equip;
        // 花色
        public Dictionary<string, Sprite> seat_suit;
        // 黑色点数
        public Dictionary<int, Sprite> seat_blackWeight;
        // 红色点数
        public Dictionary<int, Sprite> seat_redWeight;
        // 判定牌
        public Dictionary<string, Sprite> judgeCard;
        // 势力边框
        public Dictionary<string, Sprite> nationBack;
        // 势力
        public Dictionary<string, Sprite> nation;
        // 身份
        public Sprite[] camp;
        // 阵亡
        public Sprite[] deadText;

        // selfseat
        // 每阶段对应sprite
        public Dictionary<Phase, Sprite> self_phase;
        // 技能
        // public Sprite[,,] skills;

        // card
        // public Sprite[] cardImage;
        // public Sprite[] cardSuit;
        // public Sprite[] cardWeight;
        public Dictionary<string, Sprite> cardImage;
        public Dictionary<string, Sprite> cardSuit;
        public Dictionary<int, Sprite> blackWeight;
        public Dictionary<int, Sprite> redWeight;

        public Dictionary<string, Sprite> equipImage;

        public Sprites()
        {
            // 初始化sprites
            AssetBundle assetBundle = ABManager.Instance.ABMap["sprite"];


            // #if UNITY_EDITOR
            //             if (ABManager.Instance.ABMap.ContainsKey("sprite")) assetBundle = ABManager.Instance.ABMap["sprite"];
            //             else assetBundle = AssetBundle.LoadFromFile(Application.dataPath + "/../AssetBundles/WebGL/sprite");
            // #else
            //             assetBundle = ABManager.Instance.ABMap["sprite"];
            // #endif

            seat = assetBundle.LoadAssetWithSubAssets<Sprite>("seat");
            selfseat = assetBundle.LoadAssetWithSubAssets<Sprite>("selfseat");
            card = assetBundle.LoadAssetWithSubAssets<Sprite>("card");

            // player

            // 位置
            position = new Sprite[8];
            // 一号位
            position[0] = seat[227];
            // 二号位
            position[1] = seat[247];
            // 三号位
            position[2] = seat[271];
            // 四号位
            position[3] = seat[226];
            // 五号位
            position[4] = seat[230];
            // 六号位
            position[5] = seat[225];
            // 七号位
            position[6] = seat[246];
            // 八号位
            position[7] = seat[224];

            // 阶段信息
            phase = new Dictionary<Phase, Sprite>();
            // 准备阶段
            phase.Add(Phase.Prepare, seat[223]);
            // 判定阶段
            phase.Add(Phase.Judge, seat[213]);
            // 摸牌阶段
            phase.Add(Phase.Get, seat[211]);
            // 出牌阶段
            phase.Add(Phase.Perform, seat[210]);
            // 弃牌阶段
            phase.Add(Phase.Discard, seat[208]);
            // 结束阶段
            phase.Add(Phase.End, seat[207]);

            // 手牌数
            handCardCount = new Sprite[10];
            handCardCount[0] = seat[169];
            handCardCount[1] = seat[21];
            handCardCount[2] = seat[23];
            handCardCount[3] = seat[26];
            handCardCount[4] = seat[28];
            handCardCount[5] = seat[237];
            handCardCount[6] = seat[141];
            handCardCount[7] = seat[135];
            handCardCount[8] = seat[127];
            handCardCount[9] = seat[104];

            // 装备
            seat_equip = new Dictionary<string, Sprite>
            {
                {"青釭剑", seat[58]},
                {"藤甲", seat[84]},
                {"丈八蛇矛", seat[137]},
                {"大宛", seat[138]},
                {"爪黄飞电", seat[143]},
                {"的卢", seat[144]},
                {"诸葛连弩", seat[152]},
                {"方天画戟", seat[154]},
                {"朱雀羽扇", seat[157]},
                {"紫骍", seat[158]},
                {"贯石斧", seat[159]},
                {"八卦阵", seat[173]},
                {"白银狮子", seat[174]},
                {"赤兔", seat[175]},
                {"雌雄双股剑", seat[176]},
                {"寒冰剑", seat[177]},
                {"骅骝", seat[182]},
                {"绝影", seat[189]},
                {"麒麟弓", seat[191]},
                {"青龙偃月刀", seat[192]},
                {"仁王盾", seat[199]},
            };

            seat_suit = new Dictionary<string, Sprite>
            {
                {"黑桃", seat[10]},
                {"红桃", seat[221]},
                {"草花", seat[219]},
                {"方片", seat[220]},
            };

            seat_blackWeight = new Dictionary<int, Sprite>
            {
                {1, seat[293]},
                {2, seat[281]},
                {3, seat[258]},
                {4, seat[280]},
                {5, seat[279]},
                {6, seat[264]},
                {7, seat[263]},
                {8, seat[262]},
                {9, seat[261]},
                {10, seat[286]},
                {11, seat[292]},
                {12, seat[148]},
                {13, seat[295]},
            };

            seat_redWeight = new Dictionary<int, Sprite>
            {
                {1, seat[232]},
                {2, seat[259]},
                {3, seat[260]},
                {4, seat[236]},
                {5, seat[162]},
                {6, seat[170]},
                {7, seat[287]},
                {8, seat[288]},
                {9, seat[289]},
                {10, seat[161]},
                {11, seat[233]},
                {12, seat[235]},
                {13, seat[234]},
            };

            // 阴阳鱼
            yinYangYu = new Sprite[4];
            // 黑
            yinYangYu[0] = seat[297];
            // 红
            yinYangYu[1] = seat[298];
            // 黄
            yinYangYu[2] = seat[265];
            // 绿
            yinYangYu[3] = seat[11];

            // 判定牌
            judgeCard = new Dictionary<string, Sprite>
            {
                {"乐不思蜀", seat[18]},
                {"闪电", seat[9]},
                {"兵粮寸断", seat[124]},
            };

            // 势力边框
            nationBack = new Dictionary<string, Sprite>
            {
                {"蜀", seat[180]},
                {"吴", seat[179]},
                {"魏", seat[122]},
                {"群", seat[178]}
            };

            // 势力
            nation = new Dictionary<string, Sprite>
            {
                {"蜀", seat[40]},
                {"吴", seat[17]},
                {"魏", seat[69]},
                {"群", seat[68]}
            };

            camp = new Sprite[] { seat[57], seat[131] };
            deadText = new Sprite[] { seat[3], seat[16] };

            // self

            // 阶段信息
            self_phase = new Dictionary<Phase, Sprite>();
            // 准备阶段
            self_phase.Add(Phase.Prepare, selfseat[132]);
            // 判定阶段
            self_phase.Add(Phase.Judge, selfseat[131]);
            // 摸牌阶段
            self_phase.Add(Phase.Get, selfseat[126]);
            // 出牌阶段
            self_phase.Add(Phase.Perform, selfseat[117]);
            // 弃牌阶段
            self_phase.Add(Phase.Discard, selfseat[116]);
            // 结束阶段
            self_phase.Add(Phase.End, selfseat[128]);

            // 技能
            // skills = new Sprite[4, 4, 2]
            // {
            //     // 主动技
            //     {
            //         // normal
            //         {selfseat[0], selfseat[0]},
            //         // highlighted
            //         {selfseat[0], selfseat[0]},
            //         // pressed
            //         {selfseat[0], selfseat[0]},
            //         // disabled
            //         {selfseat[0], selfseat[0]}
            //     },
            //     {
            //         // 锁定技
            //         {selfseat[0], selfseat[0]},
            //         {selfseat[0], selfseat[0]},
            //         {selfseat[0], selfseat[0]},
            //         {selfseat[0], selfseat[0]}
            //     },
            //     {
            //         // 限定技
            //         {selfseat[0], selfseat[0]},
            //         {selfseat[0], selfseat[0]},
            //         {selfseat[0], selfseat[0]},
            //         {selfseat[0], selfseat[0]}
            //     },
            //     {
            //         // 觉醒技
            //         {selfseat[0], selfseat[0]},
            //         {selfseat[0], selfseat[0]},
            //         {selfseat[0], selfseat[0]},
            //         {selfseat[0], selfseat[0]}
            //     }
            // };


            // cardImage = new Sprite[100];
            // cardSuit = new Sprite[100];
            // cardWeight = new Sprite[100];

            // card

            equipImage = new Dictionary<string, Sprite>
            {
                {"寒冰剑", card[133]},
                {"的卢", card[134]},
                {"白银狮子", card[135]},
                {"丈八蛇矛", card[140]},
                {"诸葛连弩", card[141]},
                {"紫骍", card[142]},
                {"青釭剑", card[143]},
                {"绝影", card[144]},
                {"古锭刀", card[148]},
                {"大宛", card[149]},
                {"八卦阵", card[150]},
                {"仁王盾", card[151]},
                {"麒麟弓", card[159]},
                {"骅骝", card[161]},
                {"贯石斧", card[162]},
                {"朱雀羽扇", card[163]},
                {"雌雄双股剑", card[164]},
                {"爪黄飞电", card[169]},
                {"青龙偃月刀", card[171]},
                {"方天画戟", card[184]},
                {"赤兔", card[185]},
                {"藤甲", card[188]}
            };

            cardSuit = new Dictionary<string, Sprite>
            {
                {"黑桃", card[182]},
                {"红桃", card[181]},
                {"草花", card[200]},
                {"方片", card[180]}
            };

            blackWeight = new Dictionary<int, Sprite>
            {
                {1, card[201]},
                {2, card[205]},
                {3, card[198]},
                {4, card[197]},
                {5, card[196]},
                {6, card[195]},
                {7, card[194]},
                {8, card[199]},
                {9, card[202]},
                {10, card[201]},
                {11, card[178]},
                {12, card[211]},
                {13, card[208]},
            };

            redWeight = new Dictionary<int, Sprite>
            {
                {1, card[206]},
                {2, card[177]},
                {3, card[210]},
                {4, card[176]},
                {5, card[209]},
                {6, card[175]},
                {7, card[193]},
                {8, card[203]},
                {9, card[172]},
                {10, card[179]},
                {11, card[173]},
                {12, card[174]},
                {13, card[207]},
            };

            cardImage = new Dictionary<string, Sprite>
            {
                {"青龙偃月刀", card[0]},
                // {"青龙偃月刀",card[1]},
                {"仁王盾", card[2]},
                // {"青龙偃月刀",card[3]},
                // {"青龙偃月刀",card[4]},
                {"借刀杀人", card[5]},
                {"雷杀", card[6]},
                // {"青龙偃月刀",card[7]},
                // {"青龙偃月刀",card[8]},
                // {"青龙偃月刀",card[9]},
                // {"青龙偃月刀",card[10]},
                // {"青龙偃月刀",card[11]},
                // {"青龙偃月刀",card[12]},
                // {"青龙偃月刀",card[13]},
                {"无中生有", card[14]},
                // {"青龙偃月刀",card[15]},
                // {"青龙偃月刀",card[16]},
                // {"青龙偃月刀",card[17]},
                {"火杀", card[18]},
                {"寒冰剑", card[19]},
                // {"青龙偃月刀",card[20]},
                // {"青龙偃月刀",card[21]},
                // {"青龙偃月刀",card[22]},
                {"诸葛连弩", card[23]},
                // {"青龙偃月刀",card[24]},
                {"朱雀羽扇", card[25]},
                {"紫骍", card[26]},
                {"铁索连环", card[27]},
                // {"青龙偃月刀",card[28]},
                // {"青龙偃月刀",card[29]},
                // {"青龙偃月刀",card[30]},
                // {"青龙偃月刀",card[31]},
                // {"青龙偃月刀",card[32]},
                {"方天画戟", card[33]},
                // {"青龙偃月刀",card[34]},
                // {"青龙偃月刀",card[35]},
                // {"青龙偃月刀",card[36]},
                {"丈八蛇矛", card[37]},
                // {"青龙偃月刀",card[38]},
                {"桃园结义", card[39]},
                {"藤甲", card[40]},
                // {"青龙偃月刀",card[41]},
                // {"青龙偃月刀",card[42]},
                // {"青龙偃月刀",card[43]},
                // {"青龙偃月刀",card[44]},
                // {"青龙偃月刀",card[45]},
                {"麒麟弓", card[46]},
                // {"青龙偃月刀",card[47]},
                {"贯石斧", card[48]},
                {"骅骝", card[49]},
                {"酒", card[50]},
                // {"青龙偃月刀",card[51]},
                // {"青龙偃月刀",card[52]},
                {"顺手牵羊", card[53]},
                // {"青龙偃月刀",card[54]},
                // {"青龙偃月刀",card[55]},
                {"五谷丰登", card[56]},
                // {"青龙偃月刀",card[57]},
                {"无懈可击", card[58]},
                // {"青龙偃月刀",card[59]},
                // {"青龙偃月刀",card[60]},
                {"青釭剑", card[61]},
                // {"青龙偃月刀",card[62]},
                {"古锭刀", card[63]},
                // {"青龙偃月刀",card[64]},
                // {"青龙偃月刀",card[65]},
                // {"青龙偃月刀",card[66]},
                // {"青龙偃月刀",card[67]},
                // {"青龙偃月刀",card[68]},
                // {"青龙偃月刀",card[69]},
                // {"青龙偃月刀",card[70]},
                {"的卢", card[71]},
                {"大宛", card[72]},
                // {"青龙偃月刀",card[73]},
                {"雌雄双股剑", card[74]},
                // {"青龙偃月刀",card[75]},
                // {"青龙偃月刀",card[76]},
                {"杀", card[77]},
                {"过河拆桥", card[78]},
                {"火攻", card[79]},
                {"决斗", card[80]},
                // {"青龙偃月刀",card[81]},
                // {"青龙偃月刀",card[82]},
                // {"青龙偃月刀",card[83]},
                {"万箭齐发", card[84]},
                {"未知牌", card[85]},
                // {"青龙偃月刀",card[86]},
                // {"青龙偃月刀",card[87]},
                // {"青龙偃月刀",card[88]},
                // {"青龙偃月刀",card[89]},
                {"南蛮入侵", card[90]},
                // {"青龙偃月刀",card[91]},
                {"闪", card[92]},
                // {"青龙偃月刀",card[93]},
                // {"青龙偃月刀",card[94]},
                {"绝影", card[95]},
                // {"青龙偃月刀",card[96]},
                {"爪黄飞电", card[97]},
                {"桃", card[98]},
                // {"青龙偃月刀",card[99]},
                // {"青龙偃月刀",card[100]},
                {"赤兔", card[101]},
                {"兵粮寸断", card[102]},
                {"白银狮子", card[103]},
                // {"青龙偃月刀",card[104]},
                // {"青龙偃月刀",card[105]},
                // {"青龙偃月刀",card[106]},
                {"闪电", card[107]},
                // {"青龙偃月刀",card[108]},
                // {"青龙偃月刀",card[109]},
                {"乐不思蜀", card[110]},
                // {"青龙偃月刀",card[111]},
                // {"青龙偃月刀",card[112]},
                // {"青龙偃月刀",card[113]},
                // {"青龙偃月刀",card[114]},
                // {"青龙偃月刀",card[115]},
                // {"青龙偃月刀",card[116]},
                // {"青龙偃月刀",card[117]},
                {"八卦阵", card[118]},
            };
        }

        public void InitCard(List<Model.Card> model)
        {


            // for (int i = 1; i < model.Count; i++)
            // {
            //     cardSuit[i] = suitMap[model[i].Suit];
            //     if (model[i].Suit == "黑桃" || model[i].Suit == "草花") cardWeight[i] = blackWeightMap[model[i].Weight];
            //     else cardWeight[i] = redWeightMap[model[i].Weight];

            //     // if(!imageMap.ContainsKey(model[i].Name)) Debug.Log(model[i].Name);
            //     cardImage[i] = imageMap[model[i].Name];
            // }

            // var a = new JsonDictionary<string, int>();
            // a.dictionary = new Dictionary<string, int>();
            // a.dictionary.Add("aa", 1);
            // Debug.Log(System.Text.jso);

        }
    }



}