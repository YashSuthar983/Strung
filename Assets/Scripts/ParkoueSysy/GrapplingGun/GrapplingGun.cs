using UnityEngine;

public class GrapplingGun : MonoBehaviour
{
    private LineRenderer lr;
    private Vector3 grapplePoint;
    public LayerMask whatIsGrappleable;
    public Transform gunTip, cam, player;

    private float maxDistance = 20f; // Maximum rope length
    private bool isGrappling = false;

    private Vector3 swingVelocity;
    private float swingSpeed = 10f;
    private float gravity = -20f;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartGrapple();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            StopGrapple();
        }

        if (isGrappling)
        {
            PerformSwingMovement();
        }
    }

    private void LateUpdate()
    {
        DrawRope();
    }

    void StartGrapple()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));

        if (Physics.Raycast(ray, out hit, maxDistance, whatIsGrappleable))
        {
            grapplePoint = hit.point;

            lr.positionCount = 2;

            isGrappling = true;
            swingVelocity = Vector3.zero; // Reset swing velocity
        }
    }

    void StopGrapple()
    {
        lr.positionCount = 0;
        isGrappling = false;
        swingVelocity = Vector3.zero; // Reset velocity
    }

    void PerformSwingMovement()
    {
        // Calculate the vector between the player and the grapple point
        Vector3 toGrapplePoint = grapplePoint - player.position;

        // Rope constraint: limit the player's distance to the grapple point
        float currentDistance = toGrapplePoint.magnitude;
        Vector3 ropeDirection = toGrapplePoint.normalized;

        if (currentDistance > maxDistance)
        {
            // Pull the player back to the max distance
            player.position = grapplePoint - ropeDirection * maxDistance;
        }

        // Apply swinging physics
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate swing force based on player input and gravity
        Vector3 inputForce = (cam.right * horizontalInput + cam.forward * verticalInput) * swingSpeed;
        Vector3 gravityForce = Vector3.down * Mathf.Abs(gravity);

        // Update swing velocity with input and gravity
        swingVelocity += (inputForce + gravityForce) * Time.deltaTime;

        // Project the velocity onto the plane perpendicular to the rope direction to ensure swinging
        swingVelocity -= Vector3.Dot(swingVelocity, ropeDirection) * ropeDirection;

        // Move the player based on the swing velocity
        player.position += swingVelocity * Time.deltaTime;
    }

    void DrawRope()
    {
        if (!isGrappling) return;

        lr.SetPosition(0, gunTip.position);
        lr.SetPosition(1, grapplePoint);
    }

    public bool IsGrappling()
    {
        return isGrappling;
    }

    public Vector3 GetGrapplePoint()
    {
        return grapplePoint;
    }
}
