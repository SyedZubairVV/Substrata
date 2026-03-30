using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset;

    void Start()
    {
        if (target == null)
            target = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(FindPlayerAfterLoad());
    }

    IEnumerator FindPlayerAfterLoad()
    {
        // wait one frame for the scene to fully initialize
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            target = player.transform;
        else
            Debug.LogWarning("CameraFollow: Player not found after scene load");
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 position = target.position + offset;
        position.z = -10f;
        transform.position = position;
    }
}