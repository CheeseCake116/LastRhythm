using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class LineRenderScript : MonoBehaviour
{
    int Col = 6;

    [HideInInspector] public float xOffset = 0, yOffset = 0, playScrollSpeed = 4f; // 초당 이동하는 거리
    [HideInInspector] public List<NodeInfo> nodeList = new();

    [SerializeField] Color nodeColor, beatColor, judgeBarColor, noColor, spectrumColor, sliderColor;
    [SerializeField] GameObject noteObj, musicMng, noteMng, wfObj, loadObj, verticalLine, judgebar, horizontal, horizontal2, slider_bar, spectrumLine;
    [SerializeField] InputField bpmInput, beatInput, offsetInput, zoomInput, speedInput, nodeBpmInput;
    [SerializeField] Toggle selectToggle, noteToggle, longNoteToggle, noteSoundToggle;
    [SerializeField] TextMeshPro nodeNumberObj;

    List<LineRenderer> spectrumLines = new();

    float nodeWidth = 1;
    float nodeHeight = 2;
    float cellHeight;

    float baseX = -8f;
    float baseY = -3f;
    float winHeight = 10f;

    int node = 5;
    int beat = 4;
    float nodeOffset = 0f;
    float zoom = 1; // zoom 1 -> playScrollSpeed 4
    float bpm = 120f;
    float musicSpeed = 1f;
    public NoteInfoScript selectedNote;
    public int selectedNode = -1;

    float scrollHeight = -1;
    float yOffsetLimit = 0;
    float sliderHeight;
    float sliderOffset;
    List<TextMeshPro> nodeNumbers = new();
    List<GameObject> horizontals = new();
    List<GameObject> horizontal2s = new();

    public AudioSource mSource;
    static Material lineMaterial;
    NoteManagerScript noteMngScript;
    List<GameObject> wfList = new();
    SliderScript sliderScript;

    static readonly WaitForEndOfFrame m_wait = new();
    static readonly WaitForSeconds m_WaitSeconds = new(0.1f);

    [HideInInspector] public int noteMode = 1; // 0 = 선택 1 = 노트 2 = 롱노트

    private void Start()
    {
        bpmInput.onSubmit.AddListener(delegate { SetBPM(); });
        beatInput.onSubmit.AddListener(delegate { SetBeat(); });
        offsetInput.onSubmit.AddListener(delegate { SetOffset(); });
        zoomInput.onSubmit.AddListener(delegate { SetZoomSize(); });
        speedInput.onSubmit.AddListener(delegate { SetSpeed(); });
        nodeBpmInput.onSubmit.AddListener(delegate { SetNodeBPM(); });
        selectToggle.onValueChanged.AddListener(delegate { SetNoteMode(0); });
        noteToggle.onValueChanged.AddListener(delegate { SetNoteMode(1); });
        longNoteToggle.onValueChanged.AddListener(delegate { SetNoteMode(2); });
        zoomInput.text = zoom.ToString();
        speedInput.text = musicSpeed.ToString();

        playScrollSpeed = 4 * zoom;
        nodeHeight = playScrollSpeed / (bpm / 60f);
        cellHeight = nodeHeight / beat;

        mSource = musicMng.GetComponent<AudioSource>();
        noteMngScript = noteMng.GetComponent<NoteManagerScript>();
        NodeNumberReset();

        float xos = xOffset + baseX;
        float yos = yOffset + baseY + nodeOffset * playScrollSpeed;

        for (int i = 0; i <= Col; i++)
        {
            /*GL.Color(nodeColor);
            GL.Vertex3(xos + (i * nodeWidth), -5, 0);
            GL.Vertex3(xos + (i * nodeWidth), 5, 0);*/
            GameObject verticalObj = Instantiate(verticalLine);
            verticalObj.transform.position = new Vector3(xos + (i * nodeWidth), 0, -0.1f);
        }
        GameObject judgeObj = Instantiate(judgebar);
        judgeObj.transform.position = new Vector3(-5, -3, -0.2f);

        SliderScript sliderScript = slider_bar.GetComponent<SliderScript>();
        sliderScript.mSource = mSource;
        sliderScript.gridMngScript = this;
    }

    private void Update()
    {
        // 플레이 중에 자동 스크롤
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (mSource.isPlaying)
        {
            float playTime = mSource.time;
            yOffset = playTime * -playScrollSpeed;
            NoteRelocate();
            // ListMove();
            RenewSpectrum();
            NodeNumberReset();
        }

        // 플레이 중이 아닐 때 직접스크롤
        else
        {
            if (scroll != 0f)
            {
                yOffset += scroll * -playScrollSpeed;
                if (yOffset > 0)
                    yOffset = 0;
                else if (yOffset < yOffsetLimit)
                    yOffset = yOffsetLimit;

                NoteRelocate();
                RenewSpectrum();
                // ListMove();
                NodeNumberReset();
            }
        }
        

        // 왼쪽 마우스 클릭
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float xos = xOffset + baseX;
            float yos = yOffset + baseY + (nodeOffset * playScrollSpeed);

            // 그리드 안을 클릭했을 때
            if (pos.x >= xos && pos.x <= xos + (6 * nodeWidth))
            {
                // 선택한 노드 찾기
                int _node = 0;

                float _yoffset = pos.y - yos;
                bool _switch = false;
                if (nodeList.Count > 0)
                {
                    for (int i = 1; i < nodeList.Count; i++)
                    {
                        if (_yoffset < nodeList[i].offset)
                        {
                            _node = i - 1;
                            _switch = true;
                            break;
                        }
                    }
                    if (!_switch)
                        _node = nodeList.Count - 1;
                }

                // 마디 선택 모드
                if (noteMode == 0)
                {
                    SelectNode(_node);
                }

                // 노트 생성 모드, 롱노트 생성 모드
                else
                {
                    float _offset = nodeList[_node].offset;
                    float _nodeHeight = nodeList[_node].height;

                    int _line = (int)((pos[0] - xos) / nodeWidth);
                    int _pos = (int)((pos.y - (yos + _offset)) / (_nodeHeight / beat));

                    NoteInfo createdNote = new NoteInfo(null, _line, _node, beat, _pos);
                    if (noteMngScript.IsThereNote(createdNote))
                        return;

                    float noteX = xos + _line * nodeWidth;
                    float noteY = yos + _offset + ((float)_pos / beat * _nodeHeight);

                    Vector3 notePos = new Vector3(noteX, noteY, -1);
                    GameObject inst = Instantiate(noteObj);
                    inst.transform.position = notePos;
                    createdNote.inst = inst;

                    NoteInfoScript noteScript = inst.GetComponent<NoteInfoScript>();
                    noteScript.musicMng = musicMng;
                    noteScript.noteMngScript = noteMngScript;
                    noteScript.gridMngScript = this;
                    noteScript.noteMode = noteMode;
                    noteScript.note = createdNote;
                    noteScript.noteNumber = "01";
                    noteScript.noteSoundToggle = noteSoundToggle;

                    noteMngScript.AddNote(createdNote);
                }
            }

            // 스크롤바를 클릭했을 때
            if (pos.x >= -9 && pos.x <= -8.5 && pos.y >= -4.5 && pos.y <= 4.5)
            {
                StartCoroutine(SliderScroll());
            }
        }
    }

    static void CreateLineMaterial()
    {
        if (!lineMaterial)
        {
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
        }
    }

    /*private void OnDrawGizmos()
    {
        // Draw Scene
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.Flush();
        GL.PushMatrix();

        DrawGrid();

        GL.PopMatrix();
    }*/

    /*private void OnRenderObject()
    {
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.Flush();
        GL.PushMatrix();

        DrawGrid();

        GL.PopMatrix();
    }*/

    void DrawGrid()
    {
        GL.Begin(GL.LINES);

        float xos = xOffset + baseX;
        float yos = yOffset + baseY + nodeOffset * playScrollSpeed;
        // float yos = (yOffset + baseY + nodeOffset * playScrollSpeed) % nodeHeight - ((int)(5 / nodeHeight) + 1) * nodeHeight;
        node = (int)(winHeight / nodeHeight) + 3;

        if (nodeList.Count > 0)
        {
            float _nodeHeight = nodeHeight;

            // 시작위치 찾기
            int startPos = 0;
            for (int i = 1; i < nodeList.Count; i++)
            {
                if (nodeList[i].offset + yos > -5)
                {
                    startPos = i - 1;
                    break;
                }
            }

            // 종료지점 찾기
            int endPos = nodeList.Count - 1;
            for (int i = startPos; i < nodeList.Count; i++)
            {
                if (nodeList[i].offset + yos > 5)
                {
                    endPos = i - 1;
                }
            }

            // row
            for (int i = startPos; i <= endPos; i++)
            {
                GL.Color(nodeColor);
                GL.Vertex3(xos, yos + nodeList[i].offset, 0);
                GL.Vertex3(xos + (float)(Col * nodeWidth), yos + nodeList[i].offset, 0);

                _nodeHeight = nodeList[i].height;

                for (int j = 1; j < beat; j++)
                {
                    GL.Color(beatColor);
                    GL.Vertex3(xos, yos + nodeList[i].offset + ((float)j / beat * _nodeHeight), 0);
                    GL.Vertex3(xos + (float)(Col * nodeWidth), yos + nodeList[i].offset + ((float)j / beat * _nodeHeight), 0);
                }
            }

            GL.Color(nodeColor);
            GL.Vertex3(xos, yos + nodeList[endPos].offset + _nodeHeight, 0);
            GL.Vertex3(xos + (float)(Col * nodeWidth), yos + nodeList[endPos].offset + _nodeHeight, 0);
        }

        // col
        /*for (int i = 0; i <= Col; i++)
        {
            GL.Color(nodeColor);
            GL.Vertex3(xos + (i * nodeWidth), -5, 0);
            GL.Vertex3(xos + (i * nodeWidth), 5, 0);
        }*/

        

        GL.End();

        // 스크롤바 그리기
        /*DrawSquareLine(baseX - 1, 5, baseX - 0.5f, -5, 0.03f, nodeColor);

        if (scrollHeight > 0) // 스펙트럼 만들 때 할당됨
        {
            sliderOffset = (-yOffset / scrollHeight * (10 - sliderHeight)) - 5; // 화면높이 10
            DrawSquareLine(baseX - 1.01f, sliderHeight + sliderOffset, baseX - 0.49f, sliderOffset, 0.07f, sliderColor);
        }*/

        // 판정바 그리기
        /*GL.Begin(GL.QUADS);
        GL.Color(judgeBarColor);
        GL.Vertex3(xos, -3, 0);
        GL.Vertex3(xos + 6, -3, 0);
        GL.Vertex3(xos + 6, -3.1f, 0);
        GL.Vertex3(xos, -3.1f, 0);

        GL.End();*/

        // 마디 외곽선 그리기
        DrawNodeLine();
        // DrawNoteLine();
    }

    void DrawNoteLine()
    {
        if (selectedNote != null)
        {
            NoteInfo note = selectedNote.note;
            int line = note.line;
            int node = note.node;
            int beat = note.beat;
            int pos = note.pos;

            float noteHeight = selectedNote.longNoteHeight;

            float xos = xOffset + baseX;
            float yos = yOffset + baseY;

            float size = 0.05f;
            float x1 = xos + line;
            float x2 = x1 + 1;
            float y2 = yos + (node + (float)pos / beat) * nodeHeight;
            float y1 = y2 + noteHeight + 0.2f;

            DrawSquareLine(x1 - size, y1 + size, x2 + size, y2 - size, size, judgeBarColor);
        }
    }

    void DrawNodeLine()
    {
        if (selectedNode > -1)
        {
            float xos = xOffset + baseX;
            float yos = yOffset + baseY + (nodeOffset * playScrollSpeed) + nodeList[selectedNode].offset;

            float size = 0.05f;
            float x1 = xos;
            float x2 = x1 + 6;
            float y2 = yos;
            float y1 = y2 + nodeList[selectedNode].height;

            DrawSquareLine(x1 - size, y1 + size, x2 + size, y2 - size, size, judgeBarColor);
        }
    }

    void DrawSquareLine(float x1, float y1, float x2, float y2, float size, Color color)
    {
        GL.Begin(GL.QUADS);
        GL.Color(color);
        // Top
        GL.Vertex3(x1, y1, 0);
        GL.Vertex3(x2, y1, 0);
        GL.Vertex3(x2, y1 - size, 0);
        GL.Vertex3(x1, y1 - size, 0);

        // right
        GL.Vertex3(x2 - size, y1, 0);
        GL.Vertex3(x2, y1, 0);
        GL.Vertex3(x2, y2, 0);
        GL.Vertex3(x2 - size, y2, 0);

        // bottom
        GL.Vertex3(x1, y2 + size, 0);
        GL.Vertex3(x2, y2 + size, 0);
        GL.Vertex3(x2, y2, 0);
        GL.Vertex3(x1, y2, 0);

        // left
        GL.Vertex3(x1, y1, 0);
        GL.Vertex3(x1 + size, y1, 0);
        GL.Vertex3(x1 + size, y2, 0);
        GL.Vertex3(x1, y2, 0);
        GL.End();
    }

    void DrawSpectrum()
    {
        float xos = xOffset + baseX + 3;
        float yos = yOffset + baseY;

        int width = 600;
        int height = (int)(mSource.clip.length * playScrollSpeed) * 100;
        int specStart = -200;
        int specEnd = 1100;
        float widthscale = (float)width * 1f / 200f;

        if (height <= specEnd + (int)(yOffset * -100))
            specEnd = height - (int)(yOffset * -100);

        if (yOffset > -2)
            specStart = (int)(yOffset * 100);


        float[,] waveform = new float[2, height];
        int samplesize = mSource.clip.samples * mSource.clip.channels;
        float[] samples = new float[samplesize];
        mSource.clip.GetData(samples, 0);

        float packsize = (samplesize / 2 / height);

        for (int w = 0; w < height; w++)
        {
            waveform[0, w] = Mathf.Abs(samples[(int)(w * packsize) * 2]);
            waveform[1, w] = Mathf.Abs(samples[(int)(w * packsize) * 2 + 1]);
        }

        GL.Color(spectrumColor);

        for (int y = specStart; y < specEnd; y++)
        {
            GL.Vertex3(xos - waveform[0, y + (int)(yOffset * -100)] * widthscale, baseY + (y / 100f), 0);
            GL.Vertex3(xos + waveform[1, y + (int)(yOffset * -100)] * widthscale, baseY + (y / 100f), 0);

        }
    }

    public void MakeSpectrum()
    {
        if (mSource.clip != null)
        {
            // StartCoroutine(CoroutSpectrum());
            ResetSpectrum();
        }
    }

    IEnumerator CoroutSpectrum()
    {
        float xos = xOffset + baseX;
        float yos = yOffset + baseY;
        GameObject loadInst = Instantiate(loadObj);

        int height = (int)(mSource.clip.length * playScrollSpeed) * 100;
        float widthscale = (float)1f / 2f;

        float[,] waveform = new float[2, height];
        int samplesize = mSource.clip.samples * mSource.clip.channels;
        float[] samples = new float[samplesize];
        mSource.clip.GetData(samples, 0);

        yield return m_wait;
        for (int w = 0; w < height; w++)
        {
            waveform[0, w] = Mathf.Abs(samples[(int)(w * ((float)(samplesize / 2) / height)) * 2]);
            waveform[1, w] = Mathf.Abs(samples[(int)(w * ((float)(samplesize / 2) / height)) * 2 + 1]);
        }

        ListClear();
        int originHeight = 1000;

        for (int w = 0; w < (float)height / originHeight; w++)
        {
            GameObject inst = Instantiate(wfObj);
            WaveFormScript wf = inst.GetComponent<WaveFormScript>();
            yield return m_wait;

            int start = w * originHeight;
            int end = start + originHeight;
            if (end > height)
                end = height;

            int tileWidth = 600;
            int tileHeight = end - start;

            Texture2D tex = new Texture2D(tileWidth, tileHeight, TextureFormat.RGBA32, false);

            for (int y = 0; y < tileHeight; y++)
            {
                int chan0 = (int)(waveform[0, start + y] * widthscale * tileWidth);
                int chan1 = (int)(waveform[1, start + y] * widthscale * tileWidth);
                int middle = tileWidth / 2;
                int left = middle - chan0;
                int right = middle + chan1;

                for (int x = 0; x < left; x++) // left space
                    tex.SetPixel(x, y, noColor);

                for (int x = left; x < middle; x++) // channal 1
                    tex.SetPixel(x, y, spectrumColor);

                for (int x = middle; x < right; x++) // channal 2
                    tex.SetPixel(x, y, spectrumColor);

                for (int x = right; x < tileWidth; x++) // right space
                    tex.SetPixel(x, y, noColor);
            }

            tex.Apply();

            Rect rect = new(Vector2.zero, new Vector2(tileWidth, tileHeight));
            wf.GetWaveForm(xos, yos + yOffset + w * (originHeight / 100), tex, rect);
            wfList.Add(inst);
        }

        Debug.Log("전체 시간 : " + mSource.clip.length);
        scrollHeight = playScrollSpeed * mSource.clip.length;
        yOffsetLimit = -scrollHeight;
        sliderHeight = 10 / scrollHeight * 10;
        sliderOffset = (-yOffset / scrollHeight * (10 - sliderHeight)) - 5; // 화면높이 10

        Destroy(loadInst);
    }

    int spectHeight;
    float[,] waveform;

    public void ResetSpectrum()
    {
        spectHeight = (int)(mSource.clip.length * playScrollSpeed) * 100;
        waveform = new float[2, spectHeight];
        int samplesize = mSource.clip.samples * mSource.clip.channels;
        float[] samples = new float[samplesize];
        mSource.clip.GetData(samples, 0);

        for (int i = spectrumLines.Count; i < 1000; i++)
        {
            LineRenderer inst = Instantiate(spectrumLine).GetComponent<LineRenderer>();
            inst.widthMultiplier = 0.01f; //선 너비
            //inst.startColor = spectrumColor; //선 시작점 색
            //inst.endColor = spectrumColor; //선 끝점 색
            spectrumLines.Add(inst);
        }

        for (int w = 0; w < spectHeight; w++)
        {
            waveform[0, w] = Mathf.Abs(samples[(int)(w * ((float)(samplesize / 2) / spectHeight)) * 2]);
            waveform[1, w] = Mathf.Abs(samples[(int)(w * ((float)(samplesize / 2) / spectHeight)) * 2 + 1]);
        }

        Debug.Log("전체 시간 : " + mSource.clip.length);
        scrollHeight = playScrollSpeed * mSource.clip.length;
        yOffsetLimit = -scrollHeight;

        RenewSpectrum();
    }

    public void RenewSpectrum()
    {
        float xos = xOffset + baseX + 3;
        float yos = yOffset + baseY;

        // index
        int start = (int)((yos + 5) * 100);
        int end = start + spectHeight;
        Debug.Log("start : " + start + ", end : " + end);

        float widthScale = 3;
        for (int i = 0; i < spectrumLines.Count; i++)
        {
            if (i < start || i >= end)
            {
                spectrumLines[i].SetPosition(0, Vector3.zero);
                spectrumLines[i].SetPosition(1, Vector3.zero);
            }
            else
            {
                spectrumLines[i].SetPosition(0, new Vector3(xos - waveform[0, i - start] * widthScale, i * 0.01f - 5, -0.1f));
                spectrumLines[i].SetPosition(1, new Vector3(xos + waveform[1, i - start] * widthScale, i * 0.01f - 5, -0.1f));
            }
        }
    }

    // waveForm Object List Clear
    private void ListClear()
    {
        foreach (GameObject wf in wfList)
        {
            Destroy(wf);
        }
        wfList.Clear();
    }

    // waveForm Objects Move
    public void ListMove()
    {
        float xos = xOffset + baseX;
        float yos = yOffset + baseY;

        for (int i = 0; i < wfList.Count; i++)
        {
            wfList[i].transform.position = new Vector3(xos, yos + i * 10, -0.15F);
        }
    }

    // 마우스 누르는 동안 슬라이더 이동시킴
    IEnumerator SliderScroll()
    {
        while (Input.GetMouseButton(0) && mSource && mSource.clip)
        {
            float posY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
            if (posY > 4.05f)
                posY = 4.05f;
            else if (posY < -4.05f)
                posY = -4.05f;

            yOffset = (posY + 4.05f) / 8.1f * mSource.clip.length * -playScrollSpeed;

            NoteRelocate();
            RenewSpectrum();
            //ListMove();
            NodeNumberReset();
            yield return m_WaitSeconds;
        }
    }

    // 불러오기 시에 노트 생성
    public NoteInfoScript NoteCreation(int _line, int _node, int _beat, int _pos, string _num)
    {
        float xos = xOffset + baseX;
        float yos = yOffset + baseY + (nodeOffset * playScrollSpeed);

        float _offset = nodeList[_node].offset;
        float _nodeHeight = nodeList[_node].height;

        float noteX = xos + _line * nodeWidth;
        float noteY = yos + _offset + ((float)_pos / beat * _nodeHeight);

        Vector3 notePos = new Vector3(noteX, noteY, -3);
        GameObject inst = Instantiate(noteObj);
        inst.transform.position = notePos;

        NoteInfoScript noteScript = inst.GetComponent<NoteInfoScript>();
        noteScript.musicMng = musicMng;
        noteScript.noteMngScript = noteMngScript;
        noteScript.gridMngScript = this;

        NoteInfo createdNote = new NoteInfo(inst, _line, _node, _beat, _pos);
        noteMngScript.StackNote(createdNote);
        noteScript.note = createdNote;
        noteScript.noteNumber = _num;
        noteScript.noteSoundToggle = noteSoundToggle;

        return noteScript;
    }

    // 불러오기 시에 롱노트 생성
    public void LongNoteCreation(NoteInfoScript instScript, int _line, int _node, int _beat, int _pos)
    {
        NoteInfo createdNote = new NoteInfo(null, _line, _node, _beat, _pos);
        noteMngScript.StackNote(createdNote);
        instScript.endNote = createdNote;
        instScript.SetLongNoteHeight();
    }

    // 노트 재배치
    public void NoteRelocate()
    {
        float xos = xOffset + baseX;
        float yos = yOffset + baseY + (nodeOffset * playScrollSpeed);

        noteMngScript.NoteRelocate(xos, yos, nodeWidth, this);
    }

    // 롱노트에서 호출하여 노트 크기를 조정해주는 함수
    public void noteResize(GameObject note)
    {
        float yos = yOffset + baseY + (nodeOffset * playScrollSpeed);

        float pos = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;

        int _node = (int)((pos - yos) / nodeHeight);
        int _pos = (int)((pos - yos) / cellHeight) % beat;

        float noteY = (_node * nodeHeight) + (_pos * cellHeight) + yos;

        NoteInfo createdNote = new NoteInfo(null, 0, _node, beat, _pos);
        NoteInfoScript noteScript = note.GetComponent<NoteInfoScript>();
        noteScript.endNote = createdNote;
        noteScript.longNoteHeight = noteY - note.transform.position.y;
    }

    // BPM InputField 변경 이벤트핸들러
    private void SetBPM()
    {
        if (float.TryParse(bpmInput.text, out float _bpm))
        {
            if (bpm != _bpm)
            {
                bpm = _bpm;
                nodeHeight = playScrollSpeed / (bpm / 60f);
                cellHeight = nodeHeight / beat;
                noteMngScript.nodeBpmList.Clear(); // 변경된 BPM들 모두 초기화
                NodeNumberReset();
                SetNodeList();
            }
        }
    }

    // 박자 InputField 변경 이벤트핸들러
    private void SetBeat()
    {
        if (Int32.TryParse(beatInput.text, out int _beat))
        {
            if (beat != _beat)
            {
                beat = _beat;
                cellHeight = nodeHeight / beat;
                NoteRelocate();
            }
        }
    }

    // Offset InputField 변경 이벤트핸들러
    private void SetOffset()
    {
        if (float.TryParse(offsetInput.text, out float _offset))
        {
            if (nodeOffset != _offset * 0.001f)
            {
                nodeOffset = _offset * 0.001f;
                NoteRelocate();
                NodeNumberReset();
                SetNodeList();
            }
        }
    }

    // 확대값 InputField 변경 이벤트핸들러
    private void SetZoomSize()
    {
        if (float.TryParse(zoomInput.text, out float _zoom))
        {
            if (zoom != _zoom)
            {
                zoom = _zoom;
                playScrollSpeed = 4 * zoom;
                nodeHeight = playScrollSpeed / (bpm / 60f);
                cellHeight = nodeHeight / beat;
                NoteRelocate();
                MakeSpectrum();
                NodeNumberReset();
            }
        }
    }

    // 노래속도 InputField 변경 이벤트핸들러
    private void SetSpeed()
    {
        if (float.TryParse(speedInput.text, out float _speed))
        {
            if (musicSpeed != _speed)
            {
                musicSpeed = _speed;
                mSource.pitch = musicSpeed;
            }
        }
    }

    // 노드BPM InputField 변경 이벤트핸들러
    private void SetNodeBPM()
    {
        Debug.Log("Grid.SetNoteBPM");
        if (float.TryParse(nodeBpmInput.text, out float _bpm))
        {
            if (selectedNode != -1)
            {
                /*selectedNote.SetNoteBPM(_bpm);
                int tempIndex = noteMngScript.AddNoteBpm(_bpm);
                selectedNote.noteNumber = tempIndex.ToString("00");*/
                noteMngScript.AddNodeBpm(selectedNode, _bpm);
                GridReset();
            }
        }
    }

    // 노트 모드 변경 이벤트핸들러 (노드선택 / 노트생성 / 롱노트생성)
    private void SetNoteMode(int mode)
    {
        noteMode = mode;
        if (mode != 0)
        {
            selectedNote = null;
            selectedNode = -1;
        }
    }

    // nodeList 정보 새로고침
    private void SetNodeList()
    {
        nodeList.Clear();
        List<NodeInfo> nodeBpmList = noteMngScript.nodeBpmList;
        int preNode = 0, nextNode = 0, totalNode;
        
        if (mSource.clip == null)
            return;

        float clipLength = mSource.clip.length;
        float _bpm = bpm;
        float _yoffset = 0;
        float _nodeHeight = nodeHeight;

        // BPM 수정된 노드가 없는경우
        if (nodeBpmList.Count == 0)
        {
            totalNode = (int)Mathf.Ceil(clipLength / (60f / bpm));
            for (int n = 0; n < totalNode; n++)
            {
                NodeInfo nodeData = new(n, _bpm, _yoffset + _nodeHeight * n, _nodeHeight);
                nodeList.Add(nodeData);
            }
        }
        else
        {
            for (int i = 0; i < nodeBpmList.Count; i++)
            {
                nextNode = nodeBpmList[i].node;
                for (int j = 0; j < nextNode - preNode; j++)
                {
                    NodeInfo nodeData = new(preNode + j, _bpm, _yoffset + _nodeHeight * j, _nodeHeight);
                    nodeList.Add(nodeData);
                }
                _yoffset += _nodeHeight * (nextNode - preNode);
                _bpm = nodeBpmList[i].bpm;
                _nodeHeight = playScrollSpeed / (_bpm / 60f);
                preNode = nextNode;
            }
            totalNode = preNode + (int)Mathf.Ceil((clipLength * playScrollSpeed - nextNode) / _nodeHeight);
            for (int j = 0; j < totalNode - preNode; j++)
            {

                NodeInfo nodeData = new(preNode + j, _bpm, _yoffset + _nodeHeight * j, _nodeHeight);
                nodeList.Add(nodeData);
            }
        }
    }

    // 격자 새로고침
    public void GridReset()
    {
        Debug.Log("Grid.GridReset");
        if (float.TryParse(bpmInput.text, out float _bpm))
        {
            bpm = _bpm;
            nodeHeight = playScrollSpeed / (bpm / 60f);
        }

        if (Int32.TryParse(beatInput.text, out int _beat))
        {
            beat = _beat;
            cellHeight = nodeHeight / beat;
        }

        if (float.TryParse(offsetInput.text, out float _offset))
        {
            nodeOffset = _offset * 0.001f;
        }

        NoteRelocate();
        SetNodeList();
        NodeNumberReset();
    }

    // 격자에 붙은 마디 번호 새로고침
    public void NodeNumberReset()
    {
        int textCount = nodeNumbers.Count;
        float yos = yOffset + baseY + (nodeOffset * playScrollSpeed);

        // 시작위치 찾기
        int startPos = 0;
        for (int i = 1; i < nodeList.Count; i++)
        {
            if (nodeList[i].offset + yos > -5)
            {
                startPos = i - 1;
                break;
            }
        }

        // 종료지점 찾기
        int endPos = nodeList.Count - 1;
        for (int i = startPos; i < nodeList.Count; i++)
        {
            if (nodeList[i].offset + yos > 5)
            {
                endPos = i - 1;
                break;
            }
        }

        // 부족한 TMP, horizontal 갯수 채우기
        if (endPos - startPos + 1 > textCount)
        {
            for (int i = textCount; i <= endPos - startPos + 1; i++)
            {
                TextMeshPro inst = Instantiate(nodeNumberObj);
                nodeNumbers.Add(inst);
                GameObject hinst = Instantiate(horizontal);
                horizontals.Add(hinst);
                for (int j = 0; j < beat - 1; j++)
                {
                    GameObject hinst2 = Instantiate(horizontal2);
                    horizontal2s.Add(hinst2);
                }
            }
            textCount = endPos - startPos + 1;
        }

        // TMP 오브젝트들 설정
        for (int i = 0; i < endPos - startPos + 1; i++)
        {
            nodeNumbers[i].text = (startPos + i).ToString();

            // BPM 변경된 마디인지 확인
            if (noteMngScript.IsNodeInBpmList(startPos + i) > -1)
                nodeNumbers[i].text += " (BPM " + nodeList[startPos + i].bpm.ToString() + ")";
            nodeNumbers[i].transform.position = new Vector3(-1.9f, yos + nodeList[startPos + i].offset, 0);

            // 마디선 설정
            horizontals[i].transform.position = new Vector3(-5, yos + nodeList[startPos + i].offset, 0);
            for (int j = 0; j < beat - 1; j++)
            {
                horizontal2s[i * (beat - 1) + j].transform.position = new Vector3(-5, yos + nodeList[startPos + i].offset + (nodeList[startPos + i].height / 4 * (j + 1)), 0);
            }

        }

        // 남는 TMP 오브젝트 제거
        if (textCount > endPos - startPos + 1)
        {
            for (int i = textCount - 1; i > endPos - startPos; i--)
            {
                Destroy(nodeNumbers[i].gameObject);
                nodeNumbers.RemoveAt(i);

                Destroy(horizontals[i]);
                horizontals.RemoveAt(i);
                for (int j = beat - 2; j >= 0; j--)
                {
                    Destroy(horizontal2s[i * (beat - 1) + j]);
                    horizontal2s.RemoveAt(i * (beat - 1) + j);
                }
            }
            textCount = endPos - startPos + 1;
        }
    }

    /*public void SelectNote(NoteInfoScript noteScript)
    {
        if (noteMode == 0)
        {
            if (ReferenceEquals(noteScript, selectedNote))
            {
                selectedNote = null;
                noteBpmInput.text = "";
            }
            else
            {
                selectedNote = noteScript;
                noteBpmInput.text = noteScript.noteBpm.ToString();
            }
        }
    }*/

    /*public void SelectNode(int nodeNumber, int selectedMode) // 0 = 그냥 클릭 1=컨트롤+클릭 2=시프트+클릭
    {
        if (noteMode == 0)
        {
            if (selectedMode == 0)
            {
                selectedNode.Clear();
                selectedNode.Add(nodeNumber);

            }
            else if (selectedMode == 1)
            {
                bool res = selectedNode.Contains(nodeNumber);
                if (res)
                    selectedNode.Remove(nodeNumber);
                else
                    selectedNode.Add(nodeNumber);
            }
            else if (selectedMode == 2)
            {
                int lastNode = selectedNode[-1];
                if (lastNode > nodeNumber)
                {
                    for (int i = nodeNumber; i < lastNode; i++)
                    {
                        if (!selectedNode.Contains(i))
                            selectedNode.Add(i);
                    }
                }
                else if (lastNode < nodeNumber)
                {
                    for (int i = lastNode + 1; i <= nodeNumber; i++)
                    {
                        if (!selectedNode.Contains(i))
                            selectedNode.Add(i);
                    }
                }
            }
        }
    }*/

    public void SelectNode(int nodeNumber)
    {
        if (noteMode == 0)
        {
            if (selectedNode == nodeNumber)
                nodeNumber = -1;
            else
                selectedNode = nodeNumber;
        }
    }
}
