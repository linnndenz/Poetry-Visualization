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
    private GameObject prefab_verses;//ÿ��������������ĸ�����
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
        prefab_verses = Resources.Load<GameObject>("Verses");
        centerPos = showArea.Find("CenterPos");
        tmp_verses = showArea.Find("Verses").gameObject;

        //verseList = PoetryManager.GetVerseList('��', ref markList);
        //RandomShowVerseList();
    }

    const string CHARACTER = "Character";
    void Update()
    {
        Hit();
    }

    //������
    public void Hit()
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

    //���ȡ��������verseNUm��
    public void RandomShowVerseList()
    {
        //���show��
        showList.Clear();
        //for (int i = 0; i < tmp_verses.transform.childCount; i++) {
        //    Destroy(tmp_verses.transform.GetChild(i).gameObject);
        //}

        //��ǰʫ������showIndexList��0��λ*******
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
    GameObject tmp_verses;
    private void ShowVerse()
    {
        tmp_verses.transform.localEulerAngles = new Vector3(90, 0, 0);
        tmp_verses = Instantiate(prefab_verses, showArea) as GameObject;
        //��������
        for (int i = 0; i < showList.Count; i++) {
            for (int j = 0; j < verseList[showList[i]].Length; j++) {
                GameObject o = Instantiate(prefab_character, tmp_verses.transform);
                o.GetComponent<Character>().SetCharacter(verseList[showList[i]][j], markList[showList[i]]);
                o.transform.position = new Vector3(centerPos.position.x + j * 1, centerPos.position.y - i * 1, 0);
            }
        }
    }
}
