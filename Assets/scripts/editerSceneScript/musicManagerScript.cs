using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class musicManagerScript : MonoBehaviour
{
    AudioSource mSource;

    public AudioClip[] musics;
    float[] bpms;
    int[] Offsets;
    [SerializeField] InputField offsetInput;
    [SerializeField] InputField bpmInput;
    [SerializeField] InputField beatInput;
    [SerializeField] InputField nodeInput;
    [SerializeField] Button playButton;
    [SerializeField] Button stopButton;
    [SerializeField] Dropdown musicList;
    int selectedMusic = 0;
    bool playStatus = false;

    [SerializeField] GameObject metronome;
    AudioSource metAudioSource;
    [SerializeField] GameObject gridObj;
    lineRenderScript gridScript;
    float tikTime;
    float nextTime;

    // Start is called before the first frame update
    void Start()
    {

        bpms = new float[10]
        {
            144f,
            120f,
            142f,
            149.5f,
            120f,
            120f,
            120f,
            120f,
            120f,
            120f
        };
        Offsets = new int[10]
        {
            60,
            0,
            49,
            34,
            0,
            0,
            0,
            0,
            0,
            0
        };

        mSource = GetComponent<AudioSource>();
        mSource.clip = musics[selectedMusic];
        settingReload();

        bpmInput.onSubmit.AddListener(delegate { setBpm(); });
        offsetInput.onSubmit.AddListener(delegate { setOffset(); });
        playButton.onClick.AddListener(delegate { musicPlay(); });
        stopButton.onClick.AddListener(delegate { musicStop(); });
        musicList.onValueChanged.AddListener(delegate { SelectMusic(); });

        metAudioSource = metronome.GetComponent<AudioSource>();

        gridScript = gridObj.GetComponent<lineRenderScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mSource.isPlaying)
        {
            float playTime = mSource.time;
            if (playTime > nextTime)
            {
                metAudioSource.Play();
                nextTime += tikTime;
            }
        }
    }

    void setBpm()
    {
        if (float.TryParse(bpmInput.text, out float _bpm))
        {
            Debug.Log(_bpm);
            bpms[selectedMusic] = _bpm;
            tikTime = 60f / bpms[selectedMusic];
        }
    }

    void setOffset()
    {
        if (Int32.TryParse(offsetInput.text, out int _offset))
        {
            Debug.Log(_offset);
            Offsets[selectedMusic] = _offset;
            nextTime = Offsets[selectedMusic] * 0.001f;
        }
    }

    void musicPlay()
    {
        if (playStatus == false)
        {
            mSource.Play();
            playStatus = true;
        }
        else
        {
            mSource.Pause();
            playStatus = false;
        }
        
    }

    void musicStop()
    {
        mSource.Stop();
        playStatus = false;
        nextTime = 0f;
        gridScript.yOffset = 0;
        gridScript.noteRelocate();
    }

    void SelectMusic()
    {
        musicStop();
        selectedMusic = musicList.value;
        mSource.clip = musics[selectedMusic];
        settingReload();
    }

    void settingReload()
    {
        bpmInput.text = bpms[selectedMusic].ToString();
        beatInput.text = "4";
        offsetInput.text = Offsets[selectedMusic].ToString();

        tikTime = 60f / bpms[selectedMusic];
        nextTime = Offsets[selectedMusic] / 1000;
    }
}
