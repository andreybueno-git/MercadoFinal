using UnityEngine;

// TutorialScene.cs — controlador da CENA "ComoJogar". A interface (texto explicativo, botão
// VOLTAR) é montada dentro da cena pelo SetupTool. Aqui só a música e a ação do botão.
public class TutorialScene : MonoBehaviour {
    void Start() {
        App.Ensure();
        if (AudioManager.I != null) AudioManager.I.PlayMusic("ambiente");
    }
    void Som() { if (AudioManager.I != null && AssetDB.I != null) AudioManager.I.Sfx(AssetDB.I.sfxDing); }

    public void Voltar() { Som(); Flow.Ir(Flow.Menu); }
}
