using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class noteEffectScript : MonoBehaviour
{
    SpriteRenderer sprRend;
    float r;
    float g;
    float b;
    float a;
    GameObject me;

    // Start is called before the first frame update
    void Start()
    {
        sprRend = GetComponent<SpriteRenderer>();
        r = sprRend.color.r;
        g = sprRend.color.g;
        b = sprRend.color.b;
        a = sprRend.color.a;
        me = this.gameObject;
        
        StartCoroutine(Fadeout());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator Fadeout()
    {
        for(int i = 0; i < 10; i++)
        {
            a -= 0.1f;
            sprRend.color = new Color(r, g, b, a);
            yield return new WaitForSeconds(0.05f);
        }
        Destroy(me);
        
    }
}
