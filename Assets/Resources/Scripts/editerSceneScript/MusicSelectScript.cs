using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicSelectScript : MonoBehaviour
{
    [Header("Dropdown")]
    public Dropdown dropdown;
    public GameObject musicMng;
    MusicManagerScript musicMngScript;
    string[] optionTexts =
    {
        "�뷡 ����",
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
        "����� �ĸ���",
        "����� �ĸ���_inst",
    };
    AudioClip[] musics;

    void Start()
    {
        musicMngScript = musicMng.GetComponent<MusicManagerScript>();
        musics = musicMngScript.musics;

        SetDropdownOptions();
    }

    void Update()
    {
        
    }

    private void SetDropdownOptions()// Dropdown ��� ����
    {
        dropdown.options.Clear();
        Dropdown.OptionData option = new Dropdown.OptionData();
        option.text = optionTexts[0];
        dropdown.options.Add(option);
        for (int i = 1; i < optionTexts.Length; i++)
        {
            option = new Dropdown.OptionData();
            option.text = i.ToString() + ". " + optionTexts[i];
            dropdown.options.Add(option);
        }
    }
}
