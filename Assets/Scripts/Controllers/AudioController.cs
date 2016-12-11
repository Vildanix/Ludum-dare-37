using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour {

    // BG music
    public AudioSource bg_a;
    public AudioSource bg_b;

    // Audio effects
    public AudioSource place;
    public AudioSource destroy;
    public AudioSource message;
    public AudioSource rotate;

    public bool isMuted = false;
    private bool playFirstBG = true;
	
	// Update is called once per frame
	void Update () {
        // alternate bg music
        if (!bg_a.isPlaying && !bg_b.isPlaying) {
            if (playFirstBG) {
                bg_a.Play();
            }
            else {
                bg_b.Play();
            }
            playFirstBG = !playFirstBG;
        }
    }

    public void PlayPlaceSound() {
        place.Play();
    }

    public void PlayDestroySound() {
        destroy.Play();
    }

    public void PlayMessageSound() {
        message.Play();
    }

    public void PlayRotateSound() {
        rotate.Play();
    }

    public void MuteSounds() {
        isMuted = !isMuted;

        bg_a.mute = isMuted;
        bg_b.mute = isMuted;
        place.mute = isMuted;
        destroy.mute = isMuted;
        message.mute = isMuted;
        rotate.mute = isMuted;
    }
}
