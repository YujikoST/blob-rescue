using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Helpers
{
    // -- The juicy stuff --

    public static Func<float, Func<GameObject, float>>
        GrowBlob = eatedArea => blob =>
        {
            var blobArea = GetBlobArea(blob);
            var newArea = blobArea + eatedArea;

            return ReplaceBlobArea(newArea)(blob);
        };

    public static Func<float, Func<GameObject, float>>
        ShrinkBlob = shrinkedArea => blob =>
        {
            var blobArea = GetBlobArea(blob);
            var newArea = blobArea - shrinkedArea;

            if (newArea < MIN_BLOB_AREA)
            {
                return 0;
            }

            return ReplaceBlobArea(newArea)(blob);
        };

    public static readonly Func<GameObject, Func<int, List<GameObject>>>
        CreatePool = gameObject => amount =>
            Enumerable
                .Repeat(0, amount)
                .Select(InstantiateUnactive(gameObject))
                .ToList();

    public static readonly Func<float, float, float, float, Color>
        FromRGBA = (r, g, b, a) => new Color(r / 255, g / 255, b / 255, a / 100);
    
    public static void HandleJump(PhysicsObject blob, bool canJump, float jumpSpeed, float scaleToStopJump)
    {
        if (WantsToJump() && canJump)
        {
            Jump(blob, jumpSpeed);
        }
        else if (WantsToStopJump() && !IsJumping(blob))
        {
            StopJump(blob, scaleToStopJump);
        }
    }

    public static void HandleHorizontalMovement(PhysicsObject blob,SpriteRenderer spriteRenderer, float maxSpeed)
    {
        var horizontalDirection = Input.GetAxisRaw("Horizontal");
        blob.targetVelocity = new Vector2(horizontalDirection * maxSpeed, 0);

        if (ShouldFlipSprite(horizontalDirection)(spriteRenderer))
        {
            FlipSprite(spriteRenderer);
        }
    }

    public static void EatBlobs(GameObject selectedBlob, List<GameObject> edibleBlobs)
    {
        var gainedArea = edibleBlobs
            .Select(GetBlobArea)
            .Sum();
        
        GrowBlob(gainedArea)(selectedBlob);
    }
    
    public static readonly Func<GameObject, Func<GameObject, float>>
        GetDistance = blob1 => blob2 =>
            Vector3.Distance(blob1.transform.position, blob2.transform.position);

    public static readonly Func<bool>
        WantsToEat = () =>
            Input.GetKey("e");
    
    // -- helper functions and data --

    private static Predicate<T> Not<T>(Predicate<T> predicate)
    {
        return v => !predicate(v);
    }

    private static readonly float DEFAULT_Z = 0;
    private static readonly float MIN_BLOB_AREA = 0.5f;

    private static readonly Func<float, float>
        GetArea = diameter =>
            Mathf.PI * Mathf.Pow(diameter / 2, 2);

    public static readonly Func<GameObject, float>
        GetBlobArea = blob =>
            GetArea(blob.transform.localScale.x);

    private static readonly Func<float, float>
        GetDiameter = area =>
            Mathf.Sqrt(area / Mathf.PI) * 2;


    private static readonly Func<GameObject, Func<int, GameObject>>
        InstantiateUnactive = gameObject => _ =>
        {
            var instance = UnityEngine.Object.Instantiate(gameObject);
            instance.SetActive(false);
            return instance;
        };

    public static readonly Func<float, Func<GameObject, float>>
        ReplaceBlobArea = newArea => blob =>
        {
            var newDiameter = GetDiameter(newArea);
            var transform = blob.transform;
            var currentDiameter = transform.localScale.x;
            transform.localScale = new Vector3(newDiameter, newDiameter, DEFAULT_Z);
            
            var diameterDifference = currentDiameter - newDiameter;
            return diameterDifference;
        };
    
    
    private static readonly Func<bool>
        WantsToJump = () =>
            Input.GetButtonDown("Jump");

    private static readonly Func<bool>
        WantsToStopJump = () =>
            Input.GetButtonUp("Jump");

    private static readonly Func<PhysicsObject, bool>
        IsJumping = blob =>
            blob.velocity.y <= 0;

    public static void Jump(PhysicsObject blob, float speed)
    {
        blob.velocity.y = speed;
    }

    private static void StopJump(PhysicsObject blob, float scale)
    {
        blob.velocity.y *= scale;
    }

    private static readonly Func<float, Func<SpriteRenderer, bool>>
        ShouldFlipSprite = horizontalDirection => spriteRenderer =>
            spriteRenderer.flipX
                ? horizontalDirection < 0f
                : horizontalDirection > 0f;

    private static void FlipSprite(SpriteRenderer sprite)
    {
        sprite.flipX = !sprite.flipX;
    }
    
}