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
        "노래 선택",
        "로그인",
        "로비",
        "전투 1",
        "보스전 1",
        "전투 2",
        "보스전 2",
        "대화 1",
        "대화 2",
        "대화 3",
        "대화 4",
        "사랑은 파르페",
        "사랑은 파르페_inst",
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

    private void SetDropdownOptions()// Dropdown 목록 생성
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
