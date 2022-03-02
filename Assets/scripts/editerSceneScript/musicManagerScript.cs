using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class musicManagerScript : MonoBehaviour
{
    AudioSource mSource;
    public AudioClip[] musics;
    public float[] bpms;
    public int[] Offsets;
    public InputField offsetInput;
    public InputField bpmInput;
    public InputField beatInput;
    public Button playButton;
    public Button stopButton;
    public Dropdown musicList;
    public GameObject waveForm;
    waveFormScript wfScript;
    public int selectedMusic = 0;
    bool playStatus = false;

    public GameObject metronome;
    AudioSource metAudioSource;
    float tikTime;
    float nextTime;

    // Start is called before the first frame update
    void Start()
    {
        bpms = new float[10]
        {
            144,
            120,
            142,
            149.5f,
            0,
            0,
            0,
            0,
            0,
            0
        };
        Offsets = new int[10]
        {
            25,
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
        wfScript = waveForm.GetComponent<waveFormScript>();
        settingReload();

        bpmInput.onSubmit.AddListener(delegate { setBpm(); });
        offsetInput.onSubmit.AddListener(delegate { setOffset(); });
        playButton.onClick.AddListener(delegate { musicPlay(); });
        stopButton.onClick.AddListener(delegate { musicStop(); });
        musicList.onValueChanged.AddListener(delegate { SelectMusic(); });

        metAudioSource = metronome.GetComponent<AudioSource>();
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
            nextTime = Offsets[selectedMusic] / 1000;
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
    }

    void SelectMusic()
    {
        musicStop();
        selectedMusic = musicList.value;
        mSource.clip = musics[selectedMusic];
        wfScript.GetWaveform();
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
