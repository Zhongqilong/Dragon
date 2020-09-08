
public class HalfBodySpine:Spine2D
{
    public override void Init()
    {
        if(_folder==null)
        {
            _folder = "Body";
            _spinePos = new UnityEngine.Vector2(0, -800);
        }
        base.Init();
    }
}
