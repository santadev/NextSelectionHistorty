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
using UnityEngine;
using Object = UnityEngine.Object;

[System.Serializable]
public class SelectionHistoryOne
{
    public Object obj;
    
    [SerializeField]
    public string sceneObjectPath;
    
    public SelectionHistoryOne(Object obj)
    {
        this.obj = obj;
        this.sceneObjectPath = GetSceneObjectPath(obj);
    }
    
    // Конструктор для копирования
    public SelectionHistoryOne(SelectionHistoryOne other)
    {
        this.obj = other.obj;
        this.sceneObjectPath = other.sceneObjectPath;
    }
    
    private string GetSceneObjectPath(Object obj)
    {
        GameObject go = obj as GameObject;
        if (go == null)
        {
            Component component = obj as Component;
            if (component != null)
            {
                go = component.gameObject;
            }
        }
        
        if (go != null && go.scene.IsValid())
        {
            return GetGameObjectPath(go);
        }
        
        return null;
    }
    
    private string GetGameObjectPath(GameObject go)
    {
        if (go == null) return null;
        
        string path = go.name;
        Transform parent = go.transform.parent;
        
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
}

[CreateAssetMenu(fileName = "SelectionHistoryData", menuName = "Selection History/Data")]
public class SelectionHistoryWindowScene : ScriptableObject
{
    [SerializeField]
    private List<SelectionHistoryOne> history = new List<SelectionHistoryOne>();
    
    [SerializeField]
    private int selectedIndex = -1;
    
    public List<SelectionHistoryOne> History => history;
    public int SelectedIndex => selectedIndex;
    
    public void SetHistory(List<SelectionHistoryOne> newHistory, int newSelectedIndex)
    {
        history.Clear();
        if (newHistory != null)
        {
            foreach (SelectionHistoryOne item in newHistory)
            {
                history.Add(new SelectionHistoryOne(item)); // Копируем с сохранением путей
            }
        }
        selectedIndex = newSelectedIndex;
    }
    
    public void Clear()
    {
        history.Clear();
        selectedIndex = -1;
    }
}