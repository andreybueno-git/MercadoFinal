using UnityEngine;

// CharAnim.cs — animação procedural de personagem (jogador OU cliente) sem precisar de
// folhas de sprite. Coloque este componente num filho "vis" que carrega o SpriteRenderer;
// quem se MOVE é o objeto PAI. O CharAnim mede o deslocamento do pai a cada frame e aplica:
// passo (sobe/desce), squash & stretch, balanço lateral, respiração quando parado e flip.
public class CharAnim : MonoBehaviour {
    public SpriteRenderer sr;
    public float baseScale = 1f;   // escala base (ex.: 0.8 pro fallback quadrado)
    public float bobAmp = 0.07f;   // altura do "pulinho" do passo

    Vector3 lastParentPos;
    float phase, moveAmt, faceDir = 1f;
    bool init;

    void OnEnable() { lastParentPos = ParentPos(); init = true; }

    Vector3 ParentPos() { return transform.parent ? transform.parent.position : transform.position; }

    void LateUpdate() {
        float dt = Time.deltaTime;
        if (dt <= 0f) return;
        Vector3 pp = ParentPos();
        if (!init) { lastParentPos = pp; init = true; }
        Vector3 delta = pp - lastParentPos;
        lastParentPos = pp;

        float speed = new Vector2(delta.x, delta.y).magnitude / dt;
        bool moving = speed > 0.06f;
        moveAmt = Mathf.MoveTowards(moveAmt, moving ? 1f : 0f, dt * 6f);

        if (Mathf.Abs(delta.x) > 0.0004f) faceDir = delta.x < 0f ? -1f : 1f;
        if (sr) sr.flipX = faceDir < 0f;

        // a fase do passo corre rápido andando, lenta (respirando) parado
        phase += dt * Mathf.Lerp(3.2f, 13f, moveAmt);

        float bob     = Mathf.Abs(Mathf.Sin(phase)) * bobAmp * moveAmt;          // pulinho do passo
        float breathe = Mathf.Sin(phase) * 0.015f * (1f - moveAmt);              // respira parado
        float sx = 1f + Mathf.Sin(phase * 2f) * 0.05f * moveAmt;
        float sy = 1f - Mathf.Sin(phase * 2f) * 0.05f * moveAmt + breathe;
        float lean = Mathf.Sin(phase) * 5f * moveAmt;                            // balança ao andar

        transform.localPosition = new Vector3(0f, bob, 0f);
        transform.localScale    = new Vector3(sx * baseScale, sy * baseScale, 1f);
        transform.localRotation = Quaternion.Euler(0f, 0f, lean);
    }
}
