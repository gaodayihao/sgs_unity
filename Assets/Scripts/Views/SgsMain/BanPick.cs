using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace View
{
    public class BanPick : SingletonMono<BanPick>
    {
        public List<General> Pool;
        public List<General> OppoPoolList;
        public GameObject poolObj;
        public GameObject SelfPool;
        public GameObject OppoPool;
        public GameObject selfPick;
        // 倒计时读条
        public Slider timer;
        // 提示
        public Text hint;

        private Model.BanPick model => Model.BanPick.Instance;
        private Team team => Model.User.Instance.team;

        // Start is called before the first frame update
        void Start()
        {
            Pool = new List<General>();
            OppoPoolList = new List<General>();
            for (int i = 0; i < 12; i++)
            {
                Pool.Add(GameObject.Find("General" + i.ToString()).GetComponent<General>());
                Pool[i].Init(Model.BanPick.Instance.Pool[i]);
            }
        }

        public void ShowTimer()
        {
            StopAllCoroutines();
            StartCoroutine(StartTimer(model.second));
            // Debug.Log(model.Current);
            // Debug.Log(team);
            if (model.Current == team)
            {
                // timer.gameObject.SetActive(true);
                foreach (var i in Pool) i.button.interactable = true;
                hint.text = "请点击选择武将";
            }
            else
            {
                foreach (var i in Pool) i.button.interactable = false;
                hint.text = "等待对方选将";
            }
        }

        public void OnPick(int id)
        {
            var general = Pool.Find(x => x.Id == id);
            if (general is null)
            {
                StopAllCoroutines();
                return;
            }
            general.button.interactable = false;
            Pool.Remove(general);
            if (model.Current == team)
            {
                general.transform.SetParent(SelfPool.transform, false);
            }
            else
            {
                OppoPoolList.Add(general);
                general.OnPick(model.Current);
            }
        }

        public void StartBan()
        {
            StopAllCoroutines();
            StartCoroutine(StartTimer(model.second));

            if (model.Current == team)
            {
                foreach (var i in Pool) i.button.interactable = true;
                hint.text = "请点击禁用武将";
            }
            else
            {
                foreach (var i in Pool) i.button.interactable = false;
                hint.text = "等待对方禁将";
            }
        }

        public void OnBan(int id)
        {
            var general = Pool.Find(x => x.Id == id);
            if (general is null)
            {
                StopAllCoroutines();
                return;
            }
            general.button.interactable = false;
            Pool.Remove(general);
            general.generalImage.color = new Color(0.4f, 0.4f, 0.4f);
        }

        public General general0;
        public General general1;
        public Transform seat0;
        public Transform seat1;
        public Image pos0;
        public Image pos1;
        public Button button;
        // public GameObject border;

        public async void SelfPick()
        {
            foreach (var i in OppoPoolList) i.transform.SetParent(OppoPool.transform);

            hint.text = "请选择己方要出场的武将";
            OppoPool.SetActive(true);
            poolObj.SetActive(false);
            var tsf = SelfPool.GetComponent<RectTransform>();
            tsf.anchorMin = new Vector2(0.5f, 1);
            tsf.anchorMax = new Vector2(0.5f, 1);
            tsf.pivot = new Vector2(0.5f, 1);
            tsf.anchoredPosition = new Vector2(0, -250);

            selfPick.SetActive(true);
            int current = Time.frameCount;
            while (Time.frameCount == current) await Task.Yield();
            SelfPool.GetComponent<HorizontalLayoutGroup>().enabled = false;
            foreach (Transform i in SelfPool.transform) i.GetComponent<General>().SelfPick();

            var posSprites = Sprites.Instance.position;
            pos0.sprite = team == Team.Blue ? posSprites[3] : posSprites[1];
            pos1.sprite = team == Team.Blue ? posSprites[0] : posSprites[2];

            button.onClick.AddListener(OnClick);
        }

        public void UpdateButton()
        {
            button.gameObject.SetActive(general0 != null && general1 != null);
        }

        public void OnClick()
        {
            model.SendSelfResult(team, general0.Id, general1.Id);
            Destroy(gameObject);
        }

        public void Destroy()
        {
            Destroy(gameObject);
        }

        private IEnumerator StartTimer(int second)
        {
            timer.value = 1;
            while (timer.value > 0)
            {
                timer.value -= Time.deltaTime / second;
                yield return null;
            }
        }
    }
}
