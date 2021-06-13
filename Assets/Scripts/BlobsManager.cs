using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.U2D.IK;
using UnityEngine;

public class BlobsManager : MonoBehaviour
{
    public static BlobsManager Instance;
    public bool IsReady { get; private set; }
    public GameObject objectToPool;
    public int amountToPool = 20;
    private PlayerPlatformerController _currentBlob;
    private List<GameObject> pool;
    private int activeBlobsIndex;

    private void Awake()
    {
        activeBlobsIndex = 0;
        Instance = this;
        pool = Helpers.CreatePool(objectToPool)(amountToPool);
        foreach (var obj in pool)
        {
            obj.transform.parent = transform;
        }
        IsReady = true;
    } 

    public GameObject GetObject()
    {
        foreach (var obj in pool)
        {
            if (obj.activeSelf == false)
            {
                obj.SetActive(true);
                return obj;
            }
        }
        return null;
    }
    
    protected void Update()
    {
        _currentBlob.targetVelocity = Vector2.zero;
        Helpers.HandleJump(_currentBlob, _currentBlob.grounded, 7, 0.35f);

        Helpers.HandleHorizontalMovement(_currentBlob, _currentBlob.spriteRenderer, 5);

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool blobFound = false;
            
            for (int i = activeBlobsIndex; i < pool.Count; i++)
            {
                if (CanBeSelected(pool[i]))
                {
                    // Deselect current blob
                    _currentBlob.targetVelocity = Vector2.zero;
                    _currentBlob.GetComponent<SpriteRenderer>().color = Color.white;
                    
                    // Select new blob
                    _currentBlob = pool[i].GetComponent<PlayerPlatformerController>();
                    _currentBlob.GetComponent<SpriteRenderer>().color = Color.cyan;
                    activeBlobsIndex = i;
                    blobFound = true;
                    break;
                }
            }

            if (!blobFound)
            {
                for (int i = 0; i < activeBlobsIndex; i++)
                {
                    if (!pool[i].GetComponent<PlayerPlatformerController>().Equals(_currentBlob) && pool[i].activeSelf)
                    { 
                        _currentBlob.targetVelocity = Vector2.zero;
                        _currentBlob.GetComponent<SpriteRenderer>().color = Color.white;
                        _currentBlob = pool[i].GetComponent<PlayerPlatformerController>();
                        _currentBlob.GetComponent<SpriteRenderer>().color = Color.cyan;
                        activeBlobsIndex = i;
                        blobFound = true;
                        break;
                    }
                }
            }
        }
    }

    private bool CanBeSelected(GameObject blob)
    {
        return !blob.GetComponent<PlayerPlatformerController>().Equals(_currentBlob) && blob.activeSelf;
    }

    public void ChangeCurrentBlob(PlayerPlatformerController blob)
    {
        _currentBlob = blob;
        _currentBlob.GetComponent<SpriteRenderer>().color = Color.cyan;
    }
}
