namespace Uqee.Resource
{
    /// <summary>
    /// Resource相关GameObject
    /// </summary>
    public class ResourceGameObjectCreator : AbstractGameObjectCreator<ResourceGameObjectCreator>
    {
        public ResourceGameObjectCreator()
        {
            _InitRoot("Resource");
        }
    }
}