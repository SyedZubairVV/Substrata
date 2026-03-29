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
				SceneSpawnManager.spawnPointName = "Spawn_Cave_Entrance";
				gameSceneManager.EnterCave();
			}
			else if (currentScene == "Cave")
			{
				SceneSpawnManager.spawnPointName = "Spawn_Surface_Entrance";
				gameSceneManager.ExitCave();
			}
			
			// TESTING
			if (currentScene == "Surface_Hermit")
			{
				SceneSpawnManager.spawnPointName = "Spawn_TestCave_Entrance";

				SceneManager.LoadScene("Test_Cave");
			}
			else if (currentScene == "Test_Cave")
			{
				SceneSpawnManager.spawnPointName = "Spawn_SurfaceHermit_Entrance";

				SceneManager.LoadScene("Surface_Hermit");
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
