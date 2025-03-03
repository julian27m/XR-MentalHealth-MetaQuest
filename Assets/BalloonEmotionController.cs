using System.Collections;
using UnityEngine;
using Oculus.Interaction;
using System;

/// <summary>
/// Controls the emotional balloon interaction in a meditation experience.
/// Handles spawn positioning, touch interactions, material changes, and audio playback.
/// </summary>
public class BalloonEmotionController : MonoBehaviour
{
    [Header("Balloon Settings")]
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Material activatedMaterial;
    [SerializeField] private Renderer balloonRenderer;
    [SerializeField] private Rigidbody balloonRigidbody;
    [SerializeField] private MonoBehaviour interactableComponent; // This will be cast to the appropriate type

    [Header("Audio Clips")]
    [SerializeField] private AudioClip emotionNameClip;
    [SerializeField] private AudioClip emotionMessageClip;
    [SerializeField] private AudioSource audioSource;

    [Header("Room Mesh Reference")]
    [SerializeField] private Transform roomMeshContainer; // Optional - not used in simplified positioning

    private bool hasBeenInteractedWith = false;
    private Vector3 frozenPosition;
    private Quaternion frozenRotation;

    [SerializeField] private bool debugMode = true; // Enable for debug logs

    // Store the original physics properties to restore them later
    private bool originalIsKinematic;
    private bool originalUseGravity;
    private CollisionDetectionMode originalCollisionDetection;
    private RigidbodyInterpolation originalInterpolation;
    private float originalDrag;
    private float originalAngularDrag;

    private void Start()
    {
        // Ensure we have the required components
        if (balloonRenderer == null)
            balloonRenderer = transform.GetChild(0).GetComponent<Renderer>();

        if (balloonRigidbody == null)
            balloonRigidbody = GetComponent<Rigidbody>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (interactableComponent == null)
        {
            // Try to find any interactable component in children
            interactableComponent = GetComponentInChildren<MonoBehaviour>();
        }

        // Store original physics properties
        if (balloonRigidbody != null)
        {
            originalIsKinematic = balloonRigidbody.isKinematic;
            originalUseGravity = balloonRigidbody.useGravity;
            originalCollisionDetection = balloonRigidbody.collisionDetectionMode;
            originalInterpolation = balloonRigidbody.interpolation;
            originalDrag = balloonRigidbody.drag;
            originalAngularDrag = balloonRigidbody.angularDrag;
        }

        // Set up interaction differently based on the type of interactable we have
        SetupInteractionEvents();

        // Position the balloon in a random position within the room boundaries
        PositionBalloonInRoom();
    }

    private void SetupInteractionEvents()
    {
        if (debugMode)
        {
            Debug.Log("Setting up interaction events");
        }

        // Check for Meta Quest hand physics hands
        // Make sure the balloon collider is set to trigger
        Collider balloonCollider = GetComponent<Collider>();
        if (balloonCollider != null)
        {
            balloonCollider.isTrigger = true;
            if (debugMode) Debug.Log("Set balloon collider to trigger mode");
        }
        else
        {
            if (debugMode) Debug.LogWarning("No collider found on balloon root");
        }

        // Add tag to gameObject for easy identification in collision
        gameObject.tag = "Balloon";

        if (debugMode)
        {
            Debug.Log("Interaction setup complete. Will use OnTriggerEnter for hand detection.");
        }
    }

    private void OnDestroy()
    {
        // We'll skip cleanup of the reflection-based events as it's more complex
        // This shouldn't cause issues as the object is being destroyed anyway
    }

