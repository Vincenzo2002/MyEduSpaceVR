using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ConstrainedGrabInteractable : XRGrabInteractable
{
    private IXRSelectInteractor currentInteractor;
    private bool leftHand, rightHand;
    private Vector3 frozenPosition;
    private Quaternion frozenRotation;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        currentInteractor = args.interactorObject;
        var t = (currentInteractor as Component)?.transform;
        leftHand  = t != null && t.CompareTag("LeftHand");
        rightHand = t != null && t.CompareTag("RightHand");

        frozenPosition = transform.position;
        frozenRotation = transform.rotation;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        currentInteractor = null;
        leftHand = rightHand = false;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (!isSelected || currentInteractor == null) return;

        var interactorTransform = (currentInteractor as Component)?.transform;
        if (interactorTransform == null) return;

        if (leftHand && !rightHand)
        {
            // Solo posizione: usa la posizione della mano ma mantieni la rotazione congelata
            transform.position = interactorTransform.position;
            transform.rotation = frozenRotation;
        }
        else if (rightHand && !leftHand)
        {
            // Sola rotazione: ruota come la mano ma mantieni la posizione congelata
            transform.position = frozenPosition;
            transform.rotation = interactorTransform.rotation;
        }
        // Se per caso entrambe tengono l'oggetto, lascia il comportamento base (o personalizza qui)
    }
}

