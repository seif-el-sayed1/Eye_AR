using UnityEngine;

public class SelectionFrame : MonoBehaviour
{
    static readonly Color PointerColor = new(0.08f, 1f, 0.82f, 1f);
    LineRenderer shaft;
    LineRenderer leftWing;
    LineRenderer rightWing;
    Collider selected;
    Camera viewCamera;

    void Awake()
    {
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default");
        var material = shader != null ? new Material(shader) { color = PointerColor } : null;
        shaft = CreateLine("Pointer Shaft", material);
        leftWing = CreateLine("Pointer Arrowhead Left", material);
        rightWing = CreateLine("Pointer Arrowhead Right", material);
        Hide();
    }

    LineRenderer CreateLine(string objectName, Material material)
    {
        var lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(transform, false);
        var line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.startColor = PointerColor;
        line.endColor = PointerColor;
        line.startWidth = 0.009f;
        line.endWidth = 0.009f;
        line.numCapVertices = 3;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.sortingOrder = 1000;
        line.sharedMaterial = material;
        return line;
    }

    public void Show(Collider target, Camera camera)
    {
        selected = target;
        viewCamera = camera;
        SetVisible(selected != null && viewCamera != null);
        RefreshPointer();
    }

    public void Hide()
    {
        selected = null;
        SetVisible(false);
    }

    void LateUpdate() => RefreshPointer();

    void RefreshPointer()
    {
        if (selected == null || viewCamera == null || shaft == null) return;

        var bounds = selected.bounds;
        var cameraTransform = viewCamera.transform;
        var size = bounds.size;
        var reach = Mathf.Max(size.magnitude * 0.45f, 0.14f);
        var target = bounds.center - cameraTransform.forward * Mathf.Max(size.magnitude * 0.18f, 0.04f);
        var start = target + cameraTransform.right * reach + cameraTransform.up * reach * 0.72f;
        var towardTarget = (target - start).normalized;
        var headLength = Mathf.Max(size.magnitude * 0.12f, 0.045f);
        var side = Vector3.Cross(towardTarget, cameraTransform.forward).normalized;
        var headBase = target - towardTarget * headLength;

        SetSegment(shaft, start, target);
        SetSegment(leftWing, target, headBase + side * headLength * 0.52f);
        SetSegment(rightWing, target, headBase - side * headLength * 0.52f);
    }

    static void SetSegment(LineRenderer line, Vector3 start, Vector3 end)
    {
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    void SetVisible(bool visible)
    {
        if (shaft != null) shaft.gameObject.SetActive(visible);
        if (leftWing != null) leftWing.gameObject.SetActive(visible);
        if (rightWing != null) rightWing.gameObject.SetActive(visible);
    }

    void OnDestroy()
    {
        if (shaft != null && shaft.sharedMaterial != null)
            Destroy(shaft.sharedMaterial);
    }
}
