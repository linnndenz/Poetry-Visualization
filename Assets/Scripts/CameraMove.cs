using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraMove : MonoBehaviour
{
    public void Zoom()
    {
        StartCoroutine("move");//Я�̼Ӳ��������max
    }
    IEnumerator move()
    {
        transform.DOMoveZ(-8, 0.5f);
        yield return new WaitForSeconds(0.5f);
        transform.DOMoveZ(-10f, 0.5f);
    }

}
