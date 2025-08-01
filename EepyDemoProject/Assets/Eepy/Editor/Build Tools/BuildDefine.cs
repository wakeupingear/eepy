using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewSimpleDefine", menuName = "Build/Build Define")]
public class BuildDefine : ScriptableObject
{
    [Tooltip("A list of scripting define symbols (e.g., 'STEAMWORKS_NET').")]
    [SerializeField] private List<string> defines;

    public string GetDefine()
    {
        return string.Join(";", defines);
    }
}