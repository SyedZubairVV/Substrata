using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager: MonoBehaviour
{
	public void PlayGame()
	{
		SceneManager.LoadScene("Surface");
	}

	public void ExitGame()
	{
		Application.Quit();
	}

	public void EnterCave()
	{
		SceneManager.LoadScene("Cave");
	}

	public void ExitCave()
	{
		SceneManager.LoadScene("Surface");
	}
}
