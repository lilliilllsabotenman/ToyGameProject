using UnityEngine;

public class CrouchAction : ItemObjectBehaviour
{
    public ItemType iType;

    private void Awake ()
    {
        rb = this.GetComponent<Rigidbody>();
    }

    public override AbilityItemSlot GetAbility(GameObject player)
    {
        PlayerController playerComtroller = player.GetComponent<PlayerController>();
        Collider playerCollider = playerComtroller.gameObject.GetComponent<Collider>();
        CrouchComponent crouchComponent = new CrouchComponent(        
            playerComtroller.playerInput,
            playerCollider,
            playerComtroller.playerStateManager,
            player.GetComponent<BoxCollider>(),
            iAction
        );

        AbilityItemSlot ability = new AbilityItemSlot(
            this,
            crouchComponent,
            this.iType
        );

        return ability;
    }
}

public class CrouchComponent : OnFixedUpdateAbility, IAbility
{
    private AbilitySituation isSituation = AbilitySituation.Pasiv; 
    
    private ActionType iAction; 
    private PlayerInputIntent inputIntent;
    private Collider playerCollider;
    private PlayerStateManager playerState;
    private CrouchBehaviour crouchBehaviour;

    private int level = 1;
    private float ObjectScale_Y;

    private CorouchStateJudgment CorouchStateJudgment = new CorouchStateJudgment();
    private UprightStateJudgment uprightStateJudgment = new UprightStateJudgment();

    public CrouchComponent( PlayerInputIntent inputIntent, 
                            Collider playerCollider,
                            PlayerStateManager playerState,
                            BoxCollider col,
                            ActionType iAction)
    {
        this.inputIntent = inputIntent;
        this.playerCollider = playerCollider;
        this.playerState = playerState;
        this.iAction = iAction;

        crouchBehaviour = new CrouchBehaviour(col);
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
        if(inputIntent.IsPressed(iAction))//この中でSwitch分けちゃっていいと思う。
        {
            playerState.TryPostureStateChange(PostureState.Crouch, CorouchStateJudgment);
        }
        if(inputIntent.IsReleased(iAction)) 
        {
            playerState.TryPostureStateChange(PostureState.Upright, uprightStateJudgment);
        }

        if(playerState.stateData.postureState == PostureState.Crouch)
        {
            Debug.Log(level);

            switch (level)
            {
                case 1: crouchBehaviour.CrouchAction_lv1(); break;
                case 2: crouchBehaviour.CrouchAction_lv2(); break;
                case 3: crouchBehaviour.CrouchAction_lv3(); break;
            }
        }

        if(playerState.stateData.postureState == PostureState.Upright)
        {
            crouchBehaviour.ReleasedCollider();
        }
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

public class UprightStateJudgment : IStateJudge
{
    public bool StateJudgment(PlayerStateData state)
    {
        return true;
    }
}

public class CorouchStateJudgment : IStateJudge
{
    public bool StateJudgment(PlayerStateData state)
    {
        if(state.positioningState != PositioningState.Clip) return true;

        else return false;
    }
}