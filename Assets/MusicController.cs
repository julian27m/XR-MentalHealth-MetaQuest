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
            //LogToFile("No se encontr� un AudioSource en MusicPlayer.");
            return;
        }

        audioSource.mute = true;
        audioSource.loop = true;

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            //LogToFile("M�sica iniciada en silencio.");
        }
    }

    public void MuteMusic()
    {
        if (audioSource != null)
        {
            audioSource.mute = true;
            //LogToFile("M�sica en MUTE");
        }
    }

    public void UnmuteMusic()
    {
        if (audioSource != null)
        {
            audioSource.mute = false;
            //LogToFile("M�sica en UNMUTE");
        }
    }

    //private void LogToFile(string message)
    //{
    //    File.AppendAllText(logFilePath, message + "\n");
    //}
}
