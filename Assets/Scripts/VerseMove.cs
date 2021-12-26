using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class VerseMove : MonoBehaviour
{
    private GamePlay game;
    private float scrollDis = 0;

    public List<GameObject> verses = new List<GameObject>();

    private float temp_angle = -90;//临时量
    const float Pi = 3.1415926535897f;
    private void Awake()
    {
        game = GameObject.Find("GamePlay").GetComponent<GamePlay>();
    }

    //在VerseBox里找诗句填充verses
    public void GetVerse()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).tag == "Verse")
                verses.Add(transform.GetChild(i).gameObject);
        }
        OpenVerse();//及时展开诗句，防止重叠
    }

    //旋转诗句及画面效果,在GamePlay里调用了
    public void ScrollVerse()
    {
        scrollDis = game.scrollDis;

        for (int i = 0; i < verses.Count; i++)
        {
            //计算旋转角度
            temp_angle = (1f + 2f * i) / (2f * verses.Count) + scrollDis;
            temp_angle = loopNum(temp_angle, 0, 1);//0~1线性
            temp_angle = (Mathf.Sin(Pi * (temp_angle-0.5f))+1)*0.5f;//0~1三角
            temp_angle = Mathf.Cos(Pi * temp_angle);//1~-1三角
            temp_angle = -90 * temp_angle;//-90~90三角
            //temp_angle = loopNum(-90 * temp_angle, -90, 90);//-90~90三角
            verses[i].transform.localEulerAngles = new Vector3(0, 0, temp_angle);
            //透明度
            var characters= verses[i].GetComponentsInChildren<Character>();
            for (int j = 0; j < characters.Length; j++)
            {
                if (verses[i].transform.localEulerAngles.z < 180)
                    characters[j].SetTrans(1 - verses[i].transform.localEulerAngles.z / 90);
                else
                    characters[j].SetTrans(verses[i].transform.localEulerAngles.z / 90 - 3);
            }
            //碰撞体
            var boxes = verses[i].GetComponentsInChildren<BoxCollider>();
            for (int j = 0; j < boxes.Length; j++)
            {
                if (verses[i].transform.localEulerAngles.z < 45 || verses[i].transform.localEulerAngles.z > 315)
                {
                    boxes[j].enabled = true;
                }
                else
                    boxes[j].enabled = false;
            }
        }
    }

    //展开诗句的效果
    public void OpenVerse()
    {
        scrollDis = game.scrollDis;

        for (int i = 0; i < verses.Count; i++)
        {
            //计算旋转角度
            temp_angle = (1f + 2f * i) / (2f * verses.Count) + scrollDis;
            temp_angle = Mathf.Cos(Pi * loopNum(temp_angle, 0, 1));
            temp_angle = -90 * temp_angle;
            verses[i].transform.localEulerAngles = new Vector3(0, 0, temp_angle);

            //透明度
            var characters = verses[i].GetComponentsInChildren<Character>();
            for (int j = 0; j < characters.Length; j++)
            {
                if (verses[i].transform.localEulerAngles.z < 180)
                    characters[j].SetTrans(1 - verses[i].transform.localEulerAngles.z / 90);
                else
                    characters[j].SetTrans(verses[i].transform.localEulerAngles.z / 90 - 3);
            }
            //碰撞体
            var boxes = verses[i].GetComponentsInChildren<BoxCollider>();
            for (int j = 0; j < boxes.Length; j++)
            {
                if (verses[i].transform.localEulerAngles.z < 45 || verses[i].transform.localEulerAngles.z > 315)
                {
                    boxes[j].enabled = true;
                }
                else
                    boxes[j].enabled = false;
            }
            //实际旋转
            verses[i].transform.localEulerAngles = new Vector3(0, 0, 180);
            verses[i].transform.DOLocalRotate(new Vector3(0, 0, temp_angle), 1);

        }

    }
    //将数 f 限制在 i1 到 i2 之间
    public float loopNum(float f,int i1,int i2)
    {
        while (f > i2)
            f-=i2-i1;
        while (f < i1)
            f += i2 - i1;
        return f;
    }
}
