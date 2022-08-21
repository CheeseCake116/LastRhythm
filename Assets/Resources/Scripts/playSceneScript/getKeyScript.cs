using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class getKeyScript : MonoBehaviour
{
    string[] keys = { "s", "d", "f", "j", "k", "l" };
    public GameObject noteManagerObj;
    NoteCreate noteManager;

    // Start is called before the first frame update
    void Start()
    {
        noteManager = noteManagerObj.GetComponent<NoteCreate>();
    }

    // Update is called once per frame
    void Update()
    {
        for (int line = 0; line < 6; line++)
        {
            string key = keys[line];
            if (Input.GetKeyDown(key))
            {
                int pos = NoteContainer.notePos[line];
                if (pos < NoteContainer.noteStack[line].Count)
                {
                    NoteContainer.noteStack[line][pos].keyDown(key);
                }
                else
                {
                    Debug.Log("no note");
                }
            }
        }
    }
}
