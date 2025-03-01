using UnityEngine;

namespace Tools
{
    
    public class ObstacleManager : MonoBehaviour
    {
        public void 添加多边形障碍物()
        {
            int index = 1;
            
            for (int i = 0, max = transform.childCount; i < max; i++)
            {
                if (transform.GetChild(i).name.Contains("多边形障碍"))
                {
                    ++index;
                }
            }
            GameObject child = new GameObject("多边形障碍" + index)
            {
                transform =
                {
                    parent = transform,
                    localPosition = Vector3.zero
                }
            };
            child.AddComponent<PolygonObstacle>().OnCreate();
        }
        
        public void 计算所有的障碍()
        {
            for (int i = 0, max = transform.childCount; i < max; i++)
            {
                if (transform.GetChild(i).TryGetComponent(out BaseObstacle obstacle))
                {
                    obstacle.Calculate();
                }
            }
        }
    }
}