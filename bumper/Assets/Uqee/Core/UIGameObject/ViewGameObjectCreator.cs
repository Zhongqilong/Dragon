using UnityEngine;

namespace Uqee.Resource
{
    /// <summary>
    /// View层的GameObject(只显示继承AbstractView的UI)
    /// </summary>
    public class ViewGameObjectCreator : AbstractUIGameObjectCreator<ViewGameObjectCreator>
    {
        public ViewGameObjectCreator()
        {
            gameObject = UIGameObjectCreator.I.GetOrCreate("View");

            var uiRootRect = gameObject.GetOrAddComponent<RectTransform>();
            uiRootRect.sizeDelta = Vector2.zero;
            uiRootRect.localScale = Vector3.one;
            uiRootRect.anchorMax = Vector2.one;
            uiRootRect.anchorMin = Vector2.zero;

            transform = gameObject.transform;
        }
    }
}