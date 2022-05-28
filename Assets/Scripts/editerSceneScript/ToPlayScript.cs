using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ToPlayScript : MonoBehaviour
{
    [SerializeField] Button me;
    // Start is called before the first frame update
    void Start()
    {
        me.onClick.AddListener(delegate { ToPlay(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ToPlay()
    {
        SceneManager.LoadScene("Scenes/playScene");
    }
}
