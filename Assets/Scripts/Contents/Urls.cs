public static class Urls
{
    /// <summary>
    /// 服务器IP地址
    /// </summary>
    public const string IP_ADDRESS = "123.56.19.80:8000";

    /// <summary>
    /// 服务器域名
    /// </summary>
    public const string DOMAIN_NAME = "https://app931.acapp.acwing.com.cn/";

    /// <summary>
    /// 登录模块地址
    /// </summary>
    public const string LOGIN_URL = DOMAIN_NAME + "login/";

    /// <summary>
    /// Django静态文件地址
    /// </summary>
    public const string STATIC_URL = DOMAIN_NAME + "static/";

    public static string JSON_URL =
#if UNITY_EDITOR
        "file:///" + UnityEngine.Application.dataPath + "/../Json/";
#else
        STATIC_URL + "Json/";
#endif

    // 图片文件地址
    public const string IMAGE_URL = STATIC_URL + "image/";
    public const string GENERAL_IMAGE = IMAGE_URL + "general/";
    
    // 音频文件地址
    public const string AUDIO_URL = STATIC_URL + "audio/";

    public const string TEST_BACKGROUND_IMAGE = "https://web.sanguosha.com/220/h5/res/runtime/pc/wallpaper/bg/10.jpg";
}
