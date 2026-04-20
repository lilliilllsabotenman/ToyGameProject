using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(menuName = "paramData")]
public class AnimationParametorDataBase : ScriptableObject
{
    public List<pData> data = new();
}
