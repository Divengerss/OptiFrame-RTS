using UnityEngine;

public class IsometricCameraController : MonoBehaviour
{
    // Movement settings
    public float panSpeed = 10f; // Speed of camera movement
    public float zoomSpeed = 2f; // Speed of zooming
    public Vector2 zoomRange = new Vector2(5f, 20f); // Min and max zoom distances

    // Pan boundaries
    public Vector2 panLimitX = new Vector2(-50f, 50f);
    public Vector2 panLimitZ = new Vector2(-50f, 50f);

    // Smooth movement settings
    public float smoothTime = 0.2f; // Time for movement to interpolate
    private Vector3 velocity = Vector3.zero;

    // Mouse drag settings
    public float dragDelay = 0.5f; // Time to hold the left mouse button before dragging
    private bool isDragging = false;
    private Vector3 lastMousePosition;

    // Default isometric angle
    private Vector3 isometricAngle = new Vector3(45f, 45f, 0f);

    private void Start()
    {
        // Lock the camera to isometric perspective and set orthographic view
        transform.rotation = Quaternion.Euler(isometricAngle);
        Camera.main.orthographic = true;
    }

    private void Update()
    {
        HandleKeyboardMovement();
        HandleMouseDrag();
        HandleZoom();
    }

    private void HandleKeyboardMovement()
    {
        Vector3 direction = Vector3.zero;

        // Get movement input relative to the camera's current view
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            direction += transform.forward; // Move forward relative to the view
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            direction -= transform.forward; // Move backward relative to the view
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            direction -= transform.right; // Move left relative to the view
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            direction += transform.right; // Move right relative to the view

        // Project movement direction to the XZ plane and normalize
        direction.y = 0;
        direction.Normalize();

        // Calculate target position
        Vector3 targetPosition = transform.position + direction * panSpeed * Time.deltaTime;

        // Clamp target position within boundaries
        targetPosition.x = Mathf.Clamp(targetPosition.x, panLimitX.x, panLimitX.y);
        targetPosition.z = Mathf.Clamp(targetPosition.z, panLimitZ.x, panLimitZ.y);

        // Smoothly move the camera to the target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }

    private void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Record the mouse position when the button is first pressed
            lastMousePosition = Input.mousePosition;
            isDragging = false; // Reset dragging state
            Invoke("StartDragging", dragDelay); // Start dragging after delay
        }

        if (Input.GetMouseButtonUp(0))
        {
            // Stop dragging when the mouse button is released
            isDragging = false;
            CancelInvoke("StartDragging");
        }

        if (isDragging)
        {
            // Calculate drag movement based on mouse delta
            Vector3 delta = Input.mousePosition - lastMousePosition;

            // Translate delta into movement relative to the camera's view
            Vector3 moveDirection = delta.x * transform.right + delta.y * transform.forward;
            moveDirection.y = 0; // Project movement direction to the XZ plane

            // Normalize and scale by pan speed
            moveDirection.Normalize();
            Vector3 move = moveDirection * panSpeed * Time.deltaTime;

            // Clamp the movement to the pan boundaries
            Vector3 targetPosition = transform.position + move;
            targetPosition.x = Mathf.Clamp(targetPosition.x, panLimitX.x, panLimitX.y);
            targetPosition.z = Mathf.Clamp(targetPosition.z, panLimitZ.x, panLimitZ.y);

            // Move the camera smoothly
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

            // Update the last mouse position
            lastMousePosition = Input.mousePosition;
        }
    }

    private void StartDragging()
    {
        isDragging = true;
    }

    private void HandleZoom()
    {
        // Adjust orthographic size based on scroll wheel input
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            Camera.main.orthographicSize -= scroll * zoomSpeed;
            Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, zoomRange.x, zoomRange.y);
        }
    }
}
