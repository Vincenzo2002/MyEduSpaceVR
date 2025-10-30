using UnityEngine;

[System.Serializable]
public class SpawnOffsetRule
{
    public GameObject prefab;
    public Vector3 offset;
}

public class PrefabSpawner : MonoBehaviour
{
    public Camera targetCamera;
    public float distanceFromCamera = 1.5f;

    [Header("Altezza")]
    public bool snapToFloor = true;
    public LayerMask floorMask = ~0;
    public float probeUp = 2f;
    public float probeDown = 6f;
    public float probeRadius = 0.1f;
    public float eyeLevelOffset = -0.2f;

    [Header("Regole offset prefab")]
    public SpawnOffsetRule[] offsetRules;

    public GameObject Spawn(GameObject prefab)
    {
        if (!prefab) return null;

        var cam = targetCamera ? targetCamera : Camera.main;
        if (!cam) { Debug.LogWarning("PrefabSpawner: nessuna camera."); return null; }

        var forwardFlat = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        var pos = cam.transform.position + forwardFlat * distanceFromCamera;
        //var rot = Quaternion.LookRotation(forwardFlat, Vector3.up);
        var rot = prefab.transform.rotation;
        var go = Instantiate(prefab, pos, rot);

        // ---- Correzione altezza con pavimento
        if (snapToFloor)
        {
            Vector3 rayOrigin = pos + Vector3.up * probeUp;
            float maxDist = probeUp + probeDown;

            if (Physics.SphereCast(rayOrigin, probeRadius, Vector3.down, out RaycastHit hit,
                                   maxDist, floorMask, QueryTriggerInteraction.Ignore))
            {
                Bounds b = GetObjectBounds(go);
                float bottomY = b.center.y - b.extents.y;
                float delta = hit.point.y - bottomY;
                go.transform.position += new Vector3(0f, delta, 0f);
            }
            else
            {
                go.transform.position = new Vector3(pos.x, cam.transform.position.y + eyeLevelOffset, pos.z);
            }
        }

        // ---- Applica offset specifico se esiste
        foreach (var rule in offsetRules)
        {
            if (rule.prefab == prefab)
            {
                go.transform.position += rule.offset;
                break;
            }
        }

        return go;
    }

    private Bounds GetObjectBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            var b = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
            return b;
        }

        var colliders = go.GetComponentsInChildren<Collider>();
        if (colliders.Length > 0)
        {
            var b = colliders[0].bounds;
            for (int i = 1; i < colliders.Length; i++) b.Encapsulate(colliders[i].bounds);
            return b;
        }

        return new Bounds(go.transform.position, Vector3.one * 0.5f);
    }
}
