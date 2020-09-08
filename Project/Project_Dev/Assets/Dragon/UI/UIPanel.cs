using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Canvas))]
    public class UIPanel : MonoBehaviour {
        private Canvas canvas;
        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
            }
            canvas.overrideSorting = true;
        }

        public int depth {
            get { return canvas.sortingOrder; }
            set { canvas.sortingOrder = value; }
        }

        static public int FullCompareFunc(UIPanel left, UIPanel right)
        {
            return 0;
        }

        static public int CompareFunc(UIPanel left, UIPanel right)
        {
            return 0;
        }

    }
}
