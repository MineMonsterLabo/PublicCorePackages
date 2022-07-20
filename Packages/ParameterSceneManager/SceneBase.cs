using System;
using UnityEngine;

namespace ParameterSceneManager
{
    public class SceneBase<TArgument> : MonoBehaviour, IScene<TArgument>
    {
        public TArgument Arg { get; private set; }

        public virtual void Initialize(TArgument argument)
        {
            Arg = argument;
        }

        public virtual void Dispose()
        {
        }
    }

    public interface IScene<in TArgument> : IDisposable
    {
        void Initialize(TArgument argument);
    }
}