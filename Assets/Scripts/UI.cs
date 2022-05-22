using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public TMP_Text blobCountText;
    public Animator blobCountAnimator;

    private int prevActiveBlobCount = -1;

    // Start is called before the first frame update
    void Start()
    {
        //
    }

    // Update is called once per frame
    void Update()
    {
        int blobCount = GameManager.instance.blobs.Count;
        int activeBlobCount = blobCount - GameManager.instance.inactiveBlobs.Count;

        if (activeBlobCount != prevActiveBlobCount)
        {
            prevActiveBlobCount = activeBlobCount;

            blobCountText.text = activeBlobCount + " / " + blobCount;

            blobCountAnimator.Play("CountUp");

            if (activeBlobCount >= blobCount)
                blobCountAnimator.Play("CountDone");
        }
    }
}