    private void PositionBalloonInRoom()
    {
        // Get the camera (user's head) position
        Vector3 cameraPosition = Camera.main.transform.position;
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0; // Make it horizontal
        cameraForward.Normalize();

        // Define a suitable spawn area around the user
        float minDistance = 0.7f;
        float maxDistance = 1.5f;

        // Height constraints - keep balloons at comfortable reaching height
        float minHeightOffset = -0.3f;
        float maxHeightOffset = 0.5f;

        // Generate random position around the user
        // Get a random angle around the user (0-360 degrees)
        float randomAngle = UnityEngine.Random.Range(0f, 360f);
        Vector3 randomDirection = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;

        // Calculate distance
        float randomDistance = UnityEngine.Random.Range(minDistance, maxDistance);

        // Calculate the position
        Vector3 targetPosition = cameraPosition + randomDirection * randomDistance;

        // Adjust height - keep it at a reachable height
        float heightOffset = UnityEngine.Random.Range(minHeightOffset, maxHeightOffset);
        targetPosition.y = cameraPosition.y + heightOffset;

        // Set the balloon's position
        transform.position = targetPosition;

        Debug.Log($"Positioned balloon at {targetPosition} (offset from camera: {targetPosition - cameraPosition})");
    }

    private bool IsPositionWithinRoomBounds(Vector3 position)
    {
        // Implement a check against the room mesh boundaries
        // This is a simplified approach

        // Get all mesh colliders in the room container
        MeshCollider[] meshColliders = roomMeshContainer.GetComponentsInChildren<MeshCollider>();

        // Check if the position is inside at least one of these colliders
        foreach (MeshCollider collider in meshColliders)
        {
            // If the room mesh uses trigger colliders, you might need a different approach
            if (collider.bounds.Contains(position))
            {
                return true;
            }
        }

        // If we have no mesh colliders or couldn't determine, default to true
        if (meshColliders.Length == 0)
        {
            Debug.LogWarning("No mesh colliders found in room container, defaulting to accept position");
            return true;
        }

        return false;
    }

    // This is our general-purpose interaction handler - it accepts any type of object
    // so it's compatible with any interactor implementation
    private void OnTouchInteraction(object interactor)
    {
        if (debugMode)
        {
            Debug.Log("OnTouchInteraction called");
        }

        // Only trigger the full interaction sequence once
        if (!hasBeenInteractedWith)
        {
            hasBeenInteractedWith = true;
            StartCoroutine(HandleInteractionSequence());
        }
    }

    // Handle trigger collisions as our primary interaction method
    private void OnTriggerEnter(Collider other)
    {
        if (debugMode)
        {
            Debug.Log($"Trigger entered by: {other.gameObject.name} with tag: {other.gameObject.tag}");
        }

        // Check if this is a hand collider - look for common tags and names
        bool isHand = other.CompareTag("Hand") ||
                      other.CompareTag("Player") ||
                      other.name.ToLower().Contains("hand") ||
                      other.gameObject.name.ToLower().Contains("hand") ||
                      other.transform.root.name.ToLower().Contains("hand") ||
                      other.transform.root.name.Contains("Synthetic") ||
                      other.transform.root.name.Contains("Real Hands") ||
                      other.name.Contains("OculusHand");

        if (debugMode)
        {
            Debug.Log($"Is collision with a hand? {isHand} | Object: {other.gameObject.name} | Root: {other.transform.root.name}");
        }

        if (!hasBeenInteractedWith && isHand)
        {
            if (debugMode)
            {
                Debug.Log("Hand collision detected - starting interaction sequence");
            }

            hasBeenInteractedWith = true;
            StartCoroutine(HandleInteractionSequence());
        }
    }

