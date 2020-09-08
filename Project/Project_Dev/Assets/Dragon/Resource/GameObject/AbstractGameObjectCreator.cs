using UnityEngine;

namespace Uqee.Resource
{
    /// <summary>
    /// 创建GameObject的根节点
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractGameObjectCreator<T> : Singleton<T> where T : class, new()
    {
        protected void _InitRoot(string name)
        {
            gameObject = GameObject.Find(name);
            if (gameObject == null)
            {
                gameObject = new GameObject(name);
            }
            transform = gameObject.transform;
            UnityEngine.Object.DontDestroyOnLoad(gameObject);

        }
        public virtual GameObject GetOrCreate(string name)
        {
            GameObject root = null;
            var tran = transform.Find(name);
            if (tran == null)
            {
                root = new GameObject(name);
                root.transform.SetParent(transform, false);
            }
            else
            {
                root = tran.gameObject;
            }
            return root;
        }
        public override void Dispose()
        {
            if (transform != null)
            {
                UnityEngine.Object.Destroy(transform.gameObject);
                transform = null;
            }
            base.Dispose();
        }
        public Transform transform { get; protected set; }
        public GameObject gameObject { get; protected set; }
    }
}