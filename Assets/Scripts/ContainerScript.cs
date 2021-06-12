using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContainerScript : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Blob"))
        {
            var obj = PoolManager.Instance.GetObject();
            obj.transform.position = transform.position;
            Destroy(gameObject);
        }
    }
}
