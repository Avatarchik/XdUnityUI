using System;
using System.Collections.Generic;
using UnityEngine;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// Element class.
    /// based on Baum2.Editor.Element class.
    /// </summary>
    public abstract class Element
    {
        public string name;
        protected bool? active;
        protected string layer;
        protected List<object> classNames;
        protected string pivot;
        protected Vector2? anchorMin;
        protected Vector2? anchorMax;
        protected Vector2? offsetMin;
        protected Vector2? offsetMax;
        protected Element parent;

        public abstract GameObject Render(Renderer renderer, GameObject parentObject);

        public virtual void RenderPass2(List<Tuple<GameObject, Element>> selfAndSiblings)
        {
        }

        public abstract Area CalcArea();

        protected Element(Dictionary<string, object> json, Element parent)
        {
            this.parent = parent;
            name = json.Get("name");
            active = json.GetBool("active");
            layer = json.Get("layer");
            classNames = json.Get<List<object>>("class_names");

            pivot = json.Get("pivot");
            anchorMin = json.GetDic("anchor_min").GetVector2("x", "y");
            anchorMax = json.GetDic("anchor_max").GetVector2("x", "y");
            offsetMin = json.GetDic("offset_min").GetVector2("x", "y");
            offsetMax = json.GetDic("offset_max").GetVector2("x", "y");
        }

        public bool HasClassName(string className)
        {
            if (classNames == null || classNames.Count == 0) return false;
            var found = classNames.Find(s => (string) s == className);
            return found != null;
        }

        protected GameObject CreateUIGameObject(Renderer renderer)
        {
            var go = new GameObject(name);
            go.AddComponent<RectTransform>();
            SetLayer(go, layer);
            if (active != null)
            {
                go.SetActive(active.Value);
            }

            return go;
        }

        protected void SetAnchor(GameObject root, Renderer renderer)
        {
            if (string.IsNullOrEmpty(pivot)) pivot = "none";
            var rect = root.GetComponent<RectTransform>();
            if (anchorMin != null) rect.anchorMin = anchorMin.Value;
            if (anchorMax != null) rect.anchorMax = anchorMax.Value;
            if (offsetMin != null) rect.offsetMin = offsetMin.Value;
            if (offsetMax != null) rect.offsetMax = offsetMax.Value;
        }

        protected void SetLayer(GameObject go, string layerName)
        {
            switch (layerName)
            {
                case "Default":
                    go.layer = 0;
                    break;
                case "UI":
                    go.layer = 5;
                    break;
            }
        }
    }
}