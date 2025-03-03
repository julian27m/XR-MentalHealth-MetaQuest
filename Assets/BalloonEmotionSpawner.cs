using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple spawner for emotional balloons. Designed to reliably spawn the requested number of balloons.
/// </summary>
public class SimpleBalloonSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EmotionBalloonData
    {
        public string emotionName;
        public Color balloonColor;
        public Material defaultMaterial;
        public Material activatedMaterial;
        public AudioClip emotionNameClip;
        public AudioClip emotionMessageClip;
    }

    [Header("Balloon Setup")]
    [SerializeField] private GameObject balloonPrefab;
    [SerializeField] private Transform roomMeshContainer;
    [SerializeField] private List<EmotionBalloonData> emotionBalloons = new List<EmotionBalloonData>();

    [Header("Spawn Settings")]
    [SerializeField] private int _numberOfBalloonsToSpawn = 2; // HARDCODED TO 2 for now to override any other logic
    [SerializeField] private float initialSpawnDelay = 5.0f;
    [SerializeField] private float timeBetweenSpawns = 2.0f;

    // Ensure this value is always used correctly
    private int numberOfBalloonsToSpawn
    {
        get { return _numberOfBalloonsToSpawn; }
    }

    private void Start()
    {
        Debug.Log($"[SimpleBalloonSpawner] Starting up - configured to spawn {numberOfBalloonsToSpawn} balloons.");

        // Basic validation
        if (balloonPrefab == null)
        {
            Debug.LogError("[SimpleBalloonSpawner] Balloon prefab not assigned!");
            return;
        }

        if (emotionBalloons.Count == 0)
        {
            Debug.LogError("[SimpleBalloonSpawner] No emotion data provided!");
            return;
        }

        if (emotionBalloons.Count < numberOfBalloonsToSpawn)
        {
            Debug.LogWarning($"[SimpleBalloonSpawner] Not enough emotion data ({emotionBalloons.Count}) for requested balloons ({numberOfBalloonsToSpawn}).");
            return;
        }

        // Start spawning after delay
        Debug.Log($"[SimpleBalloonSpawner] Will spawn {numberOfBalloonsToSpawn} balloons after {initialSpawnDelay} seconds.");
        StartCoroutine(SpawnBalloons());
    }

    private IEnumerator SpawnBalloons()
    {
        // Wait for initial delay
        yield return new WaitForSeconds(initialSpawnDelay);

        Debug.Log($"[SimpleBalloonSpawner] Beginning to spawn {numberOfBalloonsToSpawn} balloons.");

        // Spawn each balloon
        for (int i = 0; i < numberOfBalloonsToSpawn && i < emotionBalloons.Count; i++)
        {
            SpawnBalloon(i);

            // Wait between spawns
            if (i < numberOfBalloonsToSpawn - 1) // Don't wait after the last one
            {
                Debug.Log($"[SimpleBalloonSpawner] Waiting {timeBetweenSpawns} seconds before spawning next balloon.");
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
        }

        Debug.Log("[SimpleBalloonSpawner] Finished spawning all balloons.");
    }

    private void SpawnBalloon(int emotionIndex)
    {
        Debug.Log($"[SimpleBalloonSpawner] Spawning balloon {emotionIndex + 1}/{numberOfBalloonsToSpawn} - {emotionBalloons[emotionIndex].emotionName}");

        // Create balloon with a clear visual offset to ensure they're not overlapping
        Vector3 spawnPos = transform.position + new Vector3(emotionIndex * 0.3f, 0, 0);
        GameObject balloon = Instantiate(balloonPrefab, spawnPos, Quaternion.identity);
        balloon.name = $"Balloon_{emotionBalloons[emotionIndex].emotionName}_{emotionIndex}";

        // Get/configure components
        BalloonEmotionController controller = balloon.GetComponent<BalloonEmotionController>();
        Renderer balloonRenderer = null;

        // Find renderer (try first child first, then any child)
        if (balloon.transform.childCount > 0)
        {
            balloonRenderer = balloon.transform.GetChild(0).GetComponent<Renderer>();
            if (balloonRenderer == null)
            {
                balloonRenderer = balloon.GetComponentInChildren<Renderer>();
            }
        }

        // Setup audio source
        AudioSource audioSource = balloon.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = balloon.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
        }

        // Configure balloon
        if (controller != null)
        {
            // Set the materials
            if (balloonRenderer != null)
            {
                balloonRenderer.material = emotionBalloons[emotionIndex].defaultMaterial;
                Debug.Log($"[SimpleBalloonSpawner] Set balloon material to {emotionBalloons[emotionIndex].defaultMaterial.name}");
            }

            // Configure controller via reflection
            SetField(controller, "defaultMaterial", emotionBalloons[emotionIndex].defaultMaterial);
            SetField(controller, "activatedMaterial", emotionBalloons[emotionIndex].activatedMaterial);
            SetField(controller, "balloonRenderer", balloonRenderer);
            SetField(controller, "audioSource", audioSource);
            SetField(controller, "emotionNameClip", emotionBalloons[emotionIndex].emotionNameClip);
            SetField(controller, "emotionMessageClip", emotionBalloons[emotionIndex].emotionMessageClip);

            if (roomMeshContainer != null)
            {
                SetField(controller, "roomMeshContainer", roomMeshContainer);
            }

            Debug.Log($"[SimpleBalloonSpawner] Successfully configured balloon {emotionIndex + 1}");
        }
        else
        {
            Debug.LogError("[SimpleBalloonSpawner] No BalloonEmotionController component found on prefab!");
        }
    }

    private void SetField(object target, string fieldName, object value)
    {
        System.Reflection.FieldInfo field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (field != null)
        {
            field.SetValue(target, value);
        }
        else
        {
            Debug.LogWarning($"[SimpleBalloonSpawner] Field not found: {fieldName}");
        }
    }
}