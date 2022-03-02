using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class noteCreate : MonoBehaviour
{
    public GameObject notePre;
    public List<GameObject>[] noteStack = new List<GameObject>[6];
    public int[] notePos = new int[6];

    int line = 0;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 6; i++)
        {
            noteStack[i] = new List<GameObject>();
            notePos[i] = 0;
        }
        StartCoroutine(createNote());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator createNote()
    {
        while (true)
        {
            line = Random.Range(0, 6);
            
            GameObject inst = Instantiate(notePre);
            noteMovement instScript = inst.GetComponent<noteMovement>();
            inst.transform.position = new Vector3(line - 7.5f, 10, -1);
            instScript.line = line;

            noteStack[line].Add(inst);
            line++;

            yield return new WaitForSeconds(0.25f);
        }
    }
}
