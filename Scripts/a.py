import os
import json

audio_dir = "Assets/StreamingAssets/Audio/skin/"


def voice(dir, voice):
    s = "https://web.sanguosha.com/10/pc/res/assets/runtime/voice/skin/"

    for i in voice:
        for j in i["url"]:
            b = False
            for relpath, dirs, files in os.walk(audio_dir + dir):
                if j + ".mp3" in files:
                    b = True
                    break
            if not b:
                os.system(
                    "wget -P " + audio_dir + dir + " " + s + dir + "/" + j + ".mp3"
                )

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
data["id"] = 529802
data["name"] = "炙炎烽火"
data["voice"] = [
    {
        "name": "陷阵",
        "url": ["GaoShun_XianZhen_01", "GaoShun_XianZhen_01"],
    },
    {
        "name": "禁酒",
        "url": ["GaoShun_JinJiu_01", "GaoShun_JinJiu_02"],
    },
    # {
    #     "name": "咆哮",
    #     "url": ["XinGuanXingZhangBao_PaoXiao_01", "XinGuanXingZhangBao_PaoXiao_02"],
    # },
    {"name": "阵亡", "url": ["GaoShun_Dead"]},
]

dir = "gaoshun07"
voice(dir, data["voice"])
# original(data["voice"])
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
