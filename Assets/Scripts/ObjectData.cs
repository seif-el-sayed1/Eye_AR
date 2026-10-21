// ObjectData.cs
using UnityEngine;

[CreateAssetMenu(menuName = "AR/Object Data")]
public class ObjectData : ScriptableObject
{
    public string title;
    [TextArea] public string description;
}