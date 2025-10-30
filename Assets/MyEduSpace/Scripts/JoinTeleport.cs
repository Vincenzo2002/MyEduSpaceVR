using UnityEngine;
using Unity.XR.CoreUtils;                 // per XROrigin

public class JoinTeleport : MonoBehaviour
{
    [Header("References")]
    public XROrigin xrOrigin;             // il tuo XR Origin
    public Transform target;              // il punto di arrivo (StartPos)

    public void TeleportNow()
    {
        if (!xrOrigin || !target) return;

        // Se c'è un CharacterController, disabilitalo per evitare collisioni durante il salto
        var cc = xrOrigin.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        // Mantieni l’offset testa-rig e posa la CAMERA sul punto target
        xrOrigin.MoveCameraToWorldLocation(target.position);
        xrOrigin.MatchOriginUpCameraForward(target.up, target.forward);

        if (cc) cc.enabled = true;
    }
}

