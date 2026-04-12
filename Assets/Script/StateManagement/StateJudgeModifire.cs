using UnityEngine;
using System.Collections.Generic;
using System;

namespace StateJudgment//あとでEnumごとに名前空間分割
{
    public interface StateChangeModifire
    {
        public Enum SetType();
        public bool StateJudgment(PlayerStateData state);

    }

    public class UprightStateJudgment : StateChangeModifire
    {
        public Enum SetType()
        {
            return PostureState.Upright;
        }

        public bool StateJudgment(PlayerStateData state)
        {
            return true;
        }
    }

    public class CrouchStateJudgment : StateChangeModifire
    {
        public Enum SetType()
        {
            return PostureState.Crouch;
        }

        public bool StateJudgment(PlayerStateData state)
        {
            if(state.positioningState != PositioningState.Clip) return true;

            else return false;
        }
    }

    public class MoveStateJudgment : StateChangeModifire
    {
        public Enum SetType()
        {
            return MovementState.Move;
        }

        public bool StateJudgment(PlayerStateData data)
        {
            if(data.movementState == MovementState.Dash) return false;

            else return true;
        }
    }

    public class StandStateJudgment : StateChangeModifire
    {

        public Enum SetType()
        {
            return MovementState.Stand;
        }

        public bool StateJudgment(PlayerStateData data)
        {
            if(data.positioningState != PositioningState.Ground) return false;

            else return true;
        }
    }

    public class GroundStateJudgment : StateChangeModifire
    {   
        public Enum SetType()
        {
            return PositioningState.Ground;
        }

        public bool StateJudgment(PlayerStateData state)
        {
            // 空中状態なら着地を許可
            if(state.positioningState == PositioningState.Jump ||
            state.positioningState == PositioningState.Gliding ||
            state.positioningState != PositioningState.Clip)
                return true;

            return false;
        }
    }

    public class JumpStateJudgment : StateChangeModifire
    {
        public Enum SetType()
        {
            return PositioningState.Jump;
        }

        public bool StateJudgment(PlayerStateData state)
        {
            if(state.positioningState == PositioningState.Ground) return true;

            else return false;
        }
    }

    public class StateChangeJudgment
    {
        public Dictionary<MovementState, StateChangeModifire> movementModifireData {get; private set;}
        public Dictionary<PositioningState, StateChangeModifire> positioningModifireData {get; private set;}
        public Dictionary<PostureState, StateChangeModifire> postureModifireData {get; private set;}

        public StateChangeJudgment()
        {

        }
    }
}
