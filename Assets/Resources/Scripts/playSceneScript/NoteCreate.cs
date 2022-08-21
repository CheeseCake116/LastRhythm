using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class NoteCreate : MonoBehaviour
{
    [SerializeField] GameObject notePre;
    [SerializeField] Dropdown loadList;
    [SerializeField] Button startButton, stopButton;
    [SerializeField] InputField speedInput;
    [SerializeField] ParticleSystem[] particles;

    string loadfilename;
    int bgm = -1;
    float bpm = -1;
    float offset = -1;
    float noteSpeed = 8;
    AudioSource mSource;
    [SerializeField] AudioClip[] musics;
    Coroutine cor;

    // Start is called before the first frame update
    void Start()
    {
        mSource = GetComponent<AudioSource>();
        for (int i = 0; i < 6; i++)
        {
            NoteContainer.noteStack[i] = new List<NoteMovement>();
        }
        loadList.onValueChanged.AddListener(delegate { SetMusic(); });
        startButton.onClick.AddListener(delegate { MusicPlay(); });
        stopButton.onClick.AddListener(delegate { MusicStop(); });
        speedInput.text = noteSpeed.ToString();
        speedInput.onSubmit.AddListener(delegate { SetSpeed(); });
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void SetMusic()
    {
        // 드롭다운에서 고른 파일명 가져오기
        loadfilename = loadList.GetComponent<MusicListScript>().GetSelectedFile();

        // 첫번째 "불러오기" 선택시 빈 문자열 반환. 이 경우 불러오지 않음
        if (loadfilename == "")
            return;

        NoteMovement[] longNoteScriptStack = new NoteMovement[6];

        // 파일 다루는 거라 try문
        try
        {
            FileStream file = new FileStream("./patterns/" + loadfilename + ".lrs", FileMode.Open);
            StreamReader fileLD = new StreamReader(file);

            // 노트 제거
            NoteContainer.NoteClear();

            // 텍스트 파일 리딩 시작
            string str = fileLD.ReadToEnd();
            string[] firstData = str.Split(new char[] { '*' }); // "", "Header...", "Data..."

            // Header Parsing
            string[] headerData = firstData[1].Split(new char[] { '#' }); // "Header", "BGM ...", ...
            for (int i = 0; i < headerData.Length; i++)
            {
                headerData[i] = headerData[i].Trim();
            }

            if (headerData[0] != "Header")
                return;

            for (int i = 1; i < headerData.Length; i++)
            {
                string[] headerLine = headerData[i].Split(new char[] { ' ' });
                if (headerLine[0] == "BGM")
                {
                    if (Int32.TryParse(headerLine[1], out int _bgm))
                    {
                        bgm = _bgm;
                        mSource.clip = musics[bgm];
                    }
                }
                else if (headerLine[0] == "BPM")
                {
                    if (float.TryParse(headerLine[1], out float _bpm))
                    {
                        bpm = _bpm;
                    }
                }
                else if (headerLine[0] == "OFFSET")
                {
                    if (Int32.TryParse(headerLine[1], out int _offset))
                    {
                        offset = _offset;
                    }
                }
            }

            // NoteData Parsing
            string[] noteData = firstData[2].Split(new char[] { '#' }); // "NoteData", "00010 ...", ...
            for (int i = 0; i < noteData.Length; i++)
            {
                noteData[i] = noteData[i].Trim();
            }

            if (noteData[0] != "NoteData")
                return;

            for (int i = 1; i < noteData.Length; i++)
            {
                if (noteData[i] == "")
                    continue;

                string[] noteLine = noteData[i].Split(new char[] { ' ' });

                int node = -1;
                int player = -1;
                int line = -1;
                int beat = -1;

                if (Int32.TryParse(noteLine[0].Substring(0, 3), out int _node))
                {
                    node = _node;
                }
                if (Int32.TryParse(noteLine[0].Substring(3, 1), out int _player))
                {
                    player = _player;
                }
                if (Int32.TryParse(noteLine[0].Substring(4, 1), out int _line))
                {
                    line = _line;
                }

                beat = noteLine[1].Length / 2;
                for (int pos = 0; pos < beat; pos++)
                {
                    string tempNote = noteLine[1].Substring(pos * 2, 2);
                    if (tempNote != "00")
                    {
                        if (player == 5) // 롱노트
                        {
                            if (tempNote != "LN")
                            {
                                NoteMovement instScript = createNote(line, node, beat, pos, offset);
                                instScript.noteMode = 2;
                                longNoteScriptStack[line] = instScript;
                                NoteContainer.noteStack[line].Add(instScript);
                            }
                            else
                            {
                                createLongNote(longNoteScriptStack[line], line, node, beat, pos, offset);
                                longNoteScriptStack[line] = null;
                            }
                        }
                        else // 일반노트
                        {
                            NoteMovement instScript = createNote(line, node, beat, pos, offset);
                            instScript.noteMode = 1;
                            NoteContainer.noteStack[line].Add(instScript);
                        }
                    }
                }
            }
        }
        catch(Exception e)
        {
            Debug.LogError(e);
            Debug.LogError("불러오기 실패");
        }
    }

    private void MusicPlay()
    {
        cor = StartCoroutine(MusicPlayDelay(3));
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < NoteContainer.noteStack[i].Count; j++)
            {
                NoteContainer.noteStack[i][j].StartMove();
            }
        }
    }

    private void MusicStop()
    {
        // 노트 제거
        NoteContainer.NoteClear();

        if (cor != null)
            StopCoroutine(cor);

        if (mSource.isPlaying)
            mSource.Stop();
    }

    private void SetSpeed()
    {
        if (Int32.TryParse(speedInput.text, out int _speed))
        {
            noteSpeed = _speed;
        }
    }

    IEnumerator MusicPlayDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        mSource.Play();
    }

    private NoteMovement createNote(int line, int node, int beat, int pos, float offset)
    {
        while (true)
        {
            // for 5d test
            // noteSpeed = UnityEngine.Random.Range(5, 10);
            GameObject inst = Instantiate(notePre);
            NoteMovement instScript = inst.GetComponent<NoteMovement>();
            float baseY = -3;

            float arriveTiming = (offset * 0.001f) + ((60f / bpm) * ((float)node + (float)pos / beat));
            float yOffset = baseY + noteSpeed * (3f + arriveTiming);

            inst.transform.position = new Vector3(line - 7.5f, yOffset, -1);
            instScript.speed = noteSpeed;
            instScript.startTiming = -3f;
            instScript.endTiming = arriveTiming;
            instScript.line = line;
            instScript.bpm = bpm;
            instScript.noteCreateScript = this;

            return instScript;
        }
    }

    private void createLongNote(NoteMovement note, int line, int node, int beat, int pos, float offset)
    {
        float finishTiming = (offset * 0.001f) + ((60f / bpm) * (node + (float)pos / beat));
        note.finishTiming = finishTiming;
        note.noteResize();
    }

    public void particleStart(int _line)
    {
        particles[_line].Play();
    }
}
