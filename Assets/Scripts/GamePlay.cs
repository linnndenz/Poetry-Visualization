using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlay : MonoBehaviour
{
    public int verseNum;

    public Text tmp;

    //【库中】包含当前选中字的诗句列表
    [SerializeField] public List<string> currVerseList;
    [SerializeField] public List<Vector2> currVerseMarkList = new List<Vector2>();

    //【显示中】包含当前选中字的诗句列表，对应curr中的序号
    [SerializeField] public List<int> showIndexList;

    void Start()
    {
        currVerseList = PoetryManager.GetVerseList('灯', ref currVerseMarkList);

        RandomShowVerseList();
        ShowVerse();
    }

    public void RandomShowVerseList()
    {
        showIndexList.Clear();
        //curr总数小等于verseNum，则加载全部
        if (currVerseList.Count <= verseNum) {
            for (int i = 0; i < currVerseList.Count; i++) {
                showIndexList.Add(i);
            }
            return;
        }

        int maxIndex = currVerseList.Count;
        //否则随机取verseNum条
        for (int i = 0; i < verseNum; i++) {
            int num = Random.Range(0, maxIndex);
            while (showIndexList.Contains(num)) {//线性探查去重
                num = (num + 1) % maxIndex;
            }
            showIndexList.Add(num);
        }
    }

    public void ShowVerse()
    {
        tmp.text = "";
        for (int i = 0; i < showIndexList.Count; i++) {
            tmp.text += currVerseList[showIndexList[i]];
            tmp.text += "\n";
        }
    }
}
