using UnityEngine;
using System.Collections.Generic;

public class WorldManagement : MonoBehaviour
{
   
}

public enum GameFlag
{
    GetCoin,
    GetKey
}

[System.Serializable]
public class FlagDataBase
{
    [SerializeField] private MonoBehaviour[] monoBehaviour;

    public IInteract[] ValidateConfiguration()
    {
        List<IInteract> iInteract = new List<IInteract>();

        foreach(MonoBehaviour mb in monoBehaviour)
        {
            if(mb is IInteract ii) iInteract.Add(ii);
            else Debug.LogError("this object is not Interact" + mb.gameObject.name);
        }

        return iInteract.ToArray();
    }
}

[System.Serializable]
public class FlagSystem
{
    private FlagDataBase flagData = new();
    private Dictionary<GameFlag, bool> FlagDataBase = new();
    private IInteract[] iInteract;

    public void Init()
    {
        iInteract = flagData.ValidateConfiguration();

        foreach(IInteract intrect in iInteract)
        {
            //通知購読処理
        }
    }

    public void flagChangeCall(GameFlag fglug)//こいつを購読用のDelegateにぶちこむ
    {
        //判断をどこに置くか迷い中
    }
}   

//ステージオブジェクトの実装想定
public class StageObject : MonoBehaviour, IInteract
{
    public GameFlag flag;
    //デリゲート

    public void Init()
    {
        //購読処理
    }

    public void Interract()
    {
        //デリゲート起動(flag);
    }
}