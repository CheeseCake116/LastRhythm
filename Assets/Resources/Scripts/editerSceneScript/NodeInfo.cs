public class NodeInfo
{
    public int node;
    public float bpm;
    public float offset;
    public float height;

    public NodeInfo(int _node, float _bpm)
    {
        node = _node;
        bpm = _bpm;
    }

    public NodeInfo(int _node, float _bpm, float _offset, float _height)
    {
        node = _node;
        bpm = _bpm;
        offset = _offset;
        height = _height;
    }
}
