using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.U2D.IK;
using UnityEngine;

public class BlobsManager : MonoBehaviour
{
    public static BlobsManager Instance;
    private bool _isReady = false;
    public bool IsReady { get; private set; }
    public GameObject objectToPool;
    public int amountToPool = 20;
    private PlayerPlatformerController _currentBlob;
    private List<GameObject> pool;

    private void Awake() => Instance = this;

    // Start is called before the first frame update
    void Start()
    {
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

        if (Helpers.WantsToEat())
        {
            var edibleBlobs = GetEdibleBlobs();
            Helpers.EatBlobs(_currentBlob, edibleBlobs);
            edibleBlobs.ForEach(MarkAsEated);
        }

    }

    public void ChangeCurrentBlob(PlayerPlatformerController blob) => _currentBlob = blob;

    private void MarkAsEated(PhysicsObject blob)
    {
        // TODO
    }

    private List<PhysicsObject> GetEdibleBlobs()
    {
        // TODO
        return new List<PhysicsObject>();
    }
    
}
