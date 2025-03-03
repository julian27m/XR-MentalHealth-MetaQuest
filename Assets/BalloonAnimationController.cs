using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls a complex balloon animation sequence where balloons spawn in front of the user
/// and animate in a specific pattern.
/// </summary>
public class BalloonAnimationController : MonoBehaviour
{
    [Header("Balloon Setup")]
    [SerializeField] private GameObject balloonPrefab;
    [SerializeField] private List<Material> balloonMaterials = new List<Material>();

    [Header("Positioning")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float distanceFromCamera = 1.5f;
    [SerializeField] private float heightOffset = 0.1f;
    [SerializeField] private float spacingFactor = 0.125f;

    [Header("Timing")]
    [SerializeField] private float initialDelay = 1.0f;
    [SerializeField] private float centerToOthersDelay = 3.0f;
    [SerializeField] private float movementDuration = 1.0f;
    [SerializeField] private float holdDuration = 15.0f;
    [SerializeField] private float returnMovementDuration = 1.0f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private List<GameObject> balloons = new List<GameObject>();
    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Vector3> targetPositions = new List<Vector3>();

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
            Debug.Log("Starting balloon animation sequence");

        // Initial delay
        yield return new WaitForSeconds(initialDelay);

        // Spawn the center balloon first
        SpawnCenterBalloon();

        if (debugMode)
            Debug.Log("Center balloon spawned, waiting before spawning others");

        // Wait before spawning others
        yield return new WaitForSeconds(centerToOthersDelay);

        // Spawn all other balloons (also starts the emergence animation)
        SpawnOtherBalloons();

        if (debugMode)
            Debug.Log("All balloons spawned, emerging from center");

        // Wait for the emergence animation to complete
        yield return new WaitForSeconds(movementDuration);

        if (debugMode)
            Debug.Log($"Balloons in position, holding for {holdDuration} seconds");

        // Hold in position
        yield return new WaitForSeconds(holdDuration);

        if (debugMode)
            Debug.Log("Hold complete, returning balloons to center");

        // Animate balloons returning
        yield return StartCoroutine(AnimateBalloonsToOriginal(returnMovementDuration));

        if (debugMode)
            Debug.Log("All balloons returned, now removing them");

        // Remove all balloons
        RemoveAllBalloons();

        if (debugMode)
            Debug.Log("Animation sequence complete");
    }

    private void SpawnCenterBalloon()
    {
        // Calculate position in front of the camera
        Vector3 spawnPosition = CalculatePositionInFrontOfCamera(0);

        // Spawn the balloon
        GameObject balloon = Instantiate(balloonPrefab, spawnPosition, Quaternion.identity);
        balloon.name = "CenterBalloon";

        // Apply first material if available
        if (balloonMaterials.Count > 0)
        {
            Renderer balloonRenderer = GetBalloonRenderer(balloon);
            if (balloonRenderer != null)
            {
                balloonRenderer.material = balloonMaterials[0];
            }
        }

        // Add to our lists
        balloons.Add(balloon);
        originalPositions.Add(spawnPosition);
        targetPositions.Add(spawnPosition); // Center balloon doesn't move
    }

    private void SpawnOtherBalloons()
    {
        // Get the position of the center balloon
        Vector3 centerPosition = balloons[0].transform.position;

        // Spawn 4 balloons for the left side (indices 1-4)
        for (int i = 1; i <= 4; i++)
        {
            SpawnBalloon(i, -i, centerPosition);
        }

        // Spawn 4 balloons for the right side (indices 5-8)
        for (int i = 5; i <= 8; i++)
        {
            SpawnBalloon(i, i - 4, centerPosition);
        }

        // Start the animation for all balloons at once
        StartCoroutine(AnimateBalloonsEmergingFromCenter(movementDuration));
    }

    private void SpawnBalloon(int index, int offsetMultiplier, Vector3 centerPosition)
    {
        // All balloons start at exact same position as center balloon
        Vector3 spawnPosition = centerPosition;

        // Calculate the target position with the offset
        Vector3 targetPosition = CalculatePositionInFrontOfCamera(offsetMultiplier * spacingFactor);

        // Spawn the balloon exactly at the center balloon position
        GameObject balloon = Instantiate(balloonPrefab, spawnPosition, Quaternion.identity);
        balloon.name = $"Balloon_{index}";

        // Initially make the balloon small (to give emergence effect)
        //balloon.transform.localScale = Vector3.zero;

        // Apply material if available
        if (index < balloonMaterials.Count)
        {
            Renderer balloonRenderer = GetBalloonRenderer(balloon);
            if (balloonRenderer != null)
            {
                balloonRenderer.material = balloonMaterials[index];
            }
        }

        // Add to our lists
        balloons.Add(balloon);
        originalPositions.Add(spawnPosition);
        targetPositions.Add(targetPosition);
    }

