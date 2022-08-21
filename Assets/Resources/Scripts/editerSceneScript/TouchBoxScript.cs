using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchBoxScript : MonoBehaviour
{
    public NoteInfoScript note;
    SpriteRenderer noteSP;
    public NoteManagerScript noteMngScript;
    public LineRenderScript gridMngScript;
    static readonly WaitForSeconds m_wait= new(0.2f);

    private void Start()
    {
        noteSP = note.GetComponent<SpriteRenderer>();
        StartCoroutine(follow());
    }
    private void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (gridMngScript.noteMode == 2)
            {
                note.resizeMode = true;
                StartCoroutine(note.noteResize());
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (note.noteMode == 2 && note.endNote != null)
            {
                noteMngScript.NoteRemove(note.endNote);
            }
            noteMngScript.NoteRemove(note.note);
        }
    }

    IEnumerator follow()
    {
        while (true)
        {
            transform.position = note.transform.position;
            transform.localScale = new Vector3(noteSP.size.x, noteSP.size.y, -5);
            yield return m_wait;
        }
    }
}
