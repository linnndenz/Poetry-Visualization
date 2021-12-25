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

    void Awake()
    {
        //初始化
        text = GetComponentInChildren<TMP_Text>();
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
        text.color = new Color(0,0,0,t);
        //text.DOFade(t, 0.3f);
    }
    //减少文字透明度
    public void SubTrans(float t)
    {
        text.DOFade(text.color.a - t, 1);
        //text.color -= new Color(0, 0, 0, t);
    }
    //设置链接字格式
    public void SetLinkColor()
    {
        text.color = new Color(0.78f, 0.243f, 0.227f, 1);
        text.fontStyle = FontStyles.Bold;
    }
}
