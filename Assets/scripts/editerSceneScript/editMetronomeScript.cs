using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class editMetronomeScript : MonoBehaviour
{
    AudioSource mSource;
    public AudioClip tik;

    // Start is called before the first frame update
    void Start()
    {
        mSource = GetComponent<AudioSource>();
        mSource.clip = tik;
        // 재생은 musicManagerScript.Update()에서 재생
    }

    private void Update()
    {

    }
}
