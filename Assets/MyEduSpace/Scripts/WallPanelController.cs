// WallPanelController.cs
using UnityEngine;
using UnityEngine.UI;

public class WallPanelController : MonoBehaviour
{
    public WallVariantController wall; // bound a runtime
    public Button btnNoWindow;
    public Button btnWithWindow;

    public void Bind(WallVariantController w)
    {
        wall = w;
        RefreshButtons();
    }

    public void SetNoWindow()
    {
        if (!wall) return;
        wall.ActivateNoWindow();
        RefreshButtons();
    }

    public void SetWindow()
    {
        if (!wall) return;
        wall.ActivateWindow();
        RefreshButtons();
    }

    void RefreshButtons()
    {
        if (!wall || !btnNoWindow || !btnWithWindow) return;
        bool windowActive = wall.wallWithWindow && wall.wallWithWindow.activeInHierarchy;
        // se è già attiva una variante, disabilita il suo bottone
        btnNoWindow.interactable = windowActive;
        btnWithWindow.interactable = !windowActive;
    }
    
    public void SetColorRed()    => wall?.SetColor(Color.red);
    public void SetColorGreen()  => wall?.SetColor(Color.green);
    public void SetColorBlue()   => wall?.SetColor(Color.blue);
    public void SetColorWhite()  => wall?.SetColor(Color.white);
    public void SetColorYellow() => wall?.SetColor(Color.yellow);
    public void SetColorBlack() => wall?.SetColor(Color.black);
}
