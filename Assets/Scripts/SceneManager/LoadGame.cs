using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadGame : MonoBehaviour
{
    [Header("Menu Screens")]
    [SerializeField] private GameObject loadingUI;
    [SerializeField] private GameObject startUI;

    [Header("Slider")]
    [SerializeField] public Slider progressSlider;

    public void SceneLoader(int scene)
    {
        StartCoroutine(LoadScene_Coroutine(scene));
    }

    public IEnumerator LoadScene_Coroutine(int scene)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(scene);
        loadingUI.SetActive(true);

        while (!asyncOperation.isDone)
        {
            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            progressSlider.value = progress;

            yield return null;
        }
    }
}
