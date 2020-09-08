using System;
using System.Collections;
using System.Collections.Generic;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

public class PageListAdapter : DefaultListAdapter
{
    public int count_per_page { get; private set; }
    public PageController pageCtrl { get; private set; }
    protected int DataOffset;
    public Action OnJumpEnd;

    public PageListAdapter(EnhancedScroller scroller, IList list, int cellWidth,int count_per_page, Action<int, Transform> updateAction) : base(scroller, list, cellWidth, updateAction)
    {
        this.count_per_page = count_per_page;
        pageCtrl = scroller.GetOrAddComponent<PageController>();

        pageCtrl.Init(list, count_per_page);
        pageCtrl.OnPageChanged += OnPageChanged;
    }

    public void SetCallBack(System.Action<int, Transform> updateAction)
    {
        this.updateAction = updateAction;
    }

    private void OnPageChanged()
    {
        var oldOffset = this.DataOffset;
        var newOffset = pageCtrl.DataOffset;
        scroller.JumpToDataIndex(newOffset, 0, 0, true, EnhancedScroller.TweenType.easeOutExpo, 0.5f, _JumpEnd, EnhancedScroller.LoopJumpDirectionEnum.Closest, true);
    }

    private void _JumpEnd()
    {
        OnJumpEnd?.Invoke();
    }

    public override void Dispose()
    {
        base.Dispose();
        if (pageCtrl != null)
        {
            pageCtrl.OnPageChanged -= OnPageChanged;
        }
        OnJumpEnd = null;
    }
}