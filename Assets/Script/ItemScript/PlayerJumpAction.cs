using UnityEngine;

public class PlayerJumpAction : ItemObjectBehaviour
{
    public ItemType iType;

    private void Awake ()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    public override AbilityItemSlot GetAbility(GameObject player)
    {
        PlayerController playerController = player.GetComponent<PlayerController>();
        JumpComponent jumpComponent = new JumpComponent(
            player.GetComponent<Rigidbody>(),
            playerController.gameConstantParametor,
            playerController.playerInput,
            playerController.playerStateManager,
            iAction
        );

        AbilityItemSlot ability = new AbilityItemSlot(
            this,
            jumpComponent,
            this.iType
        );

        return ability;
    }
}

public class JumpComponent : OnFixedUpdateAbility, IAbility
{
    public AbilitySituation isSituation = AbilitySituation.Pasiv;      

    private ActionType iAction;
    private PlayerInputIntent inputIntent;
    private JumpBehaviour jumpBehaviour;
    private PlayerStateManager playerStateManager;

    private JumpStateJudgment jumpStateJudgment = new JumpStateJudgment();

    private int level = 1;
    private bool camJump;

    public JumpComponent(Rigidbody rb, 
                        GameConstantParametor gameConstant, 
                        PlayerInputIntent inputIntent,
                        PlayerStateManager playerStateManager,
                        ActionType iAction)
    {
        jumpBehaviour = new JumpBehaviour(rb, gameConstant);
        this.inputIntent = inputIntent;
        this.playerStateManager = playerStateManager;
        this.iAction = iAction;
    }

    public void SetLevel(int level)
    {
        this.level = level;
    }

    public void SetActive()
    {                                                                           
        isSituation = AbilitySituation.Active;
    }

    public void OnFixedUpdate()
    {
        if(isSituation == AbilitySituation.Pasiv)return;

        if(inputIntent.IsPressed(iAction))
        {
            if(playerStateManager.TryPositioningStateChange(PositioningState.Jump, jumpStateJudgment))
            {
                switch(level)
                {
                    case 1: jumpBehaviour.JumpAction_lv1(); break;
                    case 2: jumpBehaviour.JumpAction_lv2(); break;
                    case 3: jumpBehaviour.JumpAction_lv3(); break;
                }
            }
        }
    }
}

public class JumpBehaviour
{
    private Rigidbody rb;
    private GameConstantParametor gameConstant;

    public JumpBehaviour(Rigidbody rb, GameConstantParametor gameConstant)
    {
        this.rb = rb;
        this.gameConstant = gameConstant;
    }

    public void JumpAction_lv1()
    {
        if(rb == null) return;
        
        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            gameConstant.GetJumpForce(),
            rb.linearVelocity.z
        );
    }

    public void JumpAction_lv2()
    {
        //LV2処理。
    }

    public void JumpAction_lv3()
    {
        //LV3処理。
    }
}

public class JumpStateJudgment : IStateJudge
{
    public bool StateJudgment(PlayerStateData state)
    {
        if(state.positioningState == PositioningState.Ground) return true;

        else return false;
    }
}