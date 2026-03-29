using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class SceneSpawnManager : MonoBehaviour
{
    public static string spawnPointName;

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject spawn = GameObject.Find(spawnPointName);

        if (player != null && spawn != null)
        {
            Rigidbody2D rb = player.GetComponentInChildren<Rigidbody2D>();

            if (rb != null)
            {
                Transform root = rb.transform.root;

                root.position = spawn.transform.position;
                rb.linearVelocity = Vector2.zero;

                Physics2D.SyncTransforms();
            }
            else
            {
                player.transform.position = spawn.transform.position;
            }

            var vcam = FindFirstObjectByType<CinemachineCamera>();

            if (vcam != null)
            {
                Vector3 camPos = vcam.transform.position;
                camPos.x = -9.535022f;
                vcam.transform.position = camPos;

                vcam.Follow = player.transform;

                vcam.OnTargetObjectWarped(player.transform, player.transform.position);
            }
        }
    }
}