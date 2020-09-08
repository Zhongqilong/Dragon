using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ToggleExt : MonoBehaviour
{
    public GameObject[] OnList;
    public GameObject[] OffList;
    private Toggle toggle;
    void Awake()
    {
        toggle = GetComponent<Toggle>();
        UpdateView();
        toggle.onValueChanged.AddListener(_valueChange);
    }

    private void _valueChange(bool arg0)
    {
        UpdateView();
    }

    private void UpdateView()
    {
        foreach(var go in OnList)
        {
            if (go != null) go.SetActive(toggle.isOn);
        }
        foreach (var go in OffList)
        {
            if (go != null) go.SetActive(!toggle.isOn);
        }
    }

    void OnEnable()
    {
        UpdateView();
    }
}