    private IEnumerator HandleInteractionSequence()
    {
        if (debugMode)
        {
            Debug.Log("Starting interaction sequence");
        }

        // Store current position and rotation
        frozenPosition = transform.position;
        frozenRotation = transform.rotation;

        // Disable the touch hand grab component temporarily
        MonoBehaviour touchHandGrab = GetComponentInChildren<MonoBehaviour>();
        bool touchHandGrabWasEnabled = false;

        // Try to find the TouchHandGrab component by name
        foreach (MonoBehaviour component in GetComponentsInChildren<MonoBehaviour>())
        {
            if (component.GetType().Name.Contains("TouchHandGrab") ||
                component.GetType().Name.Contains("Grabbable"))
            {
                touchHandGrab = component;
                touchHandGrabWasEnabled = touchHandGrab.enabled;
                touchHandGrab.enabled = false;
                if (debugMode) Debug.Log($"Disabled {touchHandGrab.GetType().Name} during interaction sequence");
                break;
            }
        }

        // Change material to activated state
        if (activatedMaterial != null && balloonRenderer != null)
        {
            balloonRenderer.material = activatedMaterial;
            if (debugMode) Debug.Log("Changed to activated material");
        }
        else if (debugMode)
        {
            Debug.LogWarning($"Cannot change material: activatedMaterial={activatedMaterial}, balloonRenderer={balloonRenderer}");
        }

        // Freeze the balloon by disabling physics
        bool wasKinematic = balloonRigidbody.isKinematic;
        bool wasGravity = balloonRigidbody.useGravity;
        Vector3 savedVelocity = balloonRigidbody.velocity;
        Vector3 savedAngularVelocity = balloonRigidbody.angularVelocity;

        // Freeze the rigidbody completely
        balloonRigidbody.isKinematic = true;
        balloonRigidbody.useGravity = false;
        balloonRigidbody.velocity = Vector3.zero;
        balloonRigidbody.angularVelocity = Vector3.zero;
        transform.position = frozenPosition;
        transform.rotation = frozenRotation;

        if (debugMode) Debug.Log("Balloon frozen in place");

        // Check if we have audio clips and an audio source
        if (audioSource != null && emotionNameClip != null)
        {
            // Play the emotion name audio
            audioSource.clip = emotionNameClip;
            audioSource.Play();

            if (debugMode) Debug.Log($"Playing emotion name clip: {emotionNameClip.name}, length: {emotionNameClip.length}s");

            // Wait for the first audio clip to finish
            float waitTime = emotionNameClip.length;
            if (waitTime <= 0) waitTime = 1f; // Fallback if length is invalid

            yield return new WaitForSeconds(waitTime);

            if (debugMode) Debug.Log("First audio clip finished");

            // Play the emotion message audio if available
            if (emotionMessageClip != null)
            {
                audioSource.clip = emotionMessageClip;
                audioSource.Play();

                if (debugMode) Debug.Log($"Playing emotion message clip: {emotionMessageClip.name}, length: {emotionMessageClip.length}s");

                // Wait for the second audio clip to finish
                waitTime = emotionMessageClip.length;
                if (waitTime <= 0) waitTime = 3f; // Fallback if length is invalid

                yield return new WaitForSeconds(waitTime);

                if (debugMode) Debug.Log("Second audio clip finished");
            }
            else if (debugMode)
            {
                Debug.LogWarning("No emotion message clip assigned");
            }
        }
        else if (debugMode)
        {
            Debug.LogWarning($"Cannot play audio: audioSource={audioSource}, emotionNameClip={emotionNameClip}");
            // Add a delay anyway
            yield return new WaitForSeconds(2f);
        }

        // Change material back to default
        if (defaultMaterial != null && balloonRenderer != null)
        {
            balloonRenderer.material = defaultMaterial;
            if (debugMode) Debug.Log("Changed back to default material");
        }

        // Get any colliders and disable trigger mode
        Collider[] colliders = GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders)
        {
            collider.isTrigger = false;
            if (debugMode) Debug.Log($"Set collider {collider.name} trigger mode to false");
        }

        // Restore physics state and interactable component
        balloonRigidbody.isKinematic = originalIsKinematic;
        balloonRigidbody.useGravity = originalUseGravity;
        balloonRigidbody.collisionDetectionMode = originalCollisionDetection;
        balloonRigidbody.interpolation = originalInterpolation;
        balloonRigidbody.drag = originalDrag;
        balloonRigidbody.angularDrag = originalAngularDrag;

        // Add a small upward force to make the balloon float slightly when released
        balloonRigidbody.AddForce(Vector3.up * 0.1f, ForceMode.Impulse);

        // Re-enable the touch hand grab component
        if (touchHandGrab != null)
        {
            touchHandGrab.enabled = touchHandGrabWasEnabled;
            if (debugMode) Debug.Log($"Re-enabled {touchHandGrab.GetType().Name}");
        }

        if (debugMode) Debug.Log("Balloon physics and interaction restored");

        // Disable this script to allow normal balloon behavior
        if (debugMode) Debug.Log("Disabling BalloonEmotionController - balloon will now behave normally");
        this.enabled = false;
    }
}