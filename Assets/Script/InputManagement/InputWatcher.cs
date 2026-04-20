using UnityEngine;
using System.Collections.Generic;
using System;

public class InputWatcher
{
    private readonly PlayerInputIntent intent;
    private readonly InputResolver resolver;


    public InputWatcher(
            PlayerInputIntent intent,
            InputResolver resolver)
    {
        this.intent = intent;
        this.resolver = resolver;
    }

    // ===== 実行 =====

    public void onUpdate()
    {
        foreach (ActionType action in Enum.GetValues(typeof(ActionType)))//抽象化したからできる荒業
        {
            if (intent.IsPressed(action))
            {
                resolver.OnPressed(action);
            }

            if (intent.IsReleased(action))
            {
                resolver.OnReleased(action);
            }

            if (intent.IsHeld(action))
            {
                resolver.OnHeld(action);
            }
        }       
    }
}

public class InputResolver
{
    private readonly PlayerStateManager stateManager;

    private readonly Dictionary<ActionType, Action> CommandAction = new();
    private readonly Dictionary<ActionType, IAbility> AbilityActionPress = new();
    private readonly Dictionary<ActionType, IAbility> AbilityActionRelease = new();
    private readonly Dictionary<ActionType, IAbility> AbilityActionHeld = new();

    public InputResolver(PlayerStateManager stateManager)
    {
        this.stateManager = stateManager;
    }

    // ===== 実行 =====

    public void OnPressed(ActionType type)
    {
        if (AbilityActionPress.TryGetValue(type, out var ability))
        {
            stateManager.TryChangeState(ability.ActionModifyPress());
        }
    }

    public void OnReleased(ActionType type)
    {
        if (AbilityActionRelease.TryGetValue(type, out var ability))
        {
            stateManager.TryChangeState(ability.ActionModifyReleased());
        }
    }

    public void OnHeld(ActionType type)
    {
        if (AbilityActionHeld.TryGetValue(type, out var ability))
        {
            // ability.Execute();
        }
    }

    // ===== 登録 =====

    public void BindPressed(ActionType type, IAbility ability)
    {
        AbilityActionPress[type] = ability;
    }

    public void BindReleased(ActionType type, IAbility ability)
    {
        AbilityActionRelease[type] = ability;
    }

    public void BindHeld(ActionType type, IAbility ability)
    {
        AbilityActionHeld[type] = ability;
    }

    // ===== 解除（個別） =====

    public void UnbindPressed(ActionType type)
    {
        AbilityActionPress.Remove(type);
    }

    public void UnbindReleased(ActionType type)
    {
        AbilityActionRelease.Remove(type);
    }

    public void UnbindHeld(ActionType type)
    {
        AbilityActionHeld.Remove(type);
    }

    // ===== 解除（まとめ） =====

    public void Clear(ActionType type)
    {
        AbilityActionPress.Remove(type);
        AbilityActionRelease.Remove(type);
        AbilityActionHeld.Remove(type);
    }

    public void ClearAll()
    {
        AbilityActionPress.Clear();
        AbilityActionRelease.Clear();
        AbilityActionHeld.Clear();
    }
}