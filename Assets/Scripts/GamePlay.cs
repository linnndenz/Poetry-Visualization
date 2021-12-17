using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlay : MonoBehaviour
{
    [Header("����")]
    public int verseNum;

    [Header("��Ϣ�����")]
    public TMP_Text text_title;
    public TMP_Text text_author;
    public TMP_Text text_poem;

    [Header("��ʾ�����")]
    public Transform showArea;
    private Transform centerPos;
    private GameObject prefab_character;
    private GameObject prefab_verseBox;//ÿ��������������ĸ�����
    //public Text tmp;

    [Header("����")]
    public char curr_char;
    public Vector2 curr_mark;

    //�����С�������ǰѡ���ֵ�ʫ���б����������ڣ�
    [SerializeField] public List<string> verseList;
    [SerializeField] public List<Vector2> markList = new List<Vector2>();
    //����ʾ�С�������ǰѡ���ֵ�ʫ���б���Ӧcurr�е����
    [SerializeField] public List<int> showList;

    void Start()
    {
        prefab_character = Resources.Load<GameObject>("Character");
        prefab_verseBox = Resources.Load<GameObject>("VerseBox");
        centerPos = showArea.Find("CenterPos");
        tmp_verseBoxes.Add(showArea.Find("VerseBox").gameObject);

        //verseList = PoetryManager.GetVerseList('��', ref markList);
        //RandomShowVerseList();

        Init();
    }

    const string CHARACTER = "Character";
    void Update()
    {
        Hit();
    }

    //���������һ��
    private void Init()
    {
        verseList.Clear();
        markList.Clear();
        showList.Clear();

        //���ѡ��һ��
        int x = Random.Range(0, PoetryManager.poems.Length);
        int y = Random.Range(0, PoetryManager.poems[x].poemList.Count);
        int z = Random.Range(0, PoetryManager.poems[x].poemList[y].paragraphs.Count);
        curr_mark = new Vector2(x, y);

        //��������
        string str = "";
        Poem p = PoetryManager.poems[x].poemList[y];
        for (int i = 0; i < p.paragraphs[z].Length; i++) {
            if (p.paragraphs[z][i] == '��' || p.paragraphs[z][i] == '��' || p.paragraphs[z][i] == '?'
                || p.paragraphs[z][i] == '!' || p.paragraphs[z][i] == ';') break;
            GameObject o = Instantiate(prefab_character, tmp_verseBoxes[0].transform);
            o.GetComponent<Character>().SetCharacter(p.paragraphs[z][i], new Vector2(x, y));
            o.transform.position = new Vector3(centerPos.position.x + i * 1, centerPos.position.y, 0);
            str += p.paragraphs[z][i];
        }

        verseList.Add(str);
        markList.Add(new Vector2(x, y));
        showList.Add(0);
        //��Ϣ����ʾ
        ShowInfo();
    }

    //������
    private void Hit()
    {
        if (Input.GetMouseButtonDown(0)) {
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);
            if (hit.transform != null && hit.transform.CompareTag(CHARACTER)) {
                //������֣���װ����
                Character c = hit.transform.GetComponent<Character>();
                curr_char = c.M_character;
                curr_mark = c.M_mark;
                verseList = PoetryManager.GetVerseList(curr_char, ref markList);
                //�����ʾverseNum��ʫ��
                RandomShowVerseList();
                //��Ϣ����ʾ
                ShowInfo();
            }
        }
    }
    //��Ϣ����ʾ
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

    //���ȡ��������verseNUm��
    private void RandomShowVerseList()
    {
        //���show��
        showList.Clear();

        //*******��ǰʫ������showIndexList��0��λ*******
        //ͬһ��ʫ���ܻ���bug
        int currIndex = markList.IndexOf(curr_mark);
        showList.Add(currIndex);

        //curr����С����verseNum�������ȫ��
        if (verseList.Count <= verseNum) {
            for (int i = 0; i < verseList.Count; i++) {
                if (!showList.Contains(i)) {
                    showList.Add(i);
                }
            }
            return;
        }

        int maxIndex = verseList.Count;
        //���������ȡverseNum-1��
        for (int i = 0; i < verseNum - 1; i++) {
            int num = Random.Range(0, maxIndex);
            while (showList.Contains(num)) {//����̽��ȥ��
                num = (num + 1) % maxIndex;
            }
            showList.Add(num);
        }
        ShowVerse();
    }
    //��ʾ����
    List<GameObject> tmp_verseBoxes = new List<GameObject>();
    private void ShowVerse()
    {
        //��ǰ���ֺ��ƣ�ȡ����ײ���
        tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.localEulerAngles = new Vector3(90, 0, 0);
        var boxes = tmp_verseBoxes[tmp_verseBoxes.Count - 1].GetComponentsInChildren<BoxCollider>();
        for (int i = 0; i < boxes.Length; i++) {
            boxes[i].enabled = false;
        }

        //�����µ�VerseBox
        tmp_verseBoxes.Add(Instantiate(prefab_verseBox, showArea));
        //��������
        for (int i = 0; i < showList.Count; i++) {
            for (int j = 0; j < verseList[showList[i]].Length; j++) {
                GameObject o = Instantiate(prefab_character, tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform);
                o.GetComponent<Character>().SetCharacter(verseList[showList[i]][j], markList[showList[i]]);
                o.transform.position = new Vector3(centerPos.position.x + j * 1, centerPos.position.y - i * 1, 0);
            }
        }
    }


    //�������
    public void ReRadom()
    {
        //��յ�ǰverseBox
        for (int i = 0; i < tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.childCount; i++) {
            Destroy(tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.GetChild(i).gameObject);
        }

        if (tmp_verseBoxes.Count <= 1) {//ֻ�г�ʼʱ�������
            Destroy(tmp_verseBoxes[0].transform.GetChild(0).gameObject);
            Init();
        } else {
            RandomShowVerseList();
        }

    }
}
