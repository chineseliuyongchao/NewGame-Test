using UnityEngine;
using UnityEngine.EventSystems;

namespace Scene
{
    public class SceneTest2 : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler
    {
        public Transform target;
        public Transform ai;

        private const float ZoomSpeed = 200f; //缩放速度
        private const float MinZoom = 128; //最小缩放值
        private const float MaxZoom = 512; //最大缩放值

        private const float DragSpeed = 400f; // 拖动速度
        private const float WorldWidth = 1820f / 2; //世界宽度
        private const float WorldHeight = 1024f / 2; //世界高度

        private bool isDrag;

        private void Awake()
        {
            target.localPosition = ai.localPosition;
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

            if (Camera.main != null) target.localPosition = Camera.main.ScreenToWorldPoint(eventData.position);
        }

        private void Update()
        {
            float scrollData = Input.GetAxis("Mouse ScrollWheel");
            var main = Camera.main;
            if (scrollData != 0 && main)
            {
                var orthographicSize = main.orthographicSize;
                orthographicSize -= scrollData * ZoomSpeed;
                main.orthographicSize = orthographicSize;
                main.orthographicSize = Mathf.Clamp(orthographicSize, MinZoom, MaxZoom);
            }
        }
    }
}