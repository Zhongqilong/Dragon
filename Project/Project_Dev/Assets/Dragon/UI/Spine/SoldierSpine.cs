
public class SoldierSpine:Spine2D
{
    public override void Init()
    {
        if(_folder==null)
        {
            _folder = "Soldier";
        }
        base.Init();
    }
    protected override void _LoadImg()
    {
        _imgName = $"SoldierIcon/{_nameSpell}";
        ResManager.LoadAssets(RESOURCE_CATEGORY.IMAGE, _imgName, false, _OnHalfLoaded);
    }
}
