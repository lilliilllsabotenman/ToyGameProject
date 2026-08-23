using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NodeRunner))]
public class OpenEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // デフォルトのInspector表示（通常のフィールド一覧）を描画
        DrawDefaultInspector();

        // ボタンを1つ追加。押された瞬間だけtrueが返る
        if (GUILayout.Button("Editing"))
        {
            // targetは今このInspectorで表示中のオブジェクト
            NodeRunner component = (NodeRunner)target;

            GameObject Owner = component.gameObject;

            // 変更フラグを立てる。これがないとUnityが変更を検知しない
            EditorUtility.SetDirty(component);

            // シーン上のオブジェクトならシーンの保存も必要
            EditorUtility.SetDirty(component.gameObject);

            // アセット（ScriptableObjectなど）の場合はこちらでディスクに書き出す
            AssetDatabase.SaveAssets();

            Animator animator = Owner.GetComponent<Animator>();
            EditorInitialized(animator);
        }
    }

    public void EditorInitialized(Animator animator)
    {
        VisualScriptingEditorWindow window = VisualScriptingEditorWindow.Open();
        window.Initialize(animator);
    }
}

