using UnityEngine;
using UnityEngine.UI;

namespace Uqee.Resource {
    /// <summary>
    /// UI相关的对象创建，默认添加RectTranform, Canvas, 可选GraphicRaycaster
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractUIGameObjectCreator<T> : AbstractGameObjectCreator<T> where T : class, new () {
        public override GameObject GetOrCreate (string name) {
            GameObject root = base.GetOrCreate (name);
            root.layer = LAYER.UI;
            return root;
        }

        /// <summary>
        /// 为解决一些层级显示问题
        /// </summary>
        /// <param name="name">UI名称</param>
        /// <param name="sortingOrder">UI层级</param>
        /// <param name="addRaycaster">是否显示GraphicRaycaster</param>
        /// <returns></returns>
        public GameObject GetOrCreate (string name, int sortingOrder, bool addRaycaster) {
            var root = GetOrCreate (name);

            var rect = root.GetOrAddComponent<RectTransform> ();
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.anchorMax = Vector2.one;
            rect.anchorMin = Vector2.zero;

            var cav = root.GetOrAddComponent<Canvas> ();
            cav.overrideSorting = true;
            cav.sortingOrder = sortingOrder;
            if (addRaycaster) {
                root.GetOrAddComponent<GraphicRaycaster> ();
            }
            return root;
        }
    }
}