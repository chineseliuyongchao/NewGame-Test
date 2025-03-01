using System;
using FluffyUnderware.Curvy;
using UnityEditor;
using UnityEngine;

namespace Tools
{
    public abstract class BaseObstacle : MonoBehaviour
    {
        /**
         * 定义障碍物计算的最大步长，值越大精度越高
         */
        public int maxStep = 2000;

        protected CurvySpline line;

        protected virtual void OnValidate()
        {
            if (!line)
            {
                Transform tmp = transform.Find("line");
                if (tmp)
                {
                    line = tmp.GetComponent<CurvySpline>();
                }
            }
        }

        public virtual void OnCreate()
        {
            GameObject obj = new GameObject("line")
            {
                transform =
                {
                    parent = transform,
                    localPosition = Vector3.zero
                }
            };
            line = obj.AddComponent<CurvySpline>();

            transform.gameObject.layer = LayerMask.NameToLayer("Obstacles");
            Selection.activeGameObject = obj;
            // SceneView.lastActiveSceneView.FrameSelected();
        }

        public abstract void Calculate();

    }
}