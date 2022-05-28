using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MusicManagerScript : MonoBehaviour
{
    AudioSource mSource;

    public AudioClip[] musics;
    [HideInInspector] public float bpm = 120f;
    [HideInInspector] public int Offset = 0;
    [SerializeField] InputField offsetInput, bpmInput, beatInput, speedInput;
    [SerializeField] Button playButton, stopButton;
    [SerializeField] Dropdown musicList;
    [SerializeField] Toggle metronomeToggle;
    [HideInInspector] public int selectedMusic = 0;
    bool playStatus = false;

    [SerializeField] GameObject metronome;
    AudioSource metAudioSource;
    [SerializeField] GameObject gridObj;
    LineRenderScript gridScript;
    float musicSpeed = 1f;
    int currentNode = -1;

    // Start is called before the first frame update
    void Start()
    {
        mSource = GetComponent<AudioSource>();
        mSource.clip = musics[selectedMusic];
        SettingReload();

        bpmInput.onSubmit.AddListener(delegate { SetBpm(); });
        offsetInput.onSubmit.AddListener(delegate { SetOffset(); });
        speedInput.onSubmit.AddListener(delegate { SetSpeed(); });
        playButton.onClick.AddListener(delegate { MusicPlay(); });
        stopButton.onClick.AddListener(delegate { MusicStop(); });
        musicList.onValueChanged.AddListener(delegate { SelectMusic(); });

        metAudioSource = metronome.GetComponent<AudioSource>();

        gridScript = gridObj.GetComponent<LineRenderScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (mSource.isPlaying)
        {
            float playTime = mSource.time;
            if (currentNode + 1 < gridScript.nodeList.Count)
            {
                if ((playTime - (Offset * 0.001f)) * gridScript.playScrollSpeed >= gridScript.nodeList[currentNode + 1].offset)
                {
                    if (metronomeToggle.isOn)
                        metAudioSource.Play();
                    currentNode++;
                }
            }
        }
    }

    void SetBpm()
    {
        if (float.TryParse(bpmInput.text, out float _bpm))
        {
            bpm = _bpm;
        }
    }

    void SetOffset()
    {
        if (Int32.TryParse(offsetInput.text, out int _offset))
        {
            Offset = _offset;
        }
    }

    void SetSpeed()
    {
        if (float.TryParse(speedInput.text, out float _speed))
        {
            musicSpeed = _speed;
        }
    }

    void MusicPlay()
    {
        if (selectedMusic > 0)
        {
            if (playStatus == false)
            {
                // 재생 시점
                float playPoint = -(gridScript.yOffset / gridScript.playScrollSpeed);
                mSource.time = playPoint;
                mSource.Play();
                playStatus = true;

                // 현재 노드
                // yOffset으로 계산 가능한데 그래도 nodeOffset을 고려해야 하기 때문에 그냥 계산함
                float currentPoint = (mSource.time - (Offset * 0.001f)) * gridScript.playScrollSpeed;
                int i;
                for (i = 0; i < gridScript.nodeList.Count; i++)
                {
                    if (currentPoint < gridScript.nodeList[i].offset)
                    {
                        currentNode = i - 1;
                        break;
                    }
                }
                if (i == gridScript.nodeList.Count)
                    currentNode = i - 1;
            }
            else
            {
                mSource.Pause();
                playStatus = false;
            }
        }
        
    }

    void MusicStop()
    {
        if (selectedMusic > 0)
        {
            mSource.Stop();
            playStatus = false;
            gridScript.yOffset = 0;
            gridScript.NoteRelocate();
            gridScript.RenewSpectrum();
            // gridScript.ListMove();
            gridScript.NodeNumberReset();
        }
    }

    void SelectMusic()
    {
        if (musicList.value > 0)
        {
            if (selectedMusic != musicList.value)
            {
                selectedMusic = musicList.value;
                mSource.clip = musics[selectedMusic];
                SettingReload();
                gridScript.GridReset();
                gridScript.MakeSpectrum();
            }
        }
        MusicStop();
    }

    void SettingReload()
    {
        bpmInput.text = "120";
        beatInput.text = "4";
        offsetInput.text = "0";
    }

    public void musicReset()
    {
        SetBpm();
        SetOffset();
        MusicStop();
        selectedMusic = musicList.value;
        mSource.clip = musics[selectedMusic];
    }
}
