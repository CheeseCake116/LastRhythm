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

    public float xOffset;
    public float yOffset;

    public Color nodeColor;
    public Color beatColor;

    public float nodeWidth;
    public float nodeHeight;
    float cellHeight;

    float baseX = -8f;
    float baseY = -5f;
    float winHeight = 10f;

    int node = 5;
    int beat = 4;
    float bpm = 120f;
    float tikTime = 0f;
    float nextTime = 0f;
    float playScrollSpeed = 0.5f;

    public GameObject noteObj;
    public InputField bpmInput;
    public InputField beatInput;
    AudioSource mSource;
    public AudioClip tik;

    static Material lineMaterial;

    private void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            noteStack[i] = new List<noteInfo>();
        }
        bpmInput.onSubmit.AddListener(delegate { setBPM(bpmInput); });
        beatInput.onSubmit.AddListener(delegate { setBeat(beatInput); });
        cellHeight = nodeHeight / beat;
        tikTime = 60f / bpm;
        playScrollSpeed = nodeHeight / tikTime;
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

    private void OnDrawGizmos()
    {
        // Draw Scene
        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.PushMatrix();

        DrawGrid(xOffset, yOffset);

        GL.PopMatrix();
    }

    private void OnRenderObject()
    {
        if (Camera.current.name != "Main Camera")
            return;

        CreateLineMaterial();
        lineMaterial.SetPass(0);

        GL.PushMatrix();

        DrawGrid(xOffset, yOffset);
        
        GL.PopMatrix();
    }

    void DrawGrid(float xos, float yos)
    {
        GL.Begin(GL.LINES);

        xos += baseX;
        yos = yos % nodeHeight + baseY;
        node = (int)(winHeight / nodeHeight) + 2;

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

        GL.End();
    }

    private void Update()
    {

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            yOffset += scroll * -3;
            if (yOffset > 0)
                yOffset = 0;

            noteRelocate();
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float xos = xOffset + baseX;
            float yos = yOffset + baseY;

            if (pos[0] >= xos && pos[0] <= xos + (6 * nodeWidth))
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

    private void noteRelocate()
    {
        float xos = xOffset + baseX;
        float yos = yOffset + baseY;

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
            nextTime = 0f;
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
}
