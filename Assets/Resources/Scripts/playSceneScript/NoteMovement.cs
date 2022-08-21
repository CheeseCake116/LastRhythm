using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class NoteMovement : MonoBehaviour
{
    [HideInInspector] public float speed = 1, startTiming = 0.0f, currentTiming = 0.0f, endTiming, finishTiming;
    [HideInInspector] public int line;
    [HideInInspector] public NoteCreate noteCreateScript;
    float barY = -3.1f;
    Vector3 myPos;
    public GameObject effect;
    public GameObject keySound;
    public GameObject me;
    public int noteMode = 1;
    bool isMoving = false;
    float longHeight = 1f;
    public float bpm;
    WaitForSeconds m_wait;
    SpriteRenderer sp;
    [SerializeField] ParticleSystem particle;

    // Start is called before the first frame update
    void Start()
    {
        sp = GetComponent<SpriteRenderer>();
        m_wait = new WaitForSeconds(30f / bpm);
        myPos = transform.position;
        currentTiming = startTiming;
        me = gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMoving)
        {
            if (noteMode == 1)
                transform.position = new Vector3(myPos[0], speed * (endTiming - currentTiming) + barY, 0);
            else if (noteMode == 2)
                transform.position = new Vector3(myPos[0], speed * (endTiming - currentTiming) + barY + (longHeight / 2), 0);
        }
    }

    public void StartMove()
    {
        isMoving = true;
        if (noteMode == 1)
        {
            DOTween.To(() => startTiming, x => currentTiming = x, endTiming, endTiming - startTiming);
            StartCoroutine(DelayDisappear(endTiming - startTiming + 0.2f));
        }
        else if (noteMode == 2)
        {
            DOTween.To(() => startTiming, x => currentTiming = x, finishTiming, finishTiming - startTiming);
            StartCoroutine(DelayDisappear(finishTiming - startTiming + 0.2f));
        }
    }

    public void keyDown(string key)
    {
        float judgement = endTiming - currentTiming;
        if (judgement < 0.2f)
        {
            // Debug.Log((int)(judgement * 1000) + "ms");
            /*GameObject inst = Instantiate(effect);
            inst.transform.position = new Vector3(myPos[0], -3, -2);*/
            noteCreateScript.particleStart(line);

            Instantiate(keySound);

            if (noteMode == 1)
            {
                NoteContainer.notePos[line]++;
                disappear();
            }
            else if (noteMode == 2)
            {
                NoteContainer.notePos[line]++;
                StartCoroutine(LongNoteKeyDown(key));
            }
        }
        else
        {
            Debug.LogError("judgement["+line+"] : " + judgement);
        }
    }
    IEnumerator LongNoteKeyDown(string key)
    {
        while (true)
        {
            yield return m_wait;
            if (currentTiming >= finishTiming)
            {
                disappear();
            }
            else if (Input.GetKey(key))
            {
                /*GameObject inst = Instantiate(effect);
                inst.transform.position = new Vector3(myPos[0], -3, -2);*/
                noteCreateScript.particleStart(line);
                Instantiate(keySound);
            }
            else
            {
                disappear();
            }
        }
    }

    IEnumerator DelayDisappear(float delay)
    {
        yield return new WaitForSeconds(delay);
        disappear();
    }

    void disappear()
    {
        Destroy(me);
    }

    public void noteResize()
    {
        sp = GetComponent<SpriteRenderer>();
        longHeight = (finishTiming - endTiming) * speed;
        Debug.Log(longHeight);
        sp.size = new Vector2(sp.size.x, longHeight);
    }
}
