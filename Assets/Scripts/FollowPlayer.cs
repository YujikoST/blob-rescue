using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{

    public BlobsManager blobsManager;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void followBlob(GameObject blob, CinemachineVirtualCamera vcam)
    {
        if (blob != null)
        {
            var _followCurrentBlob = blob.transform;
            vcam.LookAt = _followCurrentBlob;
            vcam.Follow = _followCurrentBlob;
        }
    }
}
