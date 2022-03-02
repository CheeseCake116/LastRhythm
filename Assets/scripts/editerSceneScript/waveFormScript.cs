using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Sprite))]
public class waveFormScript : MonoBehaviour
{

    int width = 1024;
    int height = 200;
    public Color background = Color.black;
    public Color foreground = Color.yellow;
    public GameObject musicMng;

    private AudioSource aud = null;
    private SpriteRenderer sprend = null;
    private int samplesize;
    private float[] samples = null;
    private float[] waveform = null;

    // Start is called before the first frame update
    void Start()
    {
        // reference components on the gameobject
        sprend = this.GetComponent<SpriteRenderer>();

        Texture2D texwav = GetWaveform();
        Rect rect = new Rect(Vector2.zero, new Vector2(width, height));
        sprend.sprite = Sprite.Create(texwav, rect, Vector2.zero);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Texture2D GetWaveform()
    {
        aud = musicMng.GetComponent<AudioSource>();
        Debug.Log(aud.clip);

        int halfheight = height / 2;
        float heightscale = (float)height * 0.75f;

        // get the sound data
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        waveform = new float[width];

        samplesize = aud.clip.samples * aud.clip.channels;
        samples = new float[samplesize];
        aud.clip.GetData(samples, 0);

        int packsize = (samplesize / width);
        for (int w = 0; w < width; w++)
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
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < waveform[x] * heightscale; y++)
            {
                tex.SetPixel(x, halfheight + y, foreground);
                tex.SetPixel(x, halfheight - y, foreground);
            }
        }

        tex.Apply();

        return tex;
    }
}
