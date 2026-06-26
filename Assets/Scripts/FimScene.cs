using UnityEngine;
using UnityEngine.UI;

// FimScene.cs — controlador da CENA "Fim". A interface é montada dentro da cena pelo SetupTool;
// as referências (titulo/resumo/fundo) são ligadas no editor. Em runtime, mostra VITÓRIA ou
// GAME OVER conforme o resultado da partida (Run.victory).
public class FimScene : MonoBehaviour {
    public Text titulo;
    public Text resumo;
    public Image fundo;

    void Start() {
        App.Ensure();
        if (AudioManager.I != null) AudioManager.I.PlayMusic("ambiente");

        if (fundo)  fundo.color  = Run.victory ? new Color(0.05f, 0.30f, 0.13f) : new Color(0.30f, 0.08f, 0.07f);
        if (titulo) titulo.text  = Run.victory ? "VOCÊ VENCEU!" : "GAME OVER";
        if (resumo) resumo.text =
            (Run.victory
                ? "Você passou pelos 3 níveis e fez o Mercado Final prosperar!\n\n"
                : ((string.IsNullOrEmpty(Run.motivo) ? "O mercado fechou." : Run.motivo) + "\n\n")) +
            "Nível alcançado: " + Mathf.Clamp(Run.level, 1, 3) + " / 3\n" +
            "Dinheiro final: R$ " + Run.money.ToString("0") + "   ·   Reputação: " + Run.rep.ToString("0") + "\n" +
            "Clientes atendidos: " + Run.servedTotal + "   ·   perdidos: " + Run.lostTotal;
    }

    void Som() { if (AudioManager.I != null && AssetDB.I != null) AudioManager.I.Sfx(AssetDB.I.sfxDing); }

    public void JogarDeNovo() { Som(); Run.NovaPartida(); Flow.Ir(Flow.Nivel1); }
    public void IrMenu()      { Som(); Flow.Ir(Flow.Menu); }
}
