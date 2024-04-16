using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SearchReferenceEditorWindow : EditorWindow
{
    [MenuItem("Assets/SearchReference")]
    static void SearchRef()
    {
        _ = EditorWindow.GetWindow(typeof(SearchReferenceEditorWindow), false, "Searching", true);
    }

    private static Object _searchObject;
    private readonly List<Object> _resultList = new();

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        _searchObject = EditorGUILayout.ObjectField(_searchObject, typeof(Object), true, GUILayout.Width(200));
        if (GUILayout.Button("Search", GUILayout.Width(100)))
        {
            _resultList.Clear();
            if (_searchObject == null)
            {
                return;
            }

            string assetPath = AssetDatabase.GetAssetPath(_searchObject); // 获取资源的路径
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath); // 获取资源的GUID

            // / 遍历工程中的所有Object,第一个参数搜索变量，第二个参数，搜索文件夹目录
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] {"Assets"});
            int length = guids.Length;
            for (int i = 0; i < length; i++)
            {
                string filePath = AssetDatabase.GUIDToAssetPath(guids[i]); // 获取每个Prefab路径
                EditorUtility.DisplayCancelableProgressBar("Checking", filePath, i / length * 1.0f);
                string content = File.ReadAllText(filePath); // 查找prefab文件中的引用
                if (content.Contains(assetGuid))
                {
                    Object fileObject = AssetDatabase.LoadAssetAtPath(filePath, typeof(Object)); // 将Prefab加载到ObjectField上
                    _resultList.Add(fileObject);
                }

                EditorUtility.ClearProgressBar();
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginVertical();
        foreach (var obj in _resultList)
        {
            EditorGUILayout.ObjectField(obj, typeof(Object), true, GUILayout.Width(300));
        }
        EditorGUILayout.EndVertical();
    }
}