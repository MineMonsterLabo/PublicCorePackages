using System;
using UnityEngine;

namespace InterfaceHost
{
    public abstract class UserInterfacePageBase : MonoBehaviour, IPageStackCallback, IDisposable
    {
        public virtual void OnBeginPushStack()
        {
        }

        public virtual void OnEndPushStack()
        {
        }

        public virtual void OnBeginPopStack()
        {
        }

        public virtual void OnEndPopStack()
        {
        }

        public virtual void Dispose()
        {
        }
    }
}