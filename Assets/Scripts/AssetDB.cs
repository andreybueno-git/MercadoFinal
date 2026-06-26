using UnityEngine;
using System.Collections.Generic;

// AssetDB.cs — carrega as imagens/áudio da Higgsfield de Resources e fatia as folhas
// (chars/products) em sprites, com filtro POINT (pixel art nítido). Tudo por código,
// então não precisa configurar Import Settings no Editor. Tolerante a asset faltando.
public class AssetDB {
    public static AssetDB I;

    public Sprite keyart;
    public Sprite[] chars;       // 0 = lojista, 1..4 = clientes
    public Sprite[] products;    // ícones de produto (índice = ProductDef.icon)
    public Sprite[] playerWalk;  // frames do ciclo de caminhada do lojista
    public Sprite playerHold;    // lojista segurando sacola (ao atender)
    public AudioClip bgmAmbiente, bgmCrise, sfxCaixa, sfxDing;

    public void Load() {
        I = this;
        keyart   = LoadSprite("art/keyart", 100);
        chars    = LoadCharSet();
        playerWalk = LoadWalk();                       // frames de caminhada (opcional)
        playerHold = LoadSprite("art/player_hold", 160f); // pose segurando (opcional)
        products = SliceSheet("art/products", GameConfig.ProductCols, GameConfig.ProductRows);
        bgmAmbiente = Resources.Load<AudioClip>("audio/bgm_ambiente");
        bgmCrise    = Resources.Load<AudioClip>("audio/bgm_crise"); // opcional
        sfxCaixa    = Resources.Load<AudioClip>("audio/sfx_caixa");
        sfxDing     = Resources.Load<AudioClip>("audio/sfx_ding");
    }

    static Texture2D LoadTex(string path) {
        var t = Resources.Load<Texture2D>(path);
        if (t != null) t.filterMode = FilterMode.Point; // pixel art nítido
        return t;
    }

    public static Sprite LoadSprite(string path, float ppu) {
        var t = LoadTex(path);
        if (t == null) return null;
        var sp = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), ppu);
        sp.name = path; // usado pela lógica de tint de paciência (StartsWith("art"))
        return sp;
    }

    // Personagens como PNGs individuais com fundo TRANSPARENTE (player + 4 clientes),
    // gerados na Higgsfield e recortados via chroma-key. Se algum faltar, cai de volta
    // pra folha antiga art/chars (mantém compatibilidade).
    static Sprite[] LoadCharSet() {
        string[] names = { "art/player", "art/cust_apressado", "art/cust_educado", "art/cust_reclamao", "art/cust_fiel" };
        var arr = new Sprite[names.Length];
        bool anyMissing = false;
        for (int i = 0; i < names.Length; i++) {
            arr[i] = LoadSprite(names[i], 160f); // ppu ~160 → personagem ≈ 1.5 tiles de altura
            if (arr[i] == null) anyMissing = true;
        }
        if (anyMissing) {
            var sheet = SliceSheet("art/chars", GameConfig.CharCols, GameConfig.CharRows);
            for (int i = 0; i < arr.Length && i < sheet.Length; i++) if (arr[i] == null) arr[i] = sheet[i];
        }
        return arr;
    }

    // Frames de caminhada do lojista: art/player_walk_0, _1, ... (para quando faltar um).
    // Vazio = sem frames (o FrameWalk usa fallback procedural).
    static Sprite[] LoadWalk() {
        var list = new List<Sprite>();
        for (int i = 0; i < 12; i++) {
            var sp = LoadSprite("art/player_walk_" + i, 160f);
            if (sp == null) break;
            list.Add(sp);
        }
        return list.ToArray();
    }

    // fatia uma folha cols×rows em sprites individuais; pivô central, ppu ~ por célula
    public static Sprite[] SliceSheet(string path, int cols, int rows) {
        var t = LoadTex(path);
        if (t == null) return new Sprite[0];
        int cw = t.width / cols, ch = t.height / rows;
        var list = new List<Sprite>();
        // ppu = célula / ~1 unidade de mundo (queremos cada célula ≈ 1 tile). Ajustado no uso.
        float ppu = cw; // 1 célula ≈ 1 unidade de mundo
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++) {
                // Texturas têm origem no canto inferior-esquerdo; a folha foi feita top-left → inverte Y
                int rect_y = t.height - (r + 1) * ch;
                var sp = Sprite.Create(t, new Rect(c * cw, rect_y, cw, ch), new Vector2(0.5f, 0.5f), ppu);
                sp.name = path + "_" + (r * cols + c);
                list.Add(sp);
            }
        return list.ToArray();
    }

    public Sprite Product(int icon) { return (products != null && icon >= 0 && icon < products.Length) ? products[icon] : null; }
    public Sprite Char(int i) { return (chars != null && i >= 0 && i < chars.Length) ? chars[i] : null; }
}
