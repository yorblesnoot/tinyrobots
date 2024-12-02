using UnityEngine;

public class CursorBehaviour : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float terrainStickiness = 1;
    [SerializeField] float scrollRate = 20;
    [SerializeField] float minimumCameraDistance = 5;
    [SerializeField] float maximumCameraDistance = 100;

    [Header("Components")]
    [SerializeField] Transform depthPlane;
    [SerializeField] GameObject airCursor;
    [SerializeField] GameObject groundCursor;
    int combinedMask;
    int terrainMask;
    int terrainLayer;
    Vector3 lastTerrainPosition;
    
    private void Awake()
    {
        combinedMask = LayerMask.GetMask("Terrain", "CursorPlanes");
        terrainMask = LayerMask.GetMask("Terrain");
        terrainLayer = LayerMask.NameToLayer("Terrain");
    }

    private void Start()
    {
        depthPlane.transform.position = PrimaryCursor.Transform.position;
    }

    public void ControlCursor()
    {
        //if the wheel is being scrolled back, snap to plane
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        bool scrollBack = scroll < 0;
        if(scroll != 0) PrimaryCursor.Unsnap();
        if (scrollBack) ScrollDepthPlane(scroll);

        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var combinedHit, maximumCameraDistance, combinedMask);
        if (scrollBack)
        {
            AirMode();
            lastTerrainPosition = Vector3.zero;
            transform.position = combinedHit.point;
            return;
        }
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var terrainHit, maximumCameraDistance, terrainMask);
        //if plane is deeper than terrain, snap to terrain
        if(combinedHit.collider != null && combinedHit.collider.gameObject.layer == terrainLayer)
        {
            SnapToTerrain(combinedHit);
            
        }
        else if(terrainHit.collider != null && Vector3.Distance(lastTerrainPosition, terrainHit.point) < terrainStickiness)
        {
            //if terrain is deeper than plane and distance from last terrain hit is within a margin, snap to terrain
            SnapToTerrain(terrainHit);
        }
        else
        {
            //otherwise, snap to plane
            AirMode();
            ScrollDepthPlane(scroll);
            transform.position = combinedHit.point;
        }
    }

    public void Hide()
    {
        airCursor.SetActive(false);
        groundCursor.SetActive(false);
    }

    void AirMode()
    {
        groundCursor.SetActive(false);
        airCursor.SetActive(true);
    }

    void SnapToTerrain(RaycastHit hit)
    {
        groundCursor.SetActive(true);
        airCursor.SetActive(false);
        groundCursor.transform.rotation = Quaternion.LookRotation(hit.normal);
        Vector3 position = hit.point;
        transform.position = position;
        depthPlane.position = position;
        lastTerrainPosition = position;
    }

    void ScrollDepthPlane(float scroll)
    {
        Vector3 newPosition = depthPlane.localPosition;
        newPosition.z += scroll * scrollRate;
        newPosition.z = Mathf.Clamp(newPosition.z, minimumCameraDistance, maximumCameraDistance);
        depthPlane.localPosition = newPosition;
    }

    public void SnapToPosition(Vector3 position)
    {
        depthPlane.position = position;
        AirMode();
    }
}
