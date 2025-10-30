using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HoverColorUISpawner : MonoBehaviour
{
    public XRBaseInteractor interactor;
    public Camera xrCamera;
    public GameObject colorUIPrefab;

    public Vector3 panelOffset = new Vector3(0f, 0.12f, 0f);
    public float faceCameraLerp = 12f;

    [Header("UI Timer Settings")]
    public float uiLifetime = 10f; // ⏱ tempo di permanenza in secondi

    GameObject _spawnedUI;
    Transform _ui;
    ColorPanelController _panel;
    ColorizableObject _currentTarget;
    Coroutine _hideRoutine;
    Coroutine _timerRoutine; // ⏱ nuova coroutine per il timer

    void Reset()
    {
        xrCamera = Camera.main;
        interactor = GetComponent<XRBaseInteractor>();
    }

    void OnEnable()
    {
        if (interactor != null)
        {
            interactor.hoverEntered.AddListener(OnHoverEntered);
            interactor.hoverExited.AddListener(OnHoverExited);
        }
    }

    void OnDisable()
    {
        if (interactor != null)
        {
            interactor.hoverEntered.RemoveListener(OnHoverEntered);
            interactor.hoverExited.RemoveListener(OnHoverExited);
        }
        CleanupUIImmediate();
    }

    void LateUpdate()
    {
        if (_ui && xrCamera)
        {
            var dir = (xrCamera.transform.position - _ui.position).normalized;
            var lookRot = Quaternion.LookRotation(dir, Vector3.up);
            _ui.rotation = Quaternion.Slerp(_ui.rotation, lookRot, Time.deltaTime * faceCameraLerp);
        }
    }

    void OnHoverEntered(HoverEnterEventArgs args)
    {
        var t = args.interactableObject?.transform;
        if (!t) return;

        var target = t.GetComponentInParent<ColorizableObject>();
        if (!target) return;

        // Se stiamo già mostrando per lo stesso target → ignora
        if (_currentTarget == target && _spawnedUI) return;

        // Se cambio oggetto → pulisco la vecchia UI
        CleanupUIImmediate();

        // Spawn UI
        _spawnedUI = Instantiate(colorUIPrefab);
        _ui = _spawnedUI.transform;
        _panel = _spawnedUI.GetComponent<ColorPanelController>();
        _panel.Bind(target);
        _currentTarget = target;

        PlaceUINearTarget(target);

        // Avvia timer di 10 secondi
        if (_timerRoutine != null) StopCoroutine(_timerRoutine);
        _timerRoutine = StartCoroutine(UITimer());
    }

    void OnHoverExited(HoverExitEventArgs args)
    {
        // Puoi decidere se farla scomparire subito o lasciare che il timer continui.
        // Se vuoi che sparisca solo col timer, NON chiamare Cleanup qui.
        // Se invece vuoi che sparisca anche quando smetti di puntare:
        // StartCoroutine(HideAfterDelay(0.05f));
    }

    IEnumerator UITimer()
    {
        yield return new WaitForSeconds(uiLifetime);
        CleanupUIImmediate();
    }

    IEnumerator HideAfterDelay(float t)
    {
        yield return new WaitForSeconds(t);
        CleanupUIImmediate();
    }

    void CleanupUIImmediate()
    {
        if (_timerRoutine != null)
        {
            StopCoroutine(_timerRoutine);
            _timerRoutine = null;
        }

        if (_spawnedUI) Destroy(_spawnedUI);
        _spawnedUI = null;
        _ui = null;
        _panel = null;
        _currentTarget = null;
        _hideRoutine = null;
    }

    void PlaceUINearTarget(ColorizableObject tgt)
    {
        if (!_ui || !tgt) return;

        var bounds = new Bounds(tgt.transform.position, Vector3.zero);
        foreach (var r in tgt.targetRenderers)
            if (r) bounds.Encapsulate(r.bounds);

        var pos = bounds.center + Vector3.up * (bounds.extents.y + 0.05f);
        if (xrCamera)
        {
            var camDir = (bounds.center - xrCamera.transform.position).normalized;
            pos += -camDir * 0.1f;
        }

        _ui.position = pos + panelOffset;
    }
}


