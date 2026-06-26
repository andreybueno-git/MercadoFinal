#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

// SetupTool.cs — cria TODAS as cenas do jogo já com CONTEÚDO VISÍVEL dentro delas (Canvas,
// textos, botões), na pasta Scenes, e registra no Build Settings. As telas Menu/ComoJogar/Fim
// têm a UI montada aqui (aparece no editor sem dar Play); os botões já vêm ligados. Os níveis
// montam o mercado em runtime (procedural) e mostram a key art como prévia no editor.
// Use: menu "Tudo Tem Preço" → "Criar Cenas do Jogo".
public static class SetupTool {
    static Font _font;
    static Sprite _keyart;
    static Font F() { if (_font == null) _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); return _font; }

    [MenuItem("Tudo Tem Preço/Criar Cenas do Jogo")]
    public static void CreateScenes() {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))  AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");

        EnsureKeyart();   // garante a key art como Sprite ANTES de montar as telas

        var scenes = new List<EditorBuildSettingsScene>();
        scenes.Add(BuildMenu());
        scenes.Add(BuildComoJogar());
        scenes.Add(BuildLevel("Nivel1", 1));
        scenes.Add(BuildLevel("Nivel2", 2));
        scenes.Add(BuildLevel("Nivel3", 3));
        scenes.Add(BuildFim());
        EditorBuildSettings.scenes = scenes.ToArray();

        if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/MercadoFinal.unity") != null)
            AssetDatabase.DeleteAsset("Assets/MercadoFinal.unity");

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");

        EditorUtility.DisplayDialog("Tudo Tem Preço",
            "6 cenas criadas em Assets/Scenes (com a interface montada dentro delas):\n\n" +
            "Menu · ComoJogar · Nivel1 · Nivel2 · Nivel3 · Fim\n\n" +
            "Com a cena Menu aberta, aperte PLAY ▶.\n\nGerar .exe: File → Build Settings → Windows → Build.", "Beleza!");
    }

    // ---------- helpers de cena ----------
    static Camera MakeCamera(Color bg) {
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 4.9f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = bg;
        camGo.tag = "MainCamera"; camGo.AddComponent<AudioListener>();
        return cam;
    }

    static RectTransform MakeCanvas() {
        var cgo = new GameObject("Canvas");
        var canvas = cgo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = cgo.AddComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1280, 720); sc.matchWidthOrHeight = 0.5f;
        cgo.AddComponent<GraphicRaycaster>();
        var es = new GameObject("EventSystem"); es.AddComponent<EventSystem>(); es.AddComponent<StandaloneInputModule>();
        return canvas.GetComponent<RectTransform>();
    }

    static Text EdText(Transform parent, string s, int size, Color col, TextAnchor anchor, Vector2 aMin, Vector2 aMax) {
        var go = new GameObject("text"); var rt = go.AddComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>(); t.font = F(); t.text = s; t.fontSize = size; t.color = col; t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; t.fontStyle = FontStyle.Bold; t.raycastTarget = false;
        return t;
    }

    static Image EdImage(Transform parent, Sprite sp, Color col, Vector2 aMin, Vector2 aMax) {
        var go = new GameObject(sp ? "imagem" : "fundo"); var rt = go.AddComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var im = go.AddComponent<Image>(); im.color = col; if (sp) { im.sprite = sp; im.preserveAspect = false; } im.raycastTarget = false;
        return im;
    }

    static Button EdButton(Transform parent, string label, Vector2 aMin, Vector2 aMax, Color col) {
        var go = new GameObject("Botao_" + label); var rt = go.AddComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var im = go.AddComponent<Image>(); im.color = col;
        var b = go.AddComponent<Button>();
        EdText(rt, label, 26, GameConfig.Ink, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
        return b;
    }

    static void EnsureKeyart() {
        var path = "Assets/Resources/art/keyart.png";
        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
        if (imp != null) {
            bool mudou = false;
            if (imp.textureType != TextureImporterType.Sprite) { imp.textureType = TextureImporterType.Sprite; mudou = true; }
            if (imp.spriteImportMode != SpriteImportMode.Single) { imp.spriteImportMode = SpriteImportMode.Single; mudou = true; }
            if (mudou) { imp.SaveAndReimport(); AssetDatabase.Refresh(); }
        }
        _keyart = AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static EditorBuildSettingsScene Save(string name, UnityEngine.SceneManagement.Scene scene) {
        string path = "Assets/Scenes/" + name + ".unity";
        EditorSceneManager.SaveScene(scene, path);
        return new EditorBuildSettingsScene(path, true);
    }

    // ---------- cenas de tela (UI montada no editor) ----------
    static EditorBuildSettingsScene BuildMenu() {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCamera(new Color(0.10f, 0.07f, 0.05f));
        var ctrl = new GameObject("MenuController").AddComponent<MenuScene>();
        var rt = MakeCanvas();
        EdImage(rt, _keyart, Color.white, Vector2.zero, Vector2.one);
        EdImage(rt, null, new Color(0.08f, 0.05f, 0.03f, 0.22f), Vector2.zero, Vector2.one);
        EdText(rt, "TUDO TEM PREÇO", 64, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0, 0.70f), new Vector2(1, 0.92f));
        EdText(rt, "Mercado Final — gerencie sua lojinha", 24, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0, 0.63f), new Vector2(1, 0.72f));
        var bIni = EdButton(rt, "INICIAR", new Vector2(0.36f, 0.40f), new Vector2(0.64f, 0.50f), GameConfig.Gold);
        var bCJ  = EdButton(rt, "COMO JOGAR", new Vector2(0.36f, 0.28f), new Vector2(0.64f, 0.37f), GameConfig.Paper);
        EdText(rt, "Pedro Henrique Araújo Ferreira · Gabriel Silva de Miranda · Rayane Araújo Teles · Andrey Bueno Isoton", 16, new Color(1, 1, 1, 0.7f), TextAnchor.LowerCenter, new Vector2(0, 0.01f), new Vector2(1, 0.06f));
        UnityEventTools.AddPersistentListener(bIni.onClick, new UnityAction(ctrl.Iniciar));
        UnityEventTools.AddPersistentListener(bCJ.onClick, new UnityAction(ctrl.AbrirComoJogar));
        return Save("Menu", scene);
    }

    static EditorBuildSettingsScene BuildComoJogar() {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCamera(new Color(0.10f, 0.07f, 0.05f));
        var ctrl = new GameObject("ComoJogarController").AddComponent<TutorialScene>();
        var rt = MakeCanvas();
        EdImage(rt, null, new Color(0.10f, 0.07f, 0.05f, 1f), Vector2.zero, Vector2.one);
        EdText(rt, "COMO JOGAR", 44, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0, 0.84f), new Vector2(1, 0.95f));
        string txt =
            "• Ande pelo mercado com WASD / setas.\n" +
            "• Clientes entram querendo um produto (veja o balão).\n" +
            "• Chegue perto e aperte E/Espaço para ATENDER (precisa do produto na prateleira).\n" +
            "• Aperte E perto de uma PRATELEIRA para REPOR do estoque.\n" +
            "• Aperte E perto do FUNDO (caixas) para COMPRAR de fornecedores.\n" +
            "• Produtos perecíveis VENCEM — venda antes ou perde reputação.\n" +
            "• Cada nível tem uma META de dinheiro. Bata a meta antes de acabar o tempo!\n" +
            "• São 3 níveis; o dinheiro e a reputação passam de um pro outro.\n" +
            "• Se o dinheiro zerar ou a reputação despencar: GAME OVER.";
        EdText(rt, txt, 22, GameConfig.Paper, TextAnchor.UpperLeft, new Vector2(0.12f, 0.18f), new Vector2(0.88f, 0.82f));
        var b = EdButton(rt, "VOLTAR", new Vector2(0.38f, 0.07f), new Vector2(0.62f, 0.16f), GameConfig.Paper);
        UnityEventTools.AddPersistentListener(b.onClick, new UnityAction(ctrl.Voltar));
        return Save("ComoJogar", scene);
    }

    static EditorBuildSettingsScene BuildFim() {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        MakeCamera(new Color(0.18f, 0.05f, 0.04f));
        var ctrl = new GameObject("FimController").AddComponent<FimScene>();
        var rt = MakeCanvas();
        var fundo  = EdImage(rt, null, new Color(0.30f, 0.08f, 0.07f, 1f), Vector2.zero, Vector2.one);
        var titulo = EdText(rt, "GAME OVER", 58, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0, 0.70f), new Vector2(1, 0.9f));
        var resumo = EdText(rt, "(o resumo da partida aparece aqui ao terminar)", 26, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0.1f, 0.34f), new Vector2(0.9f, 0.68f));
        var bJ = EdButton(rt, "JOGAR DE NOVO", new Vector2(0.34f, 0.18f), new Vector2(0.66f, 0.29f), GameConfig.Gold);
        var bM = EdButton(rt, "MENU", new Vector2(0.40f, 0.07f), new Vector2(0.60f, 0.16f), GameConfig.Paper);
        ctrl.fundo = fundo; ctrl.titulo = titulo; ctrl.resumo = resumo;
        UnityEventTools.AddPersistentListener(bJ.onClick, new UnityAction(ctrl.JogarDeNovo));
        UnityEventTools.AddPersistentListener(bM.onClick, new UnityAction(ctrl.IrMenu));
        return Save("Fim", scene);
    }

    // ---------- cenas jogáveis (mercado montado em runtime) ----------
    static EditorBuildSettingsScene BuildLevel(string name, int level) {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject(name + "Controller");
        go.AddComponent<GameScene>().level = level;
        MakeCamera(new Color(0.10f, 0.07f, 0.05f));

        // prévia: key art no Game view em modo edição (o controlador esconde ao dar Play)
        var keyTex = Resources.Load<Texture2D>("art/keyart");
        if (keyTex != null) {
            keyTex.filterMode = FilterMode.Point;
            var pv = new GameObject("PreviewArt");
            var psr = pv.AddComponent<SpriteRenderer>();
            psr.sprite = Sprite.Create(keyTex, new Rect(0, 0, keyTex.width, keyTex.height), new Vector2(0.5f, 0.5f), 100f);
            psr.sortingOrder = -100;
            pv.transform.position = new Vector3(0, 0, 1);
            float worldH = 4.9f * 2f, worldW = worldH * 16f / 9f;
            float s = Mathf.Max(worldH / (keyTex.height / 100f), worldW / (keyTex.width / 100f)) * 1.02f;
            pv.transform.localScale = new Vector3(s, s, 1f);
        }
        return Save(name, scene);
    }
}
#endif
