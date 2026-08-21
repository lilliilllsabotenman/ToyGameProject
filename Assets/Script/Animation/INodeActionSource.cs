// Script/Animation/INodeActionSource.cs
using UnityEngine;

// このインターフェースを実装したクラスのメソッドが、
// [VisualScriptingAction]/[VisualScriptingGetter]付きであればノード候補になる。
public interface INodeActionSource
{
    // メンバ変数(Component参照)をGetComponentで解決する対象
    GameObject Owner { get; }
}
