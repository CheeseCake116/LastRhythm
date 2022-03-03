using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class lineRenderScript : MonoBehaviour
{
    struct noteInfo
    {
        public GameObject inst;
        public int line;
        public int node;
        public int beat;
        public int pos;
    }

    List<noteInfo>[] noteStack = new List<noteInfo>[6];
    int[] stackCount = { 0, 0, 0, 0, 0, 0 };
    int Col = 6;

    [HideInInspector]
    public float xOffset, yOffset;

    [SerializeField] Color nodeColor, beatColor, judgeBarColor, spectrumColor;
    [SerializeField] GameObject noteObj, musicMng;
    [SerializeField] InputField bpmInput, beatInput, offsetInput, nodeSizeInput;
    [SerializeField] AudioClip tik;

    float nodeWidth = 1;
    float nodeHeight = 2;
    float cellHeight;

    float baseX = -8f;
    float baseY = -3f;
    float winHeight = 10f;

    int node = 5;
    int beat = 4;
    float nodeOffset = 0.025f;
    float bpm = 144f;
    float tikTime = 0f;
    float playScrollSpeed = 4f; // 초당 이동하는 거리


    AudioSource mSource;
    static Material lineMaterial;

    private void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            noteStack[i] = new List<noteInfo>();
        }

        bpmInput.onSubmit.AddListener(delegate { setBPM(bpmInput); });
        beatInput.onSubmit.AddListener(delegate { setBeat(beatInput); });
        offsetInput.onSubmit.AddListener(delegate { setOffset(offsetInput); });
        nodeSizeInput.onSubmit.AddListener(delegate { setNodeSize(nodeSizeInput); });
        nodeSizeInput.text = "2";

        nodeHeight = 4 / (bpm / 60f);
        cellHeight = nodeHeight / beat;
        tikTime = 60f / bpm;

        mSource = musicMng.GetComponent<AudioSource>();

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

