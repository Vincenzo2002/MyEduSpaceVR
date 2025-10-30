using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;

public class NetworkVideoSync : NetworkBehaviour
{
    public VideoProjector projector;
    // Stato condiviso
    private NetworkVariable<double> syncedTime = new(writePerm: NetworkVariableWritePermission.Server);
    private NetworkVariable<bool> isPlaying = new(writePerm: NetworkVariableWritePermission.Server);

    void Update()
    {
        if (!IsServer) return;

        // Aggiorna tempo mentre riproduce
        if (projector.vp.isPlaying)
        {
            syncedTime.Value = projector.vp.time;
            isPlaying.Value = true;
        }
        else
        {
            isPlaying.Value = false;
        }
    }

    public void ServerPlayFrom(double timeSec)
    {
        if (!IsServer) return;
        syncedTime.Value = timeSec;
        PlayClientRpc(timeSec);
    }

    public void ServerPause()
    {
        if (!IsServer) return;
        PauseClientRpc();
    }

    [ClientRpc]
    void PlayClientRpc(double timeSec)
    {
        projector.Seek(timeSec);
        projector.Play();
    }

    [ClientRpc]
    void PauseClientRpc()
    {
        projector.Pause();
    }

    // Auto-correzione drift (client)
    void LateUpdate()
    {
        if (IsServer || projector.vp.length <= 0) return;

        // Se lo scarto è > 0.2s, riallinea
        double drift = syncedTime.Value - projector.vp.time;
        if (Mathf.Abs((float)drift) > 0.2f && isPlaying.Value)
        {
            projector.Seek(syncedTime.Value);
        }
    }
}
