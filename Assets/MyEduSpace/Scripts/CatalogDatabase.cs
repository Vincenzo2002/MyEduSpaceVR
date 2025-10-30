using UnityEngine;

[CreateAssetMenu(fileName = "CatalogDatabase", menuName = "MyEduSpace/Catalog Database")]
public class CatalogDatabase : ScriptableObject
{
    public CatalogItem[] items;

    public CatalogItem GetById(string id)
    {
        foreach (var it in items)
        {
            if (it != null && it.id == id) return it;
        }
        return null;
    }
}

