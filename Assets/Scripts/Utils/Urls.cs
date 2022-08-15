using UnityEngine;

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
    public static string STATIC_URL =
#if UNITY_EDITOR
        "file://" + Application.streamingAssetsPath + "/";
#else
        Application.streamingAssetsPath + "/";
#endif
    // public const string STATIC_URL = DOMAIN_NAME + "static/";

    public static string ABUrl = Urls.STATIC_URL + "AssetBundles/";

    public static string JSON_URL = STATIC_URL + "Json/";
    // #if UNITY_EDITOR
    //         "file:///" + Application.dataPath + "/../Json/";
    // #else
    //         STATIC_URL + "Json/";
    // #endif

    // 图片文件地址
    public static string IMAGE_URL = STATIC_URL + "Image/";
    public static string GENERAL_IMAGE = IMAGE_URL + "General/";

    // 音频文件地址
    public static string AUDIO_URL = STATIC_URL + "Audio/";

    public const string TEST_BACKGROUND_IMAGE = "https://web.sanguosha.com/220/h5/res/runtime/pc/wallpaper/bg/10.jpg";
}
