using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadListScript : MonoBehaviour
{
    public List<string> fileNames;
    [SerializeField] Dropdown dropdown;

    // Start is called before the first frame update
    void Start()
    {
        SetDropdownOptions();
    }

    public string GetSelectedFile()
    {
        return fileNames[dropdown.value];
    }

    public void SetDropdownOptions()// Dropdown 목록 생성
    {
        fileNames = new List<string>();

        string FolderName = "./patterns/";
        fileNames.Add("");

        System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(FolderName);
        foreach (System.IO.FileInfo File in di.GetFiles())
        {
            if (File.Extension.ToLower().CompareTo(".lrs") == 0)
            {
                string FileNameOnly = File.Name.Substring(0, File.Name.Length - 4);
                fileNames.Add(FileNameOnly);
            }
        }

        dropdown.options.Clear();

        Dropdown.OptionData option = new Dropdown.OptionData();
        option.text = "불러오기";
        dropdown.options.Add(option);

        for (int i = 1; i < fileNames.Count; i++)
        {
            option = new Dropdown.OptionData();
            option.text = i.ToString() + ". " + fileNames[i];
            dropdown.options.Add(option);
        }
    }
}
