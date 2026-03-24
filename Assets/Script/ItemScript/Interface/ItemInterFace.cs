using UnityEngine;

public enum AbilitySituation
{
    Active,
    Pasiv
}

public interface OnUpdateAbility
{
    public void OnUpdate();
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
    public void Interract();
}