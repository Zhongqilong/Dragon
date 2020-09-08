using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class ToggleChangeText : MonoBehaviour {
    private Text text;
    public Color normColor;
    public Color onColor;
    private Toggle toggle;

    private void Awake()
    {
        text = GetComponent<Text>();
    }

    private void OnEnable()
    {
        toggle = toggle ?? this.GetComponentInParent<Toggle>();
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(_ValueChange);
            toggle.onValueChanged.AddListener(_ValueChange);
            _ValueChange(toggle.isOn);
        }
    }

    private void OnDisable()
    {
        toggle?.onValueChanged.RemoveListener(_ValueChange);
    }

    public void _ValueChange(bool toggle)
    {
        if(text!=null)
            text.color = toggle ? onColor : normColor;
    }
}
