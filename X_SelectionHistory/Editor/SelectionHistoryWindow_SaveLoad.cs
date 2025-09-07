/*
 *
 */



//============================================================================================================================================
//============================================================================================================================================
//============================================================================================================================================
//============================================================================================================================================
// Всё что ниже - сгенерено и пока обновляется
//============================================================================================================================================
//============================================================================================================================================
//============================================================================================================================================
//============================================================================================================================================
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class SelectionHistoryWindow : EditorWindow
{
    private SelectionHistoryWindowScene sceneData;
    
    private void SaveSceneData(string scenePath = "")
    {
        if (EditorApplication.isPlaying) return;
        
        if (string.IsNullOrEmpty(scenePath))
        {
            scenePath = EditorSceneManager.GetActiveScene().path;
        }
        
        if (string.IsNullOrEmpty(scenePath)) return;
        
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        string dataPath = System.IO.Path.GetDirectoryName(scenePath) + "/" + sceneName + " - [SelectionHistory].asset";
        
        if (sceneData == null)
        {
            sceneData = ScriptableObject.CreateInstance<SelectionHistoryWindowScene>();
        }
        
        sceneData.SetHistory(selectionHistory, selectedIndex);
        
        // Проверяем, существует ли уже файл
        SelectionHistoryWindowScene existingAsset = AssetDatabase.LoadAssetAtPath<SelectionHistoryWindowScene>(dataPath);
        if (existingAsset != null)
        {
            // Обновляем существующий ассет
            EditorUtility.CopySerialized(sceneData, existingAsset);
            AssetDatabase.SaveAssets();
        }
        else
        {
            // Создаем новый ассет
            AssetDatabase.CreateAsset(sceneData, dataPath);
        }
        
        AssetDatabase.Refresh();
        
        // Логируем сохранение
        string logMessage = $"<b>SelectionHistoryWindow:</b> History saved to <i>{dataPath}</i>";
        UnityEngine.Debug.Log(logMessage);
    }
    
    private void LoadSceneData()
    {
        if (EditorApplication.isPlaying) return;
        
        string scenePath = EditorSceneManager.GetActiveScene().path;
        if (string.IsNullOrEmpty(scenePath)) return;
        
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        string dataPath = System.IO.Path.GetDirectoryName(scenePath) + "/" + sceneName + " - [SelectionHistory].asset";
        
        sceneData = AssetDatabase.LoadAssetAtPath<SelectionHistoryWindowScene>(dataPath);
        if (sceneData != null)
        {
            // Преобразуем SelectionHistoryOne в Object, восстанавливая scene объекты по путям
            selectionHistory = new List<SelectionHistoryOne>();
            foreach (SelectionHistoryOne historyItem in sceneData.History)
            {
                SelectionHistoryOne newItem = new SelectionHistoryOne(historyItem); // Копируем с сохранением путей
                
                // Если объект null, но есть путь, пытаемся найти его на сцене
                if (newItem.obj == null && !string.IsNullOrEmpty(newItem.sceneObjectPath))
                {
                    newItem.obj = FindSceneObjectByPath(newItem.sceneObjectPath);
                }
                
                if (newItem.obj != null)
                {
                    selectionHistory.Add(newItem);
                }
            }
            selectedIndex = sceneData.SelectedIndex;
            
            // Логируем загрузку
            string logMessage = $"<b>SelectionHistoryWindow:</b> History loaded from <i>{dataPath}</i>";
            UnityEngine.Debug.Log(logMessage);
        }
        else
        {
            selectionHistory = new List<SelectionHistoryOne>();
            selectedIndex = -1;
        }
    }
    
    private Object FindSceneObjectByPath(string path)
    {
        if (string.IsNullOrEmpty(path)) return null;
        
        // Ищем корневой объект
        string[] pathParts = path.Split('/');
        GameObject[] rootObjects = EditorSceneManager.GetActiveScene().GetRootGameObjects();
        
        GameObject foundObject = null;
        
        // Ищем корневой объект
        foreach (GameObject root in rootObjects)
        {
            if (root.name == pathParts[0])
            {
                foundObject = root;
                break;
            }
        }
        
        if (foundObject == null) return null;
        
        // Проходим по остальным частям пути
        for (int i = 1; i < pathParts.Length; i++)
        {
            bool foundChild = false;
            for (int j = 0; j < foundObject.transform.childCount; j++)
            {
                Transform child = foundObject.transform.GetChild(j);
                if (child.name == pathParts[i])
                {
                    foundObject = child.gameObject;
                    foundChild = true;
                    break;
                }
            }
            
            if (!foundChild)
            {
                return null;
            }
        }
        
        return foundObject;
    }
    
    private string GetSceneDataPath()
    {
        string scenePath = EditorSceneManager.GetActiveScene().path;
        if (string.IsNullOrEmpty(scenePath)) return null;
        
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
        return System.IO.Path.GetDirectoryName(scenePath) + "/" + sceneName + " - [SelectionHistory].asset";
    }
}