using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace ParameterSceneManager
{
    public static class SceneManager
    {
        [RuntimeInitializeOnLoadMethod]
        private static void Initialize()
        {
            var rootGameObjects = UnitySceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var rootGameObject in rootGameObjects)
            {
                var scene = rootGameObject.GetComponent<IScene<Unit>>();
                if (scene != null)
                {
                    scene.Initialize(Unit.Default);
                    Debug.Log($"Initialized {scene.GetType().Name} scene.");
                    break;
                }
            }
        }

        public static async UniTask LoadScene<TScene>() where TScene : SceneBase<Unit>
        {
            await LoadScene<TScene, Unit>(Unit.Default);
        }

        public static async UniTask LoadScene<TScene, TArgument>(TArgument argument) where TScene : SceneBase<TArgument>
        {
            var sceneName = typeof(TScene).Name;
            await UnitySceneManager.LoadSceneAsync(sceneName).ToUniTask();

            var rootGameObjects = UnitySceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var rootGameObject in rootGameObjects)
            {
                var scene = rootGameObject.GetComponent<IScene<TArgument>>();
                if (scene != null)
                {
                    scene.Initialize(argument);
                    Debug.Log($"Initialized {scene.GetType().Name} scene.");
                    break;
                }
            }
        }
    }
}