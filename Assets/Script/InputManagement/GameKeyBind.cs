using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public enum ActionType
{
    GetAction,
    JumpAction,
    DashAction,
    CrouchAction
}

public class KeyBindingEntry
{
    public ActionType action;
    public KeyCode key;
}

public class KeyBindingSaveData //この世のゴミ、あとでリファクタして消す
{
    public string PlayerName;
    public List<KeyBindingEntry> bindings = new List<KeyBindingEntry>();
}

///<summary>
/// キーバインドをJsonで保持・読み書きするクラス
/// ・Saveにキー設定の入ったDictionaryを入れるとJsonに保存される
/// ・Load保存されてるキー設定を読みだしてDectionaryに変換して返す
/// </summary>
public class GameKeyBindSaving
{
    public void Save(Dictionary<ActionType, KeyCode> dict)
    {
        var data = CreateSaveData(dict);

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, "keybind.json");

        File.WriteAllText(path, json);
        Debug.Log("Saved to: " + path);
    }

    public Dictionary<ActionType, KeyCode> Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "keybind.json");

        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        KeyBindingSaveData data = JsonUtility.FromJson<KeyBindingSaveData>(json);

        Dictionary<ActionType, KeyCode> dict = new();

        foreach (var entry in data.bindings)
            dict[entry.action] = entry.key;

        return dict;
    }

    KeyBindingSaveData CreateSaveData(Dictionary<ActionType, KeyCode> dict)
    {
        KeyBindingSaveData data = new KeyBindingSaveData();

        foreach (var pair in dict)
        {
            data.bindings.Add(new KeyBindingEntry
            {
                action = pair.Key,
                key = pair.Value
            });
        }

        return data;
    }
}

public class PlayerKeyBindData
{
    public Dictionary<ActionType, KeyCode> PlayerAction;

    public PlayerKeyBindData(Dictionary<ActionType, KeyCode> PlayerAction)
    {
        this.PlayerAction = PlayerAction;
    }
}
/// <summary>
/// そのフレームで確定した入力状態のスナップショット。
/// Abilityは必ずこのデータのみを参照する。
/// Unity Input を直接読んではいけない。
/// </summary>
public class PlayerInputFrameState
{
    public Dictionary<ActionType, bool> Pressed = new();
    public Dictionary<ActionType, bool> Held = new();
    public Dictionary<ActionType, bool> Released = new();
}

/// <summary>
/// 実際の入力デバイス(Unity Input)を読み取る層。
/// BindingData(ActionType→KeyCode)を参照し、
/// そのフレームの入力状態を生成する。
/// </summary>
public class PlayerInputInterpreter
{
    private PlayerKeyBindData bindData;

    public PlayerInputInterpreter(PlayerKeyBindData bindData)
    {
        this.bindData = bindData;
    }

    /// <summary>
    /// 現在フレームの入力状態を生成する（Snapshot作成）
    /// </summary>
    public PlayerInputFrameState Interpret()
    {
        PlayerInputFrameState state = new PlayerInputFrameState();

        foreach(var pair in bindData.PlayerAction)
        {
            var action = pair.Key;
            var key = pair.Value;

            state.Pressed[action]  = Input.GetKeyDown(key);
            state.Held[action]     = Input.GetKey(key);
            state.Released[action] = Input.GetKeyUp(key);
        }

        return state;
    }
}

/// <summary>
/// フレーム単位で入力状態を保持するバッファ。
/// Ability間で入力認識を一致させるためのキャッシュ層。
/// </summary>
public class PlayerInputBuffer
{
    private PlayerInputInterpreter interpreter;
    private PlayerInputFrameState currentFrameState;

    public PlayerInputBuffer(PlayerInputInterpreter interpreter)
    {
        this.interpreter = interpreter;
        currentFrameState = new PlayerInputFrameState();
    }

    /// <summary>
    /// フレームの最初に必ず呼ぶ。
    /// 入力状態を確定させる。
    /// </summary>
    public void onUpdate()
    {
        currentFrameState = interpreter.Interpret();
    }

    public PlayerInputFrameState GetFrameState()
    {
        return currentFrameState;
    }
}


/// <summary>
/// 入力の意味的解釈を行う層。
/// Abilityはここだけを参照する。
/// 同時押しや長押しなどのルールはここに書く。
/// </summary>
public class PlayerInputIntent
{
    private PlayerInputBuffer buffer;

    public PlayerInputIntent(PlayerInputBuffer buffer)
    {
        this.buffer = buffer;
    }

    public bool IsPressed(ActionType action)
    {
        return buffer.GetFrameState().Pressed[action];
    }

    public bool IsHeld(ActionType action)
    {
        return buffer.GetFrameState().Held[action];
    }

    public bool IsReleased(ActionType action)
    {
        return buffer.GetFrameState().Released[action];
    }
}

public class DefaultKeyBindData//応急処置。とりあえずの操作は可能
{
    public Dictionary<ActionType, KeyCode> DefaultSettings(KeyType[] key)
    {
        Dictionary<ActionType, KeyCode> defaultKeyBind = new Dictionary<ActionType, KeyCode>();

        foreach (KeyType k in key)
        {
            if(defaultKeyBind.ContainsKey(k.iAction)) 
            {
                Debug.LogError("キー設定に重複あり。");
                continue;
            }
    
            defaultKeyBind[k.iAction] = k.iKeyCode;
        }

        return defaultKeyBind;
    }
}