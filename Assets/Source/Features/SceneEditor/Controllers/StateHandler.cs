using System;
using System.Collections.Generic;
using System.Linq;
using Source.Features.SceneEditor.Interfaces;
using UnityEngine;

namespace Source.Features.SceneEditor.Controllers
{
    public class StateHandler<T>
    {
        private readonly List<IChangeStateListener<T>> _listeners;

        public StateHandler(IEnumerable<IChangeStateListener<T>> listeners)
        {
            _listeners = listeners.ToList();
        }

        public void ChangeState(T state)
        {
            foreach (var listener in _listeners)
            {
                listener.OnStateChange(state);
            }
        }
    }
}