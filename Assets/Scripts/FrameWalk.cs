using UnityEngine;

// FrameWalk.cs — animador por FRAMES reais (sprite sheet) pro jogador. Coloque num filho
// "vis" com o SpriteRenderer; quem se move é o PAI. Anda → cicla os frames de caminhada;
// parado → frame idle com respiração leve; ao atender → mostra a pose "segurando" por um
// tempo. Se não houver frames de caminhada, cai num passo PROCEDURAL (bob/squash) — robusto.
public class FrameWalk : MonoBehaviour {
    public SpriteRenderer sr;
    public Sprite[] walk;        // frames do ciclo de caminhada
    public Sprite idle;          // pose parada
    public float fps = 9f;       // velocidade base da troca de frames
    public float baseScale = 1f;

    Vector3 lastParentPos;
    float t, phase, faceDir = 1f, holdTimer;
    Sprite holdSprite;
    bool init;

    Vector3 ParentPos() { return transform.parent ? transform.parent.position : transform.position; }
    void OnEnable() { lastParentPos = ParentPos(); init = true; }

    // mostra a pose "segurando algo" por `dur` segundos (ex.: ao atender um cliente)
    public void ShowHold(Sprite s, float dur) { if (s) { holdSprite = s; holdTimer = dur; } }

    void LateUpdate() {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        Vector3 pp = ParentPos();
        if (!init) { lastParentPos = pp; init = true; }
        Vector3 d = pp - lastParentPos;
        lastParentPos = pp;

        float speed = new Vector2(d.x, d.y).magnitude / dt;
        bool moving = speed > 0.06f;
        if (Mathf.Abs(d.x) > 0.0004f) faceDir = d.x < 0f ? -1f : 1f;
        if (sr) sr.flipX = faceDir < 0f;

        // reset dos transforms de animação (cada ramo ajusta o que precisa)
        transform.localPosition = Vector3.zero;
        transform.localScale = new Vector3(baseScale, baseScale, 1f);
        transform.localRotation = Quaternion.identity;

        // 1) pose "segurando" tem prioridade
        if (holdTimer > 0f) {
            holdTimer -= dt;
            if (sr && holdSprite) sr.sprite = holdSprite;
            float b = 1f + Mathf.Sin(Time.time * 3f) * 0.02f;
            transform.localScale = new Vector3(baseScale, baseScale * b, 1f);
            return;
        }

        bool hasFrames = walk != null && walk.Length > 0;

        if (moving && hasFrames) {
            // 2a) caminhada com FRAMES reais
            t += dt * fps * Mathf.Clamp(speed / GameConfig.PlayerSpeed, 0.6f, 1.7f);
            int f = ((int)t) % walk.Length;
            if (sr && walk[f]) sr.sprite = walk[f];
            // um pulinho sutil por cima dos frames dá mais vida
            transform.localPosition = new Vector3(0f, Mathf.Abs(Mathf.Sin(t * Mathf.PI)) * 0.03f, 0f);
        } else if (moving) {
            // 2b) sem frames → passo PROCEDURAL (fallback)
            phase += dt * 13f;
            if (sr && idle) sr.sprite = idle;
            float bob = Mathf.Abs(Mathf.Sin(phase)) * 0.07f;
            float sx = 1f + Mathf.Sin(phase * 2f) * 0.05f;
            float sy = 1f - Mathf.Sin(phase * 2f) * 0.05f;
            transform.localPosition = new Vector3(0f, bob, 0f);
            transform.localScale = new Vector3(sx * baseScale, sy * baseScale, 1f);
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(phase) * 5f);
        } else {
            // 3) parado → idle + respiração
            t = 0f; phase = 0f;
            if (sr && idle) sr.sprite = idle;
            float br = 1f + Mathf.Sin(Time.time * 2.2f) * 0.02f;
            transform.localScale = new Vector3(baseScale, baseScale * br, 1f);
        }
    }
}
