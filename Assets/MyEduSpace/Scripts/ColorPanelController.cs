// ColorPanelController.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ColorPanelController : MonoBehaviour
{
    [Tooltip("Assigned at runtime when we spawn this panel.")]
    public ColorizableObject target;

    // Called by UI Buttons (assign in Inspector):
    public void SetColorToWhite()  => SetColor(Color.white);
    public void SetColorToBlack()  => SetColor(Color.black);
    public void SetColorToRed()    => SetColor(Color.red);
    public void SetColorToGreen()  => SetColor(Color.green);
    public void SetColorToBlue()   => SetColor(Color.blue);
    public void SetColorToYellow() => SetColor(Color.yellow);
    // ...add others as needed

    public void SetColor(Color c)
    {
        if (target) target.SetColor(c);
    }

    // Utility to bind the target at spawn time
    public void Bind(ColorizableObject t) => target = t;

    /*public void DeleteTarget(){
        if (target == null) return;
        Destroy(target.gameObject);
        Destroy(gameObject);
    }*/

    public void DeleteTarget()
    {
        if (target == null) return;

        // 1) prova a risalire al root grabbabile (Chair)
        var grabRoot = target.GetComponentInParent<XRGrabInteractable>();
        Transform root = grabRoot ? grabRoot.transform : target.transform.root;

        // 2) distruggi TUTTA la Chair
        if (root != null) Destroy(root.gameObject);

        // 3) chiudi anche la UI
        Destroy(gameObject);
    }

}
