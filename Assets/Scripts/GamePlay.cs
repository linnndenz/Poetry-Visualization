using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePlay : MonoBehaviour
{
    public int verseNum;

    public Text tmp;

    //�����С�������ǰѡ���ֵ�ʫ���б�
    [SerializeField] public List<string> currVerseList;
    [SerializeField] public List<Vector2> currVerseMarkList = new List<Vector2>();

    //����ʾ�С�������ǰѡ���ֵ�ʫ���б���Ӧcurr�е����
    [SerializeField] public List<int> showIndexList;

    void Start()
    {
        currVerseList = PoetryManager.GetVerseList('��', ref currVerseMarkList);

        RandomShowVerseList();
        ShowVerse();
    }

    public void RandomShowVerseList()
    {
        showIndexList.Clear();
        //curr����С����verseNum�������ȫ��
        if (currVerseList.Count <= verseNum) {
            for (int i = 0; i < currVerseList.Count; i++) {
                showIndexList.Add(i);
            }
            return;
        }

        int maxIndex = currVerseList.Count;
        //�������ȡverseNum��
        for (int i = 0; i < verseNum; i++) {
            int num = Random.Range(0, maxIndex);
            while (showIndexList.Contains(num)) {//����̽��ȥ��
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