/*    private void OnDrawGizmos()
    {
        // Draw Scene
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.Flush();
        GL.PushMatrix();

        DrawGrid();

        GL.PopMatrix();
    }*/

    private void OnRenderObject()
    {
        if (Camera.current.name != "Main Camera")
            return;

        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.Flush();
        GL.PushMatrix();

        DrawGrid();
        
        GL.PopMatrix();
    }

    void DrawGrid()
    {
        GL.Begin(GL.LINES);

        float xos = xOffset + baseX;
        float yos = (yOffset + baseY + nodeOffset * playScrollSpeed) % nodeHeight - ((int)(5 / nodeHeight) + 1) * nodeHeight;
        node = (int)(winHeight / nodeHeight) + 3;

        // row
        for (int i = 0; i < node; i++)
        {
            GL.Color(nodeColor);
            GL.Vertex3(xos, yos + (i * nodeHeight), 0);
            GL.Vertex3(xos + (float)(Col * nodeWidth), yos + (i * nodeHeight), 0);

            for (int j = 1; j < beat; j++)
            {
                GL.Color(beatColor);
                GL.Vertex3(xos, yos + (i * nodeHeight) + (j * cellHeight), 0);
                GL.Vertex3(xos + (float)(Col * nodeWidth), yos + (i * nodeHeight) + (j * cellHeight), 0);
            }
        }

        GL.Color(nodeColor);
        GL.Vertex3(xos, yos + (node * nodeHeight), 0);
        GL.Vertex3(xos + (float)(Col * nodeWidth), yos + (node * nodeHeight), 0);

        // col
        for (int i = 0; i <= Col; i++)
        {
            GL.Color(nodeColor);
            GL.Vertex3(xos + (i * nodeWidth), yos, 0);
            GL.Vertex3(xos + (i * nodeWidth), yos + (nodeHeight * node), 0);
        }

        if (mSource)
            DrawSpectrum();

        GL.End();

        GL.Begin(GL.QUADS);
        GL.Color(judgeBarColor);
        GL.Vertex3(xos, -3, 0);
        GL.Vertex3(xos + 6, -3, 0);
        GL.Vertex3(xos + 6, -3.1f, 0);
        GL.Vertex3(xos, -3.1f, 0);
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


        float[] waveform = new float[height];
        int samplesize = mSource.clip.samples * mSource.clip.channels;
        float[] samples = new float[samplesize];
        mSource.clip.GetData(samples, 0);

        float packsize = (samplesize / height);

        if (packsize >= 1f)
        {
            for (int w = 0; w < height; w++)
            {
                waveform[w] = Mathf.Abs(samples[(int)(w * packsize)]);
            }
        }
        else
        {
            for (int w = 0; w < height; w++)
            {
                float dec = (w * packsize) - (int)(w * packsize);
                waveform[w] = samples[(int)(w * packsize)] * (1 - dec) + samples[(int)(w * packsize + 1)] * dec;
            }
        }

        GL.Color(spectrumColor);

        for (int y = specStart; y < specEnd; y++)
        {
            GL.Vertex3(xos - waveform[y + (int)(yOffset * -100)] * widthscale, baseY + (y / 100f), 0);
            GL.Vertex3(xos + waveform[y + (int)(yOffset * -100)] * widthscale, baseY + (y / 100f), 0);

        }
    }

    private void Update()
    {

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (mSource.isPlaying)
        {
            float playTime = mSource.time;
            yOffset = playTime * -playScrollSpeed;
            noteRelocate();
        }
        else
        {
            if (scroll != 0f)
            {
                yOffset += scroll * -playScrollSpeed;
                if (yOffset > 5)
                    yOffset = 5;

                noteRelocate();
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float xos = xOffset + baseX;
            float yos = yOffset + baseY + (nodeOffset * playScrollSpeed);

            if (pos[0] >= xos && pos[0] <= xos + (6 * nodeWidth) )
            {
                int _line = (int)((pos[0] - xos) / nodeWidth);
                int _node = (int)((pos[1] - yos) / nodeHeight);
                int _pos = (int)((pos[1] - yos) / cellHeight) % beat;

                float noteX = _line * nodeWidth + xos;
                float noteY = (_node * nodeHeight) + (_pos * cellHeight) + yos;

                Vector3 notePos = new Vector3(noteX, noteY, -1);
                GameObject inst = Instantiate(noteObj);
                inst.transform.position = notePos;

                noteInfo createdNote;
                createdNote.inst = inst;
                createdNote.line = _line;
                createdNote.node = _node;
                createdNote.beat = beat;
                createdNote.pos = _pos;
                noteStack[_line].Add(createdNote);
                stackCount[_line]++;

                noteInfoScript noteScript = inst.GetComponent<noteInfoScript>();
                noteScript.line = _line;
                noteScript.node = _node;
                noteScript.beat = beat;
                noteScript.pos = _pos;
                noteScript.lrs = this;

            }
        }
    }

    public void noteRelocate()
    {
        float xos = xOffset + baseX;
        float yos = yOffset + baseY + (nodeOffset * playScrollSpeed);

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < stackCount[i]; j++)
            {
                noteInfo note = noteStack[i][j];
                float _cellHeight = nodeHeight / note.beat;
                float noteX = note.line * nodeWidth + xos;
                float noteY = (note.node * nodeHeight) + (note.pos * _cellHeight) + yos;
                note.inst.transform.position = new Vector3(noteX, noteY, -1);
            }
        }
    }

    public void noteRemove(GameObject noteObj, int _line, int _node, int _beat, int _pos)
    {
        for(int i = 0; i < stackCount[_line]; i++)
        {
            noteInfo note = noteStack[_line][i];
            if (note.node == _node && note.beat == _beat && note.pos == _pos)
            {
                stackCount[_line]--;
                noteStack[_line].RemoveAt(i);
                Destroy(noteObj);
                break;
            }
        }
    }

    private void setBPM(InputField input)
    {
        if (float.TryParse(input.text, out float _bpm))
        {
            Debug.Log(_bpm);
            bpm = _bpm;
            tikTime = 60f / bpm;
            playScrollSpeed = nodeHeight / tikTime;
        }
    }

    private void setBeat(InputField input)
    {
        if (Int32.TryParse(input.text, out int _beat))
        {
            Debug.Log(_beat);
            beat = _beat;
            cellHeight = nodeHeight / beat;
            noteRelocate();
        }
    }

    private void setOffset(InputField input)
    {
        if(float.TryParse(input.text, out float _offset))
        {
            Debug.Log(_offset);
            nodeOffset = _offset * 0.001f;
            noteRelocate();
        }
    }

    private void setNodeSize(InputField input)
    {
        if (float.TryParse(input.text, out float _nodeSize))
        {
            Debug.Log(_nodeSize);
            playScrollSpeed = _nodeSize * bpm / 60;
            nodeHeight = _nodeSize;
            cellHeight = nodeHeight / beat;
            noteRelocate();
        }
    }
}
