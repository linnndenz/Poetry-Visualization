using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

//处理原始数据，加上标题
public class AddTitle
{
    [MenuItem("Tools/ProcessData/AddTitle")]
    public static void ProcessData()
    {
        string path = Application.dataPath + "/Plugins/ProcessData/Finish/";

        int successCnt = 0;
        int failCnt = 0;

        //创建目标文件夹
        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }

        TextAsset[] textAssets = Selection.GetFiltered<TextAsset>(SelectionMode.Assets);//选中原始文件

        if (textAssets.Length <= 0 || textAssets == null) {
            Debug.Log("未选中文件");
            return;
        }

        for (int i = 0; i < textAssets.Length; i++) {
            if (!File.Exists(path + textAssets[i].name + ".json")) {
                //创建文件
                StreamWriter fileWriter = new StreamWriter(File.Create(path + textAssets[i].name + ".json"));

                //读取文件内容
                string context = textAssets[i].text;


                //写入格式
                string data = "";
                data += "{" + "\n";
                data += "  \"poemList\": ";
                data += context;
                //写入格式
                data += "}";

                //转进json文件
                JsonUtility.ToJson(data);


                fileWriter.Write(data);
                fileWriter.Close();
                fileWriter.Dispose();

                successCnt++;

            } else {
                Debug.Log("重名文件： " + textAssets[i].name);
                failCnt++;
            }
        }

        AssetDatabase.Refresh();//刷新资源窗口界面
        Debug.Log("转化完成。 " + "成功： " + successCnt.ToString() + " 失败： " + failCnt.ToString());
    }
}
