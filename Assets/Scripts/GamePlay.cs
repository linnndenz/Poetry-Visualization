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
    private GameObject prefab_verseBox;//每次生成收纳字体的父物体
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
        prefab_verseBox = Resources.Load<GameObject>("VerseBox");
        centerPos = showArea.Find("CenterPos");
        tmp_verseBoxes.Add(showArea.Find("VerseBox").gameObject);

        //verseList = PoetryManager.GetVerseList('春', ref markList);
        //RandomShowVerseList();

        Init();
    }

    const string CHARACTER = "Character";
    void Update()
    {
        Hit();
    }

    //先随机生成一句
    private void Init()
    {
        verseList.Clear();
        markList.Clear();
        showList.Clear();

        //随机选中一句
        int x = Random.Range(0, PoetryManager.poems.Length);
        int y = Random.Range(0, PoetryManager.poems[x].poemList.Count);
        int z = Random.Range(0, PoetryManager.poems[x].poemList[y].paragraphs.Count);
        curr_mark = new Vector2(x, y);

        //逐字生成
        string str = "";
        Poem p = PoetryManager.poems[x].poemList[y];
        for (int i = 0; i < p.paragraphs[z].Length; i++) {
            if (p.paragraphs[z][i] == '，' || p.paragraphs[z][i] == '。' || p.paragraphs[z][i] == '?'
                || p.paragraphs[z][i] == '!' || p.paragraphs[z][i] == ';') break;
            GameObject o = Instantiate(prefab_character, tmp_verseBoxes[0].transform);
            o.GetComponent<Character>().SetCharacter(p.paragraphs[z][i], new Vector2(x, y));
            o.transform.position = new Vector3(centerPos.position.x + i * 1, centerPos.position.y, 0);
            str += p.paragraphs[z][i];
        }

        verseList.Add(str);
        markList.Add(new Vector2(x, y));
        showList.Add(0);
        //信息栏显示
        ShowInfo();
    }

    //点击检测
    private void Hit()
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
                ShowInfo();
            }
        }
    }
    //信息栏显示
    private void ShowInfo()
    {
        Poem p = PoetryManager.poems[(int)curr_mark.x].poemList[(int)curr_mark.y];
        text_title.text = p.title;
        text_author.text = p.author;
        text_poem.text = "";
        for (int i = 0; i < p.paragraphs.Count; i++) {
            if (p.paragraphs[i].Contains(verseList[showList[0]])) {
                text_poem.text += "<#880000>";
            }
            text_poem.text += p.paragraphs[i];
            if (p.paragraphs[i].Contains(verseList[showList[0]])) {
                text_poem.text += "</color>";
            }
            text_poem.text += "\n";
        }
    }

    //随机取包含库中verseNUm条
    private void RandomShowVerseList()
    {
        //清空show区
        showList.Clear();

        //*******当前诗句置于showIndexList的0号位*******
        //同一首诗可能会有bug
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
    List<GameObject> tmp_verseBoxes = new List<GameObject>();
    private void ShowVerse()
    {
        //当前文字后移，取消碰撞检测
        tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.localEulerAngles = new Vector3(90, 0, 0);
        var boxes = tmp_verseBoxes[tmp_verseBoxes.Count - 1].GetComponentsInChildren<BoxCollider>();
        for (int i = 0; i < boxes.Length; i++) {
            boxes[i].enabled = false;
        }

        //生成新的VerseBox
        tmp_verseBoxes.Add(Instantiate(prefab_verseBox, showArea));
        //逐字生成
        for (int i = 0; i < showList.Count; i++) {
            for (int j = 0; j < verseList[showList[i]].Length; j++) {
                GameObject o = Instantiate(prefab_character, tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform);
                o.GetComponent<Character>().SetCharacter(verseList[showList[i]][j], markList[showList[i]]);
                o.transform.position = new Vector3(centerPos.position.x + j * 1, centerPos.position.y - i * 1, 0);
            }
        }
    }


    //随机按键
    public void ReRadom()
    {
        //清空当前verseBox
        for (int i = 0; i < tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.childCount; i++) {
            Destroy(tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.GetChild(i).gameObject);
        }

        if (tmp_verseBoxes.Count <= 1) {//只有初始时重新随机
            Destroy(tmp_verseBoxes[0].transform.GetChild(0).gameObject);
            Init();
        } else {
            RandomShowVerseList();
        }

    }
}
