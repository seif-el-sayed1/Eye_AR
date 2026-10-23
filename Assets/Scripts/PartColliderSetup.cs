using System.Collections.Generic;
using UnityEngine;

public class PartColliderSetup : MonoBehaviour
{
    [SerializeField] PartsRepository repository;
    [SerializeField] Transform modelRoot;

    void Start()
    {
        if (repository.IsLoaded) Setup();
        else repository.OnLoaded += Setup;
    }

    void OnDestroy() => repository.OnLoaded -= Setup;

    void Setup()
    {
        var found = new HashSet<string>();
        int added = 0;

        foreach (var mf in modelRoot.GetComponentsInChildren<MeshFilter>(true))
        {
            if (!repository.TryGet(mf.name, out _)) continue;
            AddCollider(mf.gameObject, mf.sharedMesh, ref added);
            found.Add(mf.name);
        }

        foreach (var smr in modelRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (!repository.TryGet(smr.name, out _)) continue;
            AddCollider(smr.gameObject, smr.sharedMesh, ref added);
            found.Add(smr.name);
        }

        Debug.Log($"Colliders added: {added}");

        foreach (var name in repository.AllNames())
            if (!found.Contains(name)) Debug.LogWarning($"No mesh found in model for: {name}");
    }

    static void AddCollider(GameObject go, Mesh mesh, ref int added)
    {
        if (go.TryGetComponent<MeshCollider>(out _)) return;
        var col = go.AddComponent<MeshCollider>();
        col.sharedMesh = mesh;
        added++;
    }
}