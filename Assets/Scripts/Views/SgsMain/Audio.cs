using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;

namespace View
{
    public class Audio : MonoBehaviour
    {
        public AudioSource effect;
        // private Dictionary<string, int> urls;
        private Dictionary<System.Type, int> urls;

        // Start is called before the first frame update
        void Start()
        {
            // urls = new Dictionary<string, int>
            // {
            //     { "杀", 1 },
            //     { "闪", 2 },
            //     { "桃", 3 },
            //     { "顺手牵羊", 4 },
            //     { "过河拆桥", 5 },
            //     { "五谷丰登", 6 },
            //     { "无中生有", 7 },
            //     { "决斗", 8 },
            //     { "南蛮入侵", 9 },
            //     { "万箭齐发", 10 },
            //     { "闪电", 11 },
            //     { "桃园结义", 12 },
            //     { "无懈可击", 13 },
            //     { "借刀杀人", 14 },
            //     { "乐不思蜀", 15 }
            // };

            urls = new Dictionary<System.Type, int>
            {
                { typeof(Model.杀), 1 },
                { typeof(Model.闪), 2 },
                { typeof(Model.桃), 3 },
                { typeof(Model.顺手牵羊), 4 },
                { typeof(Model.过河拆桥), 5 },
                // { typeof(Model.五谷丰登), 6 },
                { typeof(Model.无中生有), 7 },
                { typeof(Model.决斗), 8 },
                { typeof(Model.南蛮入侵), 9 },
                { typeof(Model.万箭齐发), 10 },
                // { "闪电", 11 },
                { typeof(Model.桃园结义), 12 },
                { typeof(Model.无懈可击), 13 },
                // { "借刀杀人", 14 },
                { typeof(Model.乐不思蜀), 15 },
                { typeof(Model.Weapon), 16 },
                { typeof(Model.Armor), 16 },
                { typeof(Model.SubHorse), 17 },
                { typeof(Model.PlusHorse), 17 }
            };
        }

        public async void CardVoice(Model.Card card)
        {
            string url = Urls.AUDIO_URL + "spell/";

            if (!urls.ContainsKey(card.GetType()))
            {
                if (card is Model.Weapon || card is Model.Armor)
                {
                    url += "equipArmor.mp3";
                }
                else if (card is Model.SubHorse || card is Model.PlusHorse)
                {
                    url += "equipHorse.mp3";
                }
                else return;
            }
            else
            {
                int gender = card.Src.general.gender ? 1 : 2;
                url += "spell" + urls[card.GetType()] + "_" + gender + ".mp3";
            }

            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            www.SendWebRequest();

            while (!www.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                return;
            }

            var audioSource = gameObject.AddComponent<AudioSource>();
            var clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
            Destroy(audioSource, clip.length);
        }

        public async void Damage(Model.UpdateHp model)
        {
            if (!(model is Model.Damaged)) return;
            string url = Urls.AUDIO_URL + "spell/hurtSound.mp3";
            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            www.SendWebRequest();

            while (!www.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                return;
            }

            var audioSource = gameObject.AddComponent<AudioSource>();
            var clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
            Destroy(audioSource, clip.length);
        }

        public async void SkillVoice(Model.Skill model)
        {
            var player = FindObjectOfType<SgsMain>().players[model.Src.Position].GetComponent<Player>();
            var urls = player.voices[player.CurrentSkin.id][model.Name];
            string url = Urls.AUDIO_URL + "skin/" + urls[Random.Range(0, urls.Count)] + ".mp3";

            UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.MPEG);
            www.SendWebRequest();

            while (!www.isDone) await Task.Yield();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                Debug.Log(url);
                return;
            }

            var audioSource = gameObject.AddComponent<AudioSource>();
            var clip = DownloadHandlerAudioClip.GetContent(www);
            audioSource.clip = clip;
            audioSource.Play();
            Destroy(audioSource, clip.length);

            // effect.clip = DownloadHandlerAudioClip.GetContent(www);
            // effect.Play();
        }

        // private IEnumerable AutoDestroy(AudioSource audioSource)
        // {
        //     var s=audioSource.clip.length;
        //     yield return new WaitForSeconds(s);
        //     Destroy(audioSource,);
        // }
    }
}