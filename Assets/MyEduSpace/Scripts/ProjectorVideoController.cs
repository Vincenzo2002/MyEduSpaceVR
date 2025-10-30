using UnityEngine;
using UnityEngine.Video;
using Unity.Netcode;

[RequireComponent(typeof(VideoPlayer), typeof(AudioSource), typeof(NetworkObject))]
public class ProjectorVideoController : NetworkBehaviour
{
    [Header("Playlist (stessi clip in tutti i client)")]
    public VideoClip[] playlist;

    [Header("Sync")]
    public float resyncInterval = 3f;       // ogni N s, il server riallinea i client
    public double desyncThreshold = 0.25;   // se differenza > soglia, fa seek

    // Stato condiviso
    private NetworkVariable<int> currentIndex = new(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> isPlaying   = new(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<double> serverTime = new(writePerm: NetworkVariableWritePermission.Server);

    private VideoPlayer vp;
    private AudioSource  au;
    private float syncTimer;

    void Awake()
    {
        vp = GetComponent<VideoPlayer>();
        au = GetComponent<AudioSource>();

        vp.playOnAwake = false;
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.EnableAudioTrack(0, true);
        vp.SetTargetAudioSource(0, au);

        vp.prepareCompleted += OnPrepared;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Clip di default
            currentIndex.Value = Mathf.Clamp(currentIndex.Value, 0, Mathf.Max(0, playlist.Length - 1));
            LoadClipServer(currentIndex.Value);
            isPlaying.Value = false;
            serverTime.Value = 0;
        }
        else
        {
            // Client: quando cambiano le variabili, reagisci
            currentIndex.OnValueChanged += (_, idx) => LoadClipClient(idx);
            isPlaying.OnValueChanged += (_, play) =>
            {
                if (play) vp.Play(); else vp.Pause();
            };
            serverTime.OnValueChanged += (_, t) => SoftSeekTo(t);
        }
    }

    void Update()
    {
        if (!IsServer) return;

        syncTimer += Time.deltaTime;
        if (syncTimer >= resyncInterval)
        {
            syncTimer = 0f;
            if (vp.clip != null)
            {
                serverTime.Value = vp.time;
            }
        }
    }

    // ---- API UI (chiamate dai pulsanti) ----
    public void UI_TogglePlay() { TogglePlayServerRpc(); }
    public void UI_Stop()       { StopServerRpc(); }
    public void UI_Next()
    {
        int next = (currentIndex.Value + 1) % Mathf.Max(1, playlist.Length);
        SetClipServerRpc(next);
    }
    public void UI_SeekNormalized(float t01)
    {
        if (vp.clip == null) return;
        double t = t01 * vp.clip.length;
        SeekServerRpc(t);
    }

    // ---- Server logic ----
    [ServerRpc(RequireOwnership = false)]
    private void TogglePlayServerRpc(ServerRpcParams _ = default)
    {
        if (vp.clip == null) return;
        if (isPlaying.Value) { vp.Pause(); isPlaying.Value = false; }
        else { vp.Play(); isPlaying.Value = true; }
        serverTime.Value = vp.time;
    }

    [ServerRpc(RequireOwnership = false)]
    private void StopServerRpc(ServerRpcParams _ = default)
    {
        if (vp.clip == null) return;
        vp.Pause(); vp.time = 0;
        isPlaying.Value = false;
        serverTime.Value = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetClipServerRpc(int index, ServerRpcParams _ = default)
    {
        LoadClipServer(index);
        // riparte in pausa da 0
        isPlaying.Value = false;
        serverTime.Value = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SeekServerRpc(double t, ServerRpcParams _ = default)
    {
        if (vp.clip == null) return;
        vp.time = Mathf.Clamp((float)t, 0f, (float)vp.clip.length);
        serverTime.Value = vp.time;
    }

    private void LoadClipServer(int index)
    {
        index = Mathf.Clamp(index, 0, Mathf.Max(0, playlist.Length - 1));
        currentIndex.Value = index;

        vp.Pause();
        vp.clip = playlist.Length > 0 ? playlist[index] : null;
        vp.Prepare();
    }

    private void OnPrepared(VideoPlayer _)
    {
        // Server: allineati al tempo condiviso
        if (IsServer)
        {
            vp.time = serverTime.Value;
            if (isPlaying.Value) vp.Play();
        }
        else
        {
            // Client: allineati appena pronto
            SoftSeekTo(serverTime.Value, force:true);
            if (isPlaying.Value) vp.Play();
        }
    }

    // ---- Client helpers ----
    private void LoadClipClient(int index)
    {
        vp.Pause();
        vp.clip = playlist.Length > 0 ? playlist[Mathf.Clamp(index, 0, playlist.Length - 1)] : null;
        vp.Prepare();
    }

    private void SoftSeekTo(double target, bool force = false)
    {
        if (vp.clip == null) return;
        double diff = Mathf.Abs((float)(vp.time - target));
        if (force || diff > desyncThreshold)
        {
            vp.time = Mathf.Clamp((float)target, 0f, (float)vp.clip.length);
        }
    }
}
