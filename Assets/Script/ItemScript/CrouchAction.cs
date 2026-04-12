using UnityEngine;
using System;

public class CrouchAction : ItemObjectBehaviour
{
    [SerializeField] protected PostureState iState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override AbilityItemData GetAbility(GameObject player)
    {
        BoxCollider playerCollider = player.GetComponent<BoxCollider>();

        var ability = new CrouchComponent(itemType, actionType);

        var behaviour = new CrouchBehaviour(playerCollider, iState);

        return new AbilityItemData(
            this,
            ability,
            behaviour,
            itemType
        );
    }
}

public class CrouchComponent : IAbility
{
    private ItemType itemType;
    private ActionType actionType;

    public CrouchComponent(ItemType itemType, ActionType actionType)
    {
        this.itemType = itemType;
        this.actionType = actionType;
    }

    public ItemType GetItemType() => itemType;

    public ActionType GetActionType() => actionType;

    public Enum ActionModifyPress() => PostureState.Crouch;

    public Enum ActionModifyReleased() => PostureState.Upright;
}

public class CrouchBehaviour : AbilityBehaviour
{
    private BoxCollider col;
    private PostureState iState;

    private Vector3 defaultSize;
    private Vector3 defaultCenter;

    private int level = 1; // ★デフォルト設定

    public CrouchBehaviour(BoxCollider col, PostureState state)
    {
        this.col = col;
        this.iState = state;

        defaultSize = col.size;
        defaultCenter = col.center;
    }

    public void SetLevel(int level)
    {
        this.level = level;
    }

    public Enum IGetMyAbilityState() => iState;

    public bool IEventDriven() => false;

    public float GetValidityTime() => Mathf.Infinity;

    public void Behaviour()
    {
        switch (level)
        {
            case 1:
                CrouchAction_lv1();
                break;

            case 2:
                CrouchAction_lv2();
                break;

            case 3:
                CrouchAction_lv3();
                break;

            default:
                CrouchAction_lv1();
                break;
        }
    }

    public bool Cancel()
    {
        //Todo:キャンセル必要判定の追加
        ReleasedCollider();

        return false;
    }

    private void CrouchAction_lv1()
    {
        float targetHeight = defaultSize.y / 2;

        float newY = Mathf.Abs(col.size.y - targetHeight) >= 0.01f
            ? col.size.y + (targetHeight - col.size.y) / 5f
            : targetHeight;

        ApplyHeight(newY);
    }

    private void CrouchAction_lv2()
    {
        // TODO: 強化版（例：さらに低く or 速く）
        CrouchAction_lv1();
    }

    private void CrouchAction_lv3()
    {
        // TODO: 最大強化
        CrouchAction_lv1();
    }

    private void ReleasedCollider()
    {
        float targetHeight = defaultSize.y;

        float newY = Mathf.Abs(col.size.y - targetHeight) >= 0.01f
            ? col.size.y + (targetHeight - col.size.y) / 5f
            : targetHeight;

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