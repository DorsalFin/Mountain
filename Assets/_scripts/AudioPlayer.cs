using UnityEngine;
using System.Collections;

public class AudioPlayer : MonoBehaviour {

    public AudioSource sfx;

    void Start()
    {
        sfx = GetComponent<AudioSource>();
    }

    public void PlaySound(AudioClip clip)
    {
        sfx.PlayOneShot(clip);
    }
}
