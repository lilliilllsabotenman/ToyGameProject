using UnityEngine;
using System;

public enum AbilitySituation
{
    Active,
    Pasiv
}

public interface OnPlayerAction
{
    ActionType ActionType { get; }

    // 「Abilityを直接実行しない」こと
    // ここで返すのは Modifier適用など
    Action GetAction();
}

public interface OnFixedUpdateAbility
{
    public void OnFixedUpdate();
}

public interface OnEventAbility
{
    public void OnEvent();
}

public interface IAbility
{ 
    public void SetActive();
    public void SetLevel(int level);
}

public interface IInteract
{
    public void Init(); 
    public void Interract();
}