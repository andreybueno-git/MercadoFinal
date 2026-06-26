using UnityEngine;
using UnityEngine.EventSystems;

// Bootstrap.cs — ponto de entrada. Coloque este componente num GameObject vazio na cena
// (o menu "Tudo Tem Preço/Criar Cena do Jogo" faz isso) e dê Play. Ele monta TODO o jogo
// por código: câmera, assets, managers, jogador, EventSystem. Nenhum prefab necessário.
public class Bootstrap : MonoBehaviour {
    void Awake() {
        // 1) assets (sprites + áudio de Resources)
        var db = new AssetDB(); db.Load();

        // 2) câmera ortográfica que enquadra o mercado inteiro. Reaproveita a câmera que a
        //    cena já tem (criada pelo menu de setup); assim, ao PARAR o Play, o Game view
        //    mostra o fundo da câmera em vez de "No cameras rendering" (tela preta).
        var cam = Camera.main;
        if (cam == null) {
            var camGo = new GameObject("Main Camera");
            cam = camGo.AddComponent<Camera>();
            camGo.tag = "MainCamera";
            camGo.AddComponent<AudioListener>(); // ouvir música e SFX
        }
        cam.orthographic = true; cam.orthographicSize = 4.9f;
        cam.transform.position = new Vector3(0, 0, -10);
        cam.backgroundColor = new Color(0.10f, 0.07f, 0.05f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        // esconde a pré-visualização de edição (key art) — o jogo monta a própria UI
        var preview = GameObject.Find("PreviewArt");
        if (preview != null) preview.SetActive(false);

        // 3) EventSystem (necessário p/ os botões da UI funcionarem)
        if (FindObjectOfType<EventSystem>() == null) {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }

        // 4) systems (a ordem importa: AssetDB já carregado; UI por último pois usa keyart)
        var sys = new GameObject("Systems");
        sys.AddComponent<AudioManager>();
        sys.AddComponent<Market>();
        sys.AddComponent<GameManager>();

        // 5) jogador
        var pl = new GameObject("Jogador");
        pl.AddComponent<PlayerCtrl>();

        // 6) UI (constrói canvas/HUD/telas)
        sys.AddComponent<UI>();

        // 7) começa no menu
        GameManager.I.SetScreen(Screen.Menu);
    }
}
