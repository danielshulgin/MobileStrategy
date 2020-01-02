using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ScenLoading : MonoBehaviour
{
    public int sceneId = 1;
    public GameObject scrollbar;
    public TMP_Text text;

    void Start()
    {
        StartCoroutine(AsyncLoad());
    }

    IEnumerator AsyncLoad()
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneId);
        while (!loadOperation.isDone)
        {
            Debug.Log(loadOperation.progress);
            scrollbar.transform.localScale = new Vector3(loadOperation.progress / 0.9f, 1f, 1f);
            text.text = Mathf.CeilToInt(loadOperation.progress * 111f)  + "%";
            yield return null;
        }
    }
}
