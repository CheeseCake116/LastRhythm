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

                    // 롱노트 끝자락 노트일 경우 이동 안시킴
                    if (note.inst == null)
                        continue;

                    float nodeHeight = nodeList[n].height;
                    float noteX = note.line * nodeWidth + xos;
                    float noteY = yos + nodeList[n].offset + ((float)note.pos / note.beat * nodeHeight);
                    note.inst.transform.position = new Vector3(noteX, noteY, -1);

                    NoteInfoScript noteScript = note.inst.GetComponent<NoteInfoScript>();

                    // 노트 bpm 텍스트도 이동
                    /*if (noteScript.textInst != null)
                        noteScript.textInst.transform.position = noteScript.transform.position;*/

                    // 롱노트의 경우 길이 재측정
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

                    // 롱노트의 끝을 표시하는 노트일 경우 noteData보다 같거나 늦게 나오면 true 반환. 아니어도 continue
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
                    

                    // 이 노트들이 noteData보다 늦게 나오면 false 반환. (시작노트는 이전 마디에 있다는 뜻이므로)
                    if (startNote.node == node && startNote.pos * beat > pos * startNote.beat)
                        return false;

                    // 이 노트들이 noteData보다 일찍 나오지만 늦게 끝나면 true 반환
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
                    return false; // 현재 마디에 노트가 있었다면 다른 마디를 찾을 필요 없음
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
        
        // bpm 넣을 인덱스 찾기
        int tempIndex = 2; // 시작값 2
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
        RemoveNodeBpm(node); // 중복 노드 제거
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