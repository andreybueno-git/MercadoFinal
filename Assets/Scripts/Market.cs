using UnityEngine;
using System.Collections.Generic;

// Shelf — uma prateleira (slot). Guarda produto/quantidade/validade e o visual (ícone + contador).
public class Shelf {
    public Vector2Int cell;
    public Vector3 world;
    public string productId;
    public int count;
    public int placedDay;
    public SpriteRenderer icon;
    public TextMesh label;

    public void SetProduct(string id, int newCount, int day) {
        productId = id; count = newCount; placedDay = day; Refresh();
    }
    public void Take(int n) { count = Mathf.Max(0, count - n); if (count == 0) productId = null; Refresh(); }
    public bool Expired(int day) {
        if (string.IsNullOrEmpty(productId) || count <= 0) return false;
        var p = GameConfig.Product(productId);
        return p != null && p.perish && (day - placedDay) >= p.shelfLife;
    }
    public void Refresh() {
        bool has = !string.IsNullOrEmpty(productId) && count > 0;
        if (icon) {
            icon.enabled = has;
            if (has) { var sp = AssetDB.I.Product(GameConfig.Product(productId).icon); icon.sprite = sp ? sp : Util.SolidSprite(GameConfig.Good); }
        }
        if (label) label.text = has ? count.ToString() : "";
    }
}

// Market — monta a sala a partir de GameConfig.Map e gerencia colisão/posições/validade.
public class Market : MonoBehaviour {
    public static Market I;
    public List<Shelf> shelves = new List<Shelf>();
    Vector2Int registerCell, backroomCell;
    List<Vector2Int> doorCells = new List<Vector2Int>();
    Transform root;
    int W, H;

    void Awake() { I = this; }

    public Vector3 World(int tx, int ty) {
        return new Vector3(tx - W / 2f + 0.5f, (H - 1 - ty) - H / 2f + 0.5f, 0);
    }

    public void Build() {
        if (root) Destroy(root.gameObject);
        root = new GameObject("Mercado").transform;
        shelves.Clear(); doorCells.Clear();
        var map = GameConfig.Map; H = map.Length; W = map[0].Length;

        for (int ty = 0; ty < H; ty++) {
            for (int tx = 0; tx < W; tx++) {
                char ch = map[ty][tx];
                Vector3 w = World(tx, ty);
                // chão em tudo que não é parede
                if (ch != '#') Util.Quad(root, w, Vector3.one * 1.01f, ((tx + ty) % 2 == 0) ? GameConfig.Floor : GameConfig.Floor2, 0);
                if (ch == '#') {
                    Util.Quad(root, w, Vector3.one * 1.01f, GameConfig.Wall, 1);
                    Util.Quad(root, w + Vector3.up * 0.18f, new Vector3(1.01f, 0.5f, 1f), GameConfig.WallTop, 2);
                } else if (ch == 'S') {
                    var baseSr = Util.Quad(root, w, new Vector3(0.96f, 0.96f, 1f), GameConfig.Shelf, 3);
                    Util.Quad(root, w + Vector3.up * 0.30f, new Vector3(0.96f, 0.36f, 1f), GameConfig.ShelfTop, 4);
                    var sh = new Shelf { cell = new Vector2Int(tx, ty), world = w };
                    var iconGo = new GameObject("icon"); iconGo.transform.SetParent(root); iconGo.transform.position = w + new Vector3(0, 0.05f, -0.01f); iconGo.transform.localScale = Vector3.one * 0.62f;
                    sh.icon = iconGo.AddComponent<SpriteRenderer>(); sh.icon.sortingOrder = 6; sh.icon.enabled = false;
                    shelves.Add(sh);
                } else if (ch == 'R') {
                    registerCell = new Vector2Int(tx, ty);
                    Util.Quad(root, w, new Vector3(0.92f, 0.7f, 1f), GameConfig.Register, 3);
                    Util.Quad(root, w + Vector3.up * 0.28f, new Vector3(0.55f, 0.3f, 1f), GameConfig.ShelfTop, 4);
                } else if (ch == 'B') {
                    backroomCell = new Vector2Int(tx, ty);
                    Util.Quad(root, w, new Vector3(0.86f, 0.86f, 1f), GameConfig.WallTop, 3);
                    Util.Quad(root, w + Vector3.up * 0.1f, new Vector3(0.6f, 0.45f, 1f), GameConfig.Shelf, 4);
                } else if (ch == 'D') {
                    doorCells.Add(new Vector2Int(tx, ty));
                }
            }
        }
    }

    // colisão por tile (parede/prateleira/caixa/fundo bloqueiam)
    public bool IsSolid(float wx, float wy) {
        int tx = Mathf.RoundToInt(wx + W / 2f - 0.5f);
        int ty = (H - 1) - Mathf.RoundToInt(wy + H / 2f - 0.5f);
        if (ty < 0 || ty >= H || tx < 0 || tx >= W) return true;
        char ch = GameConfig.Map[ty][tx];
        return ch == '#' || ch == 'S' || ch == 'R' || ch == 'B';
    }

    public Vector3 DoorWorld() {
        if (doorCells.Count == 0) return Vector3.zero;
        Vector3 sum = Vector3.zero; foreach (var d in doorCells) sum += World(d.x, d.y); return sum / doorCells.Count + Vector3.up * 0.6f;
    }
    public Vector3 RegisterWorld() { return World(registerCell.x, registerCell.y); }
    public Vector3 BackroomWorld() { return World(backroomCell.x, backroomCell.y); }
    public Vector3 QueueWorld(int slot) {
        // fila em arco aberto na metade de baixo da loja
        Vector3 baseP = new Vector3(-1.5f, -1.6f, 0);
        return baseP + new Vector3((slot % 4) * 1.1f, -(slot / 4) * 1.0f, 0);
    }

    public Shelf ShelfWith(string id) { foreach (var s in shelves) if (s.productId == id && s.count > 0) return s; return null; }
    public Shelf NearestShelf(Vector3 p, float maxDist) {
        Shelf best = null; float bd = maxDist;
        foreach (var s in shelves) { float d = Vector3.Distance(p, s.world); if (d < bd) { bd = d; best = s; } }
        return best;
    }
    public float DistTo(Vector3 p, Vector3 cellWorld) { return Vector3.Distance(p, cellWorld); }

    public int CountExpired(int day) { int n = 0; foreach (var s in shelves) if (s.Expired(day)) n += s.count; return n; }
    public int ExpireOldStock(int day) {
        int n = 0; foreach (var s in shelves) if (s.Expired(day)) { n += s.count; s.SetProduct(null, 0, day); } return n;
    }
}
