using UnityEngine;

/// <summary>
/// プレイヤーに関する動作の実行を管轄する基幹クラス
/// できればこいつ以外にMonoBehaviourはあまり生やしたくない
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("アイテムを視線で判定するための設定諸々")]
    [SerializeField] private ScreenCenterDetector screenCenterDetector;

    [Header("デフォルトキー設定をScriptableObjectにて設定")]
    [SerializeField] private GameKeyBindInspector keyBind;

    [Header("アニメーション関連設定")]
    [Header("Animator")]
    [SerializeField] private Animator animator;
    
    [Header("壁の張り付き防止用レイヤーマスク")]
    [SerializeField] private LayerMask wallMask;

    //↑の地面着地判定に使うレイヤーマスク
    [Header("地面レイヤー")]
    [SerializeField] private LayerMask groundLayer;

    //Modifire系
    // private VelocityUtil velocityUtil;

    //多くが参照する基礎パラメーター、プレイヤーごとに独立、存在意義を疑い中
    public GameConstant Constant;
    public ConstansModify constansModify = new ConstansModify();
    public GameConstantParametor gameConstantParametor;

    public AbilityManager abilityManager;
    private AbilityDataBase abilityDataBase = new();
    private ItemRemover removeItem;
    private BehaviourExecutor behaviourExecutor = new();

    //プレイヤーの状態を制御するクラス Script/GameOption/Gamerule.cs参照
    public PlayerStateData playerStateData = new PlayerStateData();
    public PlayerStateManager playerStateManager; // プレイヤーの状態制御
    public StateWatcher stateWatcher; //状態変化を抽象化して通知

    //プレイヤーのデフォルト動作を規定したクラス、Script/Player/PlayerDefaultAction.cs参照
    private PlayerMoveAction playerMoveAction; 
    private GetItemAction getItemAction;
    private GroundCollisionLogic groundCollisionLogic;
    public WallCollisionResolver wallResolver;

    //キーバインド系のクラス全般　Script.GameOption.GameKyeBind.cs参照
    public PlayerInputIntent playerInput;//最終的な入力を抽象化して渡すクラス
    private PlayerKeyBindData keyBindData;
    private PlayerInputInterpreter playerInputInterpreter;
    private PlayerInputBuffer playerInputBuffer;      

    private InputWatcher inputWatcher;
    private InputResolver resolver;


    private DefaultKeyBindData defaultData = new DefaultKeyBindData();

    private void Awake()
    {
        Transform tf = this.transform;
        //状態初期化
        playerStateManager = new PlayerStateManager(playerStateData);//状態本体。
        stateWatcher = new StateWatcher(playerStateManager);//状態を見張る人

        //キーバインド系の初期化全般
        keyBindData = new PlayerKeyBindData(defaultData.DefaultSettings(keyBind.Get()));
        playerInputInterpreter = new PlayerInputInterpreter(keyBindData);
        playerInputBuffer = new PlayerInputBuffer(playerInputInterpreter);
        playerInput = new PlayerInputIntent(playerInputBuffer);
        resolver = new InputResolver(playerStateManager);
        inputWatcher = new InputWatcher(
            playerInput,
            resolver);

        //プレイヤーアクション初期化
        abilityManager = new AbilityManager(
            abilityDataBase,
            behaviourExecutor,
            tf,
            stateWatcher,
            resolver);
        getItemAction = new GetItemAction(abilityManager, playerInput);
        groundCollisionLogic = new GroundCollisionLogic(playerStateManager,
                                                        groundLayer);

        gameConstantParametor = new GameConstantParametor(
            Constant,
            constansModify,
            playerStateManager
        );

        wallResolver = new WallCollisionResolver(wallMask);

        Rigidbody rb = this.GetComponent<Rigidbody>();
        Camera mainCam = Camera.main;
        
        playerMoveAction = new PlayerMoveAction(rb,
                                                mainCam.transform,
                                                gameConstantParametor,
                                                playerStateManager,
                                                wallResolver);
    }

    private void Update()
    {
        playerInputBuffer.onUpdate();
        inputWatcher.onUpdate();
        getItemAction.getAction(this.gameObject, screenCenterDetector);
        behaviourExecutor.OnUpdate();
    }

    private void FixedUpdate()
    {
        if (playerMoveAction == null) return;
        playerMoveAction.PlayerMoving();
    }

    private void iEvent()
    {
    }

    private void OnCollisionEnter (Collision collision)
    {
    } 

    private void OnCollisionStay (Collision collision)
    {
        wallResolver.RegisterCollision(collision);
    }

    private void OnCollisionExit(Collision collision)
    {
        wallResolver.Clear();
    }
}
