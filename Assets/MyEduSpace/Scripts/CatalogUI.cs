using UnityEngine;
using UnityEngine.UIElements;

public class CatalogUI : MonoBehaviour
{
    [Header("Data")]
    public CatalogDatabase catalog;

    [Header("UI")]
    public UIDocument uiDocument;             // quello con CatalogPanel.uxml
    public VisualTreeAsset itemRowTemplate;    // CatalogItemRow.uxml
    public StyleSheet styleSheet;              // Catalog.uss (opzionale)

    [Header("Spawn")]
    public PrefabSpawner spawner;

    void OnEnable()
    {
        if (!uiDocument) uiDocument = GetComponent<UIDocument>();
        var root = uiDocument.rootVisualElement;
        if (styleSheet != null && !root.styleSheets.Contains(styleSheet))
            root.styleSheets.Add(styleSheet);

        var scroll = root.Q<ScrollView>("catalogScroll");
        scroll.Clear();

        if (catalog == null || catalog.items == null) return;

        foreach (var item in catalog.items)
        {
            var row = itemRowTemplate.Instantiate();
            row.name = "row";

            var nameLabel = row.Q<Label>("nameLabel");
            var iconVE = row.Q<VisualElement>("icon");
            var spawnBtn = row.Q<Button>("spawnBtn");

            nameLabel.text = string.IsNullOrEmpty(item.displayName) ? item.name : item.displayName;

            if (item.icon != null)
            {
                // UI Toolkit runtime: usa backgroundImage
                iconVE.style.backgroundImage = new StyleBackground(item.icon);
            }

            spawnBtn.clicked += () =>
            {
                if (spawner == null)
                    spawner = FindFirstObjectByType<PrefabSpawner>();
                spawner?.Spawn(item.prefab);
            };

            scroll.Add(row);
        }
    }
}
