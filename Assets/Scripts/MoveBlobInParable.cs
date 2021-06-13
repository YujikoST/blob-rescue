using UnityEngine;

public class MoveBlobInParable : MonoBehaviour
{
    // Constants defined on start
    private PhysicsObject blob;
    private float horizontalVelocity;

    // Variables
    private float currentVerticalVelocity;

    // Set constants defined on start. ONLY CALL ONCE
    public void SetBlob(PhysicsObject blob)
    {
        this.blob = blob;
    }

    public void SetInitialDirection(Vector2 initialImpulse)
    {
        this.horizontalVelocity = initialImpulse.x;
        this.currentVerticalVelocity = initialImpulse.y;
    }

    public bool brake = false;

    // Move blob every frame if brake is not active
    void Update()
    {
        if (brake)
        {
            return;
        }

        Helpers.Jump(blob, currentVerticalVelocity);
        blob.targetVelocity = new Vector2(horizontalVelocity, 0);

        currentVerticalVelocity = Mathf.Max(0, currentVerticalVelocity - 0.2f);
    }
}