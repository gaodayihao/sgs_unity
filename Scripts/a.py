import os
import json

audio_dir = "Assets/StreamingAssets/Audio/skin/"


def voice(dir, voice):
    s = "https://web.sanguosha.com/10/pc/res/assets/runtime/voice/skin/"

    for i in voice:
        for j in i["url"]:
            os.system("wget -P " + audio_dir + dir + " " + s + dir + "/" + j + ".mp3")

        for j in range(len(i["url"])):
            i["url"][j] = dir + "/" + i["url"][j]


def original(voice):
    s = "https://web.sanguosha.com/10/pc/res/assets/runtime/voice/skin/"

    for i in voice:
        if i["name"] == "阵亡":
            for j in i["url"]:
                os.system("wget -P " + audio_dir + "dead " + s + "dead/" + j + ".mp3")

            for j in range(len(i["url"])):
                i["url"][j] = "dead/" + i["url"][j]

        else:
            for j in i["url"]:
                os.system(
                    "wget -P " + audio_dir + "original " + s + "1-spell/" + j + ".mp3"
                )

            for j in range(len(i["url"])):
                i["url"][j] = "original/" + i["url"][j]


data = {}
data["id"] = 529702
data["name"] = "公台献城"
data["voice"] = [
    {"name": "明策", "url": ["ChenGong_MingCe_01", "ChenGong_MingCe_02"]},
    {"name": "智迟", "url": ["ChenGong_ZhiChi_01", "ChenGong_ZhiChi_02"]},
    {"name": "阵亡", "url": ["ChenGong_Dead"]},
]

dir = "chengong04"
voice(dir, data["voice"])
# original(data['voice'])
image_dir = "Assets/StreamingAssets/Image/General/"
os.system(
    "wget -P "
    + image_dir
    + "Seat https://web.sanguosha.com/10/pc/res/assets/runtime/general/seat/static/"
    + str(data["id"])
    + ".png"
)
os.system(
    "wget -P "
    + image_dir
    + "Window https://web.sanguosha.com/10/pc/res/assets/runtime/general/window/"
    + str(data["id"])
    + ".png"
)

print(json.dumps(data, ensure_ascii=False))
# print(json.dumps(data, ensure_ascii=False).encode('utf8').decode())
