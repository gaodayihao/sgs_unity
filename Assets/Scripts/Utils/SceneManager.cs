using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : Singleton<SceneManager>
{
    // private string currentAB = null;

    private bool isDone = true;

    /// <summary>
    /// 场景名与AB包名的映射
    /// </summary>
    /// <typeparam name="string">场景名</typeparam>
    /// <typeparam name="string">AB包名</typeparam>
    /// <returns></returns>
    private Dictionary<string, string> sceneMap = new Dictionary<string, string>
    {
        {"Menu","menu"},
        {"SgsMain","sgsmain"}
    };

    private string localScene = "Login";

    public IEnumerator LoadSceneFromAB(string sceneName)
    {
        // 若场景正在加载，则直接返回
        if (!isDone)
        {
            yield return null;
        }
        else
        {
            isDone = false;
            // 卸载当前场景的AssetBundle
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (currentScene != localScene)
            {
                string currentAB = sceneMap[currentScene];
                ABManager.Instance.Unload(currentAB);
                // debug
                Debug.Log("unload " + currentAB);
            }
            // 若新场景为本地场景，则直接加载场景，不需加载AssetBundle
            if (sceneName == localScene)
            {
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName);
            }
            else
            {
                // 加载包含场景资源的AssetBundle
                string abName = sceneMap[sceneName];
                yield return ABManager.Instance.LoadAssetBundle(abName);
                if (abName == "sgsmain") yield return ABManager.Instance.LoadAssetBundle("sgsasset");
                // debug
                Debug.Log("load " + abName);
                // 获取场景路径
                string[] scenePath = ABManager.Instance.ABMap[abName].GetAllScenePaths();
                // 加载场景
                var operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenePath[0]);
                // operation.allowSceneActivation = false;
            }
            isDone = true;
        }
    }
}