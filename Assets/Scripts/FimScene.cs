using UnityEngine;

// FimScene.cs — controlador da CENA "Fim". Mostra VITÓRIA ou GAME OVER (lendo Run.victory),
// o resumo, e botões JOGAR DE NOVO / MENU.
public class FimScene : MonoBehaviour {
    void Start() {
        App.Ensure();
        var cam = SceneUI.MakeCamera();
        cam.backgroundColor = Run.victory ? new Color(0.05f, 0.14f, 0.07f) : new Color(0.18f, 0.05f, 0.04f);
        var rt = SceneUI.MakeCanvas();

        if (Run.victory) {
            SceneUI.Label(rt, "VOCÊ VENCEU!", 60, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0, 0.70f), new Vector2(1, 0.9f));
            SceneUI.Label(rt, "Você passou pelos 3 níveis e fez o Mercado Final prosperar!", 24, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.7f));
        } else {
            SceneUI.Label(rt, "GAME OVER", 60, new Color(0.95f, 0.5f, 0.45f), TextAnchor.UpperCenter, new Vector2(0, 0.70f), new Vector2(1, 0.9f));
            SceneUI.Label(rt, string.IsNullOrEmpty(Run.motivo) ? "O mercado fechou." : Run.motivo, 24, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0.1f, 0.58f), new Vector2(0.9f, 0.7f));
        }

        string resumo =
            "Nível alcançado: " + Mathf.Clamp(Run.level, 1, 3) + " / 3\n" +
            "Dinheiro final: R$ " + Run.money.ToString("0.00") + "\n" +
            "Reputação: " + Run.rep.ToString("0") + "\n" +
            "Clientes atendidos: " + Run.servedTotal + "   ·   perdidos: " + Run.lostTotal;
        SceneUI.Label(rt, resumo, 24, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0.1f, 0.36f), new Vector2(0.9f, 0.56f));

        SceneUI.Btn(rt, "JOGAR DE NOVO", new Vector2(0.34f, 0.20f), new Vector2(0.66f, 0.30f), GameConfig.Gold,
            () => { Run.NovaPartida(); Flow.Ir(Flow.Nivel1); });
        SceneUI.Btn(rt, "MENU", new Vector2(0.40f, 0.08f), new Vector2(0.60f, 0.17f), GameConfig.Paper,
            () => Flow.Ir(Flow.Menu));

        if (AudioManager.I != null) AudioManager.I.PlayMusic("ambiente");
    }
}
