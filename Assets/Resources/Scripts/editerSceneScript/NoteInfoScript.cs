using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NoteInfoScript : MonoBehaviour
{
    // script 객체는 lineRenderScript에서 생성 시 할당받음
    [HideInInspector] public NoteInfo note, endNote;
    [HideInInspector] public NoteManagerScript noteMngScript;
    [HideInInspector] public LineRenderScript gridMngScript;
    [HideInInspector] public GameObject musicMng;
    [SerializeField] GameObject TouchBox;
    [SerializeField] Sprite longNote;
    [SerializeField] TextMeshPro bpmText;
    [HideInInspector] public TextMeshPro textInst;
    AudioSource tikSource, mSource;
    TouchBoxScript touchBoxScript;
    public Toggle noteSoundToggle;
    bool underBar = false;
    public int noteMode;
    public bool resizeMode = true;
    public float longNoteHeight = 0;
    static readonly WaitForSeconds m_wait = new WaitForSeconds(0.1f);
    public float noteBpm;
    public string noteNumber;

    private void Start()
    {
        if (noteMode == 2)
        {
            GetComponent<SpriteRenderer>().sprite = longNote;
            GameObject inst = Instantiate(TouchBox);
            touchBoxScript = inst.GetComponent<TouchBoxScript>();
            touchBoxScript.note = this;
            touchBoxScript.noteMngScript = noteMngScript;
            touchBoxScript.gridMngScript = gridMngScript;
        }

        tikSource = GetComponent<AudioSource>();
        mSource = musicMng.GetComponent<AudioSource>();
        StartCoroutine(noteResize());

    }

    private void OnMouseOver()
    {
        if (Input.GetMouseButton(1))
        {
            /*if (noteMode == 2 && endNote != null)
            {
                noteMngScript.NoteRemove(endNote);
            }*/
            if (noteMode == 1)
                noteMngScript.NoteRemove(note);
        }
        /*else if (noteMode == 2 && Input.GetMouseButtonDown(0))
        {
            if (gridMngScript.noteMode == 2)
            {
                resizeMode = true;
                StartCoroutine(noteResize());
            }
        }*/
        /*if (Input.GetMouseButtonDown(0) && gridMngScript.noteMode == 0)
        {
            gridMngScript.SelectNote(this);
        }*/
    }

    private void Update()
    {
        if (transform.position.y <= -3.1f && underBar == false && mSource.isPlaying)
        {
            underBar = true;
            if (noteSoundToggle.isOn)
                tikSource.Play();
        }
        else if (transform.position.y > -3.1f && underBar == true && mSource.isPlaying == false)
        {
            underBar = false;
        }

        if (textInst != null)
            textInst.transform.position = transform.position;
    }

    public IEnumerator noteResize()
    {
        while(noteMode == 2 && resizeMode == true && Input.GetMouseButton(0))
        {
            if (endNote != null)
            {
                noteMngScript.NoteRemove(endNote);
            }

            gridMngScript.noteResize(gameObject);

            if (longNoteHeight >= 0)
                SetLongNoteHeight();
            
            // 위치가 시작부분과 동일하면 endNote 삭제
            if (endNote.node == note.node && note.IsEqual(endNote))
                endNote = null;
            else
            {
                endNote.line = note.line;

                noteMngScript.AddNote(endNote);
            }


            yield return m_wait;
        }
        resizeMode = false;
    }

    public void SetLongNoteHeight()
    {
        float noteGap = 0;
        if (endNote != null)
        {
            noteGap = gridMngScript.nodeList[endNote.node].offset - gridMngScript.nodeList[note.node].offset;
            noteGap -= gridMngScript.nodeList[note.node].height * ((float)note.pos / note.beat);
            noteGap += gridMngScript.nodeList[endNote.node].height * ((float)endNote.pos / endNote.beat);
        }
        if (noteGap < 0.26f)
            noteGap = 0.26f;

        longNoteHeight = noteGap;

        SpriteRenderer sp = GetComponent<SpriteRenderer>();
        sp.size = new Vector2(sp.size.x, longNoteHeight);

    }

    private void OnDestroy()
    {
        if (touchBoxScript)
            Destroy(touchBoxScript.gameObject);
    }

    /*public void SetNoteBPM(float bpm)
    {
        if (textInst != null)
            Destroy(textInst);

        noteBpm = bpm;
        textInst = Instantiate(bpmText);
        textInst.text = bpm.ToString();
        textInst.transform.position = transform.position;
    }

    private void OnDestroy()
    {
        Destroy(textInst);
        noteMngScript.RemoveNoteBpm(int.Parse(noteNumber));
    }*/

}
