using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPlatformerController : PhysicsObject
{
    public float maxSpeed = 5;
    public float jumpTakeOffSpeed = 5;

    private bool shouldMoveAsParable = false;
    private float currentVerticalVelocity;
    private float horizontalVelocity;

    public SpriteRenderer spriteRenderer;
    public Animator _animator;
    private bool _isRising;

    private bool _isFalling;

    private bool _isJumping;
    private bool _isLanding;

    private static readonly int IsRising = Animator.StringToHash("isRising");
    private static readonly int VelocityX = Animator.StringToHash("velocityX");

    private static readonly int Grounded = Animator.StringToHash("grounded");

    private static readonly int IsJumping = Animator.StringToHash("isJumping");
    private static readonly int IsFalling = Animator.StringToHash("isFalling");
    private static readonly int IsLanding = Animator.StringToHash("isLanding");
    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    public void MoveAsParable(Vector2 impulse)
    {
        shouldMoveAsParable = true;
        currentVerticalVelocity = impulse.y;
        horizontalVelocity = impulse.x;
    }

    public void CancelParableMovement()
    {
        shouldMoveAsParable = false;
    }

    protected override void ComputeVelocity()
    {
        // Calculate booleans
        _isRising = velocity.y > 0.1;
        _isFalling = velocity.y < -0.1;
        _isJumping = velocity.y > 0 && grounded;
        _isLanding = velocity.y < -0.1 && grounded;

        // Set values in animator
        _animator.SetBool(Grounded, grounded);
        _animator.SetFloat(VelocityX, Mathf.Abs(velocity.x) / maxSpeed);
        _animator.SetBool(IsRising, _isRising);
        _animator.SetBool(IsFalling, _isFalling);
        _animator.SetBool(IsJumping, _isJumping);
        _animator.SetBool(IsLanding, _isLanding);
        
        // Move as parable
        if (shouldMoveAsParable)
        {
            if (grounded)
            {
                shouldMoveAsParable = false;
                targetVelocity = Vector2.zero;
                return;
            }
            Helpers.Jump(this, currentVerticalVelocity);
            currentVerticalVelocity -= 0.05f;
            targetVelocity.x = horizontalVelocity;
        }
    }

    public void TriggerMergingAnimation(bool isMerging)
    {
        _animator.SetBool("isMerging", isMerging);
    }
}