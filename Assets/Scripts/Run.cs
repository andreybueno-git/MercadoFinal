using System.Collections.Generic;

// Run.cs — estado da PARTIDA que passa de uma cena pra outra (Nível 1 → 2 → 3 → Fim).
// É estático, então sobrevive ao SceneManager.LoadScene sem precisar de objeto persistente.
public static class Run {
    public static int level = 1;            // nível atual (1..3)
    public static float money, rep;         // dinheiro e reputação acumulados
    public static int day;                  // dia/contador
    public static bool victory;             // resultado mostrado na cena Fim
    public static bool iniciado;            // já começou uma partida? (proteção)
    public static string motivo = "";       // texto do porquê acabou

    // estoque e produtos desbloqueados passam entre os níveis
    public static Dictionary<string, int> backstock = new Dictionary<string, int>();
    public static List<string> unlocked = new List<string>();

    // estatísticas acumuladas (pra tela de fim)
    public static int servedTotal, lostTotal, expiredTotal;

    // começa uma partida nova do zero (chamado pelo Menu ao apertar INICIAR)
    public static void NovaPartida() {
        level = 1;
        money = GameConfig.StartMoney;
        rep = GameConfig.StartRep;
        day = 1;
        victory = false; motivo = ""; iniciado = true;
        backstock = new Dictionary<string, int>();
        unlocked = new List<string>();
        servedTotal = lostTotal = expiredTotal = 0;
    }
}
