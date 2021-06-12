using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerPlatformerController : PhysicsObject
{
    public float maxSpeed = 5;
    public float jumpTakeOffSpeed = 5;

    public const float DEFAULT_Z = 0;
    public const float MIN_BLOB_AREA = 0.5f;

    private SpriteRenderer _spriteRenderer;
    private Animator _animator;

    // Start is called before the first frame update
    void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    protected override void ComputeVelocity()
    {
        Vector2 move = Vector2.zero;

        move.x = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && grounded)
        {
            velocity.y = jumpTakeOffSpeed;
        }
        else if (Input.GetButtonUp("Jump"))
        {
            if (velocity.y > 0)
            {
                velocity.y *= .35f;
            }
        }

        bool flipSprite = (_spriteRenderer.flipX ? (move.x > 0.01f) : (move.x < 0.01f));

        if (flipSprite)
        {
            _spriteRenderer.flipX = !_spriteRenderer.flipX;
        }


        targetVelocity = move * maxSpeed;
    }

    static readonly Func<float, float>
        GetArea = diameter =>
            Mathf.PI * Mathf.Pow(diameter / 2, 2);

    static readonly Func<GameObject, float>
        GetBlobArea = blob =>
            GetArea(blob.transform.localScale.x);

    static readonly Func<float, float>
        GetDiameter = area =>
            Mathf.Sqrt(area / Mathf.PI) * 2;


    static float GrowBlob(GameObject blob, float eatedArea)
    {
        var blobArea = GetBlobArea(blob);
        var newArea = blobArea + eatedArea;

        var diameter = GetDiameter(newArea);
        var difference = Mathf.Abs(diameter - blob.transform.localScale.x);

        blob.transform.localScale = new Vector3(diameter, diameter, DEFAULT_Z);

        return difference;
    }

    static float ShrinkBlob(GameObject blob, float shrinkedArea)
    {
        var blobArea = GetBlobArea(blob);
        var newArea = blobArea - shrinkedArea;

        if (newArea < MIN_BLOB_AREA)
        {
            return 0;
        }

        var diameter = GetDiameter(newArea);
        var difference = Mathf.Abs(diameter - blob.transform.localScale.x);

        blob.transform.localScale = new Vector3(diameter, diameter, DEFAULT_Z);
        return difference;
    }

    static readonly Func<GameObject, Func<int, List<GameObject>>>
        CreatePool = gameObject => amount =>
            Enumerable
                .Repeat(0, amount)
                .Select(InstantiateUnactive(gameObject))
                .ToList();


    static readonly Func<GameObject, Func<int, GameObject>>
        InstantiateUnactive = gameObject => _ =>
        {
            var instance = Instantiate(gameObject);
            instance.SetActive(false);
            return instance;
        };
}
