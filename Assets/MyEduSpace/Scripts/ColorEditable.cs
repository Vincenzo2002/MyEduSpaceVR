// Assets/Scripts/ColorEditable.cs
using UnityEngine;

[DisallowMultipleComponent]
public class ColorEditable : MonoBehaviour
{
    [Tooltip("Renderer da colorare; se vuoto, uso Renderer sullo stesso GameObject.")]
    public Renderer targetRenderer;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null)
        {
            // istanzia materiale per evitare side-effect su altri oggetti
            var instanced = new Material(targetRenderer.sharedMaterial);
            targetRenderer.material = instanced;
        }
    }

    public void SetColor(Color c)
    {
        if (targetRenderer != null)
            targetRenderer.material.color = c;
    }

    public Color GetColor()
    {
        if (targetRenderer != null)
            return targetRenderer.material.color;
        return Color.white;
    }
}


