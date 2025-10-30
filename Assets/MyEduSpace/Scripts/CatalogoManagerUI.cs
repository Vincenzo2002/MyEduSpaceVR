using UnityEngine;

public class CatalogoManagerUI : MonoBehaviour
{
    public GameObject menu;

    [Header("Posizione fissa")]
    public Transform anchor;     // Empty nella scena: posizione/rotazione desiderate
    public bool faceHead = false;
    public Transform head;       // opzionale: solo se vuoi che guardi la testa

    public void ToggleMenu() => SetMenuVisible(!menu.activeSelf);
    public void ShowMenu()   => SetMenuVisible(true);
    public void HideMenu()   => SetMenuVisible(false);

    void SetMenuVisible(bool visible)
    {
        if (!menu) return;

        menu.SetActive(visible);
        if (!visible) return;

        if (anchor)
            menu.transform.SetPositionAndRotation(anchor.position, anchor.rotation);

        if (faceHead && head)
            FaceHead();
    }

    void Update()
    {
        if (faceHead && menu && menu.activeSelf && head)
            FaceHead(); // solo rotazione, posizione resta fissa
    }

    void FaceHead()
    {
        Vector3 look = new Vector3(head.position.x, menu.transform.position.y, head.position.z);
        menu.transform.LookAt(look);
        menu.transform.forward *= -1f;
    }
}
