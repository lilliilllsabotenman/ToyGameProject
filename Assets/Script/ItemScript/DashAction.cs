using UnityEngine;
using System;

public class DashAction : ItemObjectBehaviour
{
    public MovementState iState;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    public override AbilityItemData GetAbility(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();

        DashComponent ability = new DashComponent(itemType, actionType);

        DashBehaviour behaviour = new DashBehaviour(
            player.transform,                     
            player.GetComponent<Rigidbody>(),     
            pc.gameConstantParametor,                      
            pc.wallResolver                       
        );

        return new AbilityItemData(
            this,
            ability,
            behaviour,  
            itemType
        );
    }
}

public class DashComponent : IAbility
{
    private ItemType itemType;
    private ActionType actionType;

    public DashComponent(ItemType itemType, ActionType actionType)
    {
        this.itemType = itemType;
        this.actionType = actionType;
    }

    public ItemType GetItemType() => itemType;

    public ActionType GetActionType() => actionType;

    public Enum ActionModifyPress() => MovementState.Dash;

    public Enum ActionModifyReleased() => MovementState.Stand;
}

public class DashBehaviour : AbilityBehaviour
{
    private Transform cam;
    private Rigidbody rb;
    private GameConstantParametor gameConstant;
    private WallCollisionResolver wallResolver;

    private int level = 1;

    private float elapsedTime = 0f;   // ★追加
    private bool isDashing = true;    // ★追加

    public DashBehaviour(
        Transform cam,
        Rigidbody rb,
        GameConstantParametor gameConstant,
        WallCollisionResolver wallResolver)
    {
        this.cam = cam;
        this.rb = rb;
        this.gameConstant = gameConstant;
        this.wallResolver = wallResolver;
    }

    public void SetLevel(int level)
    {
        this.level = level;
    }

    public Enum IGetMyAbilityState() => MovementState.Dash;

    public bool IEventDriven() => false;

    public float GetValidityTime() => 0.2f; // ★短時間化

    public void Behaviour()
    {
        if (!isDashing) return;

        elapsedTime += Time.deltaTime;

        ExecuteDash();

        if (elapsedTime >= GetValidityTime())
        {
            Cancel();
        }
    }

    public bool Cancel()
    {
        isDashing = false;

        // 軽く減速（急停止防止）
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x * 0.5f,
            rb.linearVelocity.y,
            rb.linearVelocity.z * 0.5f
        );

        return true;
    }

    private void ExecuteDash()
    {
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;

        float dashSpeed = gameConstant.GetMoveSpeed() * GetDashMultiplier();

        Vector3 velocity = new Vector3(
            camForward.x * dashSpeed,
            rb.linearVelocity.y,
            camForward.z * dashSpeed
        );

        velocity = wallResolver.Resolve(velocity);

        rb.linearVelocity = velocity;
    }

    private float GetDashMultiplier()
    {
        return level switch
        {
            1 => 2.0f,
            2 => 2.5f,
            3 => 3.0f,
            _ => 2.0f
        };
    }
}