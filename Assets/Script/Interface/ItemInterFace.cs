using UnityEngine;
using System;

public enum AbilitySituation
{
    Active,
    Pasiv
}

public interface OnPlayerAction
{
    public ActionType ActionType { get; }

    // 「Abilityを直接実行しない」こと
    // ここで返すのは Modifier適用など
    public Action GetAction();
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
    public ActionType GetActionType();
    public ItemType GetItemType();
    public Enum ActionModifyPress();
    public Enum ActionModifyReleased();
}

public interface AbilityBehaviour
{
    public void SetLevel(int level);
    public Enum IGetMyAbilityState();
    public bool IEventDriven();
    public float GetValidityTime();
    public void Behaviour();
    public bool Cancel();
}

public interface IInteract
{
    public void Init(); 
    public void Interract();
}

public class ItemObjectBehaviour : MonoBehaviour
{
    protected Rigidbody rb;
    
    [SerializeField] protected ItemType itemType;
    [SerializeField] protected ActionType actionType;


    public virtual AbilityItemData GetAbility(GameObject obj)
    {
        return null;
    }

    public void SuccessJoinItem()
    {
        this.gameObject.SetActive(false);
    }

    public void Restoration(Vector3 pos)
    {
        this.transform.position = new Vector3(pos.x, pos.y + 2, pos.z);
        this.gameObject.SetActive(true);

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            7,
            rb.linearVelocity.z
        );
    }
}
