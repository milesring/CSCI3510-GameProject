using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadSceneOnClick : MonoBehaviour
{
	public Slider progress;

	void Start(){
		if (progress) {
			progress.gameObject.SetActive (false);
		}
	}
	 
    public void LoadByIndex(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

	public void LoadByIndexAsync(int sceneIndex){
		StartCoroutine (LoadNewScene (sceneIndex));
	}

	IEnumerator LoadNewScene(int sceneIndex){
		AsyncOperation async = SceneManager.LoadSceneAsync(sceneIndex);
		progress.gameObject.SetActive (true);
		while (!async.isDone) {
			Debug.Log (async.progress);
			progress.value = async.progress;
			yield return null;
		}


	}
}