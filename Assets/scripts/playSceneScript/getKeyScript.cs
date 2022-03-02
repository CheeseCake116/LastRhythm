using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getKeyScript : MonoBehaviour
{
    string[] keys = { "s", "d", "f", "j", "k", "l" };
    public GameObject noteManagerObj;
    noteCreate noteManager;

    // Start is called before the first frame update
    void Start()
    {
        noteManager = noteManagerObj.GetComponent<noteCreate>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int line = 0; line < 6; line++)
        {
            string key = keys[line];
            if (Input.GetKeyDown(key))
            {
                Debug.Log(key);
                int pos = noteManager.notePos[line];
                try
                {
                    noteManager.noteStack[line][pos].GetComponent<noteMovement>().keyDown();
                }
                catch
                {
                    Debug.Log("no note");
                }
            }
        }
    }
}
