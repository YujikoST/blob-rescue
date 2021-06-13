using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    private void Update()
    {
        if (!BlobsManager.Instance.IsReady) return;
        var blob = BlobsManager.Instance.GetObject();
        blob.transform.position = transform.position;
        BlobsManager.Instance.ChangeCurrentBlob(blob.GetComponent<PlayerPlatformerController>());
        gameObject.SetActive(false);
    }
}
