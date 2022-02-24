using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Model
{
    public class User : Singleton<User>
    {
        // Start is called before the first frame update
        // 用户名
        public string Username { get; private set; }
        // 昵称

        // 头像
        public string Portrait { get; private set; }

        public void Init(GetinfoResponse getinfoResponse)
        {
            Username = getinfoResponse.username;
            Portrait = getinfoResponse.portrait;
        }
    }
}
