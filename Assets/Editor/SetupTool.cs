#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

// SetupTool.cs — menu de Editor que monta a cena do jogo num clique.
// Use: menu superior "Tudo Tem Preço" → "Criar Cena do Jogo", depois aperte PLAY.
public static class SetupTool {
    const string ScenePath = "Assets/MercadoFinal.unity";

    [MenuItem("Tudo Tem Preço/Criar Cena do Jogo")]
    public static void CreateScene() {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var go = new GameObject("Bootstrap");
        go.AddComponent<Bootstrap>();

        // Câmera persistente na cena: evita o Game view ficar PRETO ("No cameras rendering")
        // quando o Play está parado. Em runtime o Bootstrap reaproveita esta mesma câmera.
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 4.9f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.backgroundColor = new Color(0.10f, 0.07f, 0.05f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        camGo.tag = "MainCamera";
        camGo.AddComponent<AudioListener>();

        // Pré-visualização em modo EDIÇÃO: mostra a key art no Game view quando o Play está
        // PARADO (evita o "vazio/preto" confuso). O Bootstrap esconde isto ao dar Play.
        var keyTex = Resources.Load<Texture2D>("art/keyart");
        if (keyTex != null) {
            keyTex.filterMode = FilterMode.Point;
            var pv = new GameObject("PreviewArt");
            var psr = pv.AddComponent<SpriteRenderer>();
            psr.sprite = Sprite.Create(keyTex, new Rect(0, 0, keyTex.width, keyTex.height), new Vector2(0.5f, 0.5f), 100f);
            psr.sortingOrder = -100;
            pv.transform.position = new Vector3(0, 0, 1);
            float worldH = 4.9f * 2f, worldW = worldH * 16f / 9f;
            float spriteH = keyTex.height / 100f, spriteW = keyTex.width / 100f;
            float s = Mathf.Max(worldH / spriteH, worldW / spriteW) * 1.02f;
            pv.transform.localScale = new Vector3(s, s, 1f);
        }

        EditorSceneManager.SaveScene(scene, ScenePath);

        var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == ScenePath))
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();

        EditorUtility.DisplayDialog("Tudo Tem Preço",
            "Cena criada e salva em " + ScenePath + ".\n\nAgora é só apertar PLAY ▶ para jogar.\n\nPara gerar o .exe: File → Build Settings → Windows → Build.", "Beleza!");
    }
}
#endif
