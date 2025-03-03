using System.Collections;
using UnityEngine;

/// <summary>
/// A simplified test script to spawn multiple balloons.
/// </summary>
public class TestBalloonSpawner : MonoBehaviour
{
    [SerializeField] private GameObject balloonPrefab;
    [SerializeField] private int numberOfBalloonsToSpawn = 2;
    [SerializeField] private float spawnDelay = 2.0f;
    [SerializeField] private float spacingBetweenBalloons = 0.5f;

    private void Start()
    {
        if (balloonPrefab == null)
        {
            Debug.LogError("Balloon prefab not assigned to TestBalloonSpawner!");
            return;
        }

        // Start spawning after a short delay
        StartCoroutine(SpawnBalloons());
    }

    private IEnumerator SpawnBalloons()
    {
        Debug.Log($"Starting to spawn {numberOfBalloonsToSpawn} balloons");

        yield return new WaitForSeconds(spawnDelay);

        // Spawn balloons in a row for easy visibility
        for (int i = 0; i < numberOfBalloonsToSpawn; i++)
        {
            Vector3 spawnPosition = transform.position + Vector3.right * i * spacingBetweenBalloons;

            GameObject balloon = Instantiate(balloonPrefab, spawnPosition, Quaternion.identity);
            balloon.name = $"TestBalloon_{i + 1}";

            Debug.Log($"Spawned balloon {i + 1} at position {spawnPosition}");

            yield return new WaitForSeconds(0.5f); // Small delay between spawns
        }

        Debug.Log("Finished spawning all balloons");
    }
}