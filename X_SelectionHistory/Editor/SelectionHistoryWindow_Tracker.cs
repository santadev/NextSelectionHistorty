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
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public partial class SelectionHistoryWindow : EditorWindow
{
    private static string lastActiveScenePath = "";
    private static bool isTracking = false;
    
    static SelectionHistoryWindow()
    {
        InitializeTracker();
    }
    
    private static void InitializeTracker()
    {
        if (isTracking) return;
        
        isTracking = true;
        
        // Подписываемся на события
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        EditorApplication.quitting += OnEditorQuitting;
        
        // Подписываемся на события SceneManager
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorSceneManager.sceneClosed += OnSceneClosed;
        EditorSceneManager.newSceneCreated += OnNewSceneCreated;
        
        // Проверяем состояние при инициализации
        EditorApplication.delayCall += CheckInitialSceneState;
        
        UnityEngine.Debug.Log("<b>SelectionHistoryWindow:</b> Tracker initialized");
    }
    
    private static void CheckInitialSceneState()
    {
        // При открытии Unity, если загружена одна сцена, загружаем её историю
        if (IsSingleSceneLoaded())
        {
            lastActiveScenePath = EditorSceneManager.GetActiveScene().path;
            LoadHistoryForCurrentScene();
        }
    }
    
    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Перед входом в PlayMode сохраняем историю
            if (IsSingleSceneLoaded())
            {
                SaveHistoryForCurrentScene();
            }
        }
    }
    
    private static void OnEditorQuitting()
    {
        // При закрытии Unity сохраняем историю, если загружена одна сцена
        if (IsSingleSceneLoaded())
        {
            SaveHistoryForCurrentScene();
        }
    }
    
    private static void OnSceneOpened(UnityEngine.SceneManagement.Scene scene, OpenSceneMode mode)
    {
        EditorApplication.delayCall += CheckSceneAfterDelay;
    }
    
    private static void OnSceneClosed(UnityEngine.SceneManagement.Scene scene)
    {
        EditorApplication.delayCall += CheckSceneAfterDelay;
    }
    
    private static void OnNewSceneCreated(UnityEngine.SceneManagement.Scene scene, NewSceneSetup setup, NewSceneMode mode)
    {
        EditorApplication.delayCall += CheckSceneAfterDelay;
    }
    
    private static void CheckSceneAfterDelay()
    {
        CheckSceneStateChange();
    }
    
    private static void CheckSceneStateChange()
    {
        if (IsSingleSceneLoaded())
        {
            string currentScenePath = EditorSceneManager.GetActiveScene().path;
            
            // Если сменилась активная сцена
            if (currentScenePath != lastActiveScenePath)
            {
                // Сохраняем историю предыдущей сцены (если была)
                if (!string.IsNullOrEmpty(lastActiveScenePath))
                {
                    SaveHistoryForScene(lastActiveScenePath);
                }
                
                // Загружаем историю новой сцены
                LoadHistoryForCurrentScene();
                
                lastActiveScenePath = currentScenePath;
            }
        }
    }
    
    private static bool IsSingleSceneLoaded()
    {
        int loadedSceneCount = 0;
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
        {
            var scene = EditorSceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                loadedSceneCount++;
            }
        }
        return loadedSceneCount == 1;
    }
    
    private static void SaveHistoryForCurrentScene()
    {
        SaveHistoryForScene(EditorSceneManager.GetActiveScene().path);
    }
    
    private static void SaveHistoryForScene(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath)) return;
        if (EditorApplication.isPlaying) return;
        
        // Находим открытое окно и сохраняем через него
        SelectionHistoryWindow window = Resources.FindObjectsOfTypeAll<SelectionHistoryWindow>().FirstOrDefault();
        if (window != null)
        {
            window.SaveSceneData(scenePath);
        }
    }
    
    private static void LoadHistoryForCurrentScene()
    {
        // Находим открытое окно и загружаем через него
        SelectionHistoryWindow window = Resources.FindObjectsOfTypeAll<SelectionHistoryWindow>().FirstOrDefault();
        if (window != null)
        {
            window.LoadSceneData();
            window.Repaint();
        }
    }
}