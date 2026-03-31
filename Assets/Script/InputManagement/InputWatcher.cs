using UnityEngine;
using System.Collections.Generic;
using System;

public class InputWatcher
{
    private readonly PlayerInputIntent intent;

    private readonly Dictionary<ActionType, IAbility> onPressed = new();
    private readonly Dictionary<ActionType, IAbility> onReleased = new();
    private readonly Dictionary<ActionType, IAbility> onHeld = new();

    public InputWatcher(PlayerInputIntent intent)
    {
        this.intent = intent;
    }

    // ===== 登録 =====

    public void BindPressed(ActionType type, IAbility ability)//押したら
    {
        if (onPressed.ContainsKey(type))
            onPressed[type] = ability;
        else
            onPressed[type] = ability;
    }

    public void BindReleased(ActionType type, IAbility ability)//離したら
    {
        if (onReleased.ContainsKey(type))
            onReleased[type] = ability;
        else
            onReleased[type] = ability;
    }

    public void BindHeld(ActionType type, IAbility ability)//押し続けたら
    {
        if (onHeld.ContainsKey(type))
            onHeld[type] = ability;
        else
            onHeld[type] = ability;
    }

    // ===== 解除（個別） =====

    public void UnbindPressed(ActionType type)
    {
        onPressed.Remove(type);
    }

    public void UnbindReleased(ActionType type)
    {
        onReleased.Remove(type);
    }

    public void UnbindHeld(ActionType type)
    {
        onHeld.Remove(type);
    }

    // ===== 全解除 =====

    public void Clear(ActionType type)
    {
        onPressed.Remove(type);
        onReleased.Remove(type);
        onHeld.Remove(type);
    }

    public void ClearAll()
    {
        onPressed.Clear();
        onReleased.Clear();
        onHeld.Clear();
    }

    // ===== 実行 =====

    public void onUpdate()
    {
        foreach (var pair in onPressed)
        {
            if (intent.IsPressed(pair.Key))
                pair.Value.ActionModifyPress();
        }

        foreach (var pair in onReleased)
        {
            if (intent.IsReleased(pair.Key))
                pair.Value?.ActionModifyReleased();
        }

        foreach (var pair in onHeld)
        {
            // if (intent.IsHeld(pair.Key))
            //     pair.Value?.Invoke();
        }
    }
}