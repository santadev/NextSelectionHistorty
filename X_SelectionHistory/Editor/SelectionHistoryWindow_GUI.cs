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
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

public partial class SelectionHistoryWindow : EditorWindow
{
    private Vector2 scrollPos;

    private void OnGUI()
    {
        isFocused = isFocused || (Event.current.type == EventType.MouseMove);

        using (new EditorGUILayout.HorizontalScope())
        {
            using (new EditorGUI.DisabledScope(selectionHistory.Count == 0))
            {
                using (new EditorGUI.DisabledScope(selectedIndex >= selectionHistory.Count-1 || selectedIndex == -1))
                {
                    if (GUILayout.Button(
                        new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "back@2x").image,
                            "Select previous (Right bracket key)"), EditorStyles.miniButtonLeft, GUILayout.Height(20f),
                        GUILayout.Width(30f)))
                    {
                        SelectPrevious();
                    }
                }

                using (new EditorGUI.DisabledScope(selectedIndex <= 0))
                {
                    if (GUILayout.Button(
                        new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "forward@2x").image,
                            "Select next (Left bracket key)"), EditorStyles.miniButtonRight, GUILayout.Height(20),
                        GUILayout.Width(30f)))
                    {
                        SelectNext();
                    }
                }

                // Кнопки сохранения/загрузки
                string scenePath = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
                bool canSaveLoad = !string.IsNullOrEmpty(scenePath) && !EditorApplication.isPlaying;
                
                using (new EditorGUI.DisabledScope(!canSaveLoad || selectionHistory.Count == 0))
                {
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "SaveAs").image, "Save history"), EditorStyles.miniButtonMid, GUILayout.Width(30f)))
                    {
                        SaveSceneData();
                    }
                }
                
                using (new EditorGUI.DisabledScope(!canSaveLoad))
                {
                    if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "Refresh").image, "Load history"), EditorStyles.miniButtonMid, GUILayout.Width(30f)))
                    {
                        if (EditorUtility.DisplayDialog("Load History", "Load history from file?", "Load", "Cancel"))
                        {
                            LoadSceneData();
                            Repaint();
                        }
                    }
                }

                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "TreeEditor.Trash").image, "Clear history"), EditorStyles.miniButtonRight, GUILayout.Width(30f)))
                {
                    if (EditorUtility.DisplayDialog("Clear History", "Clear history?", "Clear", "Cancel"))
                    {
                        // Логируем очистку
                        UnityEngine.Debug.Log("<b>SelectionHistoryWindow:</b> History cleared");
                        historyVisible = false;
                    }
                }
            }
            
            GUILayout.FlexibleSpace();
            
            settingExpanded = GUILayout.Toggle(settingExpanded, new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "Settings").image, "Edit settings"), EditorStyles.miniButtonMid);
            settingAnimation.target = settingExpanded;
        }
        
        if (EditorGUILayout.BeginFadeGroup(settingAnimation.faded))
        {
            EditorGUILayout.Space();

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Record", EditorStyles.boldLabel, GUILayout.Width(100f));
                RecordHierarchy = EditorGUILayout.ToggleLeft("Hierarchy", RecordHierarchy, GUILayout.MaxWidth(80f));
                RecordProject = EditorGUILayout.ToggleLeft("Project window", RecordProject);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("History size", EditorStyles.boldLabel,GUILayout.Width(100f));
                MaxHistorySize = EditorGUILayout.IntField(MaxHistorySize, GUILayout.MaxWidth(40f));
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Duplicate search", EditorStyles.boldLabel, GUILayout.Width(100f));
                DuplicateSearchDepth = EditorGUILayout.IntField(DuplicateSearchDepth, GUILayout.MaxWidth(40f));
                EditorGUILayout.LabelField("depth", GUILayout.Width(40f));
            }
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Keep Play Mode", EditorStyles.boldLabel, GUILayout.Width(100f));
                KeepPlayMode = EditorGUILayout.Toggle(KeepPlayMode, GUILayout.MaxWidth(40f));
            }
            
            EditorGUILayout.Space();
            
            // Locate Button
            string dataPath = GetSceneDataPath();
            bool fileExists = !string.IsNullOrEmpty(dataPath) && File.Exists(dataPath);
            
            using (new EditorGUI.DisabledScope(!fileExists))
            {
                if (GUILayout.Button("Locate", GUILayout.Width(100f)))
                {
                    if (fileExists)
                    {
                        // Выбираем файл в проекте Unity
                        SelectionHistoryWindowScene asset = AssetDatabase.LoadAssetAtPath<SelectionHistoryWindowScene>(dataPath);
                        if (asset != null)
                        {
                            Selection.activeObject = asset;
                            EditorGUIUtility.PingObject(asset);
                        }
                    }
                }
            }
            
            // FAQ Button
            if (GUILayout.Button("FAQ", GUILayout.Width(100f)))
            {
                string faqPath = @"o:\Dev\Next\Project\Assets\Next\X_SelectionHistory\Editor\__Next_SelectionHistory__FAQ.txt";
                if (File.Exists(faqPath))
                {
                    try
                    {
                        Process.Start(faqPath);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError($"Failed to open FAQ file: {e.Message}");
                        EditorUtility.RevealInFinder(faqPath);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("File Not Found", "FAQ file not found at: " + faqPath, "OK");
                }
            }
            
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndFadeGroup();
        
        clearAnimation.target = !historyVisible;
        
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, EditorStyles.helpBox, GUILayout.MaxHeight(this.maxSize.y-20f));
        {
            EditorGUILayout.BeginFadeGroup(1f-clearAnimation.faded);
            
            var prevColor = GUI.color;
            var prevBgColor = GUI.backgroundColor;

            for (int i = 0; i < selectionHistory.Count; i++)
            {
                if(selectionHistory[i].obj == null) continue;
                
                var rect = EditorGUILayout.BeginHorizontal();
                
                GUI.color = i % 2 == 0 ?  Color.grey * (EditorGUIUtility.isProSkin ? 1f : 1.7f) : Color.grey * (EditorGUIUtility.isProSkin ? 1.05f : 1.66f);
                
                //Hover color
                if (rect.Contains(Event.current.mousePosition) || (selectedIndex == i))
                {
                    GUI.color = EditorGUIUtility.isProSkin ? Color.grey * 1.1f : Color.grey * 1.5f;
                }
                
                //Selection outline
                if (selectedIndex == i)
                {
                    Rect outline = rect;
                    outline.x -= 1;
                    outline.y -= 1;
                    outline.width += 2;
                    outline.height += 2;
                    EditorGUI.DrawRect(outline, EditorGUIUtility.isProSkin ? Color.gray * 1.5f : Color.gray);
                }

                //Background
                EditorGUI.DrawRect(rect, GUI.color);
                
                GUI.color = prevColor;
                GUI.backgroundColor = prevBgColor;
                

                if (GUILayout.Button(new GUIContent(" " + selectionHistory[i].obj.name, EditorGUIUtility.ObjectContent(selectionHistory[i].obj, selectionHistory[i].obj.GetType()).image), EditorStyles.label, GUILayout.MaxHeight(17f)))
                {
                    // При выборе из истории - не добавляем в историю и не изменяем порядок
                    muteRecording = true;
                    Selection.activeObject = selectionHistory[i].obj;
                    EditorGUIUtility.PingObject(selectionHistory[i].obj);
                    selectedIndex = i;
                    previouslySelectedObject = selectionHistory[i].obj;
                    muteRecording = false;
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFadeGroup();
        }
        EditorGUILayout.EndScrollView();

        //Once the list is collapse, clear the collection
        if(clearAnimation.faded == 1f) 
        {
            selectionHistory.Clear();
            selectedIndex = -1;
        }
        //Reset
        if (selectionHistory.Count == 0) 
        {
            historyVisible = true;
            selectedIndex = -1;
        }
    }
}
