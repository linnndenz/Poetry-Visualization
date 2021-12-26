using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    public float bgSpeed;
    private float bgPos;
    public Renderer bgRend;
    // Start is called before the first frame update
    void Start()
    {
        bgRend = GetComponent<MeshRenderer>();
    }
    private void FixedUpdate()
    {
        bgPos += bgSpeed;
        Scroll(bgPos);
    }
    public void Scroll(float xx)
    {
        //bgRend.material.mainTextureOffset += new Vector2(bgSpeed * Time.deltaTime, 0f);
        bgRend.material.mainTextureOffset = new Vector2(xx, 0f);
    }
    
}
