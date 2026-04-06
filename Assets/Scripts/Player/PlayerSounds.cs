using UnityEngine;

// attach this to your player GameObject alongside PlayerMovement and PlayerCombat
public class PlayerSounds : MonoBehaviour
{
    [Header("Footsteps")]
    public AudioClip[] footstepClips;   // add multiple clips for variety
    public float footstepInterval = 0.35f; // time between each footstep sound
    public float footstepVolume = 0.6f;

    [Header("Combat")]
    public AudioClip[] swingClips;      // sword swing sounds, one per attack if you want variety
    public float swingVolume = 0.8f;
    public AudioClip hitImpactClip;     // the thwack when sword connects with enemy
    public float hitImpactVolume = 0.2f;

    private PlayerMovement playerMovement;
    private float footstepTimer = 0f;
    private bool wasGrounded = false;

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
    }

    void Update()
    {
        HandleFootsteps();
    }

    void HandleFootsteps()
    {
        bool isGrounded = playerMovement.IsGroundedPublic();
        bool isMoving = Mathf.Abs(playerMovement.body.linearVelocity.x) > 0.1f;

        // only play footsteps when grounded and actually moving horizontally
        if (isGrounded && isMoving)
        {
            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                SoundManager.Instance.PlayRandom(footstepClips, transform.position, footstepVolume);
                footstepTimer = footstepInterval;
            }
        }
        else
        {
            // reset timer when not moving so first step plays immediately
            footstepTimer = 0f;
        }
    }

    // called by PlayerCombat when each attack swing starts
    public void PlaySwing(int comboStep)
    {
        if (swingClips == null || swingClips.Length == 0) return;

        int index = Mathf.Min(comboStep - 1, swingClips.Length - 1);
        SoundManager.Instance.PlayWithPitchVariation(
            swingClips[index],
            transform.position,
            swingVolume,
            0.08f // slight pitch variation so repeated swings don't sound identical
        );
    }

    // called by PlayerCombat's DealAttackDamage when the hit connects
    public void PlayHitImpact()
    {
        SoundManager.Instance.PlayWithPitchVariation(
            hitImpactClip,
            transform.position,
            hitImpactVolume,
            0.1f
        );
    }
}