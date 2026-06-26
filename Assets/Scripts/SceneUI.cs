using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// SceneUI.cs — helpers pra montar as telas simples (Menu, Como Jogar, Fim) por código:
// câmera, canvas, rótulos e BOTÕES COM SOM. Reaproveitado pelas três cenas de tela.
public static class SceneUI {
    public static Font GetFont() {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (f == null) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    // câmera ortográfica marrom + AudioListener (pra tocar o som dos botões/música)
    public static Camera MakeCamera() {
        var cam = Camera.main;
        if (cam == null) {
            var go = new GameObject("Main Camera");
            cam = go.AddComponent<Camera>();
            go.tag = "MainCamera";
            go.AddComponent<AudioListener>();
        }
        cam.orthographic = true; cam.orthographicSize = 4.9f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.10f, 0.07f, 0.05f);
        // esconde a prévia de edição (key art posta pelo SetupTool) ao entrar em jogo
        var pv = GameObject.Find("PreviewArt");
        if (pv != null) pv.SetActive(false);
        return cam;
    }

    // canvas em ScreenSpaceOverlay + EventSystem (necessário pros cliques)
    public static RectTransform MakeCanvas() {
        var cgo = new GameObject("Canvas");
        var canvas = cgo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = cgo.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(1280, 720);
        cgo.AddComponent<GraphicRaycaster>();
        if (Object.FindObjectOfType<EventSystem>() == null) {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        return canvas.GetComponent<RectTransform>();
    }

    // imagem da key art preenchendo a tela (com leve escurecida por cima)
    public static void KeyartBg(RectTransform parent, float dark = 0.25f) {
        var sp = AssetDB.I != null ? AssetDB.I.keyart : null;
        if (sp != null) {
            var go = new GameObject("keyart"); var rt = go.AddComponent<RectTransform>(); rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
            var im = go.AddComponent<Image>(); im.sprite = sp; im.preserveAspect = false; im.raycastTarget = false;
        }
        var dgo = new GameObject("escurece"); var drt = dgo.AddComponent<RectTransform>(); drt.SetParent(parent, false);
        drt.anchorMin = Vector2.zero; drt.anchorMax = Vector2.one; drt.offsetMin = Vector2.zero; drt.offsetMax = Vector2.zero;
        var dim = dgo.AddComponent<Image>(); dim.color = new Color(0.08f, 0.05f, 0.03f, dark); dim.raycastTarget = false;
    }

    public static Text Label(RectTransform parent, string s, int size, Color col, TextAnchor anchor, Vector2 aMin, Vector2 aMax) {
        var go = new GameObject("txt"); var rt = go.AddComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>(); t.text = s; t.font = GetFont(); t.fontSize = size; t.color = col;
        t.alignment = anchor; t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    // botão que toca o "ding" ao clicar
    public static Button Btn(RectTransform parent, string s, Vector2 aMin, Vector2 aMax, Color col, System.Action onClick) {
        var go = new GameObject("btn"); var rt = go.AddComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var im = go.AddComponent<Image>(); im.color = col;
        var b = go.AddComponent<Button>();
        b.onClick.AddListener(() => {
            if (AudioManager.I != null && AssetDB.I != null) AudioManager.I.Sfx(AssetDB.I.sfxDing);
            onClick?.Invoke();
        });
        Label(rt, s, 26, GameConfig.Ink, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
        return b;
    }
}
