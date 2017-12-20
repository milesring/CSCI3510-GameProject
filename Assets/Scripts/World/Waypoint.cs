using UnityEngine;

public class Waypoint : MonoBehaviour {

    private GameObject viewpoint;
    private Vector3 viewpointOffset = new Vector3(0, 1, 0);

    CapsuleCollider waypointCollider;

    public bool indoors = false;
    private float height = 2.0f;
    private float radius = 0.2f;
    private Vector3 position = new Vector3(0, 0.5f, 0);

	// Use this for initialization
	void Start () {
        waypointCollider = gameObject.AddComponent<CapsuleCollider>();
        waypointCollider.height = height;
        waypointCollider.radius = radius;
        waypointCollider.center = position;
        waypointCollider.isTrigger = true;

        viewpoint = new GameObject();
        viewpoint.name = "Viewpoint";
        viewpoint.transform.position = gameObject.transform.position + viewpointOffset;
        viewpoint.transform.SetParent(gameObject.transform);

        gameObject.tag = "Waypoint";
	}

    public void SetColliderHeight(float height)
    {
        this.height = height;
        if (waypointCollider != null)
            waypointCollider.height = height;
    }

    public void SetColliderRadius(float radius)
    {
        this.radius = radius;
        if (waypointCollider != null)
            waypointCollider.radius = radius;
    }

    public void SetColliderPosition(Vector3 position)
    {
        this.position = position;
        if (waypointCollider != null)
            waypointCollider.center = position;
    }

    public void SetViewpointOffset(Vector3 offset)
    {
        this.viewpointOffset = offset;
        if (viewpoint != null)
        {
            viewpoint.transform.position = offset;
        }
    }

    public GameObject GetViewpoint()
    {
        return viewpoint;
    }
}
