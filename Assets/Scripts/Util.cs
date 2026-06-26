using UnityEngine;
using System.Collections.Generic;

// Util.cs — helpers gerais (sprites sólidos procedurais p/ fallback e formas simples).
public static class Util {
    static Dictionary<int, Sprite> _solid = new Dictionary<int, Sprite>();

    // sprite quadrado de cor sólida (cacheado), 1 unidade de mundo
    public static Sprite SolidSprite(Color c) {
        int key = c.GetHashCode();
        if (_solid.TryGetValue(key, out var s) && s) return s;
        var t = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        t.filterMode = FilterMode.Point;
        var px = new Color[16]; for (int i = 0; i < 16; i++) px[i] = c; t.SetPixels(px); t.Apply();
        s = Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        _solid[key] = s; return s;
    }

    // cria um GameObject com SpriteRenderer de cor sólida numa célula de mundo
    public static SpriteRenderer Quad(Transform parent, Vector3 pos, Vector3 scale, Color c, int order) {
        var go = new GameObject("quad"); go.transform.SetParent(parent);
        go.transform.position = pos; go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = SolidSprite(c); sr.sortingOrder = order;
        return sr;
    }
}
