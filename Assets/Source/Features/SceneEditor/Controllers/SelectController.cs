using System;
using System.Collections.Generic;
using System.Linq;
using Source.Features.SceneEditor.Interfaces;

namespace Source.Features.SceneEditor.Controllers
{
    public class SelectController
    {
        public event Action SelectStateReset;
        
        private readonly List<ISelectListener> _listeners;

        public SelectController(IEnumerable<ISelectListener> listeners)
        {
            _listeners = listeners.ToList();
        }

        public void ChangeSelected(ISelectListener selected)
        {
            ResetSelectState();
            
            selected.OnSelectStateChange(true);
        }

        public void ResetSelectState()
        {
            SelectStateReset?.Invoke();
            
            foreach (var listener in _listeners)
            {
                listener.OnSelectStateChange(false);
            }
        }

        public void AddSelectListener(ISelectListener listener)
        {
            _listeners.Add(listener);
        }
        
        public void RemoveSelectListener(ISelectListener listener)
        {
            _listeners.Remove(listener);
        }
    }
}