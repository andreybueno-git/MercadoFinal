using UnityEngine;

// AudioManager.cs — música (ambiente/crise com crossfade) + SFX. Singleton. Mute global.
public class AudioManager : MonoBehaviour {
    public static AudioManager I;
    AudioSource musicA, musicB, sfx;   // A/B p/ crossfade
    bool useA = true; string current = "";
    public bool muted = false;
    float fade = 0f;

    void Awake() {
        I = this;
        musicA = gameObject.AddComponent<AudioSource>(); musicA.loop = true; musicA.volume = 0f; musicA.playOnAwake = false;
        musicB = gameObject.AddComponent<AudioSource>(); musicB.loop = true; musicB.volume = 0f; musicB.playOnAwake = false;
        sfx = gameObject.AddComponent<AudioSource>(); sfx.playOnAwake = false; sfx.volume = 0.7f;
    }

    public void PlayMusic(string name) {
        if (current == name) return; current = name;
        AudioClip clip = null;
        if (name == "ambiente") clip = AssetDB.I.bgmAmbiente;
        else if (name == "crise") clip = AssetDB.I.bgmCrise != null ? AssetDB.I.bgmCrise : AssetDB.I.bgmAmbiente;
        if (clip == null) return;
        var from = useA ? musicA : musicB;
        var to = useA ? musicB : musicA;
        to.clip = clip; to.volume = 0f; to.Play(); useA = !useA; fade = 1f;
        _from = from; _to = to;
    }
    AudioSource _from, _to;

    void Update() {
        float target = muted ? 0f : 0.32f;
        if (fade > 0f) {
            fade -= Time.unscaledDeltaTime / 0.8f;
            float k = Mathf.Clamp01(1f - fade);
            if (_to) _to.volume = target * k;
            if (_from) _from.volume = target * (1f - k);
            if (fade <= 0f && _from) _from.Stop();
        } else {
            if (_to) _to.volume = target;
        }
    }

    public void Sfx(AudioClip clip) { if (clip != null && !muted) sfx.PlayOneShot(clip, 0.8f); }

    public void SetMuted(bool m) { muted = m; if (m) { if (musicA) musicA.volume = 0; if (musicB) musicB.volume = 0; } }
    public void ToggleMute() { SetMuted(!muted); }
}
