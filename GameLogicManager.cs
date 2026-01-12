using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class GameLogicManager : EditorWindow
{
    private GameLogicView graphView;
    private ObjectField dataField;

    [MenuItem("Window/Game Logic Editor")]
    public static void OpenWindow()
    {
        GameLogicManager wnd = GetWindow<GameLogicManager>();
        wnd.titleContent = new GUIContent("Game Logic Editor");
    }

    private void OnEnable()
    {
        ConstructGraphView();
        GenerateToolbar();
    }

    private void OnDisable()
    {
        rootVisualElement.Remove(graphView);
    }

    private void ConstructGraphView()
    {
        graphView = new GameLogicView{ name = "My Graph"};
        graphView.StretchToParentSize();
        rootVisualElement.Add(graphView);
    }

    private void AddNode()
    {
        Vector2 windowCenter = rootVisualElement.layout.center;
        Vector2 graphPos = graphView.contentViewContainer.WorldToLocal(windowCenter);
        graphView.CreateNode("New Node", graphPos); 
    }

    private void SaveNode()
    {
        if (dataField.value != null)
            graphView.SaveGraph(dataField.value as GameLogicData);
        else
            Debug.LogError("Select GameLogicData first!");
    }

    private void GenerateToolbar()
    {
        var toolbar = new UnityEditor.UIElements.Toolbar();

        dataField = new ObjectField("Graph Data") { objectType = typeof(GameLogicData) };
        dataField.RegisterValueChangedCallback(evt => {
            graphView.LoadGraph(evt.newValue as GameLogicData);
        });
        toolbar.Add(dataField);

        var nodeCreateButton = new UnityEditor.UIElements.ToolbarButton(() => AddNode());
        nodeCreateButton.text = "Add Node";

        var nodeSaveButton = new UnityEditor.UIElements.ToolbarButton(() => SaveNode());
        nodeSaveButton.text = "Save Nodes";

        toolbar.Add(nodeSaveButton);
        toolbar.Add(nodeCreateButton);
        rootVisualElement.Add(toolbar);
    }
}