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
    }

    public interface IScene<in TArgument>
    {
        void Initialize(TArgument argument);
    }
}