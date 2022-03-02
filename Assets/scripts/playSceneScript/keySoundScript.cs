using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class keySoundScript : MonoBehaviour
{
    GameObject me;

    // Start is called before the first frame update
    void Start()
    {
        me = this.gameObject;
        StartCoroutine(playSound());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator playSound()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(me);
    }
}
