public struct UIEventData
{
    public string type;
    public string name;
    //public UI_Layer layer;
    public bool isExistingView;
    public bool isScene;
    public bool needBlur;
    public bool overlay;
}

public static class UIEvent
{
    public const string UI_BEFORE_SHOW = "UI_BEFORE_SHOW";
    public const string UI_SHOWN = "UI_SHOWN";
    public const string UI_HIDDEN = "UI_HIDDEN";
}