    private Vector3 CalculatePositionInFrontOfCamera(float xOffset)
    {
        // Get the position in front of the camera
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0; // Make it horizontal
        cameraForward.Normalize();

        Vector3 position = cameraTransform.position + cameraForward * distanceFromCamera;

        // Apply height offset and x offset
        position.y += heightOffset;

        // Apply x offset perpendicular to camera forward
        Vector3 right = Vector3.Cross(Vector3.up, cameraForward).normalized;
        position += right * xOffset;

        return position;
    }

    private IEnumerator AnimateBalloonsEmergingFromCenter(float duration)
    {
        float elapsedTime = 0;
        Vector3 centerPosition = balloons[0].transform.position;

        // Animate all non-center balloons simultaneously
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

            // Use smooth step for more natural movement
            float t = Mathf.SmoothStep(0, 1, normalizedTime);

            // Update position and scale of all balloons except the center one
            for (int i = 1; i < balloons.Count; i++)
            {
                // Interpolate position from center to target
                balloons[i].transform.position = Vector3.Lerp(centerPosition, targetPositions[i], t);

                // Interpolate scale from 0 to normal size
                //balloons[i].transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            }

            yield return null;
        }

        // Ensure final positions are exactly as specified
        for (int i = 1; i < balloons.Count; i++)
        {
            balloons[i].transform.position = targetPositions[i];
            //balloons[i].transform.localScale = Vector3.one;
        }

        if (debugMode)
            Debug.Log("All balloons have emerged and are in final positions");
    }

    private IEnumerator AnimateBalloonsToOriginal(float duration)
    {
        float elapsedTime = 0;
        Vector3 centerPosition = balloons[0].transform.position;

        // Animate all non-center balloons simultaneously
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

            // Use smooth step for more natural movement
            float t = Mathf.SmoothStep(0, 1, normalizedTime);

            // Update position and scale of all balloons except the center one
            for (int i = 1; i < balloons.Count; i++)
            {
                // Interpolate position from current to center
                balloons[i].transform.position = Vector3.Lerp(targetPositions[i], centerPosition, t);

                // Interpolate scale from normal size to 0
                //balloons[i].transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            }

            yield return null;
        }

        // Ensure final positions and scales
        for (int i = 1; i < balloons.Count; i++)
        {
            balloons[i].transform.position = centerPosition;
            //balloons[i].transform.localScale = Vector3.zero;
        }

        if (debugMode)
            Debug.Log("All balloons have returned to the center");
    }

    private void RemoveAllBalloons()
    {
        // First fade out the center balloon
        if (balloons.Count > 0)
        {
            StartCoroutine(FadeOutBalloon(balloons[0], 0.5f));

            // Wait a moment before removing the other balloons
            for (int i = 1; i < balloons.Count; i++)
            {
                // Destroy with a small delay to avoid destroying all at once
                Destroy(balloons[i], 0.6f);
            }
        }

        // Clear our tracking lists after a slight delay
        StartCoroutine(ClearListsAfterDelay(0.7f));
    }

    private IEnumerator FadeOutBalloon(GameObject balloon, float duration)
    {
        // Get the renderer
        Renderer renderer = GetBalloonRenderer(balloon);
        if (renderer != null)
        {
            // Get the original material and make a copy of it
            Material material = renderer.material;
            Color originalColor = material.color;

            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = Mathf.Clamp01(elapsedTime / duration);

                // Fade out the alpha
                Color newColor = originalColor;
                newColor.a = 1 - normalizedTime;
                material.color = newColor;

                yield return null;
            }
        }

        // Destroy the balloon after fading
        Destroy(balloon);
    }

    private IEnumerator ClearListsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        balloons.Clear();
        originalPositions.Clear();
        targetPositions.Clear();

        if (debugMode)
            Debug.Log("All balloons removed and lists cleared");
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
        RemoveAllBalloons();
        StartCoroutine(AnimationSequence());
    }
}