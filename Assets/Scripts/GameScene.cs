using UnityEngine;

// GameScene.cs — controlador das CENAS jogáveis (Nivel1/Nivel2/Nivel3). Monta o mercado,
// o jogador e o HUD, e inicia o nível. O campo `level` é definido por cada cena (1, 2 ou 3).
public class GameScene : MonoBehaviour {
    public int level = 1;

    void Start() {
        App.Ensure();                 // garante AssetDB + AudioManager (persistentes)
        if (!Run.iniciado) Run.NovaPartida();   // abriu o nível direto? começa uma partida
        Run.level = level;

        SceneUI.MakeCamera();         // câmera marrom + AudioListener

        var sys = new GameObject("Systems");
        sys.AddComponent<Market>();
        sys.AddComponent<GameManager>();

        var pl = new GameObject("Jogador");
        pl.AddComponent<PlayerCtrl>();

        sys.AddComponent<UI>();       // HUD + diálogos (evento/compra/relatório)

        GameManager.I.StartLevel();
    }
}
