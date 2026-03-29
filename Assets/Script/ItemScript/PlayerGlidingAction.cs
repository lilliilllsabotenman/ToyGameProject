// using UnityEngine;

// public class PlayerGlidingAction : ItemObjectBehaviour
// {
//     public ItemType iType;

//     private void Awake ()
//     {
//         rb = this.GetComponent<Rigidbody>();
//     }

//     public override AbilityItemSlot GetAbility(GameObject player)
//     {
//         PlayerController controller = player.GetComponent<PlayerController>();

//         GlidingComponent glidingComponent = new GlidingComponent(
//             player.GetComponent<Rigidbody>(),
//             controller.playerInput,
//             controller.playerStateManager,
//             iAction
//             );

//         AbilityItemSlot ability = new AbilityItemSlot(
//             this,
//             glidingComponent,
//             this.iType
//         );

//         return ability;
//     }
// }

// public class GlidingComponent : OnFixedUpdateAbility, IAbility
// {
//     private AbilitySituation situation = AbilitySituation.Pasiv;

//     private Rigidbody rb;
//     private ActionType iAction;
//     private PlayerInputIntent inputIntent;
//     private PlayerStateManager playerStateManager;

//     private GlidingBehaviour behaviour;

//     private GlidingStateJudgment glidingStateJudgment = new GlidingStateJudgment();
//     private FloatingStateJudgment floatingStateJudgment = new FloatingStateJudgment();

//     private int level = 1;

//     public GlidingComponent(
//                         Rigidbody rb, 
//                         PlayerInputIntent inputIntent, 
//                         PlayerStateManager playerStateManager,
//                         ActionType iAction)
//     {
//         this.rb = rb;
//         this.inputIntent = inputIntent;
//         this.playerStateManager = playerStateManager;
//         this.iAction = iAction;

//         behaviour = new GlidingBehaviour(rb);
//     }

//     public void SetLevel(int level)
//     {
//         this.level = level;
//     }

//     public void SetActive()
//     {
//         situation = AbilitySituation.Active;
//     }

//     public void OnFixedUpdate()
//     {
//         if (situation == AbilitySituation.Pasiv) return;

//         if (inputIntent.IsPressed(iAction))
//         {
//             playerStateManager.TryPositioningStateChange(PositioningState.Gliding, glidingStateJudgment);
//         }

//         if (inputIntent.IsReleased(iAction))
//         {
//             playerStateManager.TryPositioningStateChange(PositioningState.Floating, floatingStateJudgment);
//         }   

//         if (playerStateManager.stateData.positioningState != PositioningState.Gliding) return;

//         switch (level)
//         {
//             case 1: behaviour.GlideActrion_lv1(); break;

//             case 2: behaviour.GlideActrion_lv2(); break;

//             case 3: behaviour.GlideActrion_lv3(); break;
//         }
//     }
// }

// public class GlidingBehaviour
// {
//     private Rigidbody rb;

//     private bool isGliding;

//     public GlidingBehaviour(Rigidbody rb)
//     {
//         this.rb = rb;
//     }


//     public void GlideActrion_lv1()
//     {

//         Vector3 v = rb.linearVelocity;

//         float targetFallSpeed = -0.8f;
//         float easing = 0.15f;

//         if (v.y < targetFallSpeed)
//         {
//             v.y += (targetFallSpeed - v.y) * easing;
//             rb.linearVelocity = v;
//         }
//     }

//     public void GlideActrion_lv2()
//     {

//     }

//     public void GlideActrion_lv3()
//     {

//     }
// }

// public class GlidingStateJudgment : IStateJudge
// {
//     public bool StateJudgment(PlayerStateData state)
//     {
//         return state.positioningState != PositioningState.Ground;
//     }
// }

// public class FloatingStateJudgment : IStateJudge
// {
//     public bool StateJudgment(PlayerStateData state)
//     {
//         return state.positioningState != PositioningState.Ground;
//     }
// }
