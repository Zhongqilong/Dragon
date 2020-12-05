using UnityEngine;

namespace Uqee.Resource {
    public abstract class AbstractGameObjectCreator<T> : Singleton<T> where T : class, new () {
        public Transform transform;
        public GameObject gameObject;

        protected void _InitRoot (string name) {
            gameObject = GameObject.Find (name);
            if (gameObject == null) {
                gameObject = new GameObject (name);
            }

            transform = gameObject.transform;
            UnityEngine.Object.DontDestroyOnLoad (gameObject);
        }

        /// <summary>
        /// 如果成员trans里面没有参数trans那么就新建一个trans并且将成员作为其父trans
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public virtual GameObject GetOrCreate (string name) {
            GameObject cur_go = null;
            Transform cur_trs = transform.Find (name);
            if (cur_trs == null) {
                cur_go = new GameObject (name);
                cur_go.transform.SetParent (transform, false);
            } else {
                cur_go = cur_trs.gameObject;
            }
            return cur_go;
        }

        public override void Dispose () {
            if (transform != null) {
                UnityEngine.Object.Destroy (transform.gameObject);
                transform = null;
            }

            base.Dispose ();
        }
    }
}