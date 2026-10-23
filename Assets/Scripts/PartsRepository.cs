using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
public class PartData
{
    public string blenderName;
    public string type;
    public string title;
    public string content;
    public string source;
    public string disease;
}

[Serializable]
class PartsResponse
{
    public PartData[] parts;
}

public class PartsRepository : MonoBehaviour
{
    [SerializeField] string baseUrl = "http://localhost:5050";
    [SerializeField] string partsPath = "/api/v1/unity/parts";
    [SerializeField] int timeoutSeconds = 30;

    readonly Dictionary<string, PartData> parts = new();

    public bool IsLoaded { get; private set; }
    public event Action OnLoaded;

    IEnumerator Start()
    {
        using var req = UnityWebRequest.Get(baseUrl + partsPath);
        req.timeout = timeoutSeconds;
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"GET failed: {req.error} ({req.url})");
            yield break;
        }

        var response = JsonUtility.FromJson<PartsResponse>(req.downloadHandler.text);
        if (response == null || response.parts == null)
        {
            Debug.LogError($"Unexpected response from {req.url}");
            yield break;
        }

        foreach (var p in response.parts) parts[p.blenderName] = p;

        IsLoaded = true;
        Debug.Log($"Loaded {parts.Count} parts");
        OnLoaded?.Invoke();
    }

    public bool TryGet(string blenderName, out PartData data) =>
        parts.TryGetValue(blenderName, out data);

    public IEnumerable<string> AllNames() => parts.Keys;
}