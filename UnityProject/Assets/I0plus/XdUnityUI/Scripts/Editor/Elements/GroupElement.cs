using System;
using System.Collections.Generic;
using System.Reflection;
#if ODIN_INSPECTOR
using Sirenix.Utilities;
#endif
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace XdUnityUI.Editor
{
    /// <summary>
    /// GroupElement class.
    /// based on Baum2.Editor.GroupElement class.
    /// </summary>
    public class GroupElement : Element
    {
        protected readonly List<Element> Elements;
        private Area _areaCache;
        private Dictionary<string, object> _canvasGroupParam;
        protected Dictionary<string, object> LayoutParam;
        protected Dictionary<string, object> ContentSizeFitterParam;
        protected Dictionary<string, object> LayoutElementParam;
        protected bool? RectMask2DParam;
        protected string FillColorParam;
        protected Dictionary<string, object> addComponentJson;
        protected List<object> componentsJson;

        public GroupElement(Dictionary<string, object> json, Element parent, bool resetStretch = false) : base(json,
            parent)
        {
            Elements = new List<Element>();
            var jsonElements = json.Get<List<object>>("elements");
            foreach (var jsonElement in jsonElements)
            {
                Elements.Add(ElementFactory.Generate(jsonElement as Dictionary<string, object>, this));
            }

            Elements.Reverse();
            _areaCache = CalcAreaInternal();
            _canvasGroupParam = json.GetDic("canvas_group");
            LayoutParam = json.GetDic("layout");
            ContentSizeFitterParam = json.GetDic("content_size_fitter");
            LayoutElementParam = json.GetDic("layout_element");
            RectMask2DParam = json.GetBool("rect_mask_2d");
            FillColorParam = json.Get("fill_color");
            addComponentJson = json.GetDic("add_component");
            componentsJson = json.Get<List<object>>("components");
        }

        /**
         * 子供の中にImageComponent化するものが無いか検索し、追加する
         */
        protected static Image SetupChildImageComponent(GameObject gameObject,
            List<Tuple<GameObject, Element>> createdChildren)
        {
            // コンポーネント化するImageをもっているオブジェクトを探す
            Tuple<GameObject, Element> childImageBeComponent = null;
            // imageElementを探し､それがコンポーネント化のオプションをもっているか検索
            foreach (var createdChild in createdChildren)
            {
                //TODO: item1がDestroyされていれば、コンティニューの処理が必要
                if (!(createdChild.Item2 is ImageElement imageElement)) continue;
                if (imageElement.component == null) continue;
                childImageBeComponent = createdChild;
            }

            // イメージコンポーネント化が見つかった場合､それのSpriteを取得し､設定する
            Image goImage = null;
            if (childImageBeComponent != null)
            {
                var imageComponent = childImageBeComponent.Item1.GetComponent<Image>();
                goImage = gameObject.AddComponent<Image>();
                goImage.sprite = imageComponent.sprite;
                goImage.type = imageComponent.type;

                // Spriteを取得したあと､必要ないため削除
                Object.DestroyImmediate(childImageBeComponent.Item1);
            }

            createdChildren.Remove(childImageBeComponent);

            return goImage;
        }

        public static void SetupFillColor(GameObject go, string fillColor)
        {
            // 背景のフィルカラー
            if (fillColor != null)
            {
                var image = go.AddComponent<Image>();
                Color color;
                if (ColorUtility.TryParseHtmlString(fillColor, out color))
                {
                    image.color = color;
                }
            }
        }

        public static ContentSizeFitter SetupContentSizeFitter(GameObject go,
            Dictionary<string, object> contentSizeFitter)
        {
            var componentContentSizeFitter = go.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null) return componentContentSizeFitter; // 引数がNULLでも持っていたら返す

            if (componentContentSizeFitter == null)
            {
                componentContentSizeFitter = go.AddComponent<ContentSizeFitter>();
            }

            if (contentSizeFitter.ContainsKey("vertical_fit"))
            {
                var verticalFit = contentSizeFitter.Get("vertical_fit");
                if (verticalFit.Contains("preferred"))
                {
                    componentContentSizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                }

                if (verticalFit.Contains("min"))
                {
                    componentContentSizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.MinSize;
                }
            }

            if (contentSizeFitter.ContainsKey("horizontal_fit"))
            {
                var verticalFit = contentSizeFitter.Get("horizontal_fit");
                if (verticalFit.Contains("preferred"))
                {
                    componentContentSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                }

                if (verticalFit.Contains("min"))
                {
                    componentContentSizeFitter.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.MinSize;
                }
            }

            return componentContentSizeFitter;
        }

#if ODIN_INSPECTOR
        public static Tuple<MemberInfo, object, Type> GetProperty(Type type, object target, string propertyPath)
        {
            // 参考サイト：　https://stackoverflow.com/questions/366332/best-way-to-get-sub-properties-using-getproperty
            // 配列、Dictionaryへのアクセスも考慮してある
            try
            {
                if (string.IsNullOrEmpty(propertyPath))
                    return null;
                string[] splitter = {"."};
                var sourceProperties = propertyPath.Split(splitter, StringSplitOptions.None);

                //TODO: さすがにfor内に入れるべき
                var infos = type.GetMember(sourceProperties[0]);
                if (infos.Length == 0) return null;
                type = infos[0].GetReturnType();
                var targetHolder = target;
                target = infos[0].GetMemberValue(target);

                // ドットで区切られたメンバー名配列で深堀りしていく
                for (var x = 1; x < sourceProperties.Length; ++x)
                {
                    infos = type.GetMember(sourceProperties[x]);
                    if (infos.Length == 0) return null;
                    type = infos[0].GetReturnType();
                    targetHolder = target;
                    target = infos[0].GetMemberValue(target);
                    var atyperef = __makeref(targetHolder);
                }

                // 値の変更方法
                // Item1.SetMemberValue(Item2, "Cartoon Blip"); 
                return new Tuple<MemberInfo, object, Type>(infos[0], targetHolder, type);
            }
            catch
            {
                throw;
            }
        }

        public static object SetProperty(Type targetType, object targetValue, string propertyPath,
            List<object> strData)
        {
            // 参考サイト：　https://stackoverflow.com/questions/366332/best-way-to-get-sub-properties-using-getproperty
            // 配列、Dictionaryへのアクセスも考慮してある
            try
            {
                if (string.IsNullOrEmpty(propertyPath))
                    return null;
                string[] splitter = {"."};
                var memberNames = propertyPath.Split(splitter, StringSplitOptions.None);

                //MemberInfo[] memberInfos;
                var nextTargetValue = targetValue;
                MemberInfo memberInfo = null;
                //object memberValue = null;

                // ドットで区切られたメンバー名配列で深堀りしていく
                foreach (var memberName in memberNames)
                {
                    targetValue = nextTargetValue;
                    var memberInfos = targetType.GetMember(memberName);
                    if (memberInfos.Length == 0) return null;
                    memberInfo = memberInfos[0];
                    var memberType = memberInfo.GetReturnType();
                    var memberValue = memberInfo.GetMemberValue(targetValue);
                    // next
                    nextTargetValue = memberValue;
                    targetType = memberType;
                }

                object value; // セットする値
                var firstStrData = strData[0] as string;
                var firstStrDataLowerCase = firstStrData.ToLower();
                if (targetType == typeof(bool))
                {
                    value = firstStrDataLowerCase != "0" && firstStrDataLowerCase != "false" &&
                            firstStrDataLowerCase != "null";
                }
                else if (targetType == typeof(string))
                {
                    value = firstStrData;
                }
                else if (targetType == typeof(float))
                {
                    value = float.Parse(firstStrData);
                }
                else if (targetType == typeof(double))
                {
                    value = double.Parse(firstStrData);
                }
                else if (targetType == typeof(Vector3))
                {
                    if (strData.Count >= 3)
                    {
                        var x = float.Parse(strData[0] as string);
                        var y = float.Parse(strData[1] as string);
                        var z = float.Parse(strData[2] as string);
                        value = new Vector3(x, y, z);
                    }
                    else
                    {
                        value = new Vector3(0, 0, 0);
                        Debug.LogError("Vector3を作成しようとしたが入力データが不足していた");
                    }
                }
                else
                {
                    // enum値などこちら
                    value = int.Parse(firstStrData);
                }

                memberInfo.SetMemberValue(targetValue, value);

                return value;
            }
            catch
            {
                throw;
            }
        }

        public static void SetupAddComponent(GameObject go, Dictionary<string, object> json)
        {
            if (json == null) return;
            var typeName = json.Get("type_name");
            if (typeName == null)
                return;
            var type = Type.GetType(typeName);
            if (type == null)
            {
                Debug.LogError($"Baum2 error*** Type.GetType({typeName})failed.");
                return;
            }

            var component = go.AddComponent(type);
        }
#endif

        /**
         * 
         */
        public static void SetupComponents(GameObject go, List<object> json)
        {
            /* フォーマットは以下のような感じでくる
             "components": [
              {
                "type": "Doozy.Engine.UI.UIButton, Doozy, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null",
                "name": "uibutton-",
                "method": "add",
                "properties": [
                  {
                    "path": "aaaa",
                    "values": ["bbbbb"]
                  },
                  {
                    "path": "aaaa",
                    "values": ["bbbbb"]
                  }
                ]
              }
            ]
             */
#if ODIN_INSPECTOR
            if (json == null) return;
            foreach (Dictionary<string, object> componentJson in json)
            {
                var typeName = componentJson.Get("type");
                if (typeName == null) continue;

                var componentType = Type.GetType(typeName);
                if (componentType == null)
                {
                    Debug.LogError($"Baum2 error*** Type.GetType({typeName})failed.");
                    return;
                }

                var component = go.AddComponent(componentType);

                var properties = componentJson.Get<List<object>>("properties");
                foreach (Dictionary<string, object> property in properties)
                {
                    SetProperty(componentType, component, property.Get("path"), property.Get<List<object>>("values"));
                }
            }
#endif
        }

        public static void SetMemberValueDirect(MemberInfo member, object obj, TypedReference typedReference,
            object value)
        {
            switch (member)
            {
                case FieldInfo _:
                    //(member as FieldInfo).SetValue(obj, value);
                    (member as FieldInfo).SetValueDirect(typedReference, value);
                    break;
                case PropertyInfo _:
                    MethodInfo setMethod = (member as PropertyInfo).GetSetMethod(true);
                    if (setMethod == null)
                        throw new ArgumentException("Property " + member.Name + " has no setter");
                    setMethod.Invoke(obj, new object[1] {value});
                    break;
                default:
                    throw new ArgumentException("Can't set the value of a " + member.GetType().Name);
            }
        }

        public List<Tuple<GameObject, Element>> RenderedChildren { get; private set; }

        public override GameObject Render(Renderer renderer, GameObject parentObject)
        {
            var go = CreateSelf(renderer);
            var rect = go.GetComponent<RectTransform>();
            if (parentObject)
            {
                //親のパラメータがある場合､親にする 後のAnchor定義のため
                rect.SetParent(parentObject.transform);
            }

            RenderedChildren = RenderChildren(renderer, go);
            SetupCanvasGroup(go, _canvasGroupParam);
            SetupChildImageComponent(go, RenderedChildren);
            SetupFillColor(go, FillColorParam);
            SetupContentSizeFitter(go, ContentSizeFitterParam);
            SetupLayoutGroup(go, LayoutParam);
            SetupLayoutElement(go, LayoutElementParam);
            SetupComponents(go, componentsJson);

            SetAnchor(go, renderer);
            return go;
        }


        /**
         * stateNameを持ちかつ、Imageをもっているオブジェクトを探す
         */
        protected static Image FindImageByClassName(
            List<Tuple<GameObject, Element>> children,
            string className
        )
        {
            Image image = null;
            var found = children.Find(child =>
            {
                // StateNameがNULLなら、ClassNameチェックなし
                if (className == null || child.Item2.HasClassName(className))
                {
                    image = child.Item1.GetComponent<Image>();
                    if (image != null) return true;
                }

                return false;
            });
            return image;
        }

        public static HorizontalOrVerticalLayoutGroup SetupLayoutGroupParam(GameObject go,
            Dictionary<string, object> layoutJson)
        {
            var method = "";
            if (layoutJson.ContainsKey("method"))
            {
                method = layoutJson.Get("method");
            }

            HorizontalOrVerticalLayoutGroup layoutGroup = null;

            if (method == "vertical")
            {
                var verticalLayoutGroup = go.AddComponent<VerticalLayoutGroup>();
                layoutGroup = verticalLayoutGroup;
            }

            if (method == "horizontal")
            {
                var horizontalLayoutGroup = go.AddComponent<HorizontalLayoutGroup>();
                layoutGroup = horizontalLayoutGroup;
            }

            if (layoutGroup == null)
            {
                return null;
            }

            // child control 子オブジェクトのサイズを変更する
            layoutGroup.childControlWidth = false;
            layoutGroup.childControlHeight = false;
            layoutGroup.childForceExpandWidth = false;
            layoutGroup.childForceExpandHeight = false;

            if (layoutJson.ContainsKey("padding"))
            {
                var padding = layoutJson.GetDic("padding");
                var left = padding.GetInt("left");
                var right = padding.GetInt("right");
                var top = padding.GetInt("top");
                var bottom = padding.GetInt("bottom");
                if (left != null && right != null && top != null && bottom != null)
                {
                    var paddingRectOffset = new RectOffset(left.Value, right.Value, top.Value, bottom.Value);
                    layoutGroup.padding = paddingRectOffset;
                }
            }

            if (method == "horizontal")
            {
                var spacing = layoutJson.GetFloat("spacing_x");
                if (spacing != null) layoutGroup.spacing = spacing.Value;
            }

            if (method == "vertical")
            {
                var spacing = layoutJson.GetFloat("spacing_y");
                if (spacing != null) layoutGroup.spacing = spacing.Value;
            }

            var childAlignment = GetChildAlignment(layoutJson);
            if (childAlignment != null)
            {
                layoutGroup.childAlignment = childAlignment.Value;
            }

            var controlChildSize = layoutJson.Get("control_child_size");
            if (!string.IsNullOrEmpty(controlChildSize))
            {
                if (controlChildSize.Contains("width"))
                    layoutGroup.childControlWidth = true;
                if (controlChildSize.Contains("height"))
                    layoutGroup.childControlHeight = true;
            }

            var controlChildScale = layoutJson.Get("use_child_scale");
            if (!string.IsNullOrEmpty(controlChildScale))
            {
                if (controlChildScale.Contains("width"))
                    layoutGroup.childScaleWidth = true;
                if (controlChildScale.Contains("height"))
                    layoutGroup.childScaleHeight = true;
            }

            var childForceExpand = layoutJson.Get("child_force_expand");
            if (!string.IsNullOrEmpty(childForceExpand))
            {
                if (childForceExpand.Contains("width"))
                    layoutGroup.childForceExpandWidth = true;
                if (childForceExpand.Contains("height"))
                    layoutGroup.childForceExpandWidth = true;
            }

            return layoutGroup;
        }

        private static TextAnchor? GetChildAlignment(Dictionary<string, object> layoutJson)
        {
            if (!layoutJson.ContainsKey("child_alignment")) return null;
            var childAlignment = layoutJson.Get("child_alignment");

            childAlignment = childAlignment.ToLower();
            if (childAlignment.Contains("upper"))
            {
                if (childAlignment.Contains("left"))
                {
                    return TextAnchor.UpperLeft;
                }

                if (childAlignment.Contains("right"))
                {
                    return TextAnchor.UpperRight;
                }

                if (childAlignment.Contains("center"))
                {
                    return TextAnchor.UpperCenter;
                }

                Debug.LogError("ChildAlignmentが設定できませんでした");
            }
            else if (childAlignment.Contains("middle"))
            {
                if (childAlignment.Contains("left"))
                {
                    return TextAnchor.MiddleLeft;
                }

                if (childAlignment.Contains("right"))
                {
                    return TextAnchor.MiddleRight;
                }

                if (childAlignment.Contains("center"))
                {
                    return TextAnchor.MiddleCenter;
                }

                Debug.LogError("ChildAlignmentが設定できませんでした");
            }
            else if (childAlignment.Contains("lower"))
            {
                if (childAlignment.Contains("left"))
                {
                    return TextAnchor.LowerLeft;
                }

                if (childAlignment.Contains("right"))
                {
                    return TextAnchor.LowerRight;
                }

                if (childAlignment.Contains("center"))
                {
                    return TextAnchor.LowerCenter;
                }

                Debug.LogError("ChildAlignmentが設定できませんでした");
            }

            return null;
        }


        public static GridLayoutGroup SetupGridLayoutGroupParam(GameObject go,
            Dictionary<string, object> layoutJson)
        {
            if (layoutJson == null) return null;

            var layoutGroup = go.AddComponent<GridLayoutGroup>();

            if (layoutJson.ContainsKey("padding"))
            {
                var padding = layoutJson.GetDic("padding");
                var left = padding.GetInt("left");
                var right = padding.GetInt("right");
                var top = padding.GetInt("top");
                var bottom = padding.GetInt("bottom");
                var paddingRectOffset = new RectOffset(left.Value, right.Value, top.Value, bottom.Value);
                layoutGroup.padding = paddingRectOffset;
            }

            var spacingX = layoutJson.GetFloat("spacing_x");
            var spacingY = layoutJson.GetFloat("spacing_y");

            layoutGroup.spacing = new Vector2(spacingX.Value, spacingY.Value);

            var cellWidth = layoutJson.GetFloat("cell_max_width");
            var cellHeight = layoutJson.GetFloat("cell_max_height");
            layoutGroup.cellSize = new Vector2(cellWidth.Value, cellHeight.Value);

            var fixedRowCount = layoutJson.GetInt("fixed_row_count");
            if (fixedRowCount != null)
            {
                layoutGroup.constraint = GridLayoutGroup.Constraint.FixedRowCount;
                layoutGroup.constraintCount = fixedRowCount.Value;
            }

            var fixedColumnCount = layoutJson.GetInt("fixed_column_count");
            if (fixedColumnCount != null)
            {
                layoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                layoutGroup.constraintCount = fixedColumnCount.Value;
            }

            var childAlignment = GetChildAlignment(layoutJson);
            if (childAlignment != null)
            {
                layoutGroup.childAlignment = childAlignment.Value;
            }

            var startAxis = layoutJson.Get("start_axis");
            switch (startAxis)
            {
                case "vertical":
                    layoutGroup.startAxis = GridLayoutGroup.Axis.Vertical;
                    break;
                case "horizontal":
                    layoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
                    break;
            }

            // 左上から配置スタート
            layoutGroup.startCorner = GridLayoutGroup.Corner.UpperLeft;

            return layoutGroup;
        }

        public static void SetupLayoutElement(GameObject go, Dictionary<string, object> layoutElement)
        {
            if (layoutElement == null) return;
            var componentLayoutElement = go.AddComponent<LayoutElement>();

            var minWidth = layoutElement.GetFloat("min_width");
            if (minWidth != null)
            {
                componentLayoutElement.minWidth = minWidth.Value;
            }

            var minHeight = layoutElement.GetFloat("min_height");
            if (minHeight != null)
            {
                componentLayoutElement.minHeight = minHeight.Value;
            }

            var preferredWidth = layoutElement.GetFloat("preferred_width");
            if (preferredWidth != null)
            {
                componentLayoutElement.preferredWidth = preferredWidth.Value;
            }

            var preferredHeight = layoutElement.GetFloat("preferred_height");
            if (preferredHeight != null)
            {
                componentLayoutElement.preferredHeight = preferredHeight.Value;
            }
        }


        public static void SetupLayoutGroup(GameObject go, Dictionary<string, object> layout)
        {
            if (layout == null) return;

            var method = (layout["method"] as string)?.ToLower();
            switch (method)
            {
                case "vertical":
                case "horizontal":
                {
                    var layoutGroup = SetupLayoutGroupParam(go, layout);
                    break;
                }
                case "grid":
                {
                    var gridLayoutGroup = SetupGridLayoutGroupParam(go, layout);
                    break;
                }
                default:
                    break;
            }
        }

        protected static void SetupCanvasGroup(GameObject go, Dictionary<string, object> canvasGroup)
        {
            if (canvasGroup != null)
            {
                go.AddComponent<CanvasGroup>();
            }
        }

        protected static void SetupRectMask2D(GameObject go, bool? param)
        {
            if (param != null && param.Value)
            {
                go.AddComponent<RectMask2D>(); // setupMask
            }
        }

        protected virtual GameObject CreateSelf(Renderer renderer)
        {
            var go = CreateUIGameObject(renderer);

            var rect = go.GetComponent<RectTransform>();
            var area = CalcArea();
            //rect.sizeDelta = area.Size;
            //rect.anchoredPosition = renderer.CalcPosition(area.Min, area.Size);

            SetMaskImage(renderer, go);
            return go;
        }

        protected void SetMaskImage(Renderer renderer, GameObject go)
        {
            var maskSource = Elements.Find(x => x is MaskElement);
            if (maskSource == null) return;

            Elements.Remove(maskSource);
            var maskImage = go.AddComponent<Image>();
            maskImage.raycastTarget = false;

            var dummyMaskImage = maskSource.Render(renderer, null);
            dummyMaskImage.transform.SetParent(go.transform);
            dummyMaskImage.GetComponent<Image>().CopyTo(maskImage);
            Object.DestroyImmediate(dummyMaskImage);

            var mask = go.AddComponent<Mask>();
            mask.showMaskGraphic = false;
        }

        protected List<Tuple<GameObject, Element>> RenderChildren(Renderer renderer, GameObject parent,
            Action<GameObject, Element> callback = null)
        {
            var list = new List<Tuple<GameObject, Element>>();
            foreach (var element in Elements)
            {
                var go = element.Render(renderer, parent);
                if (go.transform.parent != parent.transform)
                {
                    Debug.Log("親が設定されていない" + go.name);
                }

                list.Add(new Tuple<GameObject, Element>(go, element));
                callback?.Invoke(go, element);
            }

            foreach (var element in Elements)
            {
                element.RenderPass2(list);
            }

            return list;
        }

        private Area CalcAreaInternal()
        {
            var area = Area.None();
            foreach (var element in Elements) area.Merge(element.CalcArea());
            return area;
        }

        public override Area CalcArea()
        {
            return _areaCache;
        }
    }
}