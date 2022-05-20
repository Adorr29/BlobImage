using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public TMP_Text textBlobCount;

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

        if (activeBlobCount != prevActiveBlobCount && prevActiveBlobCount <= blobCount)
        {
            prevActiveBlobCount = activeBlobCount;

            textBlobCount.text = activeBlobCount + " / " + blobCount;
        }
    }
}
