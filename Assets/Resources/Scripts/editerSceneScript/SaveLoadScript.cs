using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;


public class SaveLoadScript : MonoBehaviour
{
    [SerializeField] GameObject gridObj, musicMng, noteMng, noteObj;
    [SerializeField] InputField fileNameInput, offsetInput, bpmInput;
    [SerializeField] Dropdown musicList, loadList;
    [SerializeField] Button saveButton;
    LineRenderScript gridObjScript;
    LoadListScript loadListScript;
    MusicManagerScript musicMngScript;
    NoteManagerScript noteMngScript;
    NoteInfoScript[] longNoteScriptStack;

    string savefilename = "";
    string loadfilename = "";

    Dictionary<int, List<NoteInfo>[]> noteDict;

    // Start is called before the first frame update
    void Start()
    {
        gridObjScript = gridObj.GetComponent<LineRenderScript>();
        musicMngScript = musicMng.GetComponent<MusicManagerScript>();
        noteMngScript = noteMng.GetComponent<NoteManagerScript>();

        fileNameInput.onSubmit.AddListener(delegate { SetFileName(); });
        saveButton.onClick.AddListener(delegate { SaveFile(); });
        loadList.onValueChanged.AddListener(delegate { LoadFile(); });
        loadListScript = loadList.GetComponent<LoadListScript>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SetFileName()
    {
        savefilename = fileNameInput.text;
    }

    void SaveFile()
    {
        // ���ϸ� ������ �������
        if (savefilename == "")
            return;

        // ���� �б� ����
        FileStream file = new FileStream("./patterns/" + savefilename + ".lrs", FileMode.Create);
        StreamWriter fileSW = new StreamWriter(file);
        string fmt = "000";
        noteDict = noteMngScript.noteDict;

        int selectedMusic = musicMngScript.selectedMusic;
        float bpm = musicMngScript.bpm;
        float offset = musicMngScript.Offset;

        // ������� �ۼ�
        fileSW.Write("*Header\n");
        fileSW.Write("#BGM " + selectedMusic + "\n");
        fileSW.Write("#BPM " + bpm + "\n");
        fileSW.Write("#OFFSET " + offset + "\n");
        fileSW.Write("\n");

        // BPM ���� ���� �ۼ�
        List<NodeInfo> nodeBpmList = noteMngScript.nodeBpmList;
        for (int i = 0; i < nodeBpmList.Count; i++)
        {
            fileSW.Write("#SETBPM " + nodeBpmList[i].node + " " + nodeBpmList[i].bpm + "\n");
        }
        fileSW.Write("\n");

        // ��Ʈ���� �ۼ�
        fileSW.Write("*NoteData\n");

        List<int> keys = new List<int>();
        keys.AddRange(noteDict.Keys);
        keys.Sort();

        foreach (int n in keys)
        {
            // �ش� ��忡 ������ ���� ��� ���ư�
            if (noteDict[n] == null)
                continue;

            for (int l = 0; l < 6; l++)
            {
                // �ش� ������ ��忡 ������ ���� ��� ���ư�
                if (noteDict[n][l] == null || noteDict[n][l].Count == 0)
                    continue;

                int beat = 1;
                List<int> notes = new List<int>();
                string[] strArray, strArrayLong;
                string saveStr = "";
                string saveStrLong = "";

                // ��Ʈ ����� ���ϱ�
                for (int i = 0; i < noteDict[n][l].Count; i++)
                {
                    NoteInfo note = noteDict[n][l][i];
                    if (beat != note.beat)
                    {
                        beat = beat * note.beat * GetGCD(beat, note.beat);
                    }
                }

                // ��Ʈ ���� ������ �迭
                strArray = new string[beat]; // �Ϲݳ�Ʈ
                strArrayLong = new string[beat]; // �ճ�Ʈ


                // ��Ʈ ���� ����
                for (int i = 0; i < noteDict[n][l].Count; i++)
                {
                    NoteInfo note = noteDict[n][l][i];
                    

                    // �ճ�Ʈ �� ǥ���ϴ� ��Ʈ
                    if (note.inst == null)
                    {
                        strArrayLong[beat / note.beat * note.pos] = "LN";
                        continue;
                    }

                    NoteInfoScript noteScript = note.inst.GetComponent<NoteInfoScript>();
                    string noteNumber = noteScript.noteNumber; // "01", "02", ... ���

                    // �Ϲ� ��Ʈ. endNote�� �������� ���� �ճ�Ʈ�� �Ϲݳ�Ʈ�� ����ϱ� ���� endNote�� �����Ѵ�
                    if (noteScript.endNote == null)
                        strArray[beat / note.beat * note.pos] = noteNumber;
                    
                    // �ճ�Ʈ
                    else
                    {
                        strArrayLong[beat / note.beat * note.pos] = noteNumber;
                    }
                }

                bool nullString = true;
                // ����� �Ϲݳ�Ʈ�� �ϳ��� ���ڿ��� ����
                for (int i = 0; i < beat; i++)
                {
                    if (strArray[i] == null)
                        saveStr += "00";
                    else
                    {
                        nullString = false;
                        saveStr += strArray[i];
                    }
                        
                }

                // ������ �ۼ�
                if (nullString == false)
                    fileSW.Write("#" + n.ToString(fmt) + "1" + l.ToString() + " " + saveStr + "\n");

                nullString = true;
                // ����� �ճ�Ʈ�� �ϳ��� ���ڿ��� ����
                for (int i = 0; i < beat; i++)
                {
                    if (strArrayLong[i] == null)
                        saveStrLong += "00";
                    else
                    {
                        nullString = false;
                        saveStrLong += strArrayLong[i];
                    }
                }

                // ������ �ۼ�
                if (nullString == false)
                    fileSW.Write("#" + n.ToString(fmt) + "5" + l.ToString() + " " + saveStrLong + "\n");
            }
            fileSW.Write("\n");
        }

        fileSW.Close();

        loadListScript.SetDropdownOptions();
    }

    int GetGCD(int a, int b)
    {
        int c;
        while (b != 0)
        {
            c = a % b;
            a = b;
            b = c;
        }
        return a;
    }

    void readStringArray(string[] strArray)
    {
        foreach(string tempstr in strArray)
        {
            Debug.Log(tempstr);
        }
    }

    void LoadFile()
    {
        // ��Ӵٿ�� �� ���ϸ� ��������
        loadfilename = loadList.GetComponent<LoadListScript>().GetSelectedFile();
        
        // ù��° "�ҷ�����" ���ý� �� ���ڿ� ��ȯ. �� ��� �ҷ����� ����
        if (loadfilename == "")
            return;

        // �ҷ��� �� �����ϱ� ���ϰ� ������ ���ϸ� �ҷ��� ���ϸ����� �ٲ���
        savefilename = loadfilename;
        fileNameInput.text = savefilename;

        // ���� �ٷ�� �Ŷ� try��
        try
        {
            FileStream file = new FileStream("./patterns/" + loadfilename + ".lrs", FileMode.Open);
            StreamReader fileLD = new StreamReader(file);
            longNoteScriptStack = new NoteInfoScript[6];

            // ��Ʈ ����
            noteMngScript.NoteClear();

            // �ؽ�Ʈ ���� ���� ����
            string str = fileLD.ReadToEnd();
            string[] firstData = str.Split(new char[] { '*' }); // "", "Header...", "Data..."

            // Header Parsing
            string[] headerData = firstData[1].Split(new char[] { '#' }); // "Header", "BGM ...", ...
            for (int i = 0; i < headerData.Length; i++)
            {
                headerData[i] = headerData[i].Trim();
            }
            
            if (headerData[0] != "Header")
                return;

            int bgm = -1;
            float bpm = -1;
            int offset = -1;

            noteMngScript.nodeBpmList.Clear();
            for (int i = 1; i < headerData.Length; i++)
            {
                string[] headerLine = headerData[i].Split(new char[] { ' ' });
                if (headerLine[0] == "BGM")
                {
                    if (Int32.TryParse(headerLine[1], out int _bgm))
                    {
                        bgm = _bgm;
                    }
                }
                else if (headerLine[0] == "BPM")
                {
                    if (float.TryParse(headerLine[1], out float _bpm))
                    {
                        bpm = _bpm;
                    }
                }
                else if (headerLine[0] == "OFFSET")
                {
                    if (Int32.TryParse(headerLine[1], out int _offset))
                    {
                        offset = _offset;
                    }
                }
                else if (headerLine[0] == "SETBPM")
                {
                    if (Int32.TryParse(headerLine[1], out int _node))
                    {
                        if (Int32.TryParse(headerLine[2], out int _bpm))
                        {
                            noteMngScript.AddNodeBpm(_node, _bpm);
                        }
                    }
                }
            }

            if (bpm > -1 && bpm > -1 && offset > -1)
            {
                musicList.value = bgm;
                bpmInput.text = bpm.ToString();
                offsetInput.text = offset.ToString();

                /*musicMngScript.bpm = bpm;
                musicMngScript.selectedMusic = bgm;
                musicMngScript.Offset = offset;*/
                musicMngScript.musicReset();
                gridObjScript.GridReset();
            }
            else
            {
                return;
            }



            // NoteData Parsing
            string[] noteData = firstData[2].Split(new char[] { '#' }); // "NoteData", "00010 ...", ...
            for (int i = 0; i < noteData.Length; i++)
            {
                noteData[i] = noteData[i].Trim();
            }

            if (noteData[0] != "NoteData")
                return;

            Dictionary<int, NoteInfo[]> noteDict = new();
            int[] stackCount = new int[] { 0, 0, 0, 0, 0, 0 };

            for (int i = 1; i < noteData.Length; i++)
            {
                if (noteData[i] == "")
                    continue;

                string[] noteLine = noteData[i].Split(new char[] { ' ' });

                int node = -1;
                int player = -1;
                int line = -1;
                int beat = -1;

                if (Int32.TryParse(noteLine[0].Substring(0, 3), out int _node))
                {
                    node = _node;
                }
                if (Int32.TryParse(noteLine[0].Substring(3, 1), out int _player))
                {
                    player = _player;
                }
                if (Int32.TryParse(noteLine[0].Substring(4, 1), out int _line))
                {
                    line = _line;
                }

                beat = noteLine[1].Length / 2;
                for (int pos = 0; pos < beat; pos++)
                {
                    string tempNote = noteLine[1].Substring(pos * 2, 2);
                    if (tempNote != "00")
                    {
                        if (player == 5) // �ճ�Ʈ
                        {
                            if (longNoteScriptStack[line] == null) // ���۳�Ʈ
                            {
                                NoteInfoScript instScript = gridObjScript.NoteCreation(line, node, beat, pos, tempNote);
                                instScript.noteMode = 2;
                                longNoteScriptStack[line] = instScript;
                            }
                            else // ����Ʈ
                            {
                                gridObjScript.LongNoteCreation(longNoteScriptStack[line], line, node, beat, pos);
                                longNoteScriptStack[line] = null;
                            }
                        }
                        else // �Ϲݳ�Ʈ
                        {
                            NoteInfoScript instScript = gridObjScript.NoteCreation(line, node, beat, pos, tempNote);
                            instScript.noteMode = 1;
                        }
                    }
                }
            }
            // musicMngScript.musicReset();
            // gridObjScript.GridReset();
        }
        catch
        {
            Debug.LogError("�ҷ����� ����");
        }
        
    }
}
