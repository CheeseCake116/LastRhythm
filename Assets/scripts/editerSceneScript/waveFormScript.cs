using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Sprite))]
public class waveFormScript : MonoBehaviour
{

    int width = 600;
    int height = 1024;
    public Color background = Color.black;
    public Color foreground = Color.yellow;
    public GameObject musicMng;
    public float bpm;

    private SpriteRenderer sprend = null;
    private int samplesize;
    private float[] samples = null;
    private float[] waveform = null;

    // Start is called before the first frame update
    void Start()
    {
        // reference components on the gameobject
        sprend = this.GetComponent<SpriteRenderer>();
        transform.position = new Vector3(-8f, -5f, 0);

        /*Texture2D texwav = GetWaveform();
        Rect rect = new Rect(Vector2.zero, new Vector2(width, height));
        sprend.sprite = Sprite.Create(texwav, rect, Vector2.zero);*/
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // This method is called by lineRenderScript.cs
    public void GetWaveform(AudioSource aud, float bpm, float nodeHeight)
    {
        //aud = musicMng.GetComponent<AudioSource>();
        height = (int)(aud.clip.length * bpm / 60 * nodeHeight + 1) * 10;
        Debug.Log("aud : " + aud.clip.length + ", bpm : " + bpm + "nodeHeight : " + nodeHeight);

        int halfwidth = width / 2;
        float widthscale = (float)width * 0.75f;

        // get the sound data
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        waveform = new float[height];

        samplesize = aud.clip.samples * aud.clip.channels;
        samples = new float[samplesize];
        aud.clip.GetData(samples, 0);

        int packsize = (samplesize / height);
        for (int w = 0; w < height; w++)
        {
            waveform[w] = Mathf.Abs(samples[w * packsize]);
        }

        // map the sound data to texture
        // 1 - clear
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tex.SetPixel(x, y, background);
            }
        }

        // 2 - plot
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < waveform[y] * widthscale; x++)
            {
                tex.SetPixel(halfwidth + x, y, foreground);
                tex.SetPixel(halfwidth - x, y, foreground);
            }
        }

        tex.Apply();

        //Rect rect = new Rect(Vector2.zero, new Vector2(width, height));
        //sprend.sprite = Sprite.Create(tex, rect, Vector2.zero);
        
        // return tex;
    }
}
