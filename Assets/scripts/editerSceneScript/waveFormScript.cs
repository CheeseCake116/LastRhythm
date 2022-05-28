using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Sprite))]
public class WaveFormScript : MonoBehaviour
{
    private SpriteRenderer sprend = null;

    public void GetWaveForm(float xPos, float yPos, Texture2D tex, Rect rect)
    {
        sprend = GetComponent<SpriteRenderer>();
        sprend.sprite = Sprite.Create(tex, rect, Vector2.zero);
        // GetRenderTextureBytes(tex);

        transform.position = new Vector3(xPos, yPos, -0.15f);
    }

    private void GetRenderTextureBytes(Texture2D texture)
    {
        byte[] txtBytes;
        string str = "";
        int width = texture.width;
        int height = texture.height;
        Debug.Log(width + " " + height);

        // 매프레임 new를 해줄경우 메모리 문제 발생 -> 멤버 변수로 변경
        Texture2D txt = new Texture2D(width, height, TextureFormat.RGB24, false);
        txt.ReadPixels(new Rect(0, 0, width / 2, height / 2), 0, 0);
        txt.Apply();
        txtBytes = txt.EncodeToPNG();
        for (int i = 0; i < txtBytes.Length; i++)
        {
            str += txtBytes[i].ToString() + " ";
        }
        Debug.Log(str);
    }
}

