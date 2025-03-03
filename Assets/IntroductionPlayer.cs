using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Plays a series of introduction audio clips in order with a specified delay between each clip.
/// </summary>
public class IntroductionPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> introClips = new List<AudioClip>();

    [Header("Timing Settings")]
    [SerializeField] private float initialDelay = 3.0f;        // Delay before the first clip (in seconds)
    [SerializeField] private float delayBetweenClips = 5.0f;   // Delay between clips (in seconds)

    [Header("Debug Settings")]
    [SerializeField] private bool debugMode = true;

    private int currentClipIndex = 0;
    private bool isPlaying = false;

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
                    Debug.Log("Added AudioSource component to IntroductionPlayer");
            }
        }

        // Configure audio source
        audioSource.playOnAwake = false;
        audioSource.loop = false;

        // Start the introduction sequence
        if (introClips.Count > 0)
        {
            if (debugMode)
                Debug.Log($"Starting introduction sequence with {introClips.Count} clips. First clip in {initialDelay} seconds.");

            StartCoroutine(PlayIntroductionSequence());
        }
        else
        {
            Debug.LogWarning("No introduction clips assigned to IntroductionPlayer.");
        }
    }

    private IEnumerator PlayIntroductionSequence()
    {
        // Initial delay before starting
        yield return new WaitForSeconds(initialDelay);

        currentClipIndex = 0;

        // Play each clip with a delay between them
        while (currentClipIndex < introClips.Count)
        {
            PlayCurrentClip();

            // Wait for current clip to finish
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            // Wait for the delay between clips
            if (currentClipIndex < introClips.Count - 1) // No need to wait after the last clip
            {
                if (debugMode)
                    Debug.Log($"Waiting {delayBetweenClips} seconds before playing next clip.");

                yield return new WaitForSeconds(delayBetweenClips);
            }

            // Move to next clip
            currentClipIndex++;
        }

        if (debugMode)
            Debug.Log("Introduction sequence complete.");
    }

    private void PlayCurrentClip()
    {
        if (currentClipIndex < 0 || currentClipIndex >= introClips.Count)
        {
            Debug.LogError($"Invalid clip index: {currentClipIndex}");
            return;
        }

        AudioClip clipToPlay = introClips[currentClipIndex];

        if (clipToPlay == null)
        {
            Debug.LogError($"Clip at index {currentClipIndex} is null.");
            return;
        }

        if (debugMode)
            Debug.Log($"Playing clip {currentClipIndex + 1}/{introClips.Count}: {clipToPlay.name}, Duration: {clipToPlay.length} seconds");

        audioSource.clip = clipToPlay;
        audioSource.Play();
    }

    // Optional: Public method to manually restart the sequence
    public void RestartIntroduction()
    {
        StopAllCoroutines();
        audioSource.Stop();
        StartCoroutine(PlayIntroductionSequence());

        if (debugMode)
            Debug.Log("Introduction sequence restarted.");
    }
}