using UnityEngine;

// TutorialScene.cs — controlador da CENA "ComoJogar". Explica os controles e regras.
public class TutorialScene : MonoBehaviour {
    void Start() {
        App.Ensure();
        SceneUI.MakeCamera();
        var rt = SceneUI.MakeCanvas();
        SceneUI.KeyartBg(rt, 0.85f);

        SceneUI.Label(rt, "COMO JOGAR", 44, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0, 0.84f), new Vector2(1, 0.95f));

        string txt =
            "• Ande pelo mercado com WASD / setas.\n" +
            "• Clientes entram querendo um produto (veja o balão).\n" +
            "• Vá até o cliente e aperte E/Espaço para ATENDER (precisa do produto na prateleira).\n" +
            "• Aperte E perto de uma PRATELEIRA para REPOR do estoque.\n" +
            "• Aperte E perto do FUNDO (caixas) para COMPRAR de fornecedores.\n" +
            "• Produtos perecíveis VENCEM — venda antes ou perde reputação.\n" +
            "• Cada nível tem uma META de dinheiro. Bata a meta antes de acabar o tempo!\n" +
            "• São 3 níveis. O dinheiro e a reputação passam de um pro outro.\n" +
            "• Se o dinheiro zerar, a reputação despencar, ou não bater a meta: GAME OVER.";
        SceneUI.Label(rt, txt, 22, GameConfig.Paper, TextAnchor.UpperLeft, new Vector2(0.12f, 0.20f), new Vector2(0.88f, 0.82f));

        SceneUI.Btn(rt, "VOLTAR", new Vector2(0.38f, 0.07f), new Vector2(0.62f, 0.16f), GameConfig.Paper,
            () => Flow.Ir(Flow.Menu));

        if (AudioManager.I != null) AudioManager.I.PlayMusic("ambiente");
    }
}
