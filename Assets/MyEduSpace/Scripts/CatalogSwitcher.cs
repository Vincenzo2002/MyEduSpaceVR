using UnityEngine;
using UnityEngine.UIElements;

public class CatalogSwitcher : MonoBehaviour
{
    [Header("UI")]
    public UIDocument uiDocument;     // UIDocument con i 4 pulsanti
    public string btnGenerale = "btnGenerale";
    public string btnBanchi = "btnBanchi";
    public string btnLavagne = "btnLavagne";
    public string btnSedie = "btnSedie";

    [Header("Pannelli Catalogo")]
    public GameObject catalogoGenerale;
    public GameObject catalogoBanchi;
    public GameObject catalogoLavagne;
    public GameObject catalogoSedie;

    private GameObject[] _catalogs;

    void OnEnable()
    {
        if (!uiDocument)
            uiDocument = GetComponent<UIDocument>();

        var root = uiDocument.rootVisualElement;
        var bGenerale = root.Q<Button>(btnGenerale);
        var bBanchi   = root.Q<Button>(btnBanchi);
        var bLavagne  = root.Q<Button>(btnLavagne);
        var bSedie    = root.Q<Button>(btnSedie);

        _catalogs = new GameObject[] { catalogoGenerale, catalogoBanchi, catalogoLavagne, catalogoSedie };

        // Collega i pulsanti
        if (bGenerale != null) bGenerale.clicked += () => ShowCatalog(0);
        if (bBanchi != null)   bBanchi.clicked   += () => ShowCatalog(1);
        if (bLavagne != null)  bLavagne.clicked  += () => ShowCatalog(2);
        if (bSedie != null)    bSedie.clicked    += () => ShowCatalog(3);

        // Mostra il primo per default
        ShowCatalog(0);
    }

    private void ShowCatalog(int index)
    {
        for (int i = 0; i < _catalogs.Length; i++)
        {
            if (_catalogs[i] != null)
                _catalogs[i].SetActive(i == index);
        }
    }
}

