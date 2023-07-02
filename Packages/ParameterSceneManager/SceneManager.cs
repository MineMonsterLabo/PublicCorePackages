using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnitySceneManager = UnityEngine.SceneManagement.SceneManager;

namespace ParameterSceneManager
{
    public static class SceneManager
    {
        private static IDisposable currentScene = null;

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
                    currentScene = scene;
                    break;
                }
            }
        }

        public static async UniTask LoadSceneCustom<TScene>(Func<string, UniTask> loadFunc)
            where TScene : SceneBase<Unit>
        {
            await LoadSceneCustom<TScene, Unit>(Unit.Default, loadFunc);
        }

        public static async UniTask LoadSceneCustom<TScene, TArgument>(TArgument argument,
            Func<string, UniTask> loadFunc) where TScene : SceneBase<TArgument>
        {
            var sceneName = typeof(TScene).Name;
            await loadFunc.Invoke(sceneName);

            var rootGameObjects = UnitySceneManager.GetActiveScene().GetRootGameObjects();

            foreach (var rootGameObject in rootGameObjects)
            {
                var scene = rootGameObject.GetComponent<IScene<TArgument>>();

                if (scene != null)
                {
                    scene.Initialize(argument);
                    Debug.Log($"Initialized {scene.GetType().Name} scene.");

                    if (currentScene != null)
                    {
                        currentScene.Dispose();
                        Debug.Log($"disposed {currentScene.GetType().Name} scene.");
                    }

                    currentScene = scene;
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
            await LoadSceneCustom<TScene, TArgument>(argument,
                sceneName => UnitySceneManager.LoadSceneAsync(sceneName).ToUniTask());
        }
    }
}