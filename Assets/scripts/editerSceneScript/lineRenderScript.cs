using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public float cellHeight;
    public float cellWidth;

    float baseX = -8f;
    float baseY = -5f;
    float winHeight = 10f;

    int node = 5;
    int beat = 4;

    public GameObject noteObj;

    static Material lineMaterial;

    private void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            noteStack[i] = new List<noteInfo>();
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
        yos = yos % (cellHeight * beat) + baseY;
        node = (int)(winHeight / (cellHeight * beat)) + 2;

        // row
        for (int i = 0; i < node; i++)
        {
            GL.Color(nodeColor);
            GL.Vertex3(xos, yos + (i * cellHeight * beat), 0);
            GL.Vertex3(xos + (float)(Col * cellWidth), yos + (i * cellHeight * beat), 0);

            for (int j = 1; j < beat; j++)
            {
                GL.Color(beatColor);
                GL.Vertex3(xos, yos + (i * cellHeight * beat) + (j * cellHeight), 0);
                GL.Vertex3(xos + (float)(Col * cellWidth), yos + (i * cellHeight * beat) + (j * cellHeight), 0);
            }
        }

        GL.Color(nodeColor);
        GL.Vertex3(xos, yos + (node * cellHeight * beat), 0);
        GL.Vertex3(xos + (float)(Col * cellWidth), yos + (node * cellHeight * beat), 0);

        // col
        for (int i = 0; i <= Col; i++)
        {
            GL.Color(nodeColor);
            GL.Vertex3(xos + (i * cellWidth), yos, 0);
            GL.Vertex3(xos + (i * cellWidth), yos + (cellHeight * node * beat), 0);
        }

        GL.End();
    }

    private void Update()
    {

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            yOffset += scroll * 3;
            if (yOffset > 0)
                yOffset = 0;

            float xos = xOffset + baseX;
            float yos = yOffset + baseY;

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < stackCount[i]; j++)
                {
                    noteInfo note = noteStack[i][j];
                    float noteX = note.line * cellWidth + xos;
                    float noteY = (note.node * beat + note.pos) * cellHeight + yos;
                    note.inst.transform.position = new Vector3(noteX, noteY, -1);
                }
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float xos = xOffset + baseX;
            float yos = yOffset + baseY;

            if (pos[0] >= xos && pos[0] <= xos + (6 * cellWidth))
            {

                int _line = (int)((pos[0] - xos) / cellWidth);
                int _node = (int)((pos[1] - yos) / (cellHeight * beat));
                int _pos = (int)((pos[1] - yos) / cellHeight) % beat;

                float noteX = _line * cellWidth + xos;
                float noteY = (_node * beat + _pos) * cellHeight + yos;

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

                Debug.Log(pos);
                Debug.Log(notePos);
                Debug.Log(createdNote.node + " " + createdNote.line + " " + createdNote.beat + " " + createdNote.pos);
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
}
