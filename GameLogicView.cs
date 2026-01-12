using System;
using System.Collections.Generic;
using System.Linq; 
using Unity.VectorGraphics;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

public class GameLogicView : GraphView
{
    public class NodeDataContainer
    {
        public string description;
        public string trgObjectName;
    }

    public GameLogicView()
    {
        SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
        
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var grid = new GridBackground();
        Insert(0, grid);
        grid.StretchToParentSize();

        var menuManipulator = new ContextualMenuManipulator(evt =>
        {
            evt.menu.AppendAction("Add Logic Node", 
                action => CreateNode("Logic Node", action.eventInfo.localMousePosition));
            evt.menu.AppendAction("Add Conditional Node", 
                action => CreateNode("Conditional Node", action.eventInfo.localMousePosition));
        });
        this.AddManipulator(menuManipulator);
    }

    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        var compatiblePorts = new List<Port>();

        ports.ForEach((port) =>
        {
            if (startPort != port && startPort.node != port.node)
            {
                if (startPort.direction != port.direction)
                {
                    compatiblePorts.Add(port);
                }
            }
        });

        return compatiblePorts;
    }

    public void CreateNode(string nodeName, Vector2 position, string guid = null, string description = "", string trgObjName = null)
    {
        var node = new Node
        {
            title = nodeName,
            viewDataKey = string.IsNullOrEmpty(guid) ? Guid.NewGuid().ToString() : guid
        };
        node.SetPosition(new Rect(position, Vector2.zero));

        var inputPort = GeneratePort(node, Direction.Input, Port.Capacity.Multi);
        inputPort.portName = "Input";
        node.inputContainer.Add(inputPort);

        var outputPort = GeneratePort(node, Direction.Output, Port.Capacity.Multi);
        outputPort.portName = "Output";
        node.outputContainer.Add(outputPort);

        var container = new NodeDataContainer
        {
            description = description,
            trgObjectName = trgObjName
        };
        node.userData = container;

        var titleFiled = new TextField("Title");
        titleFiled.value = nodeName;
        titleFiled.RegisterValueChangedCallback(evt => node.title = evt.newValue);
        node.title = nodeName; 
        node.extensionContainer.Add(titleFiled);

        var textField = new TextField("Description");
        textField.value = description;
        textField.RegisterValueChangedCallback(evt => container.description = evt.newValue);
        node.extensionContainer.Add(textField);

        var objFiled = new TextField("ObjectName");
        objFiled.value = trgObjName;
        objFiled.RegisterValueChangedCallback(evt => container.trgObjectName = evt.newValue);
        node.extensionContainer.Add(objFiled);

        node.RefreshExpandedState();
        node.RefreshPorts();

        AddElement(node);
    }

    private Port GeneratePort(Node node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single)
    {
        return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
    }

    public void SaveGraph(GameLogicData graphData)
{
        if (graphData == null) return;

        graphData.nodes.Clear();
        graphData.links.Clear();

        nodes.ForEach(n =>
        {
            var node = n as Node;
            var container = node.userData as NodeDataContainer;
            
            graphData.nodes.Add(new GameLogicData.NodeData
            {
                guid = node.viewDataKey,
                title = node.title,
                description = container?.description ?? "",
                trgObjectName = container?.trgObjectName ?? "",
                position = node.GetPosition().position
            });
        });

        edges.ForEach(e =>
        {
            var outputNode = e.output.node as Node;
            var inputNode = e.input.node as Node;

            graphData.links.Add(new GameLogicData.LinkData
            {
                baseNodeGuid = outputNode.viewDataKey,
                targetNodeGuid = inputNode.viewDataKey
            });
        });

        EditorUtility.SetDirty(graphData);
        AssetDatabase.SaveAssets();
        Debug.Log($"Save: Node #{graphData.nodes.Count}, connection #{graphData.links.Count}");
    }

    public void LoadGraph(GameLogicData graphData)
    {
        DeleteElements(graphElements);

        if (graphData == null) return;

        foreach (var nodeData in graphData.nodes)
        {
            CreateNode(nodeData.title, nodeData.position, nodeData.guid, nodeData.description, nodeData.trgObjectName);
        }

        foreach (var linkData in graphData.links)
        {
            var outputNode = nodes.ToList().Cast<Node>().FirstOrDefault(n => n.viewDataKey == linkData.baseNodeGuid);
            var inputNode = nodes.ToList().Cast<Node>().FirstOrDefault(n => n.viewDataKey == linkData.targetNodeGuid);

            if (outputNode != null && inputNode != null)
            {
                var outputPort = outputNode.outputContainer[0] as Port;
                var inputPort = inputNode.inputContainer[0] as Port;
                
                var edge = outputPort.ConnectTo(inputPort);
                AddElement(edge);
            }
        }
    }
}