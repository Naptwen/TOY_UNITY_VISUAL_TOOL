using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewGameLogic", menuName = "Game Logic/Logic Graph")]
public class GameLogicData : ScriptableObject
{
    [System.Serializable]
    public class NodeData
    {
        public string guid;         
        public string title;        
        public string description;  
        public Vector2 position;    
        public string trgObjectName;
    }

    [System.Serializable]
    public class LinkData
    {
        public string baseNodeGuid;   
        public string targetNodeGuid; 
    }

    public List<NodeData> nodes = new List<NodeData>();
    public List<LinkData> links = new List<LinkData>();
}