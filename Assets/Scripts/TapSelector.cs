using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class TapSelector : MonoBehaviour
{
    // First tap places the world-fixed model; later taps select its anatomy.
    [SerializeField] Camera arCamera;
    [SerializeField] PartsRepository repository;
    [SerializeField] InfoPanel infoPanel;
    [SerializeField] ARRaycastManager arRaycastManager;
    [SerializeField] ARModelFramer modelFramer;
    SelectionFrame selectionFrame;
    readonly List<ARRaycastHit> planeHits = new();

    void Awake()
    {
        selectionFrame = GetComponent<SelectionFrame>();
        if (selectionFrame == null) selectionFrame = gameObject.AddComponent<SelectionFrame>();
        if (arCamera == null) arCamera = Camera.main;
        if (arRaycastManager == null) arRaycastManager = FindAnyObjectByType<ARRaycastManager>();
        if (modelFramer == null) modelFramer = FindAnyObjectByType<ARModelFramer>();
    }

    void Update()
    {
        var pointer = Pointer.current;
        if (pointer == null || !pointer.press.wasPressedThisFrame) return;
        if (IsOverUI()) return;

        var cam = arCamera != null ? arCamera : Camera.main;
        if (cam == null || modelFramer == null) return;

        var screenPosition = pointer.position.ReadValue();
        if (!modelFramer.IsPlaced)
        {
            PlaceModel(screenPosition, cam);
            return;
        }

        var ray = cam.ScreenPointToRay(screenPosition);
        var rayHits = Physics.RaycastAll(ray);
        RaycastHit? nearestPart = null;
        PartData selectedPart = null;
        foreach (var hit in rayHits)
        {
            if (!repository.TryGet(hit.collider.name, out PartData part)) continue;
            if (nearestPart == null || hit.distance < nearestPart.Value.distance)
            {
                nearestPart = hit;
                selectedPart = part;
            }
        }

        if (nearestPart.HasValue)
        {
            selectionFrame.Show(nearestPart.Value.collider, cam);
            infoPanel.Show(selectedPart);
            return;
        }

        if (TryGetPlanePose(screenPosition, out Pose pose))
        {
            modelFramer.PlaceAt(pose, cam);
            selectionFrame.Hide();
            infoPanel.Hide();
        }
    }

    void PlaceModel(Vector2 screenPosition, Camera cam)
    {
        if (TryGetPlanePose(screenPosition, out Pose pose))
        {
            modelFramer.PlaceAt(pose, cam);
            return;
        }

        // Keep the first tap useful even before AR has reported a trackable plane.
        var fallbackPosition = cam.transform.position + cam.transform.forward * 1.4f;
        fallbackPosition += Vector3.up * 0.08f;
        modelFramer.PlaceAt(new Pose(fallbackPosition, Quaternion.identity), cam);
    }

    bool TryGetPlanePose(Vector2 screenPosition, out Pose pose)
    {
        if (arRaycastManager != null && arRaycastManager.Raycast(
                screenPosition, planeHits, TrackableType.PlaneWithinPolygon) && planeHits.Count > 0)
        {
            pose = planeHits[0].pose;
            return true;
        }

        pose = default;
        return false;
    }

    static bool IsOverUI()
    {
        var es = EventSystem.current;
        if (es == null) return false;

        var touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.isPressed)
            return es.IsPointerOverGameObject(touch.primaryTouch.touchId.ReadValue());

        return es.IsPointerOverGameObject();
    }
}
