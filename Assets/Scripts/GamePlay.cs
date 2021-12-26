using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GamePlay : MonoBehaviour
{
    [Header("����")]
    public int verseNum;
    public float scrollDis;//���ֻ�����
    public int offsetAngle=15;//ʫ�����ƫ�ǣ���һ��15��
    public float subTransAmount = 0.25f;//ʫ�����͸���ȼ�������һ��0.25��
    public Vector3 linkPosition = new Vector3(-3, 1.5f, 0);//�����ֻ��ƶ�������Ļλ��
    public int existingVersesAmount = 2;//�������ܹ���ʫ����


    [Header("��Ϣ�����")]
    public TMP_Text text_title;
    public TMP_Text text_author;
    public TMP_Text text_poem;

    [Header("��ʾ�����")]
    public GameObject mainCamera;
    public Transform showArea;
    private Transform centerPos;
    public GameObject prefab_character;//����public��
    private GameObject prefab_verse;
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
    [SerializeField] public List<GameObject> tmp_verseBoxes = new List<GameObject>();

    void Start()
    {
        //prefab_character = Resources.Load<GameObject>("Character_White");
        prefab_verse= Resources.Load<GameObject>("Verse");
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
        Scroll();
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
        centerPos.position = linkPosition;
        string str = "";
        Poem p = PoetryManager.poems[x].poemList[y];
        for (int i = 0; i < p.paragraphs[z].Length; i++) {
            if (p.paragraphs[z][i] == '��' || p.paragraphs[z][i] == '��' || p.paragraphs[z][i] == '?'
                || p.paragraphs[z][i] == '!' || p.paragraphs[z][i] == ';') break;
            GameObject o = Instantiate(prefab_character, tmp_verseBoxes[0].transform.GetChild(0));
            o.GetComponent<Character>().SetCharacter(p.paragraphs[z][i], new Vector2(x, y));
            o.transform.position = new Vector3(centerPos.position.x + i * 1 - 3, centerPos.position.y, 0);//�����ƶ�����
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
            if (hit.transform != null && hit.transform.CompareTag(CHARACTER)) 
            {
                //�õ����ȱ�յ������³��֣�����������Ϊ�����û�û���µ�curr_char
                float transAmout = hit.transform.parent.GetComponentInChildren<TMP_Text>().color.a;
                for (int i = 0; i < hit.transform.parent.childCount - 1; i++)
                {
                    if (hit.transform.parent.name != "CenterPos" && hit.transform.parent.GetChild(i).name.ToCharArray()[0] == curr_char)
                    {
                        hit.transform.parent.GetChild(i).gameObject.SetActive(true);
                        hit.transform.parent.GetChild(i).GetComponentInChildren<Character>().SetTrans(transAmout);
                    }
                }

                //������֣���װ����
                Character c = hit.transform.GetComponent<Character>();
                curr_char = c.M_character;
                curr_mark = c.M_mark;
                verseList = PoetryManager.GetVerseList(curr_char, ref markList);
                //��centerPos����Ϊ�������
                centerPos.position = hit.transform.position;
                centerPos.eulerAngles = hit.transform.eulerAngles;

                //�����ʾverseNum��ʫ��
                RandomShowVerseList();
                //��Ϣ����ʾ
                ShowInfo();
                //��Ӱ���ƶ�
                mainCamera.GetComponent<CameraMove>().Zoom();
                //�����õ�ʫ��
                if (tmp_verseBoxes.Count > existingVersesAmount)
                    Destroy(tmp_verseBoxes[tmp_verseBoxes.Count - existingVersesAmount-1].gameObject);
            }
        }
    }
    //���ּ��
    private void Scroll()
    {
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            scrollDis += 0.05f;
            tmp_verseBoxes[tmp_verseBoxes.Count - 1].GetComponent<VerseMove>().ScrollVerse();
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            scrollDis -= 0.05f;
            tmp_verseBoxes[tmp_verseBoxes.Count - 1].GetComponent<VerseMove>().ScrollVerse();
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

    //���ȡ��������verseNum��
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
        //���������ȡverseNum����һ��verseNum+1��
        for (int i = 0; i < verseNum; i++) {
            int num = Random.Range(0, maxIndex);
            while (showList.Contains(num)) {//����̽��ȥ��
                num = (num + 1) % maxIndex;
            }
            showList.Add(num);
        }
        ShowVerse();
    }
    //��ʾ����
    private void ShowVerse()
    {
        ////////////////////////////��ʫ�䴦��////////////////////////////
        //����ԭ������
        if (tmp_verseBoxes.Count > 1)
            centerPos.GetChild(tmp_verseBoxes.Count - 2).gameObject.SetActive(false);

        //˲���ƶ�
        Vector3 rotVec = new Vector3(0, offsetAngle, 0) -centerPos.eulerAngles;//###����ƫ�ǣ���һ��15��###
        showArea.eulerAngles += rotVec;
        Vector3 moveVec = linkPosition - centerPos.position;//###�ƶ�����Ļƫ����λ�ã���һ��Vector3(-4, 1.5f, 0)###
        showArea.position += moveVec;
        //ȡ����ײ���
        var boxes = tmp_verseBoxes[tmp_verseBoxes.Count - 1].GetComponentsInChildren<BoxCollider>();
        for (int i = 0; i < boxes.Length; i++) 
        {
            boxes[i].enabled = false;
        }
        //����͸����
        var characters = showArea.GetComponentsInChildren<Character>();
        for (int i = 0; i < characters.Length; i++)
        {
            characters[i].SubTrans(subTransAmount);//###��ȫ�����ˣ���һ��0.25f��###
        }


        ////////////////////////////��ʫ������////////////////////////////
        //�����µ�VerseBox
        tmp_verseBoxes.Add(Instantiate(prefab_verseBox, showArea));
        tmp_verseBoxes[tmp_verseBoxes.Count - 1].name = (tmp_verseBoxes.Count - 1).ToString();
        tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.position = centerPos.position;//����λ��
        tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.eulerAngles = new Vector3(0, -offsetAngle, 0);//###����ƫ�ǣ���һ��-15��###
        for (int i = 1; i < showList.Count; i++) 
            //i=1������ԭʫ��
        {
            //����Verse
            GameObject oo = Instantiate(prefab_verse, tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform);
            oo.name = verseList[showList[i]].ToString();
            //��ȡ��ǰ�������������
            int charNum = verseList[showList[i]].IndexOf(curr_char);
            for (int j = 0; j < verseList[showList[i]].Length; j++) 
            {
                //����Character
                GameObject o = Instantiate(prefab_character, oo.transform);
                o.name = verseList[showList[i]][j].ToString();
                o.GetComponent<Character>().SetCharacter(verseList[showList[i]][j], markList[showList[i]]);
                //�ڷ�����
                o.transform.position = new Vector3(oo.transform.position.x + (j - charNum) * 1f, oo.transform.position.y, oo.transform.position.z);
                //����ʫ���е�������
                if (j == charNum)
                    o.SetActive(false);
            }
        }
        //����������
        GameObject link = Instantiate(prefab_character, centerPos.transform);
        link.name = curr_char.ToString();
        link.GetComponent<Character>().SetCharacter(curr_char, markList[showList[0]]);
        link.transform.eulerAngles = new Vector3(0, -offsetAngle, 0);//###����ƫ�ǣ���һ��-15��###
        link.GetComponent<Character>().SetLinkColor();


        //��ʫ�����ɺ�ع�ԭλ���������ƶ�
        showArea.eulerAngles -= rotVec;
        showArea.position -= moveVec;
        showArea.DORotate(showArea.eulerAngles+rotVec, 1);
        showArea.DOMove(showArea.position+moveVec, 1);

        //�ñ�������ʫ���ȡ���ݣ�׼���ƶ�
        scrollDis = 0;
        tmp_verseBoxes[tmp_verseBoxes.Count - 1].GetComponent<VerseMove>().GetVerse();
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
