using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using DG.Tweening;

public class Character : MonoBehaviour
{
    [SerializeField] private Vector2 m_mark;
    [SerializeField] private char m_character;
    public Vector2 M_mark { get { return m_mark; } }
    public char M_character { get { return m_character; } }

    //组件
    private TMP_Text text;

    //漂浮参数
    public Vector3 offset;  //最大的偏移量
    public float frequency;  //振动频率
    public Vector3 originPosition; //记录物体的原始坐标
    private float tick;      // 用于计算当前时间量（可以理解成函数坐标轴x轴）
    public bool animate;    //用于控制物体漂浮动画的animate值

    void Awake()
    {
        //初始化
        text = GetComponentInChildren<TMP_Text>();
        //如果没有设置频率或者设置频率为0则自动记录成1
        offset = new Vector3(0.05f, 0.05f, 0);
        frequency = 0.5f;
        originPosition = transform.localPosition;
        tick = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
        //计算振幅
        //amplitude = 2 * Mathf.PI / frequency;
        animate = true;
    }

    
    public void Play()

    {

        transform.localPosition = originPosition;

        animate = true;

    }

    public void Stop()

    {

        transform.localPosition = originPosition;

        animate = false;

    }

    private void FixedUpdate()
    {
        if (animate)
        {
            tick = tick + Time.fixedDeltaTime * frequency;
            //计算下一个偏移量
            var amp = new Vector3(Mathf.Cos(tick) * offset.x, Mathf.Sin(tick) * offset.y, Mathf.Cos(tick) * offset.z);
            // 更新坐标
            transform.localPosition = originPosition + amp*0.1f;
            transform.localEulerAngles = amp*15;
        }
    }

    //设置单字
    public void SetCharacter(char c, Vector2 m)
    {
        m_character = c;
        m_mark = m;

        //显示
        text.text = m_character.ToString();
    }

    //设置文字透明度
    public void SetTrans(float t)
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, t);
        //text.DOFade(t, 0.3f);
    }

    //减少文字透明度
    public void SubTrans(float t)
    {
        text.DOFade(text.color.a - t, 1);
        //text.color -= new Color(0, 0, 0, t);
    }

    //设置文字颜色
    public void SetColor(float r, float g, float b)
    {
        text.color = new Color(r, g, b, text.color.a);
    }

    //设置链接字格式
    public void SetLinkColor()
    {
        text.color = new Color(0.78f, 0.243f, 0.227f, 1);
        text.fontStyle = FontStyles.Bold;
    }
}
