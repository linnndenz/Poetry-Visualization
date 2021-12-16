using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlay : MonoBehaviour
{
    [Header("参数")]
    public int verseNum;

    [Header("信息栏组件")]
    public TMP_Text text_title;
    public TMP_Text text_author;
    public TMP_Text text_poem;

    [Header("显示区组件")]
    public Transform showArea;
    private Transform centerPos;
    private GameObject prefab_character;
    private GameObject prefab_verses;//每次生成收纳字体的父物体
    //public Text tmp;

    [Header("数据")]
    public char curr_char;
    public Vector2 curr_mark;

    //【库中】包含当前选中字的诗句列表（不包含当期）
    [SerializeField] public List<string> verseList;
    [SerializeField] public List<Vector2> markList = new List<Vector2>();
    //【显示中】包含当前选中字的诗句列表，对应curr中的序号
    [SerializeField] public List<int> showList;

    void Start()
    {
        prefab_character = Resources.Load<GameObject>("Character");
        prefab_verses = Resources.Load<GameObject>("Verses");
        centerPos = showArea.Find("CenterPos");
        tmp_verses = showArea.Find("Verses").gameObject;

        //verseList = PoetryManager.GetVerseList('春', ref markList);
        //RandomShowVerseList();
    }

    const string CHARACTER = "Character";
    void Update()
    {
        Hit();
    }

    //点击检测
    public void Hit()
    {
        if (Input.GetMouseButtonDown(0)) {
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);
            if (hit.transform != null && hit.transform.CompareTag(CHARACTER)) {
                //点击到字，重装数据
                Character c = hit.transform.GetComponent<Character>();
                curr_char = c.M_character;
                curr_mark = c.M_mark;
                verseList = PoetryManager.GetVerseList(curr_char, ref markList);
                //随机显示verseNum条诗句
                RandomShowVerseList();
                //信息栏显示
                Poem p = PoetryManager.poems[(int)curr_mark.x].poemList[(int)curr_mark.y];
                text_title.text = p.title;
                text_author.text = p.author;
                text_poem.text = "";
                for (int i = 0; i < p.paragraphs.Count; i++) {
                    text_poem.text += p.paragraphs[i];
                    text_poem.text += "\n";
                }
            }
        }
    }

    //随机取包含库中verseNUm条
    public void RandomShowVerseList()
    {
        //清空show区
        showList.Clear();
        //for (int i = 0; i < tmp_verses.transform.childCount; i++) {
        //    Destroy(tmp_verses.transform.GetChild(i).gameObject);
        //}

        //当前诗句置于showIndexList的0号位*******
        int currIndex = markList.IndexOf(curr_mark);
        showList.Add(currIndex);

        //curr总数小等于verseNum，则加载全部
        if (verseList.Count <= verseNum) {
            for (int i = 0; i < verseList.Count; i++) {
                if (!showList.Contains(i)) {
                    showList.Add(i);
                }
            }
            return;
        }

        int maxIndex = verseList.Count;
        //否则随机再取verseNum-1条
        for (int i = 0; i < verseNum - 1; i++) {
            int num = Random.Range(0, maxIndex);
            while (showList.Contains(num)) {//线性探查去重
                num = (num + 1) % maxIndex;
            }
            showList.Add(num);
        }
        ShowVerse();
    }
    //显示文字
    GameObject tmp_verses;
    private void ShowVerse()
    {
        tmp_verses.transform.localEulerAngles = new Vector3(90, 0, 0);
        tmp_verses = Instantiate(prefab_verses, showArea) as GameObject;
        //逐字生成
        for (int i = 0; i < showList.Count; i++) {
            for (int j = 0; j < verseList[showList[i]].Length; j++) {
                GameObject o = Instantiate(prefab_character, tmp_verses.transform);
                o.GetComponent<Character>().SetCharacter(verseList[showList[i]][j], markList[showList[i]]);
                o.transform.position = new Vector3(centerPos.position.x + j * 1, centerPos.position.y - i * 1, 0);
            }
        }
    }
}
