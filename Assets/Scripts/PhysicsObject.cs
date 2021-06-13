using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    public float minGroundNormalY = .65f;
    public float gravityModifier = .89f;

    public Vector2 targetVelocity;
    public Vector2 velocity;
    
    protected Vector2 groundNormal = Vector2.up;
    public bool grounded;
    protected Rigidbody2D rb2d;
    protected const float minMoveDistance = 0.001f;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected const float shellRadius = 0.01f;
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);

    private void OnEnable()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    // Start is called before the first frame update
    void Start()
    {
        contactFilter.useTriggers = false;
        contactFilter.SetLayerMask(Physics2D.GetLayerCollisionMask(gameObject.layer));
        contactFilter.useLayerMask = true;
    }

    // Update is called once per frame
    void Update()
    {
        // targetVelocity = Vector2.zero;
        ComputeVelocity();
    }

    protected virtual void ComputeVelocity()
    {
    }

    void FixedUpdate()
    {
        velocity += Physics2D.gravity * (gravityModifier * Time.deltaTime);
        velocity.x = targetVelocity.x;

        grounded = false;
        
        Vector2 deltaPosition = velocity * Time.deltaTime;

        Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

        Vector2 move = moveAlongGround * deltaPosition.x;

        Movement(move, false);

        move = Vector2.up * deltaPosition.y;

        Movement(move, true);
    }

    void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude;

        if (distance > minMoveDistance)
        {
            int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
            hitBufferList = hitBuffer.Take(count).ToList();

            velocity = hitBufferList
                .Select(hit => hit.normal) // to normal
                .Select(normal => // set x = 0 in grounded normals
                    IsGrounded(normal) && yMovement
                        ? new Vector2(0, normal.y)
                        : normal)
                .Aggregate(velocity, SlowAccordingToNormal); // Slow down velocity

            try
            {
                groundNormal = hitBufferList
                    .Select(hit => hit.normal)
                    .Last(normal => IsGrounded(normal) && yMovement);
            }
            catch (InvalidOperationException e)
            {
                // There's no grounded elements
            }


            grounded = hitBufferList
                .Select(hit => hit.normal)
                .Any(IsGrounded);

            distance = hitBufferList
                .Select(hit => hit.distance) // to distance
                .Select(d => d - shellRadius) // to modifiedDistance
                .Aggregate(distance, Math.Min); // get the minimum distance
        }

        rb2d.position = rb2d.position + move.normalized * distance;
    }

    bool IsGrounded(Vector2 vector)
    {
        return vector.y > minGroundNormalY;
    }

    static Vector2 SlowAccordingToNormal(Vector2 velocity, Vector2 normal)
    {
        float projection = Vector2.Dot(velocity, normal);
        return projection < 0
            ? velocity - projection * normal
            : velocity;
    }
}