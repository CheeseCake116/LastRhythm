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

    private void SetDropdownOptions()// Dropdown 목록 생성
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
