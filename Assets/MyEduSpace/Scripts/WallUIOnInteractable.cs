// WallUIOnInteractable.cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRBaseInteractable))]
public class WallUIOnInteractable : MonoBehaviour
{
    public GameObject wallUIPrefab; // Canvas World Space con WallPanelController
    public Camera xrCamera;

    [Header("UI Rotation (fissa)")]
    [Tooltip("Rotazione FISSA in gradi (world space) del pannello UI")]
    public Vector3 fixedWorldEuler = new Vector3(0f, 0f, 0f);

    public bool faceCamera = true;

    [Header("UI placement")]
    public Vector3 panelOffset = new Vector3(0f, 0.2f, 0f);
    public float uiWorldScale = 0.002f;
    public float faceCameraLerp = 12f;
    public float uiLifetime = 10f; // 0 = senza timer
    public float followLerp = 12f;

    XRBaseInteractable _interactable;
    WallVariantController _wall;
    GameObject _uiGO;
    Transform _ui;
    Transform _anchor;
    Coroutine _timer;

    void Awake()
    {
        _interactable = GetComponent<XRBaseInteractable>();
        _wall = GetComponent<WallVariantController>();
        if (!xrCamera) xrCamera = Camera.main;
    }

    void OnEnable()
    {
        _interactable.hoverEntered.AddListener(OnHoverEntered);
        _interactable.hoverExited.AddListener(OnHoverExited);
    }

    void OnDisable()
    {
        _interactable.hoverEntered.RemoveListener(OnHoverEntered);
        _interactable.hoverExited.RemoveListener(OnHoverExited);
        Cleanup();
    }

    /*void LateUpdate()
    {
        if (_ui && _anchor)
        {
            _ui.position = _anchor.position + _anchor.TransformVector(panelOffset);
            if (faceCamera && xrCamera)
            {
                var dir = (xrCamera.transform.position - _ui.position).normalized;
                var rot = Quaternion.LookRotation(dir, Vector3.up);
                _ui.rotation = Quaternion.Slerp(_ui.rotation, rot, Time.deltaTime * faceCameraLerp);
            }
        }
        if (_anchor) // aggiorna dolcemente l'ancora (se il muro si muove)
            _anchor.position = Vector3.Lerp(_anchor.position, ComputeAnchorPos(), Time.deltaTime * 12f);
    }*/

    /*void LateUpdate(){
        if (_ui && _anchor){
            // Aggiorna posizione del pannello
            _ui.position = _anchor.position + _anchor.TransformVector(panelOffset);

            // Rotazione: o guarda la camera, o applica offset manuale
            if (faceCamera && xrCamera)
                {
                // Billboard (come prima)
                var dir = (xrCamera.transform.position - _ui.position).normalized;
                var lookRot = Quaternion.LookRotation(dir, Vector3.up);
                _ui.rotation = Quaternion.Slerp(_ui.rotation, lookRot, Time.deltaTime * faceCameraLerp);
            }
            else{
                // Mantieni rotazione base dell’ancora + offset regolabile
                Quaternion offsetRot = Quaternion.Euler(rotationOffsetEuler);
                _ui.rotation = Quaternion.Slerp(_ui.rotation, _anchor.rotation * offsetRot, Time.deltaTime * faceCameraLerp);
            }
        }

        if (_anchor)
            _anchor.position = Vector3.Lerp(_anchor.position, ComputeAnchorPos(), Time.deltaTime * 12f);
    }*/

    void LateUpdate()
    {
        if (_ui && _anchor)
        {
            // Segue solo la POSIZIONE dell’ancora
            Vector3 targetPos = _anchor.position + _anchor.TransformVector(panelOffset);
            _ui.position = Vector3.Lerp(_ui.position, targetPos, Time.deltaTime * followLerp);

            // Mantiene la rotazione FISSA (impostata da Inspector)
            Quaternion targetRot = Quaternion.Euler(fixedWorldEuler);
            _ui.rotation = Quaternion.Slerp(_ui.rotation, targetRot, Time.deltaTime * followLerp);
        }

        // Aggiorna ancora se il muro si muove
        if (_anchor)
            _anchor.position = Vector3.Lerp(_anchor.position, ComputeAnchorPos(), Time.deltaTime * 12f);
    }


    void OnHoverEntered(HoverEnterEventArgs _)
    {
        if (_uiGO) { RestartTimer(); return; }

        _anchor = new GameObject("UIAnchor_Wall").transform;
        _anchor.SetParent(transform, worldPositionStays: true);
        _anchor.position = ComputeAnchorPos();

        _uiGO = Instantiate(wallUIPrefab);
        _ui = _uiGO.transform;
        _ui.localScale = Vector3.one * uiWorldScale;
        _ui.position = _anchor.position + _anchor.TransformVector(panelOffset);

        var panel = _uiGO.GetComponent<WallPanelController>();
        if (panel) panel.Bind(_wall);

        RestartTimer();
    }

    void OnHoverExited(HoverExitEventArgs _) { /* lascia che chiuda col timer */ }

    Vector3 ComputeAnchorPos()
    {
        // prova a stimare il bounds dagli eventuali renderer in figli
        var rends = GetComponentsInChildren<Renderer>(true);
        var b = new Bounds(transform.position, Vector3.zero);
        bool had = false;
        foreach (var r in rends) { if (!r) continue; if (!had) { b = r.bounds; had = true; } else b.Encapsulate(r.bounds); }
        if (!had) b = new Bounds(transform.position, Vector3.one * 0.2f);

        var pos = b.center + Vector3.up * (b.extents.y + 0.05f);
        if (xrCamera)
        {
            var camDir = (b.center - xrCamera.transform.position).normalized;
            pos += -camDir * 0.08f;
        }
        return pos;
    }

    void RestartTimer()
    {
        if (_timer != null) StopCoroutine(_timer);
        _timer = null;
        if (uiLifetime > 0f) _timer = StartCoroutine(Timer());
    }

    IEnumerator Timer() { yield return new WaitForSeconds(uiLifetime); Cleanup(); }

    void Cleanup()
    {
        if (_timer != null) { StopCoroutine(_timer); _timer = null; }
        if (_uiGO) Destroy(_uiGO);
        _uiGO = null; _ui = null;
        if (_anchor) Destroy(_anchor.gameObject);
        _anchor = null;
    }
}
