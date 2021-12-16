using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
}
