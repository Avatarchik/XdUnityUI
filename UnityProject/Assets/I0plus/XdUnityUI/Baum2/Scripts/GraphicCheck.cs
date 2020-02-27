using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;

#endif

namespace AlphaRaycaster
{
    public interface Graphic
    {
        bool Contains(Vector2 position);
        bool Contains(Vector3 pos);
    }

    public class RectGraphic : Graphic
    {
        public Vector2 pos = new Vector2(0, 0);
        public float width = 0;
        public float height = 0;

        public RectGraphic()
        {
        }

        public RectGraphic(float x, float y, float width, float height)
        {
            pos.x = x;
            pos.y = y;
            this.width = width;
            this.height = height;
        }

        public bool Contains(Vector2 position)
        {
            return position.x >= pos.x && position.x < pos.x + width && position.y >= pos.y &&
                   position.x < pos.y + height;
        }

        public bool Contains(Vector3 position)
        {
            return position.x >= pos.x && position.x < pos.x + width && position.y >= pos.y &&
                   position.x < pos.y + height;
        }
    }

    public class CircleGraphic : Graphic
    {
        public Vector2 pos = new Vector2(0, 0);
        public float radius = 0;

        public CircleGraphic()
        {
        }

        public CircleGraphic(float x, float y, float radius)
        {
            pos.x = x;
            pos.y = y;
            this.radius = radius;
        }

        public bool Contains(Vector2 position)
        {
            return (this.pos - position).magnitude < radius * radius;
        }

        public bool Contains(Vector3 pos)
        {
            var dx = this.pos.x - pos.x;
            var dy = this.pos.y - pos.y;
            return dx * dx + dy * dy < radius * radius;
        }
    }

    public class GraphicCheck :
#if ODIN_INSPECTOR
        SerializedMonoBehaviour
#else
        MonoBehaviour
#endif
        , ICanvasRaycastFilter
    {
        private GameObject gameObj;
        private Image checkedImage;
        private RawImage checkedRawImage;
        private bool isSetupValid;

        [SerializeField] public List<Graphic> graphics = new List<Graphic>();

        private void Awake()
        {
            gameObj = gameObject;
            checkedImage = GetComponent<Image>();
            checkedRawImage = GetComponent<RawImage>();
            isSetupValid = checkedImage || checkedRawImage;
        }

        private static Vector3 ScreenToLocalObjectPosition(Vector2 screenPosition, RectTransform objTrs,
            Camera eventCamera)
        {
            Vector3 pointerGPos;
            if (eventCamera)
            {
                var objPlane = new Plane(objTrs.forward, objTrs.position);
                float distance;
                var cameraRay = eventCamera.ScreenPointToRay(screenPosition);
                objPlane.Raycast(cameraRay, out distance);
                pointerGPos = cameraRay.GetPoint(distance);
            }
            else
            {
                pointerGPos = screenPosition;
                var rotationCorrection =
                    (-objTrs.forward.x * (pointerGPos.x - objTrs.position.x) -
                     objTrs.forward.y * (pointerGPos.y - objTrs.position.y)) / objTrs.forward.z;
                pointerGPos += new Vector3(0, 0, objTrs.position.z + rotationCorrection);
            }

            return objTrs.InverseTransformPoint(pointerGPos);
        }

        bool ICanvasRaycastFilter.IsRaycastLocationValid(Vector2 screenPosition, Camera eventCamera)
        {
            if (!isSetupValid) return true;

            var objectRectTransform = gameObj.transform as RectTransform;

            var pos = ScreenToLocalObjectPosition(screenPosition, objectRectTransform, eventCamera);

            foreach (var graphic in graphics)
            {
                if (graphic.Contains(pos))
                {
                    return true;
                }
            }

            return false;
        }
    }
}