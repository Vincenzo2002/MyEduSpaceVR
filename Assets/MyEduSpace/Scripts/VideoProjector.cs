using UnityEngine;
using UnityEngine.Video;
using System.IO;

public class VideoProjector : MonoBehaviour
{
    public VideoPlayer vp;
    public AudioSource audioSource;
    public string fileName = "mioVideo.mp4"; // in StreamingAssets

    void Awake()
    {
        if (!vp) vp = GetComponent<VideoPlayer>();
        if (!audioSource) audioSource = GetComponent<AudioSource>();

        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        vp.source = VideoSource.Url;
        vp.url = path;

        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.SetTargetAudioSource(0, audioSource);

        vp.playOnAwake = false;
        audioSource.playOnAwake = false;

        // Carica in anticipo per evitare “lag” all’avvio
        vp.Prepare();
        vp.prepareCompleted += _ => { /* pronto a partire */ };
        vp.skipOnDrop = true;
    }

    public void Play()
    {
        if (!vp.isPrepared) { vp.Prepare(); return; }
        vp.Play();
        audioSource.UnPause(); // Audio è agganciato al VideoPlayer
    }

    public void Pause()
    {
        vp.Pause();
        audioSource.Pause();
    }

    public void Stop()
    {
        vp.Stop();
        audioSource.Stop();
    }

    public void Seek(double seconds)
    {
        // Usa .time (in secondi) o .frame
        vp.time = Mathf.Clamp((float)seconds, 0, (float)vp.length);
        if (vp.isPlaying) vp.Play();
    }
}

