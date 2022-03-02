using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class noteMovement : MonoBehaviour
{
    float startTiming = 0.0f;
    float currentTiming = 0.0f;
    public float endTiming;
    public int line;
    public float speed;
    float barY = -3.1f;
    Vector3 myPos;
    public GameObject effect;
    public GameObject keySound;
    GameObject me;
    GameObject noteManagerObj;
    noteCreate noteManager;

    // Start is called before the first frame update
    void Start()
    {
        myPos = transform.position;
        currentTiming = startTiming;
        me = this.gameObject;
        noteManagerObj = GameObject.Find("noteManager");
        noteManager = noteManagerObj.GetComponent<noteCreate>();
    }

    // Update is called once per frame
    void Update()
    {
        if (currentTiming < endTiming + 0.2)
        { 
            currentTiming += Time.deltaTime;
            transform.position = new Vector3(myPos[0], speed * (endTiming - currentTiming) + barY, 0);
        }
        else
        {
            disappear();
        }

    }

    public void keyDown()
    {
        float judgement = endTiming - currentTiming;
        if (judgement < 0.2f)
        {
            Debug.Log((int)(judgement * 1000) + "ms");
            GameObject inst = Instantiate(effect);
            Instantiate(keySound);
            inst.transform.position = new Vector3(transform.position[0], -3, -2);

            disappear();
        }
    }

    void disappear()
    {
        noteManager.notePos[line]++;

        Destroy(me);
    }
}
