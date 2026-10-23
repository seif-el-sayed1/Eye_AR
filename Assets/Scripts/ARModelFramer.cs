using UnityEngine;

public class ARModelFramer : MonoBehaviour
{
    [SerializeField] float surfaceClearance = 0.06f;
    Renderer[] modelRenderers;
    Vector3[] modelBoundsCorners;
    float yawOffset;
    bool placed;

    public bool IsPlaced => placed;

    void Awake()
    {
        yawOffset = transform.localEulerAngles.y;
        modelRenderers = GetComponentsInChildren<Renderer>(true);
        modelBoundsCorners = new Vector3[modelRenderers.Length * 8];
        for (var i = 0; i < modelRenderers.Length; i++)
        {
            var bounds = modelRenderers[i].bounds;
            for (var corner = 0; corner < 8; corner++)
            {
                var worldPoint = new Vector3(
                    (corner & 1) == 0 ? bounds.min.x : bounds.max.x,
                    (corner & 2) == 0 ? bounds.min.y : bounds.max.y,
                    (corner & 4) == 0 ? bounds.min.z : bounds.max.z);
                modelBoundsCorners[i * 8 + corner] = transform.InverseTransformPoint(worldPoint);
            }
        }
        foreach (var renderer in modelRenderers)
            renderer.enabled = false;
    }

    public void PlaceAt(Pose pose, Camera camera)
    {
        var up = pose.rotation * Vector3.up;
        if (up.sqrMagnitude < 0.001f) up = Vector3.up;
        up.Normalize();

        var facing = camera != null
            ? Vector3.ProjectOnPlane(camera.transform.forward, up)
            : Vector3.ProjectOnPlane(Vector3.forward, up);
        if (facing.sqrMagnitude < 0.001f)
            facing = Vector3.ProjectOnPlane(Vector3.forward, up);
        facing.Normalize();

        transform.SetParent(null, true);
        transform.SetPositionAndRotation(pose.position, Quaternion.LookRotation(facing, up) * Quaternion.AngleAxis(yawOffset, up));

        // Lift the entire anatomy above the detected surface, including its lowest renderer.
        var minimumHeight = float.PositiveInfinity;
        foreach (var localPoint in modelBoundsCorners)
        {
            var point = transform.TransformPoint(localPoint);
            minimumHeight = Mathf.Min(minimumHeight, Vector3.Dot(point - pose.position, up));
        }

        if (!float.IsPositiveInfinity(minimumHeight))
            transform.position += up * (surfaceClearance - minimumHeight);

        foreach (var renderer in modelRenderers)
            renderer.enabled = true;
        placed = true;
    }
}
