using UnityEngine;
using System.IO;

public class MusicController : MonoBehaviour
{
    private AudioSource audioSource;
    //private string logFilePath;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        //logFilePath = Application.persistentDataPath + "/log.txt";

        if (audioSource == null)
        {
            //LogToFile("No se encontró un AudioSource en MusicPlayer.");
            return;
        }

        audioSource.mute = true;
        audioSource.loop = true;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            //LogToFile("Música iniciada en silencio.");
        }
    }

    public void MuteMusic()
    {
        if (audioSource != null)
        {
            audioSource.mute = true;
            //LogToFile("Música en MUTE");
        }
    }

    public void UnmuteMusic()
    {
        if (audioSource != null)
        {
            audioSource.mute = false;
            //LogToFile("Música en UNMUTE");
        }
    }

    //private void LogToFile(string message)
    //{
    //    File.AppendAllText(logFilePath, message + "\n");
    //}
}
