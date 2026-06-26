using UnityEngine;

// App.cs — objeto PERSISTENTE (sobrevive à troca de cenas). Carrega os assets uma vez e
// mantém o AudioManager vivo entre as cenas (Menu → Níveis → Fim). Cada cena chama
// App.Ensure() no começo, então AssetDB.I e AudioManager.I existem em qualquer cena.
public class App : MonoBehaviour {
    public static App I;

    void Awake() {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        new AssetDB().Load();                 // sprites + áudio (uma vez só)
        gameObject.AddComponent<AudioManager>();
    }

    public static void Ensure() {
        if (I == null) {
            var g = new GameObject("App");
            g.AddComponent<App>();
        }
    }
}
