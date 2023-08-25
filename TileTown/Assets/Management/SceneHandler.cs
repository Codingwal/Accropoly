using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : SingletonPersistant<SceneHandler>
{
    public event Action<string> SceneIsUnloading;
    public event Action<float> LoadingScene;
    public async Task LoadScene(string sceneName)
    {
        // Start loading the scene
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);

        // Prevent the scene from automatically activating
        scene.allowSceneActivation = false;

        // Invoke the "LoadingScene" event with the current progress while the scene is loading
        do
        {
            await Task.Delay(100);
            LoadingScene.Invoke(scene.progress / 0.9f);
        } while (scene.progress < 0.9f);

        SceneIsUnloading.Invoke(sceneName);

        // Activate the loaded scene
        scene.allowSceneActivation = true;

        await Task.Delay(150);
    }
}
