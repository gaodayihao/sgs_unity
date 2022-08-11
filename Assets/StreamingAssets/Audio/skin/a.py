import os
import json


def voice(dir, voice):
    s = 'https://web.sanguosha.com/10/pc/res/assets/runtime/voice/skin/'

    for i in voice:
        for j in i['url']:
            os.system('wget -P ' + dir + ' ' + s + dir + '/' + j + '.mp3')

        for j in range(len(i['url'])):
            i['url'][j] = dir+'/'+i['url'][j]


def original(voice):
    s = 'https://web.sanguosha.com/10/pc/res/assets/runtime/voice/skin/'

    for i in voice:
        if i['name'] == '阵亡':
            for j in i['url']:
                os.system('wget -P dead ' + s + 'dead/' + j + '.mp3')

            for j in range(len(i['url'])):
                i['url'][j] = 'dead/'+i['url'][j]

        else:
            for j in i['url']:
                os.system('wget -P original ' + s + '1-spell/' + j + '.mp3')

            for j in range(len(i['url'])):
                i['url'][j] = 'original/'+i['url'][j]


data = {}
data['id'] = 508301
data['name'] = '如火燎原'
data['voice'] = [
    {
        'name': '无双',
        'url': ['LvBuPo_WuShuang_01', 'LvBuPo_WuShuang_02']
    },
    {
        'name': '利驭',
        'url': ['LvBuPo_LiYu_01', 'LvBuPo_LiYu_02']
    },
    {
        'name': '阵亡',
        'url': ['LvBuPo_Dead']
    }
]

dir = 'lvbupo02'
voice(dir, data['voice'])
# original(data['voice'])
general='025'
# os.system('wget -P ../../image/general/'+general+' https://web.sanguosha.com/10/pc/res/assets/runtime/general/seat/static/'+str(data['id'])+'.png')

print(json.dumps(data, ensure_ascii=False))
# print(json.dumps(data, ensure_ascii=False).encode('utf8').decode())
