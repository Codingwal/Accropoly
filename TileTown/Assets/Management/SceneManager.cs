using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneHandler : Singleton<SceneHandler>
{
    public event Action<bool> ActivateMenu;
    public event Action<bool> ActivateLoadingScreen;
    public event Action<float> LoadingScene;
    public async void LoadScene(string sceneName)
    {
        // Start loading the scene
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);

        // Prevent the scene from automatically activating
        scene.allowSceneActivation = false;

        // Deactivate the main menu and activate the loading screen
        ActivateMenu.Invoke(false);
        ActivateLoadingScreen.Invoke(true);

        // Invoke the "LoadingScene" event with the current progress while the scene is loading
        do
        {
            await Task.Delay(100);
            LoadingScene.Invoke(scene.progress / 0.9f);
        } while (!scene.isDone);

        // Deactivate the loading screen
        ActivateLoadingScreen.Invoke(false);

        // Activate the loaded scene
        scene.allowSceneActivation = true;
    }
}
