using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class NoteManagerScript : MonoBehaviour
{
    [HideInInspector] public Dictionary<int, List<NoteInfo>[]> noteDict = new(); // Dict[node][line][i]
    [HideInInspector] public int[] stackCount = { 0, 0, 0, 0, 0, 0 };
    [HideInInspector] public Dictionary<int, float> noteBpmDict = new();
    [HideInInspector] public List<NodeInfo> nodeBpmList = new();

    public void AddNote(NoteInfo noteData)
    {
        int line = noteData.line;
        int node = noteData.node;
        int beat = noteData.beat;
        int pos = noteData.pos;

        if (!noteDict.ContainsKey(node))
        {
            noteDict[node] = new List<NoteInfo>[6];
            noteDict[node][line] = new List<NoteInfo>();
        }
        else if (noteDict[node][line] == null)
        {
            noteDict[node][line] = new List<NoteInfo>();
        }
        else
        {
            for (int i = 0; i < noteDict[node][line].Count; i++)
            {
                NoteInfo tempNote = noteDict[node][line][i];
                int oldPos = tempNote.pos * beat;
                int newPos = pos * tempNote.beat;

                if (oldPos > newPos)
                {
                    noteDict[node][line].Insert(i, noteData);
                    stackCount[line]++;
                    return;
                }
                else if (oldPos == newPos)
                {
                    return;
                }
            }
        }
        noteDict[node][line].Add(noteData);
        stackCount[line]++;
    }

    // for "SaveLoadScript.cs" to load note data
    public void StackNote(NoteInfo noteData)
    {
        int line = noteData.line;
        int node = noteData.node;

        if (!noteDict.ContainsKey(node))
        {
            noteDict[node] = new List<NoteInfo>[6];
            noteDict[node][line] = new List<NoteInfo>();
        }
        else if (noteDict[node][line] == null)
        {
            noteDict[node][line] = new List<NoteInfo>();
        }
        noteDict[node][line].Add(noteData);
        stackCount[line]++;
    }

    public void NoteRemove(NoteInfo noteData)
    {
        int line = noteData.line;
        int node = noteData.node;

        if (!noteDict.ContainsKey(node) || noteDict[node][line] == null)
        {
            return;
        }

        for (int i = 0; i < noteDict[node][line].Count; i++)
        {
            NoteInfo note = noteDict[node][line][i];
            if (ReferenceEquals(note, noteData))
            {
                stackCount[line]--;
                noteDict[node][line].RemoveAt(i);
                if (note.inst != null)
                    Destroy(note.inst);
                break;
            }
        }
    }

    public void NoteRelocate(float xos, float yos, float nodeWidth, LineRenderScript gridMngScript)
    {
        List<NodeInfo> nodeList = gridMngScript.nodeList;

        foreach (int n in noteDict.Keys)
        {
            if (noteDict[n] == null)
                continue;

            for (int l = 0; l < 6; l++)
            {
                if (noteDict[n][l] == null)
                    continue;

                for (int i = 0; i < noteDict[n][l].Count; i++)
                {
                    NoteInfo note = noteDict[n][l][i];

                    // �ճ�Ʈ ���ڶ� ��Ʈ�� ��� �̵� �Ƚ�Ŵ
                    if (note.inst == null)
                        continue;

                    float nodeHeight = nodeList[n].height;
                    float noteX = note.line * nodeWidth + xos;
                    float noteY = yos + nodeList[n].offset + ((float)note.pos / note.beat * nodeHeight);
                    note.inst.transform.position = new Vector3(noteX, noteY, -1);

                    NoteInfoScript noteScript = note.inst.GetComponent<NoteInfoScript>();

                    // ��Ʈ bpm �ؽ�Ʈ�� �̵�
                    /*if (noteScript.textInst != null)
                        noteScript.textInst.transform.position = noteScript.transform.position;*/

                    // �ճ�Ʈ�� ��� ���� ������
                    if (noteScript.noteMode == 2)
                    {
                        noteScript.SetLongNoteHeight();
                    }
                }
            }
        }
    }

    public void NoteClear()
    {
        foreach (int n in noteDict.Keys)
        {
            if (noteDict[n] == null)
                continue;

            for (int l = 0; l < 6; l++)
            {
                if (noteDict[n][l] == null)
                    continue;

                for (int i = 0; i < noteDict[n][l].Count; i++)
                {
                    NoteInfo note = noteDict[n][l][i];
                    Destroy(note.inst);
                }
            }
        }

        noteDict = new();
        stackCount = new int[] { 0, 0, 0, 0, 0, 0 };
    }

    bool IsThereLongNote(NoteInfo noteData)
    {
        int line = noteData.line;
        int node = noteData.node;
        int beat = noteData.beat;
        int pos = noteData.pos;

        for (int i = node; i >= 0; i--)
        {
            if (noteDict.ContainsKey(i) && noteDict[i][line] != null)
            {
                int n;
                for (n = 0; n < noteDict[i][line].Count; n++)
                {
                    NoteInfo startNote = noteDict[i][line][n];

                    // �ճ�Ʈ�� ���� ǥ���ϴ� ��Ʈ�� ��� noteData���� ���ų� �ʰ� ������ true ��ȯ. �ƴϾ continue
                    if (startNote.inst == null)
                    {
                        if (startNote.node == node && startNote.pos * beat >= pos * startNote.beat)
                        {
                            return true;
                        }
                        else
                            continue;
                    }

                    NoteInfoScript noteIS = noteDict[i][line][n].inst.GetComponent<NoteInfoScript>();
                    

                    // �� ��Ʈ���� noteData���� �ʰ� ������ false ��ȯ. (���۳�Ʈ�� ���� ���� �ִٴ� ���̹Ƿ�)
                    if (startNote.node == node && startNote.pos * beat > pos * startNote.beat)
                        return false;

                    // �� ��Ʈ���� noteData���� ���� �������� �ʰ� ������ true ��ȯ
                    if (noteIS.noteMode == 2)
                    {
                        NoteInfo endNote = noteIS.endNote;
                        if (endNote == null)
                            return false;
                        if (endNote.node > node)
                            return true;
                        else if (endNote.node == node && endNote.pos * beat >= pos * endNote.beat)
                            return true;
                    }
                }
                if (n > 0)
                    return false; // ���� ���� ��Ʈ�� �־��ٸ� �ٸ� ���� ã�� �ʿ� ����
            }
        }

        return false;
    }

    public bool IsThereNote(NoteInfo noteData)
    {
        if (IsThereLongNote(noteData))
            return true;

        int line = noteData.line;
        int node = noteData.node;
        int beat = noteData.beat;
        int pos = noteData.pos;

        if (noteDict.ContainsKey(node) && noteDict[node][line] != null)
        {
            for (int i = 0; i < noteDict[node][line].Count; i++)
            {
                NoteInfo tempNote = noteDict[node][line][i];
                int oldPos = tempNote.pos * beat;
                int newPos = pos * tempNote.beat;

                if (oldPos > newPos)
                {
                    return false;
                }
                else if (oldPos == newPos)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /*public int AddNoteBpm(float noteBpm)
    {
        List<int> tempList = new List<int>();
        tempList.AddRange(noteBpmDict.Keys);
        tempList.Sort();
        
        // bpm ���� �ε��� ã��
        int tempIndex = 2; // ���۰� 2
        for (int i = 0; i < tempList.Count; i++)
        {
            if (tempList[i] == tempIndex)
                tempIndex++;
            else
                break;
        }

        Debug.Log("noteBpmDict[" + tempIndex + "] = " + noteBpm + ";");
        noteBpmDict[tempIndex] = noteBpm;
        return tempIndex;
    }

    public void RemoveNoteBpm(int noteNumber)
    {
        if (noteBpmDict.ContainsKey(noteNumber))
            noteBpmDict.Remove(noteNumber);

        Debug.Log("noteBpmDict.Remove(" + noteNumber + ");");
    }*/

    

    public void AddNodeBpm(int node, float bpm)
    {
        NodeInfo nodeData = new(node, bpm);
        RemoveNodeBpm(node); // �ߺ� ��� ����
        int count = nodeBpmList.Count;

        if (count == 0)
            nodeBpmList.Add(nodeData);
        else
        {
            int i;
            for (i = 0; i < count; i++)
            {
                if (node < nodeBpmList[i].node)
                {
                    nodeBpmList.Insert(i, nodeData);
                    break;
                }
            }
            if (i == count)
            {
                nodeBpmList.Add(nodeData);
            }
        }
    }

    public void RemoveNodeBpm(int node)
    {
        for (int i = 0; i < nodeBpmList.Count; i++)
        {
            if (node == nodeBpmList[i].node)
            {
                nodeBpmList.RemoveAt(i);
            }
        }
    }

    public int IsNodeInBpmList(int node)
    {
        for (int i = 0; i < nodeBpmList.Count; i++)
        {
            if (nodeBpmList[i].node == node)
                return i;
        }
        return -1;
    }
}