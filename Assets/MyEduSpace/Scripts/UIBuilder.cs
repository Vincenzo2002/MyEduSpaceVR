using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIBuilder : MonoBehaviour
{
    [Header("Dati")]
    public CatalogDatabase database;     // Trascina qui il tuo CatalogDatabase
    public ObjectSpawner spawner;        // Trascina qui l'ObjectSpawner

    [Header("Canvas World-Space")]
    public Vector2 panelSize = new Vector2(500, 700);
    public float canvasDistance = 1.5f;  // distanza davanti alla camera
    public Vector3 canvasOffset = new Vector3(0.6f, -0.2f, 0f); // spostamento laterale/verticale

    [Header("Stile pulsanti")]
    public Vector2 buttonSize = new Vector2(420, 70);
    public int buttonSpacing = 10;

    [Header("Materiale opzionale (URP/HDRP)")]
    public Material panelMaterial;       // per sfondo pannello (opzionale)

    private Canvas _canvas;

    void Start()
    {
        Build();
    }

    /// <summary>
    /// Costruisce un Canvas world-space con ScrollView e un pulsante per ogni CatalogItem.
    /// </summary>
    public void Build()
    {
        if (database == null || database.items == null || database.items.Length == 0)
        {
            Debug.LogWarning("[UIBuilder] Database vuoto o non assegnato.");
            return;
        }
        if (spawner == null)
        {
            Debug.LogWarning("[UIBuilder] ObjectSpawner non assegnato.");
            return;
        }

        // ---- Canvas world-space ----
        var cam = Camera.main;
        var canvasGO = new GameObject("CatalogCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        _canvas = canvasGO.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.WorldSpace;

        var crt = canvasGO.GetComponent<RectTransform>();
        float worldScale = 0.0015f;                  // ~1.5 mm per “pixel” UI
        crt.localScale = Vector3.one * worldScale;   // SCALA DEL CANVAS IN WORLD SPACE
        canvasGO.GetComponent<CanvasScaler>().dynamicPixelsPerUnit = 100f; // (default)
        crt.sizeDelta = panelSize;

        // Posiziona il canvas davanti alla camera, con offset laterale
        Vector3 basePos = cam ? cam.transform.position + cam.transform.forward * canvasDistance : Vector3.zero;
        canvasGO.transform.position = basePos + (cam ? cam.transform.right * canvasOffset.x + cam.transform.up * canvasOffset.y : Vector3.zero);
        if (cam) canvasGO.transform.rotation = Quaternion.LookRotation(canvasGO.transform.position - cam.transform.position) * Quaternion.Euler(0, 180, 0);

        // ---- Pannello di sfondo ----
        var panelGO = new GameObject("Panel", typeof(Image));
        panelGO.transform.SetParent(canvasGO.transform, false);
        var prt = panelGO.GetComponent<RectTransform>();
        prt.anchorMin = Vector2.zero; prt.anchorMax = Vector2.one; prt.offsetMin = Vector2.zero; prt.offsetMax = Vector2.zero;
        var pimg = panelGO.GetComponent<Image>();
        pimg.color = new Color(0f, 0f, 0f, 0.5f);
        if (panelMaterial) pimg.material = panelMaterial;

        // ---- ScrollView ----
        var scrollGO = new GameObject("ScrollView", typeof(ScrollRect), typeof(Image), typeof(Mask));
        scrollGO.transform.SetParent(panelGO.transform, false);
        var srt = scrollGO.GetComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.05f, 0.05f);
        srt.anchorMax = new Vector2(0.95f, 0.95f);
        srt.offsetMin = Vector2.zero; srt.offsetMax = Vector2.zero;

        var viewportImg = scrollGO.GetComponent<Image>();
        viewportImg.color = new Color(1f, 1f, 1f, 0.05f);
        scrollGO.GetComponent<Mask>().showMaskGraphic = false;

        // Content
        var contentGO = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGO.transform.SetParent(scrollGO.transform, false);

        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1f);

        var vlg = contentGO.GetComponent<VerticalLayoutGroup>();
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.spacing = buttonSpacing;
        vlg.padding = new RectOffset(10, 10, 10, 10);

        var csf = contentGO.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        var sr = scrollGO.GetComponent<ScrollRect>();
        sr.content = contentRT;
        sr.viewport = srt;
        sr.horizontal = false;

        // ---- Genera un pulsante per ogni CatalogItem ----
        foreach (var item in database.items)
        {
            if (item == null || item.prefab == null) continue;

            // Button container
            var btnGO = new GameObject($"Btn_{item.id}", typeof(Button), typeof(Image));
            btnGO.transform.SetParent(contentGO.transform, false);
            var brt = btnGO.GetComponent<RectTransform>();
            brt.sizeDelta = buttonSize;

            // stile di base
            var bimg = btnGO.GetComponent<Image>();
            bimg.color = new Color(1f, 1f, 1f, 0.08f);

            // Icona (se presente)
            if (item.icon)
            {
                var iconGO = new GameObject("Icon", typeof(Image));
                iconGO.transform.SetParent(btnGO.transform, false);
                var irt = iconGO.GetComponent<RectTransform>();
                irt.anchorMin = new Vector2(0, 0.5f);
                irt.anchorMax = new Vector2(0, 0.5f);
                irt.pivot = new Vector2(0.5f, 0.5f);
                irt.anchoredPosition = new Vector2(35, 0);
                irt.sizeDelta = new Vector2(50, 50);
                iconGO.GetComponent<Image>().sprite = item.icon;
            }

            // Etichetta
            var labelGO = new GameObject("Label", typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(btnGO.transform, false);
            var lrt = labelGO.GetComponent<RectTransform>();
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(20, 10 + 0); // margini
            lrt.offsetMax = new Vector2(-20, -10);
            var txt = labelGO.GetComponent<TextMeshProUGUI>();
            txt.text = item.displayName;
            txt.enableAutoSizing = true;
            txt.alignment = TextAlignmentOptions.MidlineLeft;

            // onClick → seleziona item nello spawner
            var button = btnGO.GetComponent<Button>();
            var captured = item; // cattura per lambda
            button.onClick.AddListener(() => spawner.Select(captured));
        }

        Debug.Log("[UIBuilder] Catalogo UI creato.");
    }

    /// <summary>
    /// Rimuove il Canvas creato (se vuoi ricostruirlo).
    /// </summary>
    public void Clear()
    {
        if (_canvas != null)
        {
            Destroy(_canvas.gameObject);
            _canvas = null;
        }
    }
}

