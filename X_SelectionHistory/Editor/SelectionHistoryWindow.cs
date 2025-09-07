/*
 * https://chat.qwen.ai/c/c33198e8-8d6e-443d-8d99-0e561a3e11e2
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
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;
using Object = UnityEngine.Object;

public partial class SelectionHistoryWindow : EditorWindow
{
    [MenuItem("Window/General/Selection History")]
    public static void Init()
    {
        SelectionHistoryWindow window = GetWindow<SelectionHistoryWindow>();
        
        //Options
        window.autoRepaintOnSceneChange = true;
        window.titleContent.image = EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_UnityEditor.SceneHierarchyWindow" : "UnityEditor.SceneHierarchyWindow").image;
        window.titleContent.text = " Selection History";
        window.wantsMouseMove = true;

        //Show
        window.Show();
    }

    private string iconPrefix => EditorGUIUtility.isProSkin ? "d_" : "";
    
    public static bool RecordHierarchy
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_RecordHierachy", true); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_RecordHierachy", value); }
    }
    
    public static bool RecordProject
    {
        get { return EditorPrefs.GetBool(PlayerSettings.productName + "_SH_RecordProject", true); }
        set { EditorPrefs.SetBool(PlayerSettings.productName + "_SH_RecordProject", value); }
    }
    
    public static int MaxHistorySize
    {
        get { return EditorPrefs.GetInt(PlayerSettings.productName + "_SH_MaxHistorySize", 50); }
        set { EditorPrefs.SetInt(PlayerSettings.productName + "_SH_MaxHistorySize", Mathf.Max(1, value)); }
    }
    
    public static int DuplicateSearchDepth
    {
        get { return EditorPrefs.GetInt(PlayerSettings.productName + "_SH_DuplicateSearchDepth", 10); }
        set { EditorPrefs.SetInt(PlayerSettings.productName + "_SH_DuplicateSearchDepth", Mathf.Max(1, value)); }
    }

    private AnimBool settingAnimation;
    private bool settingExpanded;
    private AnimBool clearAnimation;
    private bool historyVisible = true;

    private List<SelectionHistoryOne> selectionHistory = new List<SelectionHistoryOne>();
    private static bool muteRecording;
    private int selectedIndex = -1;

    private bool isFocused;
    private Object previouslySelectedObject;

    private void OnFocus()
    {
        //Items have have been deleted and should be removed from history
        selectionHistory = selectionHistory.Where(x => x.obj != null).ToList();

        isFocused = true;
    }
    
    private void OnLostFocus()
    {
        isFocused = false;
    }

    private void OnInspectorUpdate() //10 fps
    {
        if (isFocused) Repaint();
    }

    private void AddToHistory()
    {
        if (Selection.activeObject == null) return;
        
        //Skip selected folders and such
        if (Selection.activeObject.GetType() == typeof(UnityEditor.DefaultAsset)) return;

        if (EditorUtility.IsPersistent(Selection.activeObject) && !RecordProject) return;
        if (EditorUtility.IsPersistent(Selection.activeObject) == false && !RecordHierarchy) return;
        
        // Always add object to the beginning
        selectionHistory.Insert(0, new SelectionHistoryOne(Selection.activeObject));
        
        // Update selected index
        selectedIndex = 0;
        
        // Remove duplicates within the search depth (excluding the first entry we just added)
        int searchDepth = Mathf.Min(DuplicateSearchDepth, selectionHistory.Count - 1);
        for (int i = 1; i <= searchDepth; i++)
        {
            if (i < selectionHistory.Count && selectionHistory[i].obj == Selection.activeObject)
            {
                selectionHistory.RemoveAt(i);
                // Adjust selected index if needed
                if (selectedIndex >= i)
                {
                    selectedIndex--;
                }
                break;
            }
        }
        
        // Trim to max size
        if(selectionHistory.Count > MaxHistorySize) 
        {
            selectionHistory.RemoveRange(MaxHistorySize, selectionHistory.Count - MaxHistorySize);
        }
        
        // Ensure selected index is valid
        selectedIndex = Mathf.Clamp(selectedIndex, -1, selectionHistory.Count - 1);
    }

    private void OnEnable()
    {
#if !UNITY_2019_1_OR_NEWER
        SceneView.onSceneGUIDelegate += ListenForNavigationInput;
#else
        SceneView.duringSceneGui += ListenForNavigationInput;
#endif
        
        EditorApplication.update += Update;
        
        settingAnimation = new AnimBool(false);
        settingAnimation.valueChanged.AddListener(this.Repaint);
        settingAnimation.speed = 4f;
        clearAnimation = new AnimBool(false);
        clearAnimation.valueChanged.AddListener(this.Repaint);
        clearAnimation.speed = settingAnimation.speed;
        
        LoadSceneData();
    }

    private void OnDisable()
    {
#if !UNITY_2019_1_OR_NEWER
        SceneView.onSceneGUIDelegate -= ListenForNavigationInput;
#else
        SceneView.duringSceneGui -= ListenForNavigationInput;
#endif
        
        EditorApplication.update -= Update;
    }

    
    private void Update()
    {
        if (Selection.activeObject && Selection.activeObject != previouslySelectedObject)
        {
            previouslySelectedObject = Selection.activeObject;
            
            if (muteRecording || !Selection.activeObject) return;
            
            this.Repaint();
            AddToHistory();
        }
    }

    private void ListenForNavigationInput(SceneView sceneView)
    {
        if (Event.current.type == EventType.KeyDown && Event.current.isKey && Event.current.keyCode == KeyCode.LeftBracket)
        {
            SelectPrevious();
        }
        if (Event.current.type == EventType.KeyDown &&  Event.current.isKey && Event.current.keyCode == KeyCode.RightBracket)
        {
            SelectNext();
        }
    }
    
    private void SetSelection(Object target, int index)
    {
        muteRecording = true;
        Selection.activeObject = target;
        EditorGUIUtility.PingObject(target);
        selectedIndex = index;
        previouslySelectedObject = target;
        muteRecording = false;
    }

    private void SelectPrevious()
    {
        if (selectedIndex < selectionHistory.Count - 1)
        {
            selectedIndex++;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, selectionHistory.Count - 1);
            // При выборе через историю - не изменяем порядок
            muteRecording = true;
            Selection.activeObject = selectionHistory[selectedIndex].obj;
            EditorGUIUtility.PingObject(selectionHistory[selectedIndex].obj);
            previouslySelectedObject = selectionHistory[selectedIndex].obj;
            muteRecording = false;
        }
    }

    private void SelectNext()
    {
        if (selectedIndex > 0)
        {
            selectedIndex--;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, selectionHistory.Count - 1);
            // При выборе через историю - не изменяем порядок
            muteRecording = true;
            Selection.activeObject = selectionHistory[selectedIndex].obj;
            EditorGUIUtility.PingObject(selectionHistory[selectedIndex].obj);
            previouslySelectedObject = selectionHistory[selectedIndex].obj;
            muteRecording = false;
        }
    }
    
    private void RemoveItem(int i)
    {
        if (i >= 0 && i < selectionHistory.Count)
        {
            selectionHistory.RemoveAt(i);
            
            // Adjust selected index
            if (selectedIndex >= i && selectedIndex > 0)
            {
                selectedIndex--;
            }
            else if (selectedIndex >= selectionHistory.Count)
            {
                selectedIndex = selectionHistory.Count - 1;
            }
        }
    }
    
    // Метод для получения списка объектов для GUI
    private List<Object> GetObjectList()
    {
        return selectionHistory.Select(item => item.obj).ToList();
    }
}