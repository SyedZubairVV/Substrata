using UnityEngine;

public class GameOverPanelRef : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Panel Start running");
        GameOverManager.Instance?.RegisterPanel(gameObject);
    }
}