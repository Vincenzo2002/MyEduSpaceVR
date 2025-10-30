using UnityEngine;

[CreateAssetMenu(fileName = "CatalogItem", menuName = "MyEduSpace/Catalog Item")]
public class CatalogItem : ScriptableObject
{
    public string id;                // identificativo univoco 
    public string displayName;       // nome visibile 
    public Sprite icon;              // immagine per il pulsante del catalogo 
    public GameObject prefab;        // prefab 3D da spawnare
}

