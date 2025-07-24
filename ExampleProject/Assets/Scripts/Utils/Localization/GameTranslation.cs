using System.Collections.Generic;
using TMPro;
using UnityEngine;

[CreateAssetMenu(fileName = "GameTranslation", menuName = "Scriptable Objects/Translation")]
public class GameTranslation : ScriptableObject
{
    [System.Serializable]
    public struct TranslationEntry
    {
        public string key;
        public string value;
    }

    public string languageCode;
    public string languageName;
    public TMP_FontAsset font;
    public List<TranslationEntry> entries;

    private Dictionary<string, string> _lookup;

    public string Get(string key)
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<string, string>();
            foreach (var entry in entries)
            {
                _lookup[entry.key] = entry.value;
            }
        }

        return _lookup.TryGetValue(key, out string value) ? value : $"[{key}]";
    }
}
