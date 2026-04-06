using UnityEngine;
using System;

public class CrouchAction : ItemObjectBehaviour
{
    // public ItemType iType;

    // private void Awake ()
    // {
    //     rb = this.GetComponent<Rigidbody>();
    // }

    // public override AbilityItemSlot GetAbility(GameObject player)
    // {
    //     PlayerController playerComtroller = player.GetComponent<PlayerController>();
    //     Collider playerCollider = playerComtroller.gameObject.GetComponent<Collider>();
    //     CrouchComponent crouchComponent = new CrouchComponent(        
    //         playerComtroller.playerInput,
    //         playerCollider,
    //         playerComtroller.playerStateManager,
    //         player.GetComponent<BoxCollider>(),
    //         iAction
    //     );

    //     AbilityItemSlot ability = new AbilityItemSlot(
    //         this,
    //         crouchComponent,
    //         this.iType
    //     );

    //     return ability;
    // }
}

public class CrouchComponent : IAbility
{
    private ActionType actionType;
    private ItemType itemType;

    public ItemType GetItemType()
    {
        return itemType;
    }

    public ActionType GetActionType()
    {
        return this.actionType;
    }

    public Enum ActionModifyPress()
    {
        return PostureState.Crouch;
    }

    public Enum ActionModifyReleased()
    {
        return PostureState.Upright;
    }
}

public class CrouchBehaviour
{
    private BoxCollider col;

    private Vector3 defaultSize;
    private Vector3 defaultCenter;

    public CrouchBehaviour(BoxCollider col)
    {
        this.col = col;
        defaultSize = col.size;
        defaultCenter = col.center;
    }

    public void CrouchAction_lv1()
    {
        float targetHeight = defaultSize.y / 2;

        float newY;

        if (Mathf.Abs(col.size.y - targetHeight) >= 0.01f)
        {
            newY = col.size.y + (targetHeight - col.size.y) / 5f;
        }
        else
        {
            newY = targetHeight;
        }

        ApplyHeight(newY);
    }

    public void CrouchAction_lv2()
    {

    }

    public void CrouchAction_lv3()
    {
        
    }

    public void ReleasedCollider()
    {
        float targetHeight = defaultSize.y;

        float newY;

        if (Mathf.Abs(col.size.y - targetHeight) >= 0.01f)
        {
            newY = col.size.y + (targetHeight - col.size.y) / 5f;
        }
        else
        {
            newY = targetHeight;
        }

        ApplyHeight(newY);
    }

    private void ApplyHeight(float newHeight)
    {
        Vector3 size = col.size;
        size.y = newHeight;
        col.size = size;

        float offset = (defaultSize.y - newHeight) / 2f;

        Vector3 center = defaultCenter;
        center.y = defaultCenter.y - offset;

        col.center = center;
    }
}