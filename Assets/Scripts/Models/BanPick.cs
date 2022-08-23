using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Threading.Tasks;
using System.Linq;

namespace Model
{
    public class BanPick : SingletonMono<BanPick>
    {
        public List<General> Pool { get; private set; }
        public Dictionary<Team, Dictionary<int, General>> TeamPool { get; private set; }

        public async Task Execute()
        {
            string url = Urls.JSON_URL + "general.json";
            List<General> json = JsonList<General>.FromJson(await WebRequest.GetString(url));

            if (Room.Instance.IsSingle)
            {
                Pool = json.OrderBy(x => Random.value).Take(12).ToList();
#if UNITY_EDITOR
                string name = "夏侯惇";
                General self = json.Find(x => x.name == name);
                if (!Pool.Contains(self)) Pool[11] = self;
#endif
            }
            else
            {
                Pool = new List<General>();
                foreach (var i in Room.Instance.Generals) Pool.Add(json[i]);

            }
            TeamPool = new Dictionary<Team, Dictionary<int, General>>
            {
                { Team.Blue, new Dictionary<int, General>() },
                { Team.Red, new Dictionary<int, General>() }
            };

            showPanelView();

            await Task.Yield();
            await Ban();
            if (SgsMain.Instance.GameIsOver) return;
            await Ban();
            if (SgsMain.Instance.GameIsOver) return;
            while (Pool.Count > 0)
            {
                await Pick();
                if (SgsMain.Instance.GameIsOver) return;
            }

            await SelfPick();
            if (SgsMain.Instance.GameIsOver) return;
        }

        private Team Not(Team t) => t == Team.Blue ? Team.Red : Team.Blue;

        private TaskCompletionSource<BanpickJson> bpTcs;
        private TaskCompletionSource<SelfpickJson> selfTcs;
        public Team Current { get; private set; } = Team.Red;
        public int second => 30;

        public async Task Ban()
        {
            if (User.Instance.team == Current || Room.Instance.IsSingle) StartCoroutine(BpAutoResult());
            startBanView();
            int id = await WaitBp();
            if (SgsMain.Instance.GameIsOver) return;
            var general = Pool.Find(x => x.id == id);
            Pool.Remove(general);
            while (banView is null) await Task.Yield();
            banView(id);
            StopAllCoroutines();
            Current = Not(Current);
        }

        public async Task Pick()
        {
            if (User.Instance.team == Current || Room.Instance.IsSingle) StartCoroutine(BpAutoResult());
            startTimerView();
            while (TeamPool[Current].Count < TeamPool[Not(Current)].Count + 1 && Pool.Count > 0)
            {
                int id = await WaitBp();
                if (SgsMain.Instance.GameIsOver) return;
                var general = Pool.Find(x => x.id == id);
                Pool.Remove(general);
                TeamPool[Current].Add(id, general);
                onPickView(id);
            }
            StopAllCoroutines();
            // await Task.Yield();
            Current = Not(Current);
        }

        public void SendBpResult(int general)
        {
            var json = new BanpickJson();
            json.eventname = "ban_pick_result";
            json.id = Wss.Instance.Count + 1;
            json.general = general;

            if (Room.Instance.IsSingle) bpTcs.TrySetResult(json);
            else Wss.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
        }

        public async Task<int> WaitBp()
        {
            BanpickJson json;
            if (Room.Instance.IsSingle)
            {
                bpTcs = new TaskCompletionSource<BanpickJson>();
                json = await bpTcs.Task;
            }
            else
            {
                var msg = await Wss.Instance.PopSgsMsg();
                json = JsonUtility.FromJson<BanpickJson>(msg);
            }

            if (SgsMain.Instance.GameIsOver) return 0;

            return json.general;
        }

        private IEnumerator BpAutoResult()
        {
            yield return new WaitForSeconds(Current == User.Instance.team ? second : 1);
            int n = Mathf.Min(TeamPool[Not(Current)].Count - TeamPool[Current].Count + 1, Pool.Count);
            for (int i = 0; i < n; i++) SendBpResult(Pool[0].id);
        }

        public async Task SelfPick()
        {
            // await Task.Yield();
            selfPickView();
            StartCoroutine(SelfAutoResult());
            if (Room.Instance.IsSingle) StartCoroutine(AiAutoResult());
            await WaitSelfPick();
            StopAllCoroutines();
        }

        public void SendSelfResult(Team team, int general0, int general1)
        {
            var json = new SelfpickJson();
            json.eventname = "self_pick_result";
            json.id = Wss.Instance.Count + 1;
            json.team = team;
            json.general0 = general0;
            json.general1 = general1;

            if (Room.Instance.IsSingle) selfTcs.TrySetResult(json);
            else Wss.Instance.SendWebSocketMessage(JsonUtility.ToJson(json));
        }

        private bool done = false;

        public async Task WaitSelfPick()
        {
            SelfpickJson json;
            if (Room.Instance.IsSingle)
            {
                selfTcs = new TaskCompletionSource<SelfpickJson>();
                json = await selfTcs.Task;
            }
            else
            {
                var msg = await Wss.Instance.PopSgsMsg();
                json = JsonUtility.FromJson<SelfpickJson>(msg);
            }

            if (SgsMain.Instance.GameIsOver) return;

            if (json.team == Team.Blue)
            {
                SgsMain.Instance.players[3].InitGeneral(TeamPool[json.team][json.general0]);
                SgsMain.Instance.players[0].InitGeneral(TeamPool[json.team][json.general1]);
            }
            else
            {
                SgsMain.Instance.players[1].InitGeneral(TeamPool[json.team][json.general0]);
                SgsMain.Instance.players[2].InitGeneral(TeamPool[json.team][json.general1]);
            }

            if (!done)
            {
                done = true;
                Wss.Instance.Count--;
                await WaitSelfPick();
            }
        }

        private IEnumerator SelfAutoResult()
        {
            yield return new WaitForSeconds(second);

            var team = User.Instance.team;
            var list = TeamPool[team].Values.ToList();
            var general0 = list[Random.Range(0, list.Count)];
            list.Remove(general0);
            var general1 = list[Random.Range(0, list.Count)];
            SendSelfResult(team, general0.id, general1.id);
        }

        private IEnumerator AiAutoResult()
        {
            yield return new WaitForSeconds(1);

            var team = Not(User.Instance.team);
            var list = TeamPool[team].Values.ToList();
            var general0 = list[Random.Range(0, list.Count)];
            list.Remove(general0);
            var general1 = list[Random.Range(0, list.Count)];
            SendSelfResult(team, general0.id, general1.id);
        }

        private UnityAction showPanelView;

        private UnityAction startBanView;
        private UnityAction<int> banView;
        private UnityAction startTimerView;
        private UnityAction<int> onPickView;
        private UnityAction selfPickView;

        public event UnityAction ShowPanelView
        {
            add => showPanelView += value;
            remove => showPanelView -= value;
        }
        public event UnityAction StartTimerView
        {
            add => startTimerView += value;
            remove => startTimerView -= value;
        }
        public event UnityAction<int> OnPickView
        {
            add => onPickView += value;
            remove => onPickView -= value;
        }
        public event UnityAction SelfPickView
        {
            add => selfPickView += value;
            remove => selfPickView -= value;
        }
        public event UnityAction StartBanView
        {
            add => startBanView += value;
            remove => startBanView -= value;
        }
        public event UnityAction<int> BanView
        {
            add => banView += value;
            remove => banView -= value;
        }
    }
}