using UnityEngine;

// MenuScene.cs — controlador da CENA "Menu". A interface (Canvas, título, botões) é montada
// DENTRO da cena pelo SetupTool (visível no editor). Aqui só ficam a música e as ações dos
// botões (ligadas pelo SetupTool via onClick).
public class MenuScene : MonoBehaviour {
    void Start() {
        App.Ensure();
        if (AudioManager.I != null) AudioManager.I.PlayMusic("ambiente");
    }
    void Som() { if (AudioManager.I != null && AssetDB.I != null) AudioManager.I.Sfx(AssetDB.I.sfxDing); }

    public void Iniciar()        { Som(); Run.NovaPartida(); Flow.Ir(Flow.Nivel1); }
    public void AbrirComoJogar() { Som(); Flow.Ir(Flow.ComoJogar); }
}
