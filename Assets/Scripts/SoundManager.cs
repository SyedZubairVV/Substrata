using UnityEngine;

// a simple utility that any script can use to play a sound at a position
// put this on a dedicated empty GameObject in your scene called "SoundManager"
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    void Awake()
    {
        // make this a singleton so any script can access it with SoundManager.Instance
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    // plays a single clip at a position in the world
    public void PlaySound(AudioClip clip, Vector3 position, float volume = 1f)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, volume);
    }

    // plays a random clip from an array — great for footsteps and hits
    // so the same sound doesn't repeat identically every time
    public void PlayRandom(AudioClip[] clips, Vector3 position, float volume = 1f)
    {
        if (clips == null || clips.Length == 0) return;
        AudioClip clip = clips[Random.Range(0, clips.Length)];
        PlaySound(clip, position, volume);
    }

    // plays a clip with a random pitch variation so repeated sounds feel less robotic
    public void PlayWithPitchVariation(AudioClip clip, Vector3 position, float volume = 1f, float pitchVariation = 0.1f)
    {
        if (clip == null) return;

        // create a temporary GameObject with an AudioSource to play with pitch
        GameObject temp = new GameObject("TempAudio");
        temp.transform.position = position;
        AudioSource source = temp.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume;
        source.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        source.Play();

        // destroy it after the clip finishes
        Destroy(temp, clip.length + 0.1f);
    }
}