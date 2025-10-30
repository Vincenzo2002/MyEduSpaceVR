// Assets/Scripts/HoverColorUIManager.cs
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class HoverColorUIManager : MonoBehaviour
{
    [Header("XR")]
    [Tooltip("Assegna il Near-Far Interactor del controller (deriva da XRBaseInteractor).")]
    public XRBaseInteractor nearFarInteractor;

    [Header("UI")]
    public GameObject colorPanelPrefab;              // il prefab creato prima
    public Vector3 panelOffset = new Vector3(0, 0.15f, 0);
    public float followLerp = 15f;

    private Camera _cam;
    private GameObject _panel;
    private UIDocument _doc;
    private VisualElement _root, _preview;
    private Slider _r, _g, _b;

    private ColorEditable _current;
    private Vector3 _targetPos;

    void Awake()
    {
        _cam = Camera.main;
        if (nearFarInteractor == null)
            nearFarInteractor = GetComponent<XRBaseInteractor>();

        // Iscrizione agli eventi hover del Near-Far Interactor
        nearFarInteractor.hoverEntered.AddListener(OnHoverEntered);
        nearFarInteractor.hoverExited.AddListener(OnHoverExited);
    }

    void OnDestroy()
    {
        if (nearFarInteractor != null)
        {
            nearFarInteractor.hoverEntered.RemoveListener(OnHoverEntered);
            nearFarInteractor.hoverExited.RemoveListener(OnHoverExited);
        }
    }

    void Update()
    {
        // Segui dolcemente il punto target e fai billboard verso camera
        if (_panel != null && _current != null)
        {
            _panel.transform.position = Vector3.Lerp(
                _panel.transform.position, _targetPos, Time.deltaTime * followLerp
            );

            if (_cam != null)
            {
                var lookPos = _panel.transform.position + (_cam.transform.rotation * Vector3.forward);
                _panel.transform.LookAt(lookPos, Vector3.up);
            }
        }
    }

    void OnHoverEntered(HoverEnterEventArgs args)
    {
        // Oggetto XR interagibile sotto al laser
        var go = args.interactableObject.transform.gameObject;

        // Risali a ColorEditable (può essere su parent)
        var editable = go.GetComponentInParent<ColorEditable>();
        if (editable == null) return;

        _current = editable;

        // Se il Near-Far è anche un ray interactor, recupero il punto preciso di hit
        if (nearFarInteractor is XRRayInteractor ray && ray.TryGetCurrent3DRaycastHit(out var hit))
            _targetPos = hit.point + panelOffset;
        else
            _targetPos = _current.transform.position + panelOffset;

        ShowPanel();
        SyncUIFromTarget();
    }

    void OnHoverExited(HoverExitEventArgs args)
    {
        // Se usciamo proprio dall'oggetto che stavamo editando → chiudi
        if (_current != null && args.interactableObject.transform.IsChildOf(_current.transform))
        {
            HidePanel();
            _current = null;
        }
    }

    void ShowPanel()
    {
        if (_panel == null)
        {
            _panel = Instantiate(colorPanelPrefab);
            _doc = _panel.GetComponent<UIDocument>();
            _root = _doc.rootVisualElement;

            _r = _root.Q<Slider>("R");
            _g = _root.Q<Slider>("G");
            _b = _root.Q<Slider>("B");
            _preview = _root.Q<VisualElement>("preview");

            if (_r != null) _r.RegisterValueChangedCallback(_ => OnColorChanged());
            if (_g != null) _g.RegisterValueChangedCallback(_ => OnColorChanged());
            if (_b != null) _b.RegisterValueChangedCallback(_ => OnColorChanged());

            _root.Q<Button>("btnRed")?.RegisterCallback<ClickEvent>(_ => SetQuick(Color.red));
            _root.Q<Button>("btnGreen")?.RegisterCallback<ClickEvent>(_ => SetQuick(Color.green));
            _root.Q<Button>("btnBlue")?.RegisterCallback<ClickEvent>(_ => SetQuick(Color.blue));
            _root.Q<Button>("btnReset")?.RegisterCallback<ClickEvent>(_ => SetQuick(Color.white));
        }

        _panel.SetActive(true);
        _panel.transform.position = _targetPos;
    }

    void HidePanel()
    {
        if (_panel != null) _panel.SetActive(false);
    }

    void SyncUIFromTarget()
    {
        if (_current == null) return;
        var c = _current.GetColor();
        if (_r != null) _r.SetValueWithoutNotify(c.r);
        if (_g != null) _g.SetValueWithoutNotify(c.g);
        if (_b != null) _b.SetValueWithoutNotify(c.b);
        UpdatePreview(c);
    }

    void OnColorChanged()
    {
        if (_current == null) return;
        float r = _r != null ? _r.value : 1f;
        float g = _g != null ? _g.value : 1f;
        float b = _b != null ? _b.value : 1f;
        var c = new Color(r, g, b, 1f);
        _current.SetColor(c);
        UpdatePreview(c);
    }

    void SetQuick(Color c)
    {
        if (_r != null) _r.SetValueWithoutNotify(c.r);
        if (_g != null) _g.SetValueWithoutNotify(c.g);
        if (_b != null) _b.SetValueWithoutNotify(c.b);
        OnColorChanged();
    }

    void UpdatePreview(Color c)
    {
        if (_preview != null)
            _preview.style.backgroundColor = new StyleColor(c);
    }
}


