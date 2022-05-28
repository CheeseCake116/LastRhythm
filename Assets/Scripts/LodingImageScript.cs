using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Jobs;
using UnityEngine.Jobs;

public class LodingImageScript : MonoBehaviour
{
    // public static readonly WaitForEndOfFrame m_wait = new WaitForEndOfFrame();

    // Start is called before the first frame update
    /*void Start()
    {
        StartCoroutine(ImageRotate());
    }*/

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(new Vector3(0, 0, -30));
    }

    /*IEnumerator ImageRotate()
    {
        while(true)
        {
            yield return m_wait;
            transform.Rotate(new Vector3(0, 0, -30));
        }
    }*/

    /*public Transform[] transforms;
    private TransformAccessArray _transformAccessArray;

    void Start()
    {
        transforms = new Transform[] { this.transform };
        _transformAccessArray = new TransformAccessArray(transforms);
        var job = new PingPongJob() { time = DateTime.Now };
        var handler = job.Schedule(_transformAccessArray);
        // handler.Complete();
    }

    public struct PingPongJob : IJobParallelForTransform
    {
        public DateTime time;
        public void Execute(int index, TransformAccess transform)
        {
            int count = 0;
            while(true)
            {
                double tempTime = (DateTime.Now - time).TotalMilliseconds;
                // Debug.Log(tempTime);
                count = (int)(tempTime / 100);
                transform.rotation = Quaternion.Euler(0, 0, -30 * count);
            }

        }
    }*/
}
