using UnityEngine;

// MenuScene.cs — controlador da CENA "Menu". Tela inicial com key art, título, som e botões.
public class MenuScene : MonoBehaviour {
    void Start() {
        App.Ensure();
        SceneUI.MakeCamera();
        var rt = SceneUI.MakeCanvas();
        SceneUI.KeyartBg(rt, 0.22f);

        SceneUI.Label(rt, "TUDO TEM PREÇO", 64, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0, 0.70f), new Vector2(1, 0.92f));
        SceneUI.Label(rt, "Mercado Final — gerencie sua lojinha", 24, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0, 0.63f), new Vector2(1, 0.72f));

        SceneUI.Btn(rt, "INICIAR", new Vector2(0.36f, 0.40f), new Vector2(0.64f, 0.50f), GameConfig.Gold,
            () => { Run.NovaPartida(); Flow.Ir(Flow.Nivel1); });
        SceneUI.Btn(rt, "COMO JOGAR", new Vector2(0.36f, 0.28f), new Vector2(0.64f, 0.37f), GameConfig.Paper,
            () => Flow.Ir(Flow.ComoJogar));

        SceneUI.Label(rt, "Pedro · Gabriel · Andrey · Rayane", 16, new Color(1, 1, 1, 0.7f), TextAnchor.LowerCenter, new Vector2(0, 0.01f), new Vector2(1, 0.06f));

        if (AudioManager.I != null) AudioManager.I.PlayMusic("ambiente");
    }
}
