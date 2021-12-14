using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;
using System;
using System.Linq;

//古诗数据导入、获取
public class PoetryManager : MonoBehaviour
{
    static public PoemList[] poems = new PoemList[50];//全局查询表

    static public PoetryManager instance;

    void Awake()
    {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(instance);
        } else {
            Destroy(gameObject);
        }

        //导入诗
        string forename = "PoetryAssets/poet.tang.";
        for (int i = 0; i < poems.Length; i++) {
            poems[i] = JsonMapper.ToObject<PoemList>((Resources.Load(forename + (i * 1000).ToString()) as TextAsset).text);
        }
    }

    //根据单字获取诗句，返回含有该字的诗句列表，ref诗句对应的诗（用poems查找）
    static public List<string> GetVerseList(char c, ref List<Vector2> mark)
    {
        List<string> result = new List<string>();
        int left, right;
        string tmpstr;
        int tmplen;
        mark.Clear();
        for (int i = 0; i < poems.Length; i++) {//每个诗文件
            for (int j = 0; j < poems[i].poemList.Count; j++) {//每个文件的每首诗
                for (int k = 0; k < poems[i].poemList[j].paragraphs.Count; k++) {//每首诗的每一句
                    if (poems[i].poemList[j].paragraphs[k].Contains(c)) {//包含这句诗
                        tmpstr = poems[i].poemList[j].paragraphs[k];
                        //摘出单句
                        left = right = tmpstr.IndexOf(c);
                        while (true) {
                            if (left - 1 < 0 || tmpstr[left - 1] == '，' || tmpstr[left - 1] == '。'
                                || tmpstr[left - 1] == '？' || tmpstr[left - 1] == '！' || tmpstr[left - 1] == '；') break;
                            left--;
                        }
                        while (true) {
                            if (right + 1 >= tmpstr.Length || tmpstr[right + 1] == '，' || tmpstr[right + 1] == '。'
                                || tmpstr[right + 1] == '？' || tmpstr[right + 1] == '！' || tmpstr[right + 1] == '；') break;
                            right++;
                        }
                        tmplen = right - left + 1;
                        if (tmplen >= 5 && tmplen <= 7) {
                            //加入result
                            string verse = tmpstr.Substring(left, tmplen);
                            result.Add(verse);
                            //记录对应Poem，x-poems,y-poemList
                            mark.Add(new Vector2(i, j));

                        }
                    }
                }
            }
        }

        return result;
    }

}

public class PoemList
{
    readonly public List<Poem> poemList;//名字对应注意
}

public class Poem
{
    readonly public string title;
    readonly public string author;
    readonly public List<string> paragraphs;
}

