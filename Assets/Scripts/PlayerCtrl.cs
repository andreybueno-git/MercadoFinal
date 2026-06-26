using UnityEngine;

// PlayerCtrl.cs — lojista top-down: movimento 4 direções com colisão por tile + interação
// contextual (E/Espaço/botão): repor prateleira, comprar no fundo, ou atender cliente/caixa.
public class PlayerCtrl : MonoBehaviour {
    public static PlayerCtrl I;
    SpriteRenderer sr;
    FrameWalk fw;
    bool active = false;
    float r = 0.32f; // raio de colisão

    void Awake() {
        I = this;
        // o SpriteRenderer fica num filho "vis" pra o FrameWalk animar (frames/bob) sem mexer
        // na posição/colisão do jogador (que vive no transform do pai).
        var vis = new GameObject("vis"); vis.transform.SetParent(transform, false);
        sr = vis.AddComponent<SpriteRenderer>(); sr.sortingOrder = 55;
        fw = vis.AddComponent<FrameWalk>(); fw.sr = sr;
    }

    public void SetActive(bool v) {
        active = v; gameObject.SetActive(true);
        if (v) {
            // sprite do lojista (PNG individual com fundo transparente) + frames de caminhada
            var sp = AssetDB.I != null ? AssetDB.I.Char(0) : null;
            sr.sprite = sp ? sp : Util.SolidSprite(GameConfig.Good);
            fw.idle = sp ? sp : sr.sprite;
            fw.walk = AssetDB.I != null ? AssetDB.I.playerWalk : null;
            fw.baseScale = sp ? 1f : 0.8f;
            transform.position = Market.I.RegisterWorld() + new Vector3(-1.4f, 0.2f, 0);
        }
        sr.enabled = v;
    }

    // mostra a pose "segurando a sacola" por um tempinho (chamado ao atender um cliente)
    public void ShowServe() {
        if (fw != null && AssetDB.I != null) fw.ShowHold(AssetDB.I.playerHold, 0.8f);
    }

    void Update() {
        if (!active || GameManager.I == null || !GameManager.I.running || GameManager.I.pendingEvent != null || (UI.I != null && UI.I.modal)) return;
        float mx = Input.GetAxisRaw("Horizontal");
        float my = Input.GetAxisRaw("Vertical");
        var v = new Vector2(mx, my); if (v.sqrMagnitude > 1f) v.Normalize();
        float step = GameConfig.PlayerSpeed * Time.deltaTime;
        TryMove(v.x * step, 0);
        TryMove(0, v.y * step);
        // o flip é tratado pelo CharAnim (segue a direção do movimento)

        bool act = Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)
                 || Input.GetKeyDown(KeyCode.Return) || Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump");
        if (act) Interact();
    }

    void TryMove(float dx, float dy) {
        Vector3 p = transform.position;
        Vector3 np = p + new Vector3(dx, dy, 0);
        // checa os 4 cantos do raio
        if (!Solid(np.x + r, np.y) && !Solid(np.x - r, np.y) && !Solid(np.x, np.y + r) && !Solid(np.x, np.y - r))
            transform.position = np;
    }
    bool Solid(float x, float y) { return Market.I.IsSolid(x, y); }

    void Interact() {
        Vector3 p = transform.position;
        // 1) perto do FUNDO (almoxarifado) → comprar estoque
        if (Vector3.Distance(p, Market.I.BackroomWorld()) < 1.3f) { GameManager.I.OpenBuyPanel(); return; }
        // 2) perto de uma PRATELEIRA → repor
        var sh = Market.I.NearestShelf(p, 1.2f);
        // 3) perto de um CLIENTE / caixa → atender (prioriza atender se houver cliente perto)
        bool served = GameManager.I.TryServeNearest(p);
        if (served) return;
        if (sh != null) { GameManager.I.RestockShelf(sh); return; }
        // perto da caixa sem cliente: nada
    }
}
