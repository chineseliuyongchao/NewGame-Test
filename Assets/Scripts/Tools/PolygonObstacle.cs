using System.Collections.Generic;
using UnityEngine;

namespace Tools
{
    [AddComponentMenu("测试/多边形障碍物", 11)]
    [RequireComponent(typeof(PolygonCollider2D))]
    public class PolygonObstacle : BaseObstacle
    {
        private PolygonCollider2D polygonCollider2D;

        public override void OnCreate()
        {
            base.OnCreate();
            polygonCollider2D = GetComponent<PolygonCollider2D>();
        }
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!polygonCollider2D)
            {
                polygonCollider2D = GetComponent<PolygonCollider2D>();
            }
        }

        public override void Calculate()
        {
            List<Vector2> pathPoints = new List<Vector2>();
            float step = 1f / maxStep;
            for (int i = 0; i <= maxStep; i++)
            {
                Vector3 worldPos = line.Interpolate(i * step);
                pathPoints.Add(new Vector2(worldPos.x, worldPos.y));
            }

            // 设置多边形碰撞器的路径
            polygonCollider2D.pathCount = 1;
            polygonCollider2D.SetPath(0, pathPoints.ToArray());
        }
    }
}