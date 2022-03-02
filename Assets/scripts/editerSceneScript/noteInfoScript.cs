using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class noteInfoScript : MonoBehaviour
{
    public int line;
    public int node;
    public int beat;
    public int pos;
    public lineRenderScript lrs;

    private void OnMouseOver()
    {
        if (Input.GetMouseButton(1))
        {
            Debug.Log("remove");
            lrs.noteRemove(this.gameObject, line, node, beat, pos);
        }
        
    }
}
