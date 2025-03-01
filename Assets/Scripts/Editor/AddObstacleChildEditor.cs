using Tools;

namespace Editor
{
    using UnityEngine;
    using UnityEditor;

    [CustomEditor(typeof(ObstacleManager))]
    public class AddObstacleChildEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUILayout.Button("添加多边形障碍物"))
            {
                ObstacleManager script = (ObstacleManager)target;
                script.添加多边形障碍物();
            }
            
            if (GUILayout.Button("计算所有的障碍"))
            {
                ObstacleManager script = (ObstacleManager)target;
                script.计算所有的障碍();
            }
        }
        
    }
}