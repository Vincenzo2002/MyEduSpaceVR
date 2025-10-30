using UnityEngine;


public class ObjectSpawner : MonoBehaviour {
    public UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor rightRay;
    public LayerMask placementMask = ~0;
    public float defaultDistance = 2f;
    public bool snapToSurfaceNormal = true;
    CatalogItem _selected;

    public void Select(CatalogItem item){ 
        _selected = item; 
    }
    
    public void SpawnSelected(){
        if (_selected==null || _selected.prefab==null) return;
        
        if (TryGetAimPoint(out var pos, out var rot)){
            var go = Instantiate(_selected.prefab, pos, rot);
            var pl = go.GetComponent<Placeable>() ?? go.AddComponent<Placeable>();
            pl.sourceId = _selected.id;
        }
    }
    
    bool TryGetAimPoint(out Vector3 p, out Quaternion r){
        p=Vector3.zero; r=Quaternion.identity;
        if (rightRay && rightRay.TryGetCurrent3DRaycastHit(out var hit)){
            p = hit.point; 
            r = snapToSurfaceNormal? Quaternion.LookRotation(-hit.normal): Quaternion.identity; 
            return true;
        }

        var cam = Camera.main; if (cam){ 
            p = cam.transform.position + cam.transform.forward*defaultDistance; 
            r = Quaternion.LookRotation(-cam.transform.forward); 
            return true; 
        }
        return false;
    }
}


