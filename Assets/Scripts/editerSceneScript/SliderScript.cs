using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SliderScript : MonoBehaviour
{
    [HideInInspector] public LineRenderScript gridMngScript;
    [HideInInspector] public float yOffset = 0;
    [HideInInspector] public float height = 1;
    [HideInInspector] public AudioSource mSource;
    static readonly WaitForSeconds m_wait = new(0.1f);

    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(-8.75f, -4, -1);
    }

    private void Update()
    {
        if (gridMngScript && mSource != null && mSource.clip != null)
        {
            yOffset = gridMngScript.yOffset;
            height = mSource.clip.length * gridMngScript.playScrollSpeed;
            transform.position = new Vector3(-8.75f, -4.05f + 8.1f * -yOffset / height, -1);
        }
    }
}
