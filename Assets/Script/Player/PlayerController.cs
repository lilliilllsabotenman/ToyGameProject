using UnityEngine;
using System;
using System.Collections.Generic;

#region PlayerController
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
    [Header("アニメーションデータベース")]
    [SerializeField] private AnimationDataBase animationDataBase;
    [Header("Animator")]
    [SerializeField] private Animator animator;
    

    [Header("壁の張り付き防止用レイヤーマスク")]
    [SerializeField] private LayerMask wallMask;

    //↑の地面着地判定に使うレイヤーマスク
    [Header("地面レイヤー")]
    [SerializeField] private LayerMask groundLayer;

    //多くが参照する基礎パラメーター、プレイヤーごとに独立
    public GameConstant Constant;
    public ConstansModify constansModify = new ConstansModify();
    public GameConstantParametor gameConstantParametor;

    public AbilityManager isItemData;
    private ItemRemover removeItem;
    private InputWatcher inputWatcher;

    //プレイヤーの状態を制御するクラス Script/GameOption/Gamerule.cs参照
    public PlayerStateData playerStateData = new PlayerStateData();
    public PlayerStateManager playerStateManager; // プレイヤーの状態制御
    public StateWatcher stateWatcher; //状態変化を抽象化して通知

    //プレイヤーのデフォルト動作を規定したクラス、Script/Player/PlayerDefaultAction.cs参照
    private PlayerMoveAction playerMoveAction; 
    private GetItemAction getItemAction;
    private GroundCollisionLogic groundCollisionLogic;
    private WallCollisionResolver wallResolver;

    //キーバインド系のクラス全般　Script.GameOption.GameKyeBind.cs参照
    public PlayerInputIntent playerInput;//最終的な入力を抽象化して渡すクラス
    private PlayerKeyBindData keyBindData;
    private PlayerInputInterpreter playerInputInterpreter;
    private PlayerInputBuffer playerInputBuffer;      

    //アニメーション系
    private AnimationSystem animationSystem;

    private DefaultKeyBindData defaultData = new DefaultKeyBindData();

    private void Awake()
    {
        //状態初期化
        playerStateManager = new PlayerStateManager(playerStateData);//状態本体。
        stateWatcher = new StateWatcher(playerStateManager);//状態を見張る人

        //キーバインド系の初期化全般
        keyBindData = new PlayerKeyBindData(defaultData.DefaultSettings(keyBind.Get()));
        playerInputInterpreter = new PlayerInputInterpreter(keyBindData);
        playerInputBuffer = new PlayerInputBuffer(playerInputInterpreter);
        playerInput = new PlayerInputIntent(playerInputBuffer);

        gameConstantParametor = new GameConstantParametor(
            Constant,
            constansModify,
            playerStateManager
        );

        //プレイヤーアクション初期化
        
        removeItem = new ItemRemover(this.gameObject.transform);
        inputWatcher = new InputWatcher(playerInput);
        isItemData = new AbilityManager(
            removeItem,
            inputWatcher);
        getItemAction = new GetItemAction(isItemData, playerInput);
        groundCollisionLogic = new GroundCollisionLogic(playerStateManager,
                                                        groundLayer);

        wallResolver = new WallCollisionResolver(wallMask);
        playerMoveAction = new PlayerMoveAction(this.GetComponent<Rigidbody>(), 
                                                Camera.main.transform,
                                                gameConstantParametor,
                                                playerStateManager,
                                                wallResolver);

        animationSystem = new AnimationSystem(animationDataBase, animator);

        animationSystem.Bind(stateWatcher);
    }

    private void Update()
    {
        playerInputBuffer.onUpdate();
        getItemAction.getAction(this.gameObject, screenCenterDetector);
    }

    private void FixedUpdate()
    {
        foreach(OnFixedUpdateAbility Ability in isItemData.FixedUpdateAbilities)
        {
            //詳細はAbilitymana.cs参照
            Ability.OnFixedUpdate();//AbilityManagerよりそれぞれのライフサイクルごとに分けられたものを実行
        }

        playerMoveAction.PlayerMoving();
    }

    private void iEvent()
    {
        foreach(OnEventAbility Ability in isItemData.EventAbilities)
        {
            //詳細はAbilitymana.cs参照
            Ability.OnEvent();//AbilityManagerよりそれぞれのライフサイクルごとに分けられたものを実行
        }
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
#endregion


#region InputWatcher
public class InputWatcher
{
    private readonly PlayerInputIntent intent;

    private readonly Dictionary<ActionType, Action> onPressed = new();
    private readonly Dictionary<ActionType, Action> onReleased = new();
    private readonly Dictionary<ActionType, Action> onHeld = new();

    public InputWatcher(PlayerInputIntent intent)
    {
        this.intent = intent;
    }

    // ===== 登録 =====

    public void BindPressed(ActionType type, Action action)//押したら
    {
        if (onPressed.ContainsKey(type))
            onPressed[type] += action;
        else
            onPressed[type] = action;
    }

    public void BindReleased(ActionType type, Action action)//離したら
    {
        if (onReleased.ContainsKey(type))
            onReleased[type] += action;
        else
            onReleased[type] = action;
    }

    public void BindHeld(ActionType type, Action action)//押し続けたら
    {
        if (onHeld.ContainsKey(type))
            onHeld[type] += action;
        else
            onHeld[type] = action;
    }

    // ===== 解除（個別） =====

    public void UnbindPressed(ActionType type, Action action)
    {
        if (onPressed.TryGetValue(type, out var del))
        {
            del -= action;
            if (del == null) onPressed.Remove(type);
            else onPressed[type] = del;
        }
    }

    public void UnbindReleased(ActionType type, Action action)
    {
        if (onReleased.TryGetValue(type, out var del))
        {
            del -= action;
            if (del == null) onReleased.Remove(type);
            else onReleased[type] = del;
        }
    }

    public void UnbindHeld(ActionType type, Action action)
    {
        if (onHeld.TryGetValue(type, out var del))
        {
            del -= action;
            if (del == null) onHeld.Remove(type);
            else onHeld[type] = del;
        }
    }

    // ===== 全解除 =====

    public void Clear(ActionType type)
    {
        onPressed.Remove(type);
        onReleased.Remove(type);
        onHeld.Remove(type);
    }

    public void ClearAll()
    {
        onPressed.Clear();
        onReleased.Clear();
        onHeld.Clear();
    }

    // ===== 実行 =====

    public void Update()
    {
        foreach (var pair in onPressed)
        {
            if (intent.IsPressed(pair.Key))
                pair.Value?.Invoke();
        }

        foreach (var pair in onReleased)
        {
            if (intent.IsReleased(pair.Key))
                pair.Value?.Invoke();
        }

        foreach (var pair in onHeld)
        {
            if (intent.IsHeld(pair.Key))
                pair.Value?.Invoke();
        }
    }
}
#endregion