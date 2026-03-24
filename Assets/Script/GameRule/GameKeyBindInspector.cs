using UnityEngine;
using System;

[System.Serializable]
public class KeyType
{
    [Header("")]
    public ActionType iAction;
    public KeyCode iKeyCode;
}

[CreateAssetMenu(menuName = "KeyBind")]
public class GameKeyBindInspector : ScriptableObject
{
    [SerializeField]
    private KeyType[] KeyBind;

    private void OnValidate()
    {
        int length = Enum.GetValues(typeof(ActionType)).Length;

        if (KeyBind == null || KeyBind.Length != length)
        {
            Array.Resize(ref KeyBind, length);
        }
    }

    public KeyType[] Get()
    {
        return KeyBind;
    }
}
