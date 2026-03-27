using UnityEngine;
using System;
using System.Collections.Generic;

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
    private AnimatonSorting animationSorting;

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
        isItemData = new AbilityManager(removeItem);
        getItemAction = new GetItemAction(isItemData, playerInput);
        groundCollisionLogic = new GroundCollisionLogic(playerStateManager,
                                                        groundLayer);

        wallResolver = new WallCollisionResolver(wallMask);
        playerMoveAction = new PlayerMoveAction(this.GetComponent<Rigidbody>(), 
                                                Camera.main.transform,
                                                gameConstantParametor,
                                                playerStateManager,
                                                wallResolver);

        animationSorting = new AnimatonSorting(animationDataBase);
    }

    private void Update()
    {
        playerInputBuffer.onUpdate();
        
        foreach(OnUpdateAbility Ability in isItemData.UpdateAbilities)
        {   
            //詳細はAbilitymana.cs参照
            Ability.OnUpdate();//AbilityManagerよりそれぞれのライフサイクルごとに分けられたものを実行
        }

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