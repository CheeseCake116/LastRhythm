using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteContainer : MonoBehaviour
{
    public static List<NoteMovement>[] noteStack = new List<NoteMovement>[6];
    public static int[] notePos = new int[] { 0, 0, 0, 0, 0, 0 };

    public static void NoteClear()
    {
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < noteStack[i].Count; j++)
            {
                Destroy(noteStack[i][j].me);
            }
        }

        for (int i = 0; i < 6; i++)
        {
            noteStack[i] = new List<NoteMovement>();
        }
        notePos = new int[] { 0, 0, 0, 0, 0, 0 };
    }
}
