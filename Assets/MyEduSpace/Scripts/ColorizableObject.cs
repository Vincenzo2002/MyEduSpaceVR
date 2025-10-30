// ColorizableObject.cs
using UnityEngine;

public class ColorizableObject : MonoBehaviour
{
    [Tooltip("Renderers to tint. If empty, will search in children.")]
    public Renderer[] targetRenderers;

    MaterialPropertyBlock _mpb;
    static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    static readonly int ColorProp = Shader.PropertyToID("_Color");

    void Awake()
    {
        if (targetRenderers == null || targetRenderers.Length == 0)
            targetRenderers = GetComponentsInChildren<Renderer>(true);
        _mpb = new MaterialPropertyBlock();
    }

    public void SetColor(Color c)
    {
        foreach (var r in targetRenderers)
        {
            if (!r) continue;
            r.GetPropertyBlock(_mpb);
            if (r.sharedMaterial && r.sharedMaterial.HasProperty(BaseColor))
                _mpb.SetColor(BaseColor, c);
            else
                _mpb.SetColor(ColorProp, c);
            r.SetPropertyBlock(_mpb);
        }
    }
}
