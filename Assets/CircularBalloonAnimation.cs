using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a circular balloon animation where balloons spawn in sequence,
/// move in circular arcs, and eventually return to the starting position.
/// </summary>
public class CircularBalloonAnimation : MonoBehaviour
{
    [Header("Balloon Setup")]
    [SerializeField] private GameObject balloonPrefab;
    [SerializeField] private List<Material> balloonMaterials = new List<Material>();

    [Header("Positioning")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float distanceFromCamera = 1.5f;
    [SerializeField] private float heightOffset = 0.1f;
    [SerializeField] private float angleStep = 40f; // Degrees each balloon moves

    [Header("Timing")]
    [SerializeField] private float initialDelay = 1.0f;
    [SerializeField] private float timeBetweenSpawns = 1.0f;
    [SerializeField] private float movementDuration = 1.0f;
    [SerializeField] private float finalDelay = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private List<GameObject> balloons = new List<GameObject>();
    private int balloonCount = 9; // Number of balloons to spawn

    private void Start()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Ensure we have a balloon prefab
        if (balloonPrefab == null)
        {
            Debug.LogError("Balloon prefab not assigned!");
            return;
        }

        // Start the animation sequence
        StartCoroutine(AnimationSequence());
    }

    private IEnumerator AnimationSequence()
    {
        if (debugMode)
            Debug.Log("Starting circular balloon animation sequence");

        // Initial delay
        yield return new WaitForSeconds(initialDelay);

        // Spawn and animate balloons one by one
        for (int i = 0; i < balloonCount; i++)
        {
            // Spawn balloon at center position (0 degrees)
            GameObject balloon = SpawnBalloon(i);

            // Start its movement coroutine (except for the last balloon)
            if (i < balloonCount - 1) // The 9th balloon doesn't move initially
            {
                float targetAngle = (i + 1) * angleStep;
                StartCoroutine(MoveBalloonToAngle(balloon, 0f, targetAngle, movementDuration));
            }

            // Wait before spawning the next balloon (if not the last one)
            if (i < balloonCount - 1)
            {
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }

        // Wait before starting the return sequence
        yield return new WaitForSeconds(timeBetweenSpawns);

        // Return balloons to starting position in reverse order
        for (int i = balloonCount - 2; i >= 0; i--)
        {
            float currentAngle = (i + 1) * angleStep;
            StartCoroutine(MoveBalloonToAngle(balloons[i], currentAngle, 0f, movementDuration));

            // Wait between movements
            yield return new WaitForSeconds(timeBetweenSpawns);
        }

        // Final delay before removing all balloons
        yield return new WaitForSeconds(finalDelay);

        // Remove all balloons
        RemoveAllBalloons();

        if (debugMode)
            Debug.Log("Animation sequence complete");
    }

    private GameObject SpawnBalloon(int index)
    {
        // Calculate spawn position in front of the camera (0 degrees)
        Vector3 spawnPosition = CalculatePositionAtAngle(0f);

        // Spawn the balloon
        GameObject balloon = Instantiate(balloonPrefab, spawnPosition, Quaternion.identity);
        balloon.name = $"Balloon_{index}";

        // Apply material if available
        if (index < balloonMaterials.Count)
        {
            Renderer balloonRenderer = GetBalloonRenderer(balloon);
            if (balloonRenderer != null)
            {
                balloonRenderer.material = balloonMaterials[index];
            }
        }

        // Add to our list
        balloons.Add(balloon);

        if (debugMode)
            Debug.Log($"Spawned balloon {index} at 0 degrees");

        return balloon;
    }

    private Vector3 CalculatePositionAtAngle(float angle)
    {
        // Convert angle to radians
        float angleRad = angle * Mathf.Deg2Rad;

        // Get the camera's forward and right vectors (on the horizontal plane)
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0;
        cameraForward.Normalize();

        Vector3 cameraRight = Vector3.Cross(Vector3.up, cameraForward).normalized;

        // Calculate position using angle
        Vector3 position = cameraTransform.position +
                          cameraForward * Mathf.Cos(angleRad) * distanceFromCamera +
                          cameraRight * Mathf.Sin(angleRad) * distanceFromCamera;

        // Apply height offset
        position.y = cameraTransform.position.y + heightOffset;

        return position;
    }

    private IEnumerator MoveBalloonToAngle(GameObject balloon, float startAngle, float endAngle, float duration)
    {
        float elapsedTime = 0;

        // Pre-calculate start and end positions
        Vector3 startPosition = CalculatePositionAtAngle(startAngle);
        Vector3 endPosition = CalculatePositionAtAngle(endAngle);

        // Update balloon position to match start angle (in case it's not already there)
        balloon.transform.position = startPosition;

        if (debugMode)
            Debug.Log($"Moving {balloon.name} from {startAngle} to {endAngle} degrees");

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

            // Use smooth step for more natural movement
            float t = Mathf.SmoothStep(0, 1, normalizedTime);

            // Interpolate between angles to create arc movement
            float currentAngle = Mathf.Lerp(startAngle, endAngle, t);
            Vector3 currentPosition = CalculatePositionAtAngle(currentAngle);

            // Update balloon position
            balloon.transform.position = currentPosition;

            yield return null;
        }

        // Ensure final position is exactly at the target angle
        balloon.transform.position = endPosition;

        if (debugMode)
            Debug.Log($"Balloon {balloon.name} reached {endAngle} degrees");
    }

    private void RemoveAllBalloons()
    {
        StartCoroutine(FadeOutBalloons(0.5f));
    }

    private IEnumerator FadeOutBalloons(float duration)
    {
        // Get renderers for all balloons
        List<Renderer> renderers = new List<Renderer>();
        List<Color> originalColors = new List<Color>();

        foreach (GameObject balloon in balloons)
        {
            Renderer renderer = GetBalloonRenderer(balloon);
            if (renderer != null)
            {
                renderers.Add(renderer);
                originalColors.Add(renderer.material.color);
            }
        }

        // Fade out all balloons simultaneously
        float elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

            // Update all materials
            for (int i = 0; i < renderers.Count; i++)
            {
                Color newColor = originalColors[i];
                newColor.a = 1 - normalizedTime;
                renderers[i].material.color = newColor;
            }

            yield return null;
        }

        // Destroy all balloons
        foreach (GameObject balloon in balloons)
        {
            Destroy(balloon);
        }

        // Clear our list
        balloons.Clear();

        if (debugMode)
            Debug.Log("All balloons removed");
    }

    private Renderer GetBalloonRenderer(GameObject balloon)
    {
        // Try to get renderer from first child (common balloon setup)
        if (balloon.transform.childCount > 0)
        {
            Renderer renderer = balloon.transform.GetChild(0).GetComponent<Renderer>();
            if (renderer != null)
            {
                return renderer;
            }
        }

        // Try to get renderer from anywhere in children
        return balloon.GetComponentInChildren<Renderer>();
    }

    // Optional: Public method to trigger the animation sequence manually
    public void StartAnimationSequence()
    {
        StopAllCoroutines();

        // Remove any existing balloons
        foreach (GameObject balloon in balloons)
        {
            if (balloon != null)
                Destroy(balloon);
        }
        balloons.Clear();

        // Start fresh animation
        StartCoroutine(AnimationSequence());
    }
}