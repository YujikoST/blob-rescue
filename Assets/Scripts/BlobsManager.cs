using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using Cinemachine;
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
    private static readonly Color SelectedColor = Helpers.FromRGBA(117, 182, 233, 85);
    private static readonly Color UnselectedColor = Color.white;
    private CinemachineVirtualCamera vcam;

    private void Start()
    {
        var obj = GameObject.FindGameObjectsWithTag("VCam")[0];
        vcam = obj.GetComponent<CinemachineVirtualCamera>();
    }

    private void Awake()
    {
        Instance = this;
        objectToPool.GetComponent<SpriteRenderer>().color = UnselectedColor;
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
            Helpers.EatBlobs(_currentBlob.gameObject, edibleBlobs);
            edibleBlobs.ForEach(MarkAsEated);
        }
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            var activeBlobsIndex = pool.IndexOf(_currentBlob.gameObject);
            
            var selectableBlob =
                pool
                    .Skip(activeBlobsIndex)
                    .Union(pool.Take(activeBlobsIndex))
                    .FirstOrDefault(CanBeSelected);

            if (selectableBlob != null)
            {
                // Deselect current blob
                _currentBlob.targetVelocity = Vector2.zero;
                _currentBlob.GetComponent<SpriteRenderer>().color = UnselectedColor;
                    
                // Select new blob
                _currentBlob = selectableBlob.GetComponent<PlayerPlatformerController>();
                _currentBlob.GetComponent<SpriteRenderer>().color = SelectedColor;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            var spittedBlob = GetObject();
            var blobPosition = _currentBlob.transform.position;
            
            var direction = GetDirectionToMouse(blobPosition);

            spittedBlob.transform.position = blobPosition + direction;

            spittedBlob.GetComponent<PlayerPlatformerController>().MoveAsParable(direction * 10);
        }
        
        FollowPlayer.followBlob(_currentBlob.gameObject, vcam);
        
    }

    private static readonly Func<Vector3, Vector3>
        GetDirectionToMouse = position =>
        {
            var camera = Camera.main;
            
            var tempMousePosition = Input.mousePosition;
            tempMousePosition.z = camera.nearClipPlane;

            var mousePosition = camera.ScreenToWorldPoint(tempMousePosition);
            position.z = 0;
            mousePosition.z = 0;
            return Vector3.Normalize(mousePosition - position);
        };

    private static Func<GameObject, float>
        ResizeToOriginal = Helpers.ReplaceBlobArea(1f);

    private void MarkAsEated(GameObject blob)
    {
        blob.SetActive(false);
        ResizeToOriginal(blob);
    }

    private List<GameObject> GetEdibleBlobs()
    {
        float minDistance = 2f;
        var distanceToCurrent = Helpers.GetDistance(_currentBlob.gameObject);
        return pool
            .Where(CanBeSelected)
            .Where((blob) => distanceToCurrent(blob) < minDistance)
            .ToList();
    }
    
    private bool CanBeSelected(GameObject blob)
    {
        return !blob.GetComponent<PlayerPlatformerController>().Equals(_currentBlob) && blob.activeSelf;
    }

    public void ChangeCurrentBlob(PlayerPlatformerController blob)
    {
        _currentBlob = blob;
        _currentBlob.GetComponent<SpriteRenderer>().color = SelectedColor;
        _currentBlob.GetComponent<PlayerPlatformerController>().CancelParableMovement();
    }
}
