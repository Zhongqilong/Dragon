using System.Collections.Generic;

namespace Dragon.Events
{
    class AbstractListenerData : IListenerData
    {
        private List<bool> _onceListenerList = new List<bool>();
        private List<string> _tagList = new List<string>();

        private SortedSet<int> _removeList = new SortedSet<int>(IdxSorter.I);
        private bool _invoking;

        protected virtual void _RemoveAt(int i)
        {
            _onceListenerList.RemoveAt(i);
            _tagList.RemoveAt(i);
        }
        protected void _CheckRemove(int i)
        {
            if (_invoking)
            {
                if (!_removeList.Contains(i))
                {
                    _removeList.Add(i);
                }
            }
            else
            {
                _RemoveAt(i);
            }
        }
        protected virtual void _Add(bool once, string tag)
        {
            _onceListenerList.Add(once);
            _tagList.Add(tag);
        }

        protected virtual bool _CanInvoke(int i)
        {
            if (_removeList.Contains(i))
            {
                return false;
            }
            if (_onceListenerList[i])
            {
                _removeList.Add(i);
            }
            return true;
        }
        protected void _InvokeBegin()
        {
            _invoking = true;
        }
        protected void _InvokeEnd()
        {
            foreach (var idx in _removeList)
            {
                _RemoveAt(idx);
            }
            _removeList.Clear();

            _invoking = false;
        }
        public void RemoveByTag(string tag)
        {
            for (int i = _tagList.Count - 1; i >= 0; i--)
            {
                if (_tagList[i] == tag)
                {
                    _CheckRemove(i);
                }
            }
        }
    }
}