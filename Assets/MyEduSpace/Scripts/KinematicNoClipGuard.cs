using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(-50)] // esegue PRIMA del Grab (Update) per clampare la mossa
public class KinematicNoClipGuard : MonoBehaviour
{
    [Header("Collisioni da bloccare")]
    public LayerMask blockingMask;                 // layer di muri/pavimenti
    [Range(0.001f, 0.05f)] public float skin = 0.01f;
    [Range(1, 8)] public int maxResolveIterations = 3;

    Collider[] _selfCols;
    HashSet<Collider> _selfSet;
    Vector3 _prevPos;
    bool _initialized;

    readonly RaycastHit[] _hits = new RaycastHit[16];
    readonly Collider[] _overlapBuf = new Collider[64];

    void Awake()
    {
        _selfCols = GetComponentsInChildren<Collider>(false);
        _selfSet = new HashSet<Collider>(_selfCols);
    }

    void OnEnable()
    {
        _prevPos = transform.position;
        _initialized = true;
    }

    void Update()
    {
        if (!_initialized) { _prevPos = transform.position; _initialized = true; return; }

        Vector3 desiredPos = transform.position;          // posizione imposta dal Grab
        Vector3 delta = desiredPos - _prevPos;
        if (delta.sqrMagnitude < 1e-8f) return;

        // Clamp movimento con uno sweep sui collider principali (basta il primo solido “rappresentativo”)
        // Useremo i bounds dei collider come box di cast.
        Vector3 allowedDelta = delta;
        foreach (var c in _selfCols)
        {
            if (c == null || !c.enabled || c.isTrigger) continue;

            Bounds b = c.bounds;
            Vector3 half = b.extents - Vector3.one * skin;
            if (half.x <= 0 || half.y <= 0 || half.z <= 0) continue;

            Vector3 dir = delta.normalized;
            float dist = delta.magnitude;

            // BoxCast dal centro precedente lungo delta
            int hitCount = Physics.BoxCastNonAlloc(
                b.center - delta,        // partiamo dalla posizione precedente
                half,
                dir,
                _hits,
                transform.rotation,
                dist + skin,
                blockingMask,
                QueryTriggerInteraction.Ignore
            );

            // Trova il primo impatto valido che NON sia un nostro collider
            float minHit = float.PositiveInfinity;
            for (int i = 0; i < hitCount; i++)
            {
                var h = _hits[i];
                if (h.collider == null || _selfSet.Contains(h.collider) || h.collider.isTrigger) continue;
                if (h.distance < minHit) minHit = h.distance;
            }

            if (minHit < float.PositiveInfinity)
            {
                float clampDist = Mathf.Max(0f, minHit - skin);
                allowedDelta = dir * clampDist;
                break; // già clamped: basta il primo blocco
            }
        }

        transform.position = _prevPos + allowedDelta;

        // Rete di sicurezza post-mossa (come prima): spingi fuori piccole penetrazioni residue
        for (int iter = 0; iter < maxResolveIterations; iter++)
        {
            bool any = false;
            foreach (var c in _selfCols)
            {
                if (c == null || !c.enabled || c.isTrigger) continue;

                Bounds b = c.bounds;
                Vector3 half = b.extents - Vector3.one * skin;
                if (half.x <= 0 || half.y <= 0 || half.z <= 0) continue;

                int count = Physics.OverlapBoxNonAlloc(
                    b.center, half, _overlapBuf, transform.rotation,
                    blockingMask, QueryTriggerInteraction.Ignore);

                for (int i = 0; i < count; i++)
                {
                    var other = _overlapBuf[i];
                    if (other == null || _selfSet.Contains(other) || other.isTrigger) continue;

                    if (Physics.ComputePenetration(
                        c, c.transform.position, c.transform.rotation,
                        other, other.transform.position, other.transform.rotation,
                        out Vector3 dir, out float dist))
                    {
                        transform.position += dir * (dist + skin);
                        any = true;
                    }
                }
            }
            if (!any) break;
        }

        _prevPos = transform.position;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (_selfCols == null) return;
        Gizmos.color = new Color(0, 0.6f, 1f, 0.15f);
        foreach (var c in _selfCols)
        {
            if (c == null || !c.enabled || c.isTrigger) continue;
            var b = c.bounds;
            var half = b.extents - Vector3.one * skin;
            if (half.x <= 0 || half.y <= 0 || half.z <= 0) continue;
            Gizmos.matrix = Matrix4x4.TRS(b.center, transform.rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, half * 2f);
        }
    }
#endif
}


