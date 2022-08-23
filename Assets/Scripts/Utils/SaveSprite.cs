// using UnityEngine;
// using UnityEditor;

// public class TestSaveSprite
// {
//     [MenuItem("Tools/导出精灵")]
//     static void SaveSprite()
//     {
//         foreach (Object obj in Selection.objects)
//         {
//             string selectionPath = AssetDatabase.GetAssetPath(obj);
//             var allAssets = AssetDatabase.LoadAllAssetsAtPath(selectionPath);
//             foreach (var asset in allAssets)
//             {
//                 if (asset is Sprite)
//                 {
//                     // 创建导出文件夹
//                     string outPath = Application.dataPath + "/outSprite/" + System.IO.Path.GetFileNameWithoutExtension(selectionPath);
//                     if (!System.IO.Directory.Exists(outPath))
//                     {
//                         System.IO.Directory.CreateDirectory(outPath);
//                     }

//                     var sprite = asset as Sprite;
//                     // 创建单独的纹理
//                     Texture2D tex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height, sprite.texture.format, false);
//                     tex.SetPixels(sprite.texture.GetPixels((int)sprite.rect.xMin, (int)sprite.rect.yMin,
//                         (int)sprite.rect.width, (int)sprite.rect.height));
//                     tex.Apply();

//                     // 写入成PNG文件
//                     System.IO.File.WriteAllBytes(outPath + "/" + sprite.name + ".png", tex.EncodeToPNG());
//                 }
//             }
//         }
//         Debug.Log("SaveSprite Finished");
//     }
// }