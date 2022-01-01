using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GamePlay : MonoBehaviour
{
    public float switchTime = 0.5f;
    [Header("参数")]
    public float scrollDis;//滚轮滑动量
    public bool DevMode = false;//开发者模式是否打开
    public int existingVersesAmount = 2;//场景内能共存诗词数
    public int interactiveZoneAngle;//可交互范围角度

    public Color characterColor = new Color(0, 0, 0, 1);//生成字颜色
    public int verseNum;//生成诗句数

    public Vector3 linkPosition = new Vector3(-3, 1.5f, 0);//链接字会移动到的屏幕位置
    public int offsetAngle=15;//诗句代际偏角（第一版15）
    public float subTransAmount = 0.25f;//诗句代际透明度减量（第一版0.25）
    public bool hideOtherVerses = true;//是否隐藏点击句同代的句子
    [Header("文字漂浮参数")]
    public bool linkCharMove;
    public bool currVerseMove;
    public bool preVerseMove;
    public Vector3 characterOffset;  //最大的偏移量
    public float characterFrequency;  //振动频率

    [Header("信息栏组件")]
    public TMP_Text text_title;
    public TMP_Text text_author;
    public TextManager textManager;

    [Header("显示区组件")]
    public GameObject mainCamera;
    public Transform showArea;
    private Transform centerPos;
    public Transform options;

    public GameObject prefab_character;//调成public了
    private GameObject prefab_verse;
    private GameObject prefab_verseBox;//每次生成收纳字体的父物体

    [Header("数据")]
    public char curr_char;
    public Vector2 curr_mark;

    //【库中】包含当前选中字的诗句列表（不包含当期）
    [SerializeField] public List<string> verseList;
    [SerializeField] public List<Vector2> markList = new List<Vector2>();
    //【显示中】包含当前选中字的诗句列表，对应curr中的序号
    [SerializeField] public List<int> showList;
    [SerializeField] public List<GameObject> tmp_verseBoxes = new List<GameObject>();

    void Start()
    {
        //prefab_character = Resources.Load<GameObject>("Character_White");
        prefab_verse= Resources.Load<GameObject>("Verse");
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
        Scroll();
        //ESC退出游戏
        if (Input.GetKeyDown(KeyCode.Escape) )
        {
            Application.Quit();
        }
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
        centerPos.position = linkPosition;
        string str = "";
        Poem p = PoetryManager.poems[x].poemList[y];
        for (int i = 0; i < p.paragraphs[z].Length; i++) {
            if (p.paragraphs[z][i] == '，' || p.paragraphs[z][i] == '。' || p.paragraphs[z][i] == '?'
                || p.paragraphs[z][i] == '!' || p.paragraphs[z][i] == ';') break;
            GameObject o = Instantiate(prefab_character, tmp_verseBoxes[0].transform.GetChild(0));
            Character c = o.GetComponent<Character>();
            c.SetCharacter(p.paragraphs[z][i], new Vector2(x, y));
            c.SetColor(characterColor.r, characterColor.g, characterColor.b);
            //摆放文字
            o.transform.position = new Vector3(centerPos.position.x + i * 1 - 3, centerPos.position.y, 0);//向左移动三格
            //字体浮动参数设置
            c.animate = currVerseMove;
            c.originPosition = o.transform.localPosition;
            c.offset = characterOffset;
            c.frequency = characterFrequency;

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
            if (hit.transform != null && hit.transform.CompareTag(CHARACTER)) 
            {
                //让点击句缺空的字重现，放在这里是为方便用还没更新的curr_char
                float transAmout = hit.transform.parent.GetComponentInChildren<TMP_Text>().color.a;
                for (int i = 0; i < hit.transform.parent.childCount; i++)
                {
                    if (hit.transform.parent.name != "CenterPos" && hit.transform.parent.GetChild(i).name.ToCharArray()[0] == curr_char)
                    {
                        hit.transform.parent.GetChild(i).gameObject.SetActive(true);
                        hit.transform.parent.GetChild(i).GetComponentInChildren<Character>().SetTrans(transAmout);
                        hit.transform.parent.GetChild(i).GetComponentInChildren<Character>().SetColor(characterColor.r, characterColor.g, characterColor.b);
                    }
                }
                //让点击的字消失
                hit.transform.GetComponentInChildren<Character>().SetTrans(0);

                //点击到字，重装数据
                Character c = hit.transform.GetComponent<Character>();
                curr_char = c.M_character;
                curr_mark = c.M_mark;
                verseList = PoetryManager.GetVerseList(curr_char, ref markList);
                //将centerPos设置为点击文字
                centerPos.position = hit.transform.position;
                centerPos.eulerAngles = hit.transform.eulerAngles;

                //随机显示verseNum条诗句
                RandomShowVerseList();
                //信息栏显示
                ShowInfo();
                //摄影机移动
                mainCamera.GetComponent<CameraMove>().Zoom();
                //清理不用的诗句
                if (tmp_verseBoxes.Count > existingVersesAmount)
                    Destroy(tmp_verseBoxes[tmp_verseBoxes.Count - existingVersesAmount - 1].gameObject);
            }
        }
    }
    //滚轮检测
    private void Scroll()
    {
        Vector3 mousePosOnScreen = Input.mousePosition;
        mousePosOnScreen.z = Camera.main.WorldToScreenPoint(centerPos.position).z;
        Vector3 mousePosInWorld = Camera.main.ScreenToWorldPoint(mousePosOnScreen);
        //根据指针位置和centerPos的位置关系调整滚轮操纵方向
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (mousePosInWorld.x < centerPos.position.x)
                scrollDis += 0.05f;
            else
                scrollDis -= 0.05f;
            tmp_verseBoxes[tmp_verseBoxes.Count - 1].GetComponent<VerseMove>().ScrollVerse();
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            if (mousePosInWorld.x < centerPos.position.x)
                scrollDis -= 0.05f;
            else
                scrollDis += 0.05f;
            tmp_verseBoxes[tmp_verseBoxes.Count - 1].GetComponent<VerseMove>().ScrollVerse();
        }
    }
    //信息栏显示
    private void ShowInfo()
    {
        Poem p = PoetryManager.poems[(int)curr_mark.x].poemList[(int)curr_mark.y];
        text_title.text = p.title;
        text_author.text = p.author;

        string str_poem = "";
        for (int i = 0; i < p.paragraphs.Count; i++) {
            if (p.paragraphs[i].Contains(verseList[showList[0]])) {
                str_poem += "<color=#880000>";
            }
            str_poem += p.paragraphs[i];
            if (p.paragraphs[i].Contains(verseList[showList[0]])) {
                str_poem += "</color>";
            }
            str_poem += "\n";
        }

        //字符渐显
        textManager.OutputText(str_poem);
    }

    //随机取包含库中verseNum条
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
        //否则随机再取verseNum条，一共verseNum+1条
        for (int i = 0; i < verseNum; i++) {
            int num = Random.Range(0, maxIndex);
            while (showList.Contains(num)) {//线性探查去重
                num = (num + 1) % maxIndex;
            }
            showList.Add(num);
        }
        ShowVerse();
    }
    //显示文字
    private void ShowVerse()
    {
        ////////////////////////////旧诗句处理////////////////////////////
        //隐藏原链接字
        if (centerPos.childCount > 0)
            Destroy(centerPos.GetChild(0).gameObject);
        /*
            if (tmp_verseBoxes.Count > 1)
            centerPos.GetChild(tmp_verseBoxes.Count - 2).gameObject.SetActive(false);*/

        //瞬间移动
        Vector3 rotVec = new Vector3(0, offsetAngle, 0) -centerPos.eulerAngles;//###设置偏角（第一版15）###
        showArea.eulerAngles += rotVec;
        Vector3 moveVec = linkPosition - centerPos.position;//###移动到屏幕偏中心位置，第一版Vector3(-4, 1.5f, 0)###
        showArea.position += moveVec;
        //取消碰撞检测
        var boxes = tmp_verseBoxes[tmp_verseBoxes.Count - 1].GetComponentsInChildren<BoxCollider>();
        for (int i = 0; i < boxes.Length; i++) 
        {
            boxes[i].enabled = false;
        }
        //降低透明度
        var characters = showArea.GetComponentsInChildren<Character>();
        for (int i = 0; i < characters.Length; i++)
        {
            if (hideOtherVerses && characters[i].gameObject.transform.parent.parent.name == (tmp_verseBoxes.Count - 1).ToString() && characters[i].M_mark != curr_mark)
                characters[i].SubTrans(1);//隐藏点击句同代的句子
            else
                characters[i].SubTrans(subTransAmount);//###第一版0.25f###
            characters[i].animate = preVerseMove;//设置旧文字浮动
        }


        ////////////////////////////新诗句生成////////////////////////////
        //生成新的VerseBox
        tmp_verseBoxes.Add(Instantiate(prefab_verseBox, showArea));
        tmp_verseBoxes[tmp_verseBoxes.Count - 1].name = (tmp_verseBoxes.Count - 1).ToString();
        tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.position = centerPos.position;//设置位置
        tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.eulerAngles = new Vector3(0, -offsetAngle, 0);//###设置偏角（第一版-15）###
        for (int i = 1; i < showList.Count; i++) 
            //i=1，跳过原诗句
        {
            //生成Verse
            GameObject oo = Instantiate(prefab_verse, tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform);
            oo.name = verseList[showList[i]].ToString();
            //获取当前这句的链接字序号
            int charNum = verseList[showList[i]].IndexOf(curr_char);
            for (int j = 0; j < verseList[showList[i]].Length; j++) 
            {
                //生成Character
                GameObject o = Instantiate(prefab_character, oo.transform);
                Character c = o.GetComponent<Character>();
                o.name = verseList[showList[i]][j].ToString();
                c.SetCharacter(verseList[showList[i]][j], markList[showList[i]]);
                //设置颜色
                c.SetColor(characterColor.r, characterColor.g, characterColor.b);
                //摆放文字
                o.transform.position = new Vector3(oo.transform.position.x + (j - charNum) * 1f, oo.transform.position.y, oo.transform.position.z);
                //字体浮动参数设置
                c.animate = currVerseMove;
                c.originPosition = o.transform.localPosition;
                c.offset = characterOffset;
                c.frequency = characterFrequency;

                //隐藏诗句中的链接字
                if (j == charNum)
                    o.SetActive(false);
            }
        }
        //生成链接字
        GameObject link = Instantiate(prefab_character, centerPos.transform);
        Character linkc = link.GetComponent<Character>();
        link.name = curr_char.ToString();
        link.GetComponent<Character>().SetCharacter(curr_char, markList[showList[0]]);
        //摆放文字
        link.transform.eulerAngles = new Vector3(0, -offsetAngle, 0);//###设置偏角（第一版-15）###
        link.transform.position = new Vector3(link.transform.position.x, link.transform.position.y, link.transform.position.z - 0.001f);//防重叠
        //设置颜色
        link.GetComponent<Character>().SetLinkColor();
        //字体浮动参数设置
        linkc.animate = linkCharMove;
        linkc.originPosition = link.transform.localPosition;
        linkc.offset = characterOffset;
        linkc.frequency = characterFrequency;



        //等诗句生成后回归原位来次慢慢移动
        showArea.eulerAngles -= rotVec;
        showArea.position -= moveVec;
        showArea.DORotate(showArea.eulerAngles+rotVec, 1);
        showArea.DOMove(showArea.position+moveVec, 1);

        //让本世代的诗句获取数据，准备移动
        scrollDis = 0;
        tmp_verseBoxes[tmp_verseBoxes.Count - 1].GetComponent<VerseMove>().GetVerse();
    }



    #region 外部调用的功能函数
    //变换开发者模式
    public void SwitchMode()
    {
        Transform btn_Switch = GameObject.Find("Btn_Switch").transform;
        var ori_clouds = GameObject.Find("Main Camera").GetComponentsInChildren<MeshRenderer>();
        GameObject dev_BG = GameObject.Find("Background_Dev");
        Image ori_panel = GameObject.Find("Img_Scroll").GetComponent<Image>();
        Image dev_panel = GameObject.Find("Img_devPanel").GetComponent<Image>();
        var allChars = showArea.GetComponentsInChildren<TMP_Text>();
        List<Transform> optionsList=new List<Transform>();
        for(int i=0;i<options.childCount;i++)
        {
            optionsList.Add(options.GetChild(i).GetComponent<Transform>());
        }

        ////////变成开发者模式////////
        if (!DevMode)
        {
            DevMode = true;
            //按钮旋转
            btn_Switch.DOLocalRotate(new Vector3(0, 0, 180), switchTime + 0.3f);
            //云层渐隐
            for (int i = 0; i < 3; i++)
            {
                ori_clouds[i].material.DOFade(0, switchTime - 0.3f);
            }
            //诗词板更换
            ori_panel.DOFade(0, switchTime);
            //文字变色
            characterColor = new Color(1, 1, 1, 1);
            for (int i = 1; i < allChars.Length; i++)//第一个字是红字，所以从1开始了，但直接进开发者模式会出bug
            {
                allChars[i].DOColor(new Color(1, 1, 1, allChars[i].color.a), switchTime - 0.2f);
            }
            if (allChars[0].transform.parent.parent.name != "CenterPos")
                allChars[0].DOColor(new Color(1, 1, 1, allChars[0].color.a), switchTime - 0.2f);

            //纯色背景出现
            dev_BG.GetComponent<Image>().DOFade(1, switchTime - 0.2f);
            dev_BG.transform.DOLocalMove(new Vector3(0, 0, 0), switchTime+0.2f);
            dev_BG.transform.DOScale(new Vector3(5, 5, 1), switchTime+0.2f);//0.7
            //功能板开启
            dev_panel.transform.DOLocalMove(new Vector3(723, 25, -13), switchTime-0.2f);
            for(int i=0; i<6;i++)
            {
                optionsList[i].DOLocalMoveX(250 + 25 * i, 0.3f + switchTime * 0.1f * i);
            }
            for (int i = 6; i < 10; i++)
            {
                optionsList[i].DOLocalMoveX(150 + 50 * i, switchTime * 0.15f * i);
            }
        }
        ////////变成一般模式////////
        else
        {
            DevMode = false;
            //按钮旋转
            btn_Switch.DOLocalRotate(new Vector3(0, 0, 360), switchTime + 0.3f);
            //云层渐显
            ori_clouds[0].material.DOFade(0.47f, switchTime);//front120
            ori_clouds[1].material.DOFade(0.588f, switchTime);//150
            ori_clouds[2].material.DOFade(0.392f, switchTime);//100
            //诗词板更换
            ori_panel.DOFade(0.666f, switchTime);
            //文字变色
            characterColor = new Color(0, 0, 0, 1);
            for (int i = 1; i < allChars.Length; i++)
            {
                allChars[i].DOColor(new Color(0, 0, 0, allChars[i].color.a), switchTime - 0.2f);
            }
            if (allChars[0].transform.parent.parent.name != "CenterPos")
                allChars[0].DOColor(new Color(0, 0, 0, allChars[0].color.a), switchTime - 0.2f);

            //纯色背景消失
            dev_BG.GetComponent<Image>().DOFade(0, switchTime);
            dev_BG.transform.DOLocalMove(new Vector3(-1100, -700, 0), switchTime);
            dev_BG.transform.DOScale(new Vector3(1, 1, 1), switchTime-0.2f);
            //功能板关闭
            dev_panel.transform.DOLocalMove(new Vector3(1200, 1370, -110), switchTime);
            /*
            dev_panel.transform.DOLocalRotate(new Vector3(0, 0, 0), 1);
            dev_panel.transform.DOScale(new Vector3(1, 1, 1), 1);*/
            for(int i=0;i<optionsList.Count;i++)
            {
                optionsList[i].DOLocalMoveX(1200, (switchTime - switchTime * 0.1f * i) * 0.4f);
            }
        }



    }
    //
    public void ChangeVerseNum()
    {
        verseNum = int.Parse(options.Find("verseNum").GetComponent<InputField>().text);
        //摧毁原本代诗句，并从列表中删除
        Destroy(tmp_verseBoxes[tmp_verseBoxes.Count - 1].gameObject);
        tmp_verseBoxes.Remove(tmp_verseBoxes[tmp_verseBoxes.Count - 1]);
        //重新生成本代诗句
        RandomShowVerseList();
    }
    public void ChangeExistingVersesAmount()
    {
        existingVersesAmount = int.Parse(options.Find("existingVersesAmount").GetComponent<InputField>().text);
        if (existingVersesAmount < 1)
            existingVersesAmount = 1;
        for (int i = 0; i < tmp_verseBoxes.Count - existingVersesAmount; i++)
            Destroy(tmp_verseBoxes[tmp_verseBoxes.Count - existingVersesAmount - 1 - i].gameObject);
        /*for (int i = 0; i < tmp_verseBoxes.Count - existingVersesAmount; i++)
            tmp_verseBoxes[tmp_verseBoxes.Count - existingVersesAmount - 1 - i].gameObject.SetActive(false);
        for (int i = 0; i < existingVersesAmount - tmp_verseBoxes.Count; i++)//有点问题，先不改了
            tmp_verseBoxes[tmp_verseBoxes.Count - existingVersesAmount - 1 - i].gameObject.SetActive(true);*/
    }
    public void ChangeLinkPosition()
    {
        linkPosition = new Vector3(-3, 1.5f, 0.1f * float.Parse(options.Find("linkPosition").GetComponent<InputField>().text));
        //摧毁原本代诗句，并从列表中删除
        Destroy(tmp_verseBoxes[tmp_verseBoxes.Count - 1].gameObject);
        tmp_verseBoxes.Remove(tmp_verseBoxes[tmp_verseBoxes.Count - 1]);
        //重新生成本代诗句
        ShowVerse();
    }
    public void ChangeOffsetAngle()
    {
        offsetAngle = (int)(int.Parse(options.Find("offsetAngle").GetComponent<InputField>().text) * 0.5f);
        //摧毁原本代诗句，并从列表中删除
        Destroy(tmp_verseBoxes[tmp_verseBoxes.Count - 1].gameObject);
        tmp_verseBoxes.Remove(tmp_verseBoxes[tmp_verseBoxes.Count - 1]);
        //重新生成本代诗句
        ShowVerse();
    }
    public void ChangeSubTransAmount()
    {
        subTransAmount = float.Parse(options.Find("subTransAmount").GetComponent<InputField>().text);
        //摧毁原本代诗句，并从列表中删除
        Destroy(tmp_verseBoxes[tmp_verseBoxes.Count - 1].gameObject);
        tmp_verseBoxes.Remove(tmp_verseBoxes[tmp_verseBoxes.Count - 1]);
        //重新生成本代诗句
        ShowVerse();
    }

    public void ChangeFloatingAmount() 
    {
        float floatingAmount = float.Parse(options.Find("floatingAmount").GetComponent<InputField>().text);//初始为1
        characterOffset = new Vector3(0.3f* floatingAmount, 0.3f* floatingAmount, 0.3f* floatingAmount);
        characterFrequency = 2 * floatingAmount;
        //摧毁原本代诗句，并从列表中删除
        Destroy(tmp_verseBoxes[tmp_verseBoxes.Count - 1].gameObject);
        tmp_verseBoxes.Remove(tmp_verseBoxes[tmp_verseBoxes.Count - 1]);
        //重新生成本代诗句
        ShowVerse();
    }

    public void ChangeHideOtherVerses()
    {
        hideOtherVerses = !hideOtherVerses;
        if (hideOtherVerses)
            options.Find("hideOtherVerses").transform.Find("whiteBlock").transform.DOScaleX(0, 0.3f);
        else
            options.Find("hideOtherVerses").transform.Find("whiteBlock").transform.DOScaleX(1, 0.3f);
    }
    public void ChangeLinkCharMove()
    {
        linkCharMove = !linkCharMove;
        if (linkCharMove)
            options.Find("linkCharMove").transform.Find("whiteBlock").transform.DOScaleX(0, 0.3f);
        else
            options.Find("linkCharMove").transform.Find("whiteBlock").transform.DOScaleX(1, 0.3f);
    }
    public void ChangeCurrVerseMove()
    {
        currVerseMove = !currVerseMove;
        if (currVerseMove)
            options.Find("currVerseMove").transform.Find("whiteBlock").transform.DOScaleX(0, 0.3f);
        else
            options.Find("currVerseMove").transform.Find("whiteBlock").transform.DOScaleX(1, 0.3f);
    }
    public void ChangePreVerseMove()
    {
        preVerseMove = !preVerseMove;
        if (preVerseMove)
            options.Find("preVerseMove").transform.Find("whiteBlock").transform.DOScaleX(0, 0.3f);
        else
            options.Find("preVerseMove").transform.Find("whiteBlock").transform.DOScaleX(1, 0.3f);
    }

    
    //随机按键
    public void ReRadom()
    {
        //清空当前verseBox
        for (int i = 0; i < tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.childCount; i++)
        {
            Destroy(tmp_verseBoxes[tmp_verseBoxes.Count - 1].transform.GetChild(i).gameObject);
        }

        if (tmp_verseBoxes.Count <= 1)
        {//只有初始时重新随机
            Destroy(tmp_verseBoxes[0].transform.GetChild(0).gameObject);
            Init();
        }
        else
        {
            RandomShowVerseList();
        }

    }
    #endregion
}
