using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class SceneSpawnManager : MonoBehaviour
{
    // Set this before loading a scene to control where the player spawns
    public static string spawnPointName;

    IEnumerator Start()
    {
        // Wait one frame for all scene objects to finish initializing
        yield return new WaitForEndOfFrame();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject spawn = GameObject.Find(spawnPointName);

        if (player == null || spawn == null) yield break;

        PositionPlayer(player, spawn.transform.position);
        SnapCamera(player);
    }

    void PositionPlayer(GameObject player, Vector3 spawnPosition)
    {
        Rigidbody2D rb = player.GetComponentInChildren<Rigidbody2D>();

        if (rb != null)
        {
            // Move the root transform so the whole player hierarchy teleports cleanly
            rb.transform.root.position = spawnPosition;
            rb.linearVelocity = Vector2.zero;
            Physics2D.SyncTransforms();
        }
        else
        {
            player.transform.position = spawnPosition;
        }
    }

    void SnapCamera(GameObject player)
    {
        CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam == null) return;

        // Snap camera X to the default position and re-assign the follow target
        // to prevent it from lerping across the map from the previous scene position
        Vector3 camPos = vcam.transform.position;
        camPos.x = -9.535022f;
        vcam.transform.position = camPos;
        vcam.Follow = player.transform;
        vcam.OnTargetObjectWarped(player.transform, player.transform.position);
    }
}