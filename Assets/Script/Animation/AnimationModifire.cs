using UnityEngine;
using System.Collections.Generic;
using System;

public enum AnimationModifireType
{
    speed,
    Crouch
}

public interface IAnimationModifire
{
    public float GetParametor();
}

public class FloatWrapper
{
    public float value;
}

public class AnimationModifire
{
    public Dictionary<AnimationModifireType, FloatWrapper> FloatModifire = new();   
    
    public void AddModifire(AnimationModifireType type, FloatWrapper value)
    {
        FloatModifire[type] = value;
    }

    public void RemoveModifire(AnimationModifireType type)
    {
        FloatModifire.Remove(type);
    }
}