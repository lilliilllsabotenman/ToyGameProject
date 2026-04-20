using System.Collections.Generic;
using System;

namespace StateJudgment
{
    public class StateChangeJudgment
    {
        private Dictionary<MovementState, StateChangeModifire> movementModifireData ;
        private Dictionary<PositioningState, StateChangeModifire> positioningModifireData ;
        private Dictionary<PostureState, StateChangeModifire> postureModifireData;

        private List<StateChangeModifire> stateModifiers;

        public StateChangeJudgment()
        {
            // ① 生成（ここが中枢）
            stateModifiers = new List<StateChangeModifire>
            {
                new UprightStateJudgment(),
                new CrouchStateJudgment(),
                new MoveStateJudgment(),
                new StandStateJudgment(),
                new GroundStateJudgment(),
                new JumpStateJudgment()
            };

            // ② Dictionary初期化
            movementModifireData = new();
            positioningModifireData = new();
            postureModifireData = new();

            // ③ 振り分け
            foreach (var mod in stateModifiers)
            {
                switch (mod.SetType())
                {
                    case MovementState m:
                        movementModifireData[m] = mod;
                        break;

                    case PositioningState p:
                        positioningModifireData[p] = mod;
                        break;

                    case PostureState po:
                        postureModifireData[po] = mod;
                        break;

                    default:
                        throw new Exception($"Unknown state type: {mod.SetType()}");
                }
            }
        }

        public bool GetStateModify<T>(T state, PlayerStateData data) where T : Enum
        {
            switch(state)
            {
                case MovementState m:
                    return movementModifireData[m].StateJudgment(data);
            
                case PositioningState p: 
                    return positioningModifireData[p].StateJudgment(data);
                
                case PostureState po:
                    return postureModifireData[po].StateJudgment(data);
            
                default:
                    throw new Exception("変な遷移要請来たよStateModifyDataBase.cs");
            }
        }
    }
    
}