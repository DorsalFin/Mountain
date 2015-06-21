using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Instrument : MonoBehaviour {

    public enum Type
    {
        ElectricGuitar = 1,
        Keyboard = 2
    }

    public Type type;

    public AudioClip[] riffParts;
    public AudioClip[] rythmParts;

    public Image inBattleImage;

    AudioSource _audioSource;
    public bool AudioPlaying { get { return _audioSource.isPlaying; } }

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            PlayRiff();

        if (Input.GetKeyDown(KeyCode.L))
            Debug.Log("AudioPlaying = " + AudioPlaying.ToString());
    }

    public void PlayRiff()
    {
        AudioClip clip = riffParts[Random.Range(0, riffParts.Length)];
        _audioSource.clip = clip;
        _audioSource.Play();
    }

    public void PlayRythm()
    {
        AudioClip clip = rythmParts[Random.Range(0, rythmParts.Length)];
        _audioSource.clip = clip;
        _audioSource.Play();
    }
}
