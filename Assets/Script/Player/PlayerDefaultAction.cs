using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveStateJudgment : IStateJudge
{
    public bool StateJudgment(PlayerStateData data)
    {
        if(data.movementState == MovementState.Dash) return false;

        else return true;
    }
}

public class StandStateJudgment : IStateJudge
{
    public bool StateJudgment(PlayerStateData data)
    {
        if(data.positioningState != PositioningState.Ground) return false;

        else return true;
    }
}

public class WallCollisionResolver
{
    private readonly List<Vector3> contactNormals = new List<Vector3>();
    private LayerMask wallMask;

    public WallCollisionResolver(LayerMask wallMask)
    {
        this.wallMask = wallMask;    
    }

    public void RegisterCollision(Collision collision)
    {
        // レイヤー判定
        if ((wallMask.value & (1 << collision.gameObject.layer)) == 0)
        return;

        foreach (var c in collision.contacts)
        {
            contactNormals.Add(c.normal);
        }
    }

    public void Clear()
    {
        contactNormals.Clear();
    }

    public Vector3 Resolve(Vector3 velocity)
    {
        for (int i = 0; i < contactNormals.Count; i++)
        {
            Vector3 n = contactNormals[i];

            // 床除外
            if (n.y > 0.5f) continue;

            float dot = Vector3.Dot(velocity, n);

            // 壁に向かっている成分だけ削除
            if (dot < 0f)
            {
                velocity -= n * dot;
            }
        }

        return velocity;
    }
}

public class PlayerMoveAction
{
    private GameConstantParametor gameConstant;
    private PlayerStateManager playerStateManager;

    private DefaultMoveBehaviour defaultMoveBehaviour;
    private DashMoveBehaviour dashMoveBehaviour;

    private WallCollisionResolver wallResolver;

    private MoveStateJudgment moveStateJudgment = new MoveStateJudgment();
    private StandStateJudgment StandStateJudgment = new StandStateJudgment();

    public PlayerMoveAction(
        Rigidbody rb,
        Transform cam,
        GameConstantParametor gameConstant,
        PlayerStateManager playerStateManager,
        WallCollisionResolver wallResolver)
    {
        this.gameConstant = gameConstant;
        this.playerStateManager = playerStateManager;

        this.wallResolver = wallResolver;

        defaultMoveBehaviour = new DefaultMoveBehaviour(cam, rb, gameConstant, wallResolver);
        dashMoveBehaviour = new DashMoveBehaviour(cam, rb, gameConstant);
    }

    public void RegisterCollision(Collision collision)
    {
        wallResolver.RegisterCollision(collision);
    }

    public void ClearCollision()
    {
        wallResolver.Clear();
    }

    public void PlayerMoving()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        if(Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0)
            playerStateManager.TryMovementStateChange(MovementState.Move, moveStateJudgment);
        else
            playerStateManager.TryMovementStateChange(MovementState.Stand, StandStateJudgment);

        if(playerStateManager.stateData.movementState == MovementState.Move)
            defaultMoveBehaviour.DefaultMove(h, v);

        if(playerStateManager.stateData.movementState == MovementState.Dash)
            dashMoveBehaviour.DashMove(h, v);
    }
}

public class DefaultMoveBehaviour
{
    private Transform cam;
    private Rigidbody rb;
    private GameConstantParametor gameConstant;

    private WallCollisionResolver wallResolver;

    public DefaultMoveBehaviour(
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

    public void DefaultMove(float h, float v)
    {
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight   = Vector3.Scale(cam.right,   new Vector3(1, 0, 1)).normalized;

        Vector3 moveDir = camForward * v + camRight * h;
        moveDir = Vector3.ClampMagnitude(moveDir, 1f);

        Vector3 velocity = new Vector3(
            moveDir.x * gameConstant.GetMoveSpeed(),
            rb.linearVelocity.y,
            moveDir.z * gameConstant.GetMoveSpeed()
        );

        // 壁補正
        velocity = wallResolver.Resolve(velocity);

        rb.linearVelocity = velocity;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            Vector3 lookDir = new Vector3(moveDir.x, 0f, moveDir.z);
            Quaternion targetRot = Quaternion.LookRotation(lookDir);

            rb.transform.rotation =
                Quaternion.Slerp(rb.transform.rotation, targetRot, 10f * Time.deltaTime);
        }
    }
}

public class DashMoveBehaviour
{
    private Transform cam;
    private Rigidbody rb;
    private GameConstantParametor gameConstant;

    public DashMoveBehaviour(Transform cam, Rigidbody rb, GameConstantParametor gameConstant)
    {
        this.cam = cam;
        this.rb = rb;
        this.gameConstant = gameConstant;
    }

    public void DashMove(float h, float v)
    {
        // カメラの forward/right をXZ平面に投影
        Vector3 camForward = Vector3.Scale(cam.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 camRight   = Vector3.Scale(cam.right,   new Vector3(1, 0, 1)).normalized;

        // 入力をカメラ基準に変換
        Vector3 moveDir = camForward * v + camRight * h;

        // 斜め移動の速度調整
        moveDir = Vector3.ClampMagnitude(moveDir, 1f);

        // 現在のY速度は維持
        Vector3 velocity = new Vector3(
            moveDir.x * gameConstant.GetMoveSpeed(),
            rb.linearVelocity.y,
            moveDir.z * gameConstant.GetMoveSpeed()
        );

        rb.linearVelocity = velocity;

        // ★ カメラ方向にプレイヤーを回転（Y軸のみ）
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Vector3 lookDir = new Vector3(moveDir.x, 0f, moveDir.z);
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            rb.transform.rotation = Quaternion.Slerp(rb.transform.rotation, targetRot, 10f * Time.deltaTime);
        }
    }
}

public class GetItemAction
{
    private AbilityManager abilityManager;
    private PlayerInputIntent playerInput;

    public GetItemAction(AbilityManager abilityManager, 
                         PlayerInputIntent playerInput)
    {
        this.abilityManager = abilityManager;
        this.playerInput = playerInput;
    }
    
    public void getAction(GameObject PlayerObject, ScreenCenterDetector scDetector)
    {
        GameObject obj = scDetector.GetTarget(Camera.main);
        if(obj == null) return;
        if(playerInput.IsPressed(ActionType.GetAction))
        {
            ItemObjectBehaviour item = obj.GetComponent<ItemObjectBehaviour>() ?? obj.GetComponentInParent<ItemObjectBehaviour>();
            if(item == null) return;
            
            AbilityItemSlot iAbility = item.GetAbility(PlayerObject);

            if(abilityManager.TryAddAbility(iAbility)) 
            {
                item.SuccessJoinItem(); 
            }
        }
    }
}

public class VelocityUtil
{
    public FloatWrapper speed = new();
    public Rigidbody rigidBody;
    public AnimationModifireType type = AnimationModifireType.speed;

    public VelocityUtil(
            Rigidbody rigidBody,
            AnimationModifire animationModifire)
    {
        this.rigidBody = rigidBody;
        animationModifire.AddModifire(type, speed);
    }

    public void MoveDriver(float maxSpeed)
    {
        Vector3 v = rigidBody.linearVelocity;
        v.y = 0f;

        float speed = v.magnitude;

        // デッドゾーン
        if (speed < 0.01f)
            speed = 0f;

        float normalized = Mathf.Clamp01(speed / maxSpeed);

        // スムージング
        this.speed.value = Mathf.Lerp(this.speed.value, normalized, Time.deltaTime * 10f);
    }
}