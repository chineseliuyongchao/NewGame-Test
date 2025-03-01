using System;
using System.Collections.Generic;
using DG.Tweening;
using FluffyUnderware.Curvy;
using Pathfinding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Scene
{
    public class SceneTest3 : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler
    {
        public Transform ai;
        public Seeker aiSeeker;

        public AstarPath aStar;

        public Transform ui;
        public GameObject test;

        private const float ZoomSpeed = 200f; // 缩放速度
        private const float MinZoom = 128; // 最小缩放值
        private const float MaxZoom = 366; // 最大缩放值

        private const float DragSpeed = 400f; // 拖动速度
        private const float WorldWidth = 1280f / 2; // 世界宽度
        private const float WorldHeight = 731f / 2; // 世界高度

        private bool isDrag;

        // private void OnValidate()
        // {
        //     List<Vector2> pathPoints = new List<Vector2>();
        //     float step = 1f / MaxStep;
        //     foreach (var tmp in splines)
        //     {
        //         Vector2 beginPosition = tmp.transform.localPosition;
        //         for (int i = 0; i <= MaxStep; i++)
        //         {
        //             Vector3 worldPos = tmp.Interpolate(i * step);
        //             pathPoints.Add(new Vector2(worldPos.x, worldPos.y) + beginPosition);
        //         }
        //     }
        //
        //     // 设置多边形碰撞器的路径
        //     polygonCollider2D.pathCount = 1;
        //     polygonCollider2D.SetPath(0, pathPoints.ToArray());
        // }

        private void Awake()
        {
            //根据六边形地图初始化ui效果
            GridGraph gridGraph = aStar.data.gridGraph;
            foreach (var graphNode in gridGraph.nodes)
            {
                GameObject obj = Instantiate(test, ui);
                obj.transform.localPosition = (Vector3)graphNode.position;
                obj.name = graphNode.NodeIndex.ToString();
                obj.SetActive(false);
            }
        }

        private static readonly Color WhiteTransparent = new Color(1, 1, 1, 0);
        private static readonly Color DefaultColor = new Color(1, 1, 1, 0.5f);

        private bool eventSystem;

        private void Disable()
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            eventSystem = false;
        }

        private void Enable()
        {
            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            eventSystem = true;
        }

        private void Start()
        {
            Disable();
            //网格ui动画
            GridGraph gridGraph = aStar.data.gridGraph;
            int width = gridGraph.width;
            int height = gridGraph.depth;

            float startDelay = 0;
            float endDelay = 1f + 0.1f * width;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    int index = i + j * width;
                    Transform tmp = ui.GetChild(index);
                    if (tmp.TryGetComponent(out Image image))
                    {
                        tmp.gameObject.SetActive(true);
                        image.color = WhiteTransparent;
                        image.DOFade(0.5f, 1f).SetDelay(startDelay);
                        image.DOFade(0, 1f).SetDelay(endDelay + startDelay).OnComplete(() =>
                        {
                            image.color = DefaultColor;
                            tmp.gameObject.SetActive(false);
                            if (index >= (width * height - 1))
                            {
                                Enable();
                            }
                        });
                    }
                }

                startDelay += 0.1f;
            }


            // //网格ui动画
            // for (int i = 0; i < ui.childCount; i++)
            // {
            //     Transform tmp = ui.GetChild(i);
            //     Tween tween = null;
            //     if (tmp.TryGetComponent(out Image image))
            //     {
            //         float delay = 0.05f * i;
            //         tmp.gameObject.SetActive(true);
            //         image.color = WhiteTransparent;
            //         image.DOFade(0.5f, 0.3f).SetDelay(delay);
            //         tween = image.DOFade(0, 0.3f).SetDelay(0.3f + delay).OnComplete(() =>
            //         {
            //             image.color = DefaultColor;
            //             tmp.gameObject.SetActive(false);
            //         });
            //     }
            //
            //     if (i >= ui.childCount - 1 && tween != null)
            //     {
            //         tween.onComplete += Enable;
            //     }
            // }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDrag = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (Camera.main != null)
            {
                float deltaX = -eventData.delta.x * DragSpeed * Time.deltaTime;
                float deltaY = -eventData.delta.y * DragSpeed * Time.deltaTime;
                Vector3 newPosition = Camera.main.transform.localPosition + new Vector3(deltaX, deltaY, 0);
                newPosition = ClampCameraLocalPosition(newPosition);
                Camera.main.transform.localPosition = newPosition;
            }
        }

        private Vector3 ClampCameraLocalPosition(Vector3 targetPosition)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                float camHeight = cam.orthographicSize;
                float camWidth = camHeight * cam.aspect;

                float minX = -WorldWidth + camWidth;
                float maxX = WorldWidth - camWidth;
                float minY = -WorldHeight + camHeight;
                float maxY = WorldHeight - camHeight;

                targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
            }

            return targetPosition;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (isDrag)
            {
                isDrag = false;
                return;
            }

            // foreach (Transform tmp in lastActiveUiArray)
            // {
            //     tmp.gameObject.SetActive(false);
            // }
            // lastActiveUiArray.Clear();
            if (currentNodeArray.Count > 0)
            {
                ai.transform.DOLocalPath(currentNodeArray.ToArray(), 3f).SetEase(Ease.Linear).OnComplete(() =>
                    {
                        lastActiveUiArray.Clear();
                    })
                    .OnWaypointChange(
                        index => { lastActiveUiArray[index].gameObject.SetActive(false); });
                currentNodeArray.Clear();
            }
            // if (Camera.main != null) target.localPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        }

        private uint lastEndNodeIndex = int.MaxValue;
        private readonly List<Transform> lastActiveUiArray = new();
        private readonly List<Vector3> currentNodeArray = new();

        private void Update()
        {
            if (!eventSystem)
            {
                return;
            }

            float scrollData = Input.GetAxis("Mouse ScrollWheel");
            var main = Camera.main;
            if (main)
            {
                if (scrollData != 0)
                {
                    var orthographicSize = main.orthographicSize;
                    orthographicSize -= scrollData * ZoomSpeed;
                    main.orthographicSize = orthographicSize;
                    main.orthographicSize = Mathf.Clamp(orthographicSize, MinZoom, MaxZoom);
                }

                // 获取鼠标在屏幕上的位置
                Vector3 mousePosition = Input.mousePosition;
                // 将鼠标位置转换为世界坐标
                Vector3 worldPosition = main.ScreenToWorldPoint(mousePosition);

                var graphNode = aStar.data.gridGraph.GetNearest(worldPosition).node;
                if (graphNode != null)
                {
                    uint index = graphNode.NodeIndex;
                    if (index != lastEndNodeIndex)
                    {
                        lastEndNodeIndex = index;
                        foreach (Transform tmp in lastActiveUiArray)
                        {
                            tmp.gameObject.SetActive(false);
                        }

                        lastActiveUiArray.Clear();
                        aiSeeker.StartPath(ai.localPosition, worldPosition, path =>
                        {
                            currentNodeArray.Clear();
                            foreach (var node in path.path)
                            {
                                Transform tmp = ui.Find(node.NodeIndex.ToString());
                                if (tmp)
                                {
                                    tmp.gameObject.SetActive(true);
                                    lastActiveUiArray.Add(tmp);
                                    currentNodeArray.Add(tmp.localPosition);
                                }
                            }
                        });
                    }
                }
            }
        }
    }
}