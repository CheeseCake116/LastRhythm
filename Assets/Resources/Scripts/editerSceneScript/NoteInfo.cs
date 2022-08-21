using UnityEngine;

public class NoteInfo
{
    // offset에 판정바로부터의 거리를 시간으로 환산한 값을 저장하기로
    public GameObject inst;
    public int line;
    public int node;
    public int beat;
    public int pos;

    public NoteInfo(GameObject _inst, int _line, int _node, int _beat, int _pos)
    {
        inst = _inst;
        line = _line;
        node = _node;
        beat = _beat;
        pos = _pos;
    }

    public bool IsEqual(NoteInfo note)
    {
        return note.pos * beat == pos * note.beat;
    }
}