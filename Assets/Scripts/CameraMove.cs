using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CameraMove : MonoBehaviour
{
    public void Zoom()
    {
        StartCoroutine("move");//携程加插件，消耗max
    }
    IEnumerator move()
    {
        transform.DOMoveZ(-8, 0.5f);
        yield return new WaitForSeconds(0.5f);
        transform.DOMoveZ(-10f, 0.5f);
    }

}
