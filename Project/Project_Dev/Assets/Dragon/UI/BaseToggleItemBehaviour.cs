using UnityEngine.UI;

public class BaseToggleItemBehaviour : BaseButtonItemBehaviour
{
    // Use this for initialization
    void Start()
    {
        GetComponent<Toggle>().onValueChanged.AddListener(_OnValueChanged);
        _OnStart();
    }
    void _OnValueChanged(bool on)
    {
        if (on)
        {
            _OnClick();
        }
    }
}
