using UnityEngine;

// attach this to your enemy GameObject
public class EnemySounds : MonoBehaviour
{
    [Header("Enemy Sounds")]
    public AudioClip aggroClip;         // plays when enemy first spots the player
    public AudioClip[] attackClips;     // plays when enemy starts attacking
    public AudioClip[] deathClips;      // plays on death

    [Header("Volumes")]
    public float aggroVolume = 0.8f;
    public float attackVolume = 0.8f;
    public float deathVolume = 1f;

    private bool hasPlayedAggro = false; // so aggro only plays once per encounter

    // called from EnemyAI when the enemy first enters chase state
    public void PlayAggro()
    {
        if (hasPlayedAggro) return;
        hasPlayedAggro = true;
        SoundManager.Instance.PlaySound(aggroClip, transform.position, aggroVolume);
    }

    // called from EnemyAI when StartAttack() fires
    public void PlayAttack()
    {
        SoundManager.Instance.PlayRandom(attackClips, transform.position, attackVolume);
    }

    // called from EnemyAI Die()
    public void PlayDeath()
    {
        SoundManager.Instance.PlayRandom(deathClips, transform.position, deathVolume);
    }
}