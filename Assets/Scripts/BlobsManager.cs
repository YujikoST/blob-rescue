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
    public static readonly float MinBlobArea = 0.5f;
    public static readonly float DefaultBlobArea = 1f;
    public static readonly float SpitForce = 9f;
    private bool _isSpitting = false;
    public float spitDelay = .3f;

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
            _currentBlob.TriggerMergingAnimation(true);
            var edibleBlobs = GetEdibleBlobs();
            Helpers.EatBlobs(_currentBlob.gameObject, edibleBlobs);
            edibleBlobs.ForEach(MarkAsEated);
        }

         if (Helpers.WantsToStopEat())
         {
             _currentBlob.TriggerMergingAnimation(false);
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

        if (Input.GetMouseButton(0))
        {
            _currentBlob.TriggerSpittingAnimation(true);
        }

        if (Input.GetMouseButtonUp(0))
        {
            _currentBlob.TriggerSpittingAnimation(false);
            if (CanSpit(_currentBlob.gameObject))
            {
                StartCoroutine(Spit());
            }
        }
        
        FollowPlayer.followBlob(_currentBlob.gameObject, vcam);
        
    }

    private IEnumerator Spit()
    {
        _isSpitting = true;
        // Get spittedBlob and direction
        var spittedBlob = GetObject();
        var blobPosition = _currentBlob.transform.position;
        var direction = GetDirectionToMouse(blobPosition);

        // Move spittedBlob
        spittedBlob.transform.position = blobPosition + direction * _currentBlob.transform.localScale.x ;
        var playerPlatformerController = spittedBlob.GetComponent<PlayerPlatformerController>();
        playerPlatformerController.grounded = false;
        playerPlatformerController.MoveAsParable(direction * SpitForce);
            
        // Resize blobs
        ResizeToMini(spittedBlob);
        Helpers.ShrinkBlob(MinBlobArea)(_currentBlob.gameObject);
        
        Debug.Log(direction.x);
        if (direction.x < 0)
        {
            spittedBlob.GetComponent<SpriteRenderer>().flipX = spittedBlob.GetComponent<SpriteRenderer>().flipX;
        }
        
        spittedBlob.GetComponent<PlayerPlatformerController>().TriggerLaunchingAnimation(true);
        
        yield return new WaitForSeconds(spitDelay);
        spittedBlob.GetComponent<PlayerPlatformerController>().TriggerLaunchingAnimation(false);
        _isSpitting = false;
    }

    private static readonly Func<Vector3, Vector3>
        GetDirectionToMouse = position =>
        {
            var camera = Camera.main;
            
            var mousePosition = Input.mousePosition;
            mousePosition.z = 0;
            var blobPositionInScreen = camera.WorldToScreenPoint(position);
            blobPositionInScreen.z = 0;

            return Vector3.Normalize(mousePosition - blobPositionInScreen);
        };

    private static Func<GameObject, float>
        ResizeToOriginal = Helpers.ReplaceBlobArea(DefaultBlobArea);
    
    private static Func<GameObject, float>
        ResizeToMini = Helpers.ReplaceBlobArea(MinBlobArea);

    private void MarkAsEated(GameObject blob)
    {
        blob.SetActive(false);
        ResizeToOriginal(blob);
    }

    private static Func<GameObject, bool>
        CanSpit = blob =>
            Helpers.GetBlobArea(blob) - MinBlobArea >= MinBlobArea;

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
