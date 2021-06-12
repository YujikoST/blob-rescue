using System;
using System.Collections;
using System.Collections.Generic;
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

    static float GetArea(GameObject blob)
    {
        var diameter = blob.transform.localScale.x;
        return Mathf.PI * Mathf.Pow(diameter / 2, 2);
    }

    static float GetDiameter(float area)
    {
        return Mathf.Sqrt(area / Mathf.PI) * 2;
    }

    static float GrowBlob(GameObject blob, float eatedArea)
    {
        var blobArea = GetArea(blob);
        var newArea = blobArea + eatedArea;

        var diameter = GetDiameter(newArea);
        var difference = Mathf.Abs(diameter - blob.transform.localScale.x);

        blob.transform.localScale = new Vector3(diameter, diameter, DEFAULT_Z);

        return difference;
    }

    static float ShrinkBlob(GameObject blob, float shrinkedArea)
    {
        var blobArea = GetArea(blob);
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
}
