using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerObjectManager : MonoBehaviour
{
	public string disabledSceneName = "Surface";
	public string childObjectName = "Headlamp";

	void OnEnable()
	{
		// Tell Unity to call "OnSceneLoaded" every time a scene changes
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// 1. Find the Player in the new scene
		GameObject player = GameObject.FindGameObjectWithTag("Player");

		if (player != null)
		{
			// 2. Find the specific child object by name
			// (Note: transform.Find only works on direct children)
			Transform targetChild = player.transform.Find(childObjectName);

			if (targetChild != null)
			{
				// 3. Deactivate if in the specific scene, otherwise activate
				bool shouldBeActive = (scene.name != disabledSceneName);
				targetChild.gameObject.SetActive(shouldBeActive);
			}
		}
	}
}
