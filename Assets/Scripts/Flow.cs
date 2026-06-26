using UnityEngine.SceneManagement;

// Flow.cs — nomes das cenas e navegação entre elas. Centraliza pra não digitar string solta.
public static class Flow {
    public const string Menu      = "Menu";
    public const string ComoJogar = "ComoJogar";
    public const string Nivel1    = "Nivel1";
    public const string Nivel2    = "Nivel2";
    public const string Nivel3    = "Nivel3";
    public const string Fim       = "Fim";

    public static string NivelDoLevel(int level) {
        if (level <= 1) return Nivel1;
        if (level == 2) return Nivel2;
        return Nivel3;
    }

    public static void Ir(string cena) { SceneManager.LoadScene(cena); }
}
