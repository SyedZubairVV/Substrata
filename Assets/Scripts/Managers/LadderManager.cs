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

    void Start()
    {
        currentScene = SceneManager.GetActiveScene().name;
    }

    void Update()
    {
        if (!playerInRange || !Input.GetKeyDown(KeyCode.F)) return;

		// Conditional which determines which scene to load based on the current scene
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
    }

    // OnTriggers for activating/deactivating the prompt
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            ladderPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            ladderPrompt.SetActive(false);
        }
    }
}