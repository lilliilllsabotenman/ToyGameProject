using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class pData
{
    public bool useMovement;
    public MovementState move;

    public bool usePositioning;
    public PositioningState posit;

    public bool usePosture;
    public PostureState post;

    public string parametorName;
}   
