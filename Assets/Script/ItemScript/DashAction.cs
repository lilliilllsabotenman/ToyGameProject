using UnityEngine;

public class DashAction : ItemObjectBehaviour
{
    public ItemType iType;

    private void Awake()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    public override AbilityItemSlot GetAbility(GameObject player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        DashComponent dashComponent = new DashComponent(playerController.playerStateManager,
                                                        playerController.playerInput,
                                                        iAction);

        AbilityItemSlot ability = new AbilityItemSlot(
            this,
            dashComponent,
            this.iType
        );

        return ability;
    }
}

public class DashComponent : OnFixedUpdateAbility, IAbility
{
    private AbilitySituation isSituation = AbilitySituation.Pasiv;

    private PlayerStateManager playerState;
    private PlayerInputIntent playerInput;
    private ActionType iAction;

    private DashStateJudgment dashStateJudgment = new DashStateJudgment();

    private int level = 1;

    public DashComponent(PlayerStateManager playerState,
                         PlayerInputIntent playerInput,
                         ActionType iAction)
    {
        this.playerState = playerState;
        this.playerInput = playerInput;
        this.iAction = iAction;
    }

    public void SetLevel(int level)//レベル２以降の挙動について仕様を決めましょう。
    {
        this.level = level;
    }

    public void SetActive()
    {
        isSituation = AbilitySituation.Active;
    }

    public void OnFixedUpdate()
    {   
        if (isSituation == AbilitySituation.Pasiv) return;

        if(playerInput.IsPressed(iAction))
        {
            playerState.TryMovementStateChange(MovementState.Dash, dashStateJudgment);
        }
    }
}

public class DashStateJudgment : IStateJudge
{
    public bool StateJudgment(PlayerStateData state)
    {
        if(state.positioningState != PositioningState.Ground) return false;

        else return true;
    }
}