using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.Events;

[RequireComponent(typeof(ToggleGroup))]
[RequireComponent(typeof(CanvasGroup))]
public class RadioToggle :MonoBehaviour {
    public ToggleGroup group { get; private set; }
    [SerializeField]
    private List<Toggle> toggles;
    public IndexEvent onValueChanged = new IndexEvent();
    private int _TabIndex;
    private bool IsInited = false;
    private bool _isStarted = false;
    public CanvasGroup canvasGroup;

    void Awake()
    {
        group = GetComponent<ToggleGroup>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        _isStarted = true;
        AddToggleListeners();
    }

    private void AddToggleListeners()
    {
        for (int i = 0; i < toggles.Count; i++)
        {
            int idx = i;
            var tog = toggles[i];
            if (tog == null) continue;
            tog.onValueChanged.Invoke(tog.isOn);
            tog.onValueChanged.AddListener((b) => OnValueChange(idx, b));
        }
    }

    public void InitToggles(List<Toggle> toggles)
    {
        if (toggles == null)
        {
            this.toggles = new List<Toggle>();

            this.toggles.AddRange(toggles);
        }
        else
        {
            if (this.toggles != toggles)
            {
                this.toggles.Clear();
                this.toggles.AddRange(toggles);
            }
        }

        if (_isStarted)
        {
            AddToggleListeners();
        }
    }

    public int TabIndex {
        get {
            return _TabIndex;
        }
        set {
            if (value >= 0 && value < toggles.Count)
            {
                bool dirty = !IsInited || _TabIndex != value;
                IsInited = true;

                _TabIndex = value;
                var tog = toggles[value];
                tog.isOn = true;
                if (group.isActiveAndEnabled)
                {
                    group.NotifyToggleOn(toggles[value]);
                }

                if (dirty)
                    onValueChanged.Invoke(_TabIndex);
            }
        }
    }

    private void OnValueChange(int idx, bool selected)
    {
        if (selected)
        {
            TabIndex = idx;
        }
    }

    public class IndexEvent : UnityEvent<int>
    {

    }
}
