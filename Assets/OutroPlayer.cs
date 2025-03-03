using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays a series of outro audio clips in order with a specified delay between each clip,
/// but only after all balloons in the scene have been interacted with (in Awake state).
/// </summary>
public class OutroPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> outroClips = new List<AudioClip>();

    [Header("Timing Settings")]
    [SerializeField] private float delayBetweenClips = 3.0f;   // Delay between clips (in seconds)
    [SerializeField] private float checkInterval = 1.0f;       // How often to check if all balloons are awake

    [Header("Balloon Settings")]
    [SerializeField] private string balloonTag = "Balloon";    // Tag used to identify balloons

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = true;

    private int currentClipIndex = 0;
    private bool hasPlayedOutro = false;
    private Coroutine checkBalloonsCoroutine;

    private void Start()
    {
        // Validate audio source
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                if (debugMode)
                    Debug.Log("Added AudioSource component to OutroPlayer");
            }
        }

        // Configure audio source
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Start checking for all balloons to be in Awake state
        if (outroClips.Count > 0)
        {
            if (debugMode)
                Debug.Log($"Starting to check for all balloons to be in Awake state");
            checkBalloonsCoroutine = StartCoroutine(CheckAllBalloonsAwake());
        }
        else
        {
            Debug.LogWarning("No outro clips assigned to OutroPlayer.");
        }
    }

    private IEnumerator CheckAllBalloonsAwake()
    {
        while (!hasPlayedOutro)
        {
            if (AreAllBalloonsAwake())
            {
                if (debugMode)
                    Debug.Log("All balloons are now in Awake state. Starting outro sequence.");

                StartCoroutine(PlayOutroSequence());
                hasPlayedOutro = true;
                yield break;
            }

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private bool AreAllBalloonsAwake()
    {
        // Find all balloon objects in the scene
        GameObject[] balloons = GameObject.FindGameObjectsWithTag(balloonTag);

        if (balloons.Length == 0)
        {
            Debug.LogWarning("No balloons found with tag: " + balloonTag);
            return false;
        }

        if (debugMode)
            Debug.Log($"Found {balloons.Length} balloons to check");

        // Check each balloon's state
        foreach (GameObject balloon in balloons)
        {
            BalloonEmotionController controller = balloon.GetComponent<BalloonEmotionController>();

            if (controller == null)
            {
                if (debugMode)
                    Debug.LogWarning($"Balloon {balloon.name} doesn't have a BalloonEmotionController component");
                return false;
            }

            // Check if the balloon is not in Awake state
            if (controller.CurrentState != BalloonEmotionController.BalloonState.Awake)
            {
                if (debugMode)
                    Debug.Log($"Balloon {balloon.name} is in {controller.CurrentState} state, not Awake yet");
                return false;
            }
        }

        // All balloons are in Awake state
        if (debugMode)
            Debug.Log("All balloons are in Awake state!");
        return true;
    }

    private IEnumerator PlayOutroSequence()
    {
        currentClipIndex = 0;

        // Play each clip with a delay between them
        while (currentClipIndex < outroClips.Count)
        {
            PlayCurrentClip();

            // Wait for current clip to finish
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            // Wait for the delay between clips
            if (currentClipIndex < outroClips.Count - 1) // No need to wait after the last clip
            {
                if (debugMode)
                    Debug.Log($"Waiting {delayBetweenClips} seconds before playing next clip.");
                yield return new WaitForSeconds(delayBetweenClips);
            }

            // Move to next clip
            currentClipIndex++;
        }

        if (debugMode)
            Debug.Log("Outro sequence complete.");
    }

    private void PlayCurrentClip()
    {
        if (currentClipIndex < 0 || currentClipIndex >= outroClips.Count)
        {
            Debug.LogError($"Invalid clip index: {currentClipIndex}");
            return;
        }

        AudioClip clipToPlay = outroClips[currentClipIndex];
        if (clipToPlay == null)
        {
            Debug.LogError($"Clip at index {currentClipIndex} is null.");
            return;
        }

        if (debugMode)
            Debug.Log($"Playing clip {currentClipIndex + 1}/{outroClips.Count}: {clipToPlay.name}, Duration: {clipToPlay.length} seconds");

        audioSource.clip = clipToPlay;
        audioSource.Play();
    }

    // Optional: Public method to manually restart the sequence if needed
    public void RestartOutro()
    {
        StopAllCoroutines();
        audioSource.Stop();
        hasPlayedOutro = false;
        checkBalloonsCoroutine = StartCoroutine(CheckAllBalloonsAwake());

        if (debugMode)
            Debug.Log("Outro check restarted.");
    }

    // Optional: Public method to force play the outro immediately
    public void ForcePlayOutro()
    {
        if (!hasPlayedOutro)
        {
            StopAllCoroutines();
            audioSource.Stop();
            StartCoroutine(PlayOutroSequence());
            hasPlayedOutro = true;

            if (debugMode)
                Debug.Log("Forcing outro sequence to play.");
        }
    }
}