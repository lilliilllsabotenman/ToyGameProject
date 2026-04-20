using UnityEngine;
using System;

public class PlayerJumpAction : ItemObjectBehaviour
{
    [SerializeField] protected PositioningState iState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override AbilityItemData GetAbility(GameObject player)
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        PlayerController controller = player.GetComponent<PlayerController>();

        JumpComponent ability = new JumpComponent(itemType, actionType);

        JumpBehaviour behaviour = new JumpBehaviour(
            playerRb,
            controller.gameConstantParametor,
            iState
        );

        AbilityItemData abilityData = new AbilityItemData(
            this,
            ability,
            behaviour,
            itemType
        );

        return abilityData;
    }
}

public class JumpComponent : IAbility
{
    private ItemType itemType;
    private ActionType actionType;

    public JumpComponent(ItemType itemType, ActionType actionType)
    {
        this.itemType = itemType;
        this.actionType = actionType;
    }

    public ItemType GetItemType()
    {
        return itemType;
    }

    public ActionType GetActionType()
    {
        return actionType;
    }

    public Enum ActionModifyPress()
    {
        return PositioningState.Jump;
    }

    public Enum ActionModifyReleased()
    {
        return PositioningState.Jump;//これカス、何とかしないといけない。
    }
}

public class JumpBehaviour : AbilityBehaviour
{
    private Rigidbody rb;
    private GameConstantParametor gameConstant;
    private PositioningState iState;

    private int level = 1;

    public JumpBehaviour(
        Rigidbody rb,
        GameConstantParametor gameConstant,
        PositioningState state)
    {
        this.rb = rb;
        this.gameConstant = gameConstant;
        this.iState = state;
    }

    public void SetLevel(int level)
    {
        this.level = level;
    }

    public Enum IGetMyAbilityState()
    {
        return iState;
    }

    public bool IEventDriven()
    {
        return true; // ★瞬間処理
    }

    public float GetValidityTime()
    {
        return 0f;
    }

    public void Behaviour()
    {
        switch (level)
        {
            case 1:
                JumpAction_lv1();
                break;

            case 2:
                JumpAction_lv2();
                break;

            case 3:
                JumpAction_lv3();
                break;

            default:
                JumpAction_lv1();
                break;
        }
    }

    public bool Cancel()
    {
        // ジャンプはキャンセル不要
        return false;
    }

    private void JumpAction_lv1()
    {
        if (rb == null) return;
    

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            gameConstant.GetJumpForce(),
            rb.linearVelocity.z
        );
    }

    private void JumpAction_lv2()
    {
        JumpAction_lv1();
    }

    private void JumpAction_lv3()
    {
        JumpAction_lv1();
    }
}