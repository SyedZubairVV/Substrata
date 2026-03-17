using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LadderManager : MonoBehaviour
{
	[Header("Scene Manager")]
	public GameSceneManager gameSceneManager;

	[Header("Prompt")]
	public GameObject ladderPrompt;

	private bool playerInRange;
	private string currentScene;
	private GameObject player;

	private void Start()
	{
		player = GameObject.FindWithTag("Player");
		currentScene = SceneManager.GetActiveScene().name;
	}

	// Update is called once per frame
	void Update()
    {
		if (playerInRange && Input.GetKeyDown(KeyCode.F))
		{
			if (currentScene == "Surface")
			{
				gameSceneManager.EnterCave();
			}
			else if (currentScene == "Cave")
			{
				gameSceneManager.ExitCave();
			}
		}
    }

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			Debug.Log("Prompt area triggered");
			playerInRange = true;
			ladderPrompt.SetActive(true);
		}
	}

	private void OnTriggerExit2D(Collider2D other)
	{
		if (other.CompareTag("Player"))
		{
			playerInRange = false;
			ladderPrompt.SetActive(false);
		}
	}
}
