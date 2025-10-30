using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Rigidbody))]
public class Placeable : MonoBehaviour {
  public string sourceId;
  public bool snapToGrid = true; public float gridStep = 0.25f;
  public bool forbidOverlap = true; public Vector3 boundsPadding = new(0.01f,0.01f,0.01f);
  Rigidbody _rb; UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable _grab;
  
  void Awake(){
    _rb = GetComponent<Rigidbody>();
    _grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>() ?? gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    _rb.isKinematic = false; _rb.useGravity = true;
    _grab.selectExited.AddListener(OnReleased);
  }
  
  void OnDestroy(){ if (_grab) _grab.selectExited.RemoveListener(OnReleased); }
  
  void OnReleased(SelectExitEventArgs _){
    if (snapToGrid){
      var p = transform.position;
      p = new Vector3(Mathf.Round(p.x/gridStep)*gridStep, Mathf.Round(p.y/gridStep)*gridStep, Mathf.Round(p.z/gridStep)*gridStep);
      transform.position = p;
    }
    if (forbidOverlap){
      var col = GetComponentInChildren<Collider>();
      if (col){
        var b = col.bounds; var size = b.size + boundsPadding;
        var hits = Physics.OverlapBox(b.center, size*0.5f, transform.rotation, ~0, QueryTriggerInteraction.Ignore);
        foreach (var h in hits) if (h.transform!=transform){ transform.position += Vector3.up*0.1f; break; }
      }
    }
  }
}

