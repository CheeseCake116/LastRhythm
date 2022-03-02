using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class musicSelectScript : MonoBehaviour
{
    [Header("Dropdown")]
    public Dropdown dropdown;
    public GameObject musicMng;
    musicManagerScript musicMngScript;
    string[] optionTexts =
    {
        "�α���",
        "�κ�",
        "���� 1",
        "������ 1",
        "���� 2",
        "������ 2",
        "��ȭ 1",
        "��ȭ 2",
        "��ȭ 3",
        "��ȭ 4",
    };
    AudioClip[] musics;

    void Start()
    {
        musicMngScript = musicMng.GetComponent<musicManagerScript>();
        musics = musicMngScript.musics;

        SetDropdownOptions();
    }

    void Update()
    {
        
    }

    private void SetDropdownOptions()// Dropdown ��� ����
    {
        dropdown.options.Clear();
        for (int i = 0; i < optionTexts.Length; i++)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = (i + 1).ToString() + ". " + optionTexts[i];
            dropdown.options.Add(option);
        }
    }
}
