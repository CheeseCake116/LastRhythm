using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ToEditScript : MonoBehaviour
{
    [SerializeField] Button me;
    // Start is called before the first frame update
    void Start()
    {
        me.onClick.AddListener(delegate { ToEditer(); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ToEditer()
    {
        SceneManager.LoadScene("Scenes/editerScene");
    }
}
