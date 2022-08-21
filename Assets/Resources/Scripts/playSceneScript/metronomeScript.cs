using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class metronomeScript : MonoBehaviour
{
    AudioSource playTik;
    public AudioClip tik;
    public InputField bpmInput;

    int bpm = 60;

    float tikTime = 0.0f;
    float nextTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        playTik = GetComponent<AudioSource>();
        tikTime = 60f / bpm;
        bpmInput.onSubmit.AddListener(delegate { setBPM(bpmInput); });
    }

    // Update is called once per frame
    void Update()
    {
        nextTime += Time.deltaTime;

        if (nextTime >= tikTime)
        {
            StartCoroutine(PlayTik(tikTime));
            nextTime -= tikTime;
        }
    }

    public void setBPM(InputField input)
    {
        if (Int32.TryParse(input.text, out int _bpm))
        {
            Debug.Log(_bpm);
            bpm = _bpm;
            tikTime = 60f / bpm;
            nextTime = 0;
        }
        else
        {
            Debug.LogError("BPM이 숫자가 아닙니다");
        }
    }

    IEnumerator PlayTik(float tikTime)
    {
        playTik.PlayOneShot(tik);
        yield return null;
    }
}
