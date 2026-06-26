#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

// SetupTool.cs — menu de Editor que cria TODAS as cenas do jogo de uma vez, na pasta Scenes,
// e as registra no Build Settings. Use: menu "Tudo Tem Preço" → "Criar Cenas do Jogo".
public static class SetupTool {

    [MenuItem("Tudo Tem Preço/Criar Cenas do Jogo")]
    public static void CreateScenes() {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))  AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets", "Prefabs");

        var scenes = new List<EditorBuildSettingsScene>();
        scenes.Add(BuildScene("Menu",      typeof(MenuScene),     0));
        scenes.Add(BuildScene("ComoJogar", typeof(TutorialScene), 0));
        scenes.Add(BuildScene("Nivel1",    typeof(GameScene),     1));
        scenes.Add(BuildScene("Nivel2",    typeof(GameScene),     2));
        scenes.Add(BuildScene("Nivel3",    typeof(GameScene),     3));
        scenes.Add(BuildScene("Fim",       typeof(FimScene),      0));
        EditorBuildSettings.scenes = scenes.ToArray();

        // remove a cena antiga single-scene, se existir
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/MercadoFinal.unity") != null)
            AssetDatabase.DeleteAsset("Assets/MercadoFinal.unity");

        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene("Assets/Scenes/Menu.unity");

        EditorUtility.DisplayDialog("Tudo Tem Preço",
            "6 cenas criadas em Assets/Scenes:\n\nMenu · ComoJogar · Nivel1 · Nivel2 · Nivel3 · Fim\n\n" +
            "Com a cena Menu aberta, aperte PLAY ▶ para jogar do começo.\n\n" +
            "Gerar .exe: File → Build Settings → Windows → Build.", "Beleza!");
    }

    static EditorBuildSettingsScene BuildScene(string name, System.Type controller, int level) {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // controlador da cena (MenuScene / TutorialScene / GameScene / FimScene)
        var go = new GameObject(name + "Controller");
        var comp = go.AddComponent(controller);
        if (controller == typeof(GameScene)) ((GameScene)comp).level = level;

        // câmera (aparece no editor e é reaproveitada em runtime)
        var camGo = new GameObject("Main Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true; cam.orthographicSize = 4.9f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.10f, 0.07f, 0.05f);
        camGo.tag = "MainCamera";
        camGo.AddComponent<AudioListener>();

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

        string path = "Assets/Scenes/" + name + ".unity";
        EditorSceneManager.SaveScene(scene, path);
        return new EditorBuildSettingsScene(path, true);
    }
}
#endif
