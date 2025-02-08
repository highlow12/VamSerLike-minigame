using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField]
    private bool isEditable = true;

    [SerializeField]
    private List<TKey> keys = new();

    [SerializeField]
    private List<TValue> values = new();

    // save the dictionary to lists
    public void OnBeforeSerialize()
    {
        if (isEditable)
        {
            return;
        }
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }

    // load dictionary from lists
    public void OnAfterDeserialize()
    {
        if (!isEditable)
        {
            return;
        }
        this.Clear();
        for (int i = 0; i < keys.Count; i++)
        {
            try
            {
                if (this.ContainsKey(keys[i]))
                {
                    Debug.LogError($"중복된 키: {keys[i]}\n중복된 키는 무시되며, isEditable 속성의 값을 false로 변경할 경우 자동으로 제거됩니다.");
                    continue;
                }
                this.Add(keys[i] ?? default, values[i] ?? default);
            }
            catch (ArgumentOutOfRangeException)
            {
            }
        }
    }
}