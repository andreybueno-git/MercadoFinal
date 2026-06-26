using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// UI.cs — TODA a interface construída por código (uGUI): HUD + telas (menu/fim-de-dia/
// gameover/vitória) + diálogo de evento moral + painel de compra + toasts + textos flutuantes.
public class UI : MonoBehaviour {
    public static UI I;
    public bool modal = false;        // diálogo aberto → pausa o jogo

    Canvas canvas; Font font;
    GameObject hudRoot, menuRoot, tutoRoot, dayendRoot, gameoverRoot, victoryRoot, eventRoot, buyRoot;
    Text tMoney, tRep, tDay, tTime, tCust, tHint, tToast, tDayReport, tGOstats, tVICstats, tEventTitle, tEventText;
    Image repBar, timeBar;
    Transform eventOpts, buyList; Text buySupplier; int buySupIdx = 0;
    float toastT = 0f;
    List<RectTransform> floats = new List<RectTransform>();

    static Font GetFont() {
        var f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!f) f = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return f;
    }

    void Awake() {
        I = this; font = GetFont();
        var cgo = new GameObject("Canvas"); cgo.transform.SetParent(transform);
        canvas = cgo.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var sc = cgo.AddComponent<CanvasScaler>(); sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; sc.referenceResolution = new Vector2(1280, 720); sc.matchWidthOrHeight = 0.5f;
        cgo.AddComponent<GraphicRaycaster>();
        BuildHUD(); BuildMenu(); BuildTutorial(); BuildDayEnd(); BuildGameOver(); BuildVictory(); BuildEvent(); BuildBuy();
        OnScreen(Screen.Menu);
    }

    // ---------- helpers ----------
    RectTransform Panel(Transform parent, Vector2 aMin, Vector2 aMax, Color col) {
        var go = new GameObject("panel"); var rt = go.AddComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var im = go.AddComponent<Image>(); im.color = col; return rt;
    }
    Text Label(Transform parent, string s, int size, Color col, TextAnchor anchor, Vector2 aMin, Vector2 aMax) {
        var go = new GameObject("text"); var rt = go.AddComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var t = go.AddComponent<Text>(); t.font = font; t.text = s; t.fontSize = size; t.color = col; t.alignment = anchor;
        t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Overflow; t.fontStyle = FontStyle.Bold;
        t.raycastTarget = false; // não bloquear cliques dos botões por baixo
        return t;
    }
    Button Btn(Transform parent, string s, Vector2 aMin, Vector2 aMax, Color col, System.Action onClick) {
        var go = new GameObject("btn"); var rt = go.AddComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var im = go.AddComponent<Image>(); im.color = col;
        var b = go.AddComponent<Button>(); b.onClick.AddListener(() => { onClick?.Invoke(); AudioManager.I.Sfx(AssetDB.I.sfxDing); });
        var t = Label(rt, s, 26, GameConfig.Ink, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
        return b;
    }

    // ---------- HUD ----------
    void BuildHUD() {
        hudRoot = new GameObject("HUD"); var rt = hudRoot.AddComponent<RectTransform>(); rt.SetParent(canvas.transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var bar = Panel(rt, new Vector2(0, 0.92f), new Vector2(1, 1f), new Color(0.13f, 0.10f, 0.08f, 0.85f));
        tMoney = Label(bar, "R$ 0", 30, GameConfig.Gold, TextAnchor.MiddleLeft, new Vector2(0.01f, 0), new Vector2(0.22f, 1));
        tDay   = Label(bar, "Dia 1", 24, GameConfig.Paper, TextAnchor.MiddleCenter, new Vector2(0.22f, 0), new Vector2(0.38f, 1));
        Label(bar, "Reputação", 16, GameConfig.Paper, TextAnchor.MiddleLeft, new Vector2(0.40f, 0.55f), new Vector2(0.62f, 1));
        var repBg = Panel(bar, new Vector2(0.40f, 0.18f), new Vector2(0.62f, 0.5f), new Color(0, 0, 0, 0.4f));
        repBar = Panel(repBg, Vector2.zero, new Vector2(0.7f, 1), GameConfig.Good).GetComponent<Image>();
        Label(bar, "Tempo do dia", 16, GameConfig.Paper, TextAnchor.MiddleLeft, new Vector2(0.64f, 0.55f), new Vector2(0.86f, 1));
        var tBg = Panel(bar, new Vector2(0.64f, 0.18f), new Vector2(0.86f, 0.5f), new Color(0, 0, 0, 0.4f));
        timeBar = Panel(tBg, Vector2.zero, new Vector2(0f, 1), GameConfig.Gold).GetComponent<Image>();
        tCust  = Label(bar, "Clientes: 0", 20, GameConfig.Paper, TextAnchor.MiddleRight, new Vector2(0.86f, 0), new Vector2(0.99f, 1));
        tHint = Label(rt, "WASD/setas: andar  ·  E/Espaço: atender / repor / comprar", 18, new Color(1,1,1,0.85f), TextAnchor.LowerCenter, new Vector2(0,0), new Vector2(1,0.06f));
    }

    // ---------- MENU ----------
    void BuildMenu() {
        menuRoot = new GameObject("Menu"); var rt = menuRoot.AddComponent<RectTransform>(); rt.SetParent(canvas.transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        // key art de fundo
        var bgGo = new GameObject("keyart"); var brt = bgGo.AddComponent<RectTransform>(); brt.SetParent(rt, false);
        brt.anchorMin = Vector2.zero; brt.anchorMax = Vector2.one; brt.offsetMin = Vector2.zero; brt.offsetMax = Vector2.zero;
        var bim = bgGo.AddComponent<Image>(); bim.color = new Color(0.5f,0.42f,0.32f);
        if (AssetDB.I != null && AssetDB.I.keyart) { bim.sprite = AssetDB.I.keyart; bim.color = Color.white; bim.preserveAspect = false; }
        Panel(rt, Vector2.zero, Vector2.one, new Color(0.08f, 0.05f, 0.03f, 0.25f)); // leve escurecida
        Label(rt, "TUDO TEM PREÇO", 64, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0, 0.70f), new Vector2(1, 0.92f));
        Label(rt, "Mercado Final — gerencie sua lojinha", 24, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0, 0.63f), new Vector2(1, 0.72f));
        Btn(rt, "INICIAR", new Vector2(0.36f, 0.40f), new Vector2(0.64f, 0.50f), GameConfig.Gold, () => GameManager.I.NewGame());
        Btn(rt, "COMO JOGAR", new Vector2(0.36f, 0.28f), new Vector2(0.64f, 0.37f), GameConfig.Paper, () => OnScreen(Screen.Tutorial));
        Label(rt, "Pedro · Gabriel · Andrey · Rayane", 16, new Color(1,1,1,0.7f), TextAnchor.LowerCenter, new Vector2(0,0.01f), new Vector2(1,0.06f));
    }

    void BuildTutorial() {
        tutoRoot = FullPanel("Tutorial", new Color(0.10f,0.07f,0.05f,0.95f)).gameObject;
        Label(tutoRoot.transform, "COMO JOGAR", 44, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0,0.84f), new Vector2(1,0.95f));
        string txt =
            "• Ande pelo mercado com WASD / setas.\n" +
            "• Clientes entram querendo um produto (veja o balão).\n" +
            "• Vá até o cliente e aperte E/Espaço para ATENDER (precisa do produto na prateleira).\n" +
            "• Aperte E perto de uma PRATELEIRA para REPOR do estoque.\n" +
            "• Aperte E perto do FUNDO (caixas) para COMPRAR de fornecedores.\n" +
            "• Produtos vencem! Reponha e venda antes do prazo.\n" +
            "• Cada dia tem despesas. Não deixe o dinheiro zerar.\n" +
            "• Eventos morais aparecem: cada escolha mexe em dinheiro e reputação.\n\n" +
            "Meta: chegar a R$ " + GameConfig.GoalMoney.ToString("0") + " sem perder a reputação.";
        Label(tutoRoot.transform, txt, 22, GameConfig.Paper, TextAnchor.UpperLeft, new Vector2(0.12f,0.22f), new Vector2(0.88f,0.82f));
        Btn(tutoRoot.transform, "VOLTAR", new Vector2(0.38f,0.08f), new Vector2(0.62f,0.17f), GameConfig.Paper, () => OnScreen(Screen.Menu));
    }

    RectTransform FullPanel(string name, Color col) {
        var go = new GameObject(name); var rt = go.AddComponent<RectTransform>(); rt.SetParent(canvas.transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = col; return rt;
    }

    // ---------- FIM DE DIA ----------
    void BuildDayEnd() {
        dayendRoot = FullPanel("DayEnd", new Color(0.10f,0.07f,0.05f,0.92f)).gameObject;
        Label(dayendRoot.transform, "FIM DO DIA", 48, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0,0.80f), new Vector2(1,0.92f));
        tDayReport = Label(dayendRoot.transform, "", 26, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0.15f,0.30f), new Vector2(0.85f,0.78f));
        Btn(dayendRoot.transform, "PRÓXIMO DIA", new Vector2(0.34f,0.12f), new Vector2(0.66f,0.23f), GameConfig.Gold, () => GameManager.I.NextDay());
    }

    void BuildGameOver() {
        gameoverRoot = FullPanel("GameOver", new Color(0.18f,0.05f,0.04f,0.95f)).gameObject;
        Label(gameoverRoot.transform, "MERCADO FECHADO", 52, GameConfig.Bad, TextAnchor.UpperCenter, new Vector2(0,0.74f), new Vector2(1,0.9f));
        tGOstats = Label(gameoverRoot.transform, "", 26, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0.15f,0.4f), new Vector2(0.85f,0.72f));
        Btn(gameoverRoot.transform, "JOGAR DE NOVO", new Vector2(0.34f,0.22f), new Vector2(0.66f,0.32f), GameConfig.Gold, () => GameManager.I.NewGame());
        Btn(gameoverRoot.transform, "MENU", new Vector2(0.40f,0.10f), new Vector2(0.60f,0.19f), GameConfig.Paper, () => GameManager.I.BackToMenu());
    }

    void BuildVictory() {
        victoryRoot = FullPanel("Victory", new Color(0.05f,0.14f,0.07f,0.95f)).gameObject;
        Label(victoryRoot.transform, "VOCÊ VENCEU!", 56, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0,0.72f), new Vector2(1,0.9f));
        tVICstats = Label(victoryRoot.transform, "", 26, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0.15f,0.4f), new Vector2(0.85f,0.7f));
        Btn(victoryRoot.transform, "JOGAR DE NOVO", new Vector2(0.34f,0.22f), new Vector2(0.66f,0.32f), GameConfig.Gold, () => GameManager.I.NewGame());
        Btn(victoryRoot.transform, "MENU", new Vector2(0.40f,0.10f), new Vector2(0.60f,0.19f), GameConfig.Paper, () => GameManager.I.BackToMenu());
    }

    // ---------- EVENTO MORAL ----------
    void BuildEvent() {
        eventRoot = new GameObject("Event"); var rt = eventRoot.AddComponent<RectTransform>(); rt.SetParent(canvas.transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        Panel(rt, Vector2.zero, Vector2.one, new Color(0,0,0,0.6f));
        var box = Panel(rt, new Vector2(0.18f,0.22f), new Vector2(0.82f,0.78f), GameConfig.Paper);
        tEventTitle = Label(box, "", 34, GameConfig.Bad, TextAnchor.UpperCenter, new Vector2(0.05f,0.78f), new Vector2(0.95f,0.96f));
        tEventText = Label(box, "", 24, GameConfig.Ink, TextAnchor.UpperCenter, new Vector2(0.08f,0.5f), new Vector2(0.92f,0.78f));
        var optsGo = new GameObject("opts"); var ort = optsGo.AddComponent<RectTransform>(); ort.SetParent(box, false);
        ort.anchorMin = new Vector2(0.08f,0.06f); ort.anchorMax = new Vector2(0.92f,0.46f); ort.offsetMin = Vector2.zero; ort.offsetMax = Vector2.zero;
        eventOpts = ort;
    }

    public void ShowEvent(ChoiceEvent ev) {
        modal = true; eventRoot.SetActive(true); eventRoot.transform.SetAsLastSibling();
        tEventTitle.text = ev.titulo; tEventText.text = ev.texto;
        foreach (Transform c in eventOpts) Destroy(c.gameObject);
        int n = ev.opcoes.Length;
        for (int i = 0; i < n; i++) {
            var opt = ev.opcoes[i];
            float h = 1f / n; float y0 = 1f - (i + 1) * h, y1 = 1f - i * h;
            string lbl = opt.label + DescEfeito(opt);
            Btn(eventOpts, lbl, new Vector2(0.02f, y0 + 0.02f), new Vector2(0.98f, y1 - 0.02f), GameConfig.Gold, () => GameManager.I.ResolveChoice(ev, opt));
        }
    }
    string DescEfeito(ChoiceOption o) {
        string s = "";
        if (o.money != 0) s += (o.money > 0 ? "  (+R$" : "  (-R$") + Mathf.Abs(o.money);
        if (o.rep != 0) s += (s == "" ? "  (" : ", ") + (o.rep > 0 ? "+" : "") + o.rep + " rep";
        if (s != "") s += ")";
        return s;
    }
    public void HideEvent() { modal = false; eventRoot.SetActive(false); }

    // ---------- COMPRAR (fornecedores) ----------
    void BuildBuy() {
        buyRoot = new GameObject("Buy"); var rt = buyRoot.AddComponent<RectTransform>(); rt.SetParent(canvas.transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        Panel(rt, Vector2.zero, Vector2.one, new Color(0,0,0,0.55f));
        var box = Panel(rt, new Vector2(0.16f,0.12f), new Vector2(0.84f,0.88f), GameConfig.Paper);
        Label(box, "COMPRAR ESTOQUE", 30, GameConfig.Ink, TextAnchor.UpperCenter, new Vector2(0,0.9f), new Vector2(1,0.99f));
        Btn(box, "◀", new Vector2(0.06f,0.82f), new Vector2(0.14f,0.9f), GameConfig.Paper, () => { buySupIdx = (buySupIdx + GameConfig.Suppliers.Length - 1) % GameConfig.Suppliers.Length; RefreshBuy(); });
        buySupplier = Label(box, "", 22, GameConfig.Ink, TextAnchor.MiddleCenter, new Vector2(0.14f,0.82f), new Vector2(0.86f,0.9f));
        Btn(box, "▶", new Vector2(0.86f,0.82f), new Vector2(0.94f,0.9f), GameConfig.Paper, () => { buySupIdx = (buySupIdx + 1) % GameConfig.Suppliers.Length; RefreshBuy(); });
        var listGo = new GameObject("list"); var lrt = listGo.AddComponent<RectTransform>(); lrt.SetParent(box, false);
        lrt.anchorMin = new Vector2(0.05f,0.14f); lrt.anchorMax = new Vector2(0.95f,0.80f); lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;
        buyList = lrt;
        Btn(box, "FECHAR", new Vector2(0.38f,0.03f), new Vector2(0.62f,0.12f), GameConfig.Gold, () => HideBuy());
    }

    public void ShowBuyPanel() { modal = true; buyRoot.SetActive(true); buyRoot.transform.SetAsLastSibling(); RefreshBuy(); }
    public void HideBuy() { modal = false; buyRoot.SetActive(false); }

    void RefreshBuy() {
        var sup = GameConfig.Suppliers[buySupIdx];
        buySupplier.text = sup.nome + "  (×" + sup.costMult.ToString("0.00") + ")  — " + sup.desc;
        foreach (Transform c in buyList) Destroy(c.gameObject);
        var ul = GameManager.I.unlocked; int n = ul.Count;
        int cols = 3; int rows = Mathf.CeilToInt(n / (float)cols);
        for (int i = 0; i < n; i++) {
            var prod = GameConfig.Product(ul[i]);
            int cx = i % cols, cy = i / cols;
            float w = 1f / cols, h = 1f / Mathf.Max(rows, 1);
            float x0 = cx * w, y1 = 1f - cy * h;
            float cost = prod.buy * sup.costMult * GameConfig.RestockChunk;
            int have = GameManager.I.backstock.ContainsKey(prod.id) ? GameManager.I.backstock[prod.id] : 0;
            string lbl = prod.nome + "\n" + GameConfig.RestockChunk + "un · R$" + cost.ToString("0") + "\n(estoque: " + have + ")";
            Btn(buyList, lbl, new Vector2(x0 + 0.02f, y1 - h + 0.02f), new Vector2(x0 + w - 0.02f, y1 - 0.02f), GameConfig.Good,
                () => { if (GameManager.I.BuyProduct(prod.id, GameConfig.RestockChunk, sup)) RefreshBuy(); });
        }
    }

    // ---------- TROCA DE TELA ----------
    public void OnScreen(Screen s) {
        if (menuRoot) menuRoot.SetActive(s == Screen.Menu);
        if (tutoRoot) tutoRoot.SetActive(s == Screen.Tutorial);
        if (hudRoot) hudRoot.SetActive(s == Screen.Playing);
        if (dayendRoot) dayendRoot.SetActive(s == Screen.DayEnd);
        if (gameoverRoot) gameoverRoot.SetActive(s == Screen.GameOver);
        if (victoryRoot) victoryRoot.SetActive(s == Screen.Victory);
        if (eventRoot) eventRoot.SetActive(false);
        if (buyRoot) buyRoot.SetActive(false);
        modal = false;
        if (s == Screen.GameOver && tGOstats) tGOstats.text = StatsText();
        if (s == Screen.Victory && tVICstats) tVICstats.text = StatsText();
    }

    string StatsText() {
        var g = GameManager.I;
        return "Dias sobrevividos: " + g.daysSurvived + "\nDinheiro final: R$ " + g.money.ToString("0") +
               "\nReputação: " + g.rep.ToString("0") + "\nClientes atendidos: " + g.servedTotal +
               "\nClientes perdidos: " + g.lostTotal;
    }

    public void ShowDayReport(int day, int served, int lost, float sales, float tips, float expense, int expired) {
        float lucro = sales + tips - expense;
        tDayReport.text =
            "Dia " + day + " encerrado\n\n" +
            "Vendas: R$ " + sales.ToString("0.0") + "\n" +
            "Gorjetas: R$ " + tips.ToString("0.0") + "\n" +
            "Despesas: -R$ " + expense.ToString("0.0") + "\n" +
            (expired > 0 ? "Vencidos: " + expired + " (reputação caiu)\n" : "") +
            "\nLucro do dia: " + (lucro >= 0 ? "+" : "") + "R$ " + lucro.ToString("0.0") + "\n\n" +
            "Caixa: R$ " + GameManager.I.money.ToString("0") + "  ·  Reputação: " + GameManager.I.rep.ToString("0") +
            "\nMeta: R$ " + GameConfig.GoalMoney.ToString("0");
    }

    // ---------- HUD update ----------
    public void UpdateHUD() {
        var g = GameManager.I;
        if (tMoney) tMoney.text = "R$ " + g.money.ToString("0");
        if (tDay) tDay.text = "Dia " + g.day;
        if (tCust) tCust.text = "Clientes: " + g.customers.Count + "/" + (g.day > 0 ? (GameConfig.CustomersBase + (g.day-1)*GameConfig.CustomersPerDay) : 0);
        if (repBar) { float f = g.rep / 100f; repBar.rectTransform.anchorMax = new Vector2(f, 1); repBar.color = f > 0.5f ? GameConfig.Good : (f > 0.25f ? GameConfig.Warn : GameConfig.Bad); }
        if (timeBar) { float f = Mathf.Clamp01(g.dayTime / g.dayLen); timeBar.rectTransform.anchorMax = new Vector2(f, 1); }
    }

    // ---------- toast + floats ----------
    public void Toast(string msg) {
        if (tToast == null) tToast = Label(canvas.transform, "", 24, GameConfig.Gold, TextAnchor.MiddleCenter, new Vector2(0.2f,0.12f), new Vector2(0.8f,0.18f));
        tToast.text = msg; toastT = 2f; tToast.gameObject.SetActive(true);
    }
    public void Float(Vector3 worldPos, string text, Color col) {
        var cam = Camera.main; if (!cam) return;
        Vector3 sp = cam.WorldToScreenPoint(worldPos + Vector3.up * 0.5f);
        var t = Label(canvas.transform, text, 22, col, TextAnchor.MiddleCenter, Vector2.zero, Vector2.zero);
        var rt = t.rectTransform; rt.sizeDelta = new Vector2(180, 40);
        rt.position = sp; floats.Add(rt);
    }

    void Update() {
        if (toastT > 0) { toastT -= Time.unscaledDeltaTime; if (toastT <= 0 && tToast) tToast.gameObject.SetActive(false); }
        for (int i = floats.Count - 1; i >= 0; i--) {
            var rt = floats[i]; if (!rt) { floats.RemoveAt(i); continue; }
            rt.position += Vector3.up * 40f * Time.unscaledDeltaTime;
            var t = rt.GetComponent<Text>(); var c = t.color; c.a -= Time.unscaledDeltaTime * 0.8f; t.color = c;
            if (c.a <= 0) { Destroy(rt.gameObject); floats.RemoveAt(i); }
        }
    }
}
