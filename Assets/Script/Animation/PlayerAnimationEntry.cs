// using UnityEngine;

// public class PlayerAnimationEntry : MonoBehaviour
// {
//     [SerializeField] private Animator anim;
//     [SerializeField] private GameObject player;

//     private AnimationManager manager;
//     private AnimationContext context;

//     void Start()
//     {
//         if(anim == null) return;
//         if(player = null) return;

//         StateWatcher watcher = player.GetComponent<PlayerController>().stateWatcher;

//         context = new AnimationContext();

//         var executor = new AnimationExecutor(anim);

//         manager = new AnimationManager(executor, context, watcher);
//     }

//     void Update()
//     {
//         manager.Update();
//     }
// }