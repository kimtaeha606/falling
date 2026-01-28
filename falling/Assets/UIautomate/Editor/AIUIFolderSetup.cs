#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

public static class AIUIFolderSetup
{
    private const string Root = "Assets";
    private const string FolderName = "AI_Generated";
    private const string FullPath = Root + "/" + FolderName;

    [MenuItem("Tools/Stat UI Kit/Setup AI UI Folder")]
    public static void SetupFolder()
    {
#if !UNITY_EDITOR
        return;
#endif
        // 에디터가 바쁜 상태면 스킵
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            return;

        // 핵심 방어: 이미 있으면 아무 것도 안 함
        if (AssetDatabase.IsValidFolder(FullPath))
            return;

        // 폴더 생성
        string guid = AssetDatabase.CreateFolder(Root, FolderName);
        if (string.IsNullOrEmpty(guid))
        {
            Debug.LogError("[Stat UI Kit] Failed to create folder: " + FullPath);
            return;
        }

        Debug.Log("[Stat UI Kit] Created folder: " + FullPath);

        CreateReadme();
        AssetDatabase.Refresh();
    }

    private static void CreateReadme()
    {
        string readmePath = FullPath + "/README.txt";
        if (File.Exists(readmePath)) return;

        try
        {
            File.WriteAllText(
                readmePath,
                "Put AI-generated UI images here.\n" +
                "Images dropped into this folder will be automatically converted\n" +
                "to UI-ready Sprites with 9-slice settings."
            );
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[Stat UI Kit] README creation failed: " + e.Message);
        }
    }
}
#endif
