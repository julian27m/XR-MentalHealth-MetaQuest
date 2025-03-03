using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creates a circular arrangement of balloons around the user,
/// with a subtle floating animation before removing them.
/// </summary>
public class CircularBalloonArrangement : MonoBehaviour
{
    [Header("Balloon Setup")]
    [SerializeField] private GameObject balloonPrefab;
    [SerializeField] private List<Material> balloonMaterials = new List<Material>();
    [SerializeField] private int balloonCount = 9; // Number of balloons to spawn

    [Header("Positioning")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float distanceFromCamera = 1.5f;
    [SerializeField] private float heightOffset = 0.1f;

    [Header("Floating Animation")]
    [SerializeField] private float floatAmplitude = 0.1f;  // How much balloons move up and down
    [SerializeField] private float floatFrequency = 1.0f;  // Complete cycle duration in seconds

    [Header("Timing")]
    [SerializeField] private float initialDelay = 1.0f;
    [SerializeField] private float experienceDuration = 10.0f;
    [SerializeField] private float fadeOutDuration = 0.5f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private List<GameObject> balloons = new List<GameObject>();
    private List<Vector3> originalPositions = new List<Vector3>();

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

        // Start the balloon experience
        StartCoroutine(BalloonExperience());
    }

    private IEnumerator BalloonExperience()
    {
        if (debugMode)
            Debug.Log("Starting balloon experience");

        // Initial delay before starting
        yield return new WaitForSeconds(initialDelay);

        // Spawn all balloons at once in a circle
        SpawnBalloonsInCircle();

        // Start the floating animation for all balloons
        StartCoroutine(FloatBalloons());

        // Wait for the experience duration
        yield return new WaitForSeconds(experienceDuration);

        // Fade out and remove all balloons
        yield return StartCoroutine(FadeOutBalloons(fadeOutDuration));

        if (debugMode)
            Debug.Log("Balloon experience complete");
    }

    private void SpawnBalloonsInCircle()
    {
        if (debugMode)
            Debug.Log($"Spawning {balloonCount} balloons in a circle");

        // Calculate the angle step between balloons
        float angleStep = 360f / balloonCount;

        for (int i = 0; i < balloonCount; i++)
        {
            // Calculate the angle for this balloon
            float angle = i * angleStep;

            // Calculate position at this angle
            Vector3 position = CalculatePositionAtAngle(angle);

            // Spawn the balloon
            GameObject balloon = Instantiate(balloonPrefab, position, Quaternion.identity);
            balloon.name = $"Balloon_{i}";

            // Apply material if available
            if (i < balloonMaterials.Count)
            {
                Renderer balloonRenderer = GetBalloonRenderer(balloon);
                if (balloonRenderer != null)
                {
                    balloonRenderer.material = balloonMaterials[i];
                }
            }

            // Store original position for floating animation
            originalPositions.Add(position);

            // Add to our list
            balloons.Add(balloon);

            if (debugMode)
                Debug.Log($"Spawned balloon {i} at {angle} degrees");
        }
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

    private IEnumerator FloatBalloons()
    {
        float startTime = Time.time;

        // Continue floating until stopped externally
        while (balloons.Count > 0)
        {
            float elapsedTime = Time.time - startTime;

            // Update position of each balloon
            for (int i = 0; i < balloons.Count; i++)
            {
                if (balloons[i] != null)
                {
                    // Calculate vertical offset using a sine wave
                    // Using different phase for each balloon for variety
                    float phase = (360f / balloonCount) * i;
                    float verticalOffset = Mathf.Sin((elapsedTime * 2f * Mathf.PI / floatFrequency) + (phase * Mathf.Deg2Rad)) * floatAmplitude;

                    // Update balloon position
                    Vector3 newPosition = originalPositions[i];
                    newPosition.y += verticalOffset;
                    balloons[i].transform.position = newPosition;
                }
            }

            yield return null;
        }
    }

    private IEnumerator FadeOutBalloons(float duration)
    {
        if (debugMode)
            Debug.Log("Starting balloon fade out");

        // Get renderers for all balloons
        List<Renderer> renderers = new List<Renderer>();
        List<Color> originalColors = new List<Color>();

        foreach (GameObject balloon in balloons)
        {
            if (balloon != null)
            {
                Renderer renderer = GetBalloonRenderer(balloon);
                if (renderer != null)
                {
                    renderers.Add(renderer);
                    originalColors.Add(renderer.material.color);
                }
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
                if (renderers[i] != null)
                {
                    Color newColor = originalColors[i];
                    newColor.a = 1 - normalizedTime;
                    renderers[i].material.color = newColor;
                }
            }

            yield return null;
        }

        // Destroy all balloons
        foreach (GameObject balloon in balloons)
        {
            if (balloon != null)
                Destroy(balloon);
        }

        // Clear our lists
        balloons.Clear();
        originalPositions.Clear();

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

    // Optional: Public method to trigger the balloon experience manually
    public void StartBalloonExperience()
    {
        StopAllCoroutines();

        // Remove any existing balloons
        foreach (GameObject balloon in balloons)
        {
            if (balloon != null)
                Destroy(balloon);
        }
        balloons.Clear();
        originalPositions.Clear();

        // Start fresh experience
        StartCoroutine(BalloonExperience());
    }
}