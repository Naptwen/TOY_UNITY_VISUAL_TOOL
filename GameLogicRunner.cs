using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework.Internal;
public class GameLogicRunner : MonoBehaviour
{
    public static GameLogicRunner Instance;

    [Header("Set GraphData")]
    public GameLogicData graphData; 

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (graphData != null)
        {
            RunGameLogic();
        }
    }

    public void RunGameLogic()
    {
        var targetGuids = new HashSet<string>(graphData.links.Select(l => l.targetNodeGuid));
        var rootNodes = graphData.nodes.Where(n => !targetGuids.Contains(n.guid)).ToList();
        GameLogicData.NodeData startNode = null;

        if (rootNodes.Count > 0)
        {
            startNode = rootNodes.OrderBy(n => n.position.x).First();
        }
        else
        {
            startNode = graphData.nodes.OrderBy(n => n.position.x).First();
        }

        if (startNode != null)
        {
            Debug.Log($"<color=red>Start Node</color>:<color=cyan>{startNode.title}</color>");
            StartCoroutine(ExecuteNode(startNode));
        }
    }

    private IEnumerator ExecuteNode(GameLogicData.NodeData nodeData)
    {
        Debug.Log($"<color=green>{nodeData.title}</color> Desc: </color=white>{nodeData.description}</color>");
        GameObject trgObj = GameObject.Find(nodeData.trgObjectName);
        if (trgObj == null)
        {   
            Debug.LogError($"Error : Can't find '{nodeData.trgObjectName}'");
            yield break;
        }
        
        MonoBehaviour[] scripts = trgObj.GetComponents<MonoBehaviour>();
        List<Coroutine> activeCoroutines = new List<Coroutine>();

        foreach (var script in scripts)
        {
            MethodInfo methodStart = script.GetType().GetMethod("LogicStart", BindingFlags.Public | BindingFlags.Instance);

            if (methodStart != null)
            {
                object returnValue = methodStart.Invoke(script, null);
                if (returnValue is IEnumerator coroutineEnumerator)
                {
                    activeCoroutines.Add(StartCoroutine(coroutineEnumerator));
                    Debug.Log($" -> <color=green>'{trgObj.name}'</color>:<color=orange>'{script.GetType().Name}'</color>:LogicStart() 코루틴 시작");
                }
                else
                {
                    Debug.Log($" -> <color=green>'{trgObj.name}'</color>:<color=orange>'{script.GetType().Name}'</color>:LogicStart() 즉시 완료 (void)");
                }
            }
        }
        foreach (var coroutine in activeCoroutines)
        {
            yield return coroutine;
        }
        Debug.Log($"<color=green>{trgObj.name}</color> <color=red>실행 종료</color>");
        var nextLinks = graphData.links.Where(l => l.baseNodeGuid == nodeData.guid).ToList();

        foreach (var link in nextLinks)
        {
            var nextNode = graphData.nodes.FirstOrDefault(n => n.guid == link.targetNodeGuid);
            if (nextNode != null)
            {
                StartCoroutine(ExecuteNode(nextNode));
            }
        }
    }
}