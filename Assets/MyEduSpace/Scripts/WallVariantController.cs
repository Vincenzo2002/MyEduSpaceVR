// WallVariantController.cs
using UnityEngine;

public class WallVariantController : MonoBehaviour
{
    [Header("Variants")]
    public GameObject wallNoWindow;
    public GameObject wallWithWindow;
    public bool startWithNoWindow = true;

    void Awake()
    {
        if (startWithNoWindow) ActivateNoWindow();
        else ActivateWindow();
    }

    public void ActivateNoWindow()
    {
        if (wallNoWindow)   wallNoWindow.SetActive(true);
        if (wallWithWindow) wallWithWindow.SetActive(false);
    }

    public void ActivateWindow()
    {
        if (wallNoWindow) wallNoWindow.SetActive(false);
        if (wallWithWindow) wallWithWindow.SetActive(true);
    }
    
    //Restituisce la variante attiva
    public GameObject GetActiveVariant()
    {
        if (wallWithWindow && wallWithWindow.activeInHierarchy) return wallWithWindow;
        if (wallNoWindow && wallNoWindow.activeInHierarchy) return wallNoWindow;
        return null;
    }

    //Cambia il colore della variante attiva
    public void SetColor(Color color)
    {
        GameObject active = GetActiveVariant();
        if (!active) return;

        var colorizable = active.GetComponentInChildren<ColorizableObject>();
        if (colorizable) colorizable.SetColor(color);
    }
}
