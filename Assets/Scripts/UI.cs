using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

// UI.cs — interface DO NÍVEL (uGUI por código): HUD (dinheiro/meta/nível/reputação/tempo) +
// relatório de fim de dia + tela "nível concluído" + diálogo de evento moral + painel de
// compra + toasts + textos flutuantes. As telas Menu/ComoJogar/Fim são cenas separadas.
public class UI : MonoBehaviour {
    public static UI I;
    public bool modal = false;        // diálogo (evento/compra) aberto → pausa o jogo

    Canvas canvas; Font font;
    GameObject hudRoot, dayendRoot, levelRoot, eventRoot, buyRoot;
    Text tMoney, tMeta, tDay, tCust, tDayReport, tLevelReport, tEventTitle, tEventText, tToast;
    Image repBar, timeBar;
    Transform eventOpts, buyList; Text buySupplier; int buySupIdx = 0;
    System.Action onNivelNext;
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
        // EventSystem é OBRIGATÓRIO pros cliques de UI (botões de evento/compra/relatório)
        if (FindObjectOfType<EventSystem>() == null) {
            var es = new GameObject("EventSystem");
            es.AddComponent<EventSystem>();
            es.AddComponent<StandaloneInputModule>();
        }
        BuildHUD(); BuildDayEnd(); BuildLevelClear(); BuildEvent(); BuildBuy();
        dayendRoot.SetActive(false); levelRoot.SetActive(false); eventRoot.SetActive(false); buyRoot.SetActive(false);
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
        t.raycastTarget = false;
        return t;
    }
    Button Btn(Transform parent, string s, Vector2 aMin, Vector2 aMax, Color col, System.Action onClick) {
        var go = new GameObject("btn"); var rt = go.AddComponent<RectTransform>(); rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var im = go.AddComponent<Image>(); im.color = col;
        var b = go.AddComponent<Button>(); b.onClick.AddListener(() => { if (AudioManager.I != null && AssetDB.I != null) AudioManager.I.Sfx(AssetDB.I.sfxDing); onClick?.Invoke(); });
        Label(rt, s, 26, GameConfig.Ink, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one);
        return b;
    }
    RectTransform FullPanel(string name, Color col) {
        var go = new GameObject(name); var rt = go.AddComponent<RectTransform>(); rt.SetParent(canvas.transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        go.AddComponent<Image>().color = col; return rt;
    }

    // ---------- HUD ----------
    void BuildHUD() {
        hudRoot = new GameObject("HUD"); var rt = hudRoot.AddComponent<RectTransform>(); rt.SetParent(canvas.transform, false);
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var bar = Panel(rt, new Vector2(0, 0.92f), new Vector2(1, 1f), new Color(0.13f, 0.10f, 0.08f, 0.85f));
        tMoney = Label(bar, "R$ 0", 28, GameConfig.Gold, TextAnchor.MiddleLeft, new Vector2(0.01f, 0), new Vector2(0.15f, 1));
        tMeta  = Label(bar, "Meta R$ 0", 18, GameConfig.Paper, TextAnchor.MiddleLeft, new Vector2(0.15f, 0), new Vector2(0.30f, 1));
        tDay   = Label(bar, "Nível 1 · Dia 1", 20, GameConfig.Paper, TextAnchor.MiddleCenter, new Vector2(0.29f, 0), new Vector2(0.47f, 1));
        Label(bar, "Reputação", 14, GameConfig.Paper, TextAnchor.MiddleLeft, new Vector2(0.47f, 0.55f), new Vector2(0.66f, 1));
        var repBg = Panel(bar, new Vector2(0.47f, 0.18f), new Vector2(0.66f, 0.5f), new Color(0, 0, 0, 0.4f));
        repBar = Panel(repBg, Vector2.zero, new Vector2(0.7f, 1), GameConfig.Good).GetComponent<Image>();
        Label(bar, "Tempo do dia", 14, GameConfig.Paper, TextAnchor.MiddleLeft, new Vector2(0.67f, 0.55f), new Vector2(0.85f, 1));
        var tBg = Panel(bar, new Vector2(0.67f, 0.18f), new Vector2(0.85f, 0.5f), new Color(0, 0, 0, 0.4f));
        timeBar = Panel(tBg, Vector2.zero, new Vector2(0f, 1), GameConfig.Gold).GetComponent<Image>();
        tCust  = Label(bar, "Clientes: 0", 18, GameConfig.Paper, TextAnchor.MiddleRight, new Vector2(0.85f, 0), new Vector2(0.99f, 1));
        Label(rt, "WASD/setas: andar  ·  E/Espaço: atender / repor / comprar", 18, new Color(1,1,1,0.85f), TextAnchor.LowerCenter, new Vector2(0,0), new Vector2(1,0.06f));
    }

    // ---------- FIM DE DIA (continua no mesmo nível) ----------
    void BuildDayEnd() {
        dayendRoot = FullPanel("DayEnd", new Color(0.10f,0.07f,0.05f,0.92f)).gameObject;
        Label(dayendRoot.transform, "FIM DO DIA", 48, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0,0.80f), new Vector2(1,0.92f));
        tDayReport = Label(dayendRoot.transform, "", 26, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0.15f,0.28f), new Vector2(0.85f,0.78f));
        Btn(dayendRoot.transform, "PRÓXIMO DIA", new Vector2(0.34f,0.12f), new Vector2(0.66f,0.23f), GameConfig.Gold,
            () => { dayendRoot.SetActive(false); GameManager.I.NextDay(); });
    }

    public void ShowDayReport(int day, int level, float meta, int served, int lost, float sales, float tips, float expense, int expired, float money) {
        float lucro = sales + tips - expense;
        tDayReport.text =
            "Nível " + level + " · Dia " + day + " encerrado\n\n" +
            "Vendas: R$ " + sales.ToString("0.0") + "    Gorjetas: R$ " + tips.ToString("0.0") + "\n" +
            "Despesas: -R$ " + expense.ToString("0.0") +
            (expired > 0 ? "    Vencidos: " + expired : "") + "\n" +
            "Lucro do dia: " + (lucro >= 0 ? "+" : "") + "R$ " + lucro.ToString("0.0") + "\n\n" +
            "Caixa: R$ " + money.ToString("0") + "   ·   Meta do nível: R$ " + meta.ToString("0") + "\n" +
            "Faltam R$ " + Mathf.Max(0, meta - money).ToString("0") + " para passar de nível!";
        dayendRoot.SetActive(true); dayendRoot.transform.SetAsLastSibling();
    }

    // ---------- NÍVEL CONCLUÍDO (vai pra próxima cena) ----------
    void BuildLevelClear() {
        levelRoot = FullPanel("LevelClear", new Color(0.05f,0.12f,0.06f,0.95f)).gameObject;
        Label(levelRoot.transform, "NÍVEL CONCLUÍDO!", 50, GameConfig.Gold, TextAnchor.UpperCenter, new Vector2(0,0.74f), new Vector2(1,0.9f));
        tLevelReport = Label(levelRoot.transform, "", 28, GameConfig.Paper, TextAnchor.UpperCenter, new Vector2(0.12f,0.36f), new Vector2(0.88f,0.72f));
        Btn(levelRoot.transform, "PRÓXIMO NÍVEL ▶", new Vector2(0.32f,0.16f), new Vector2(0.68f,0.27f), GameConfig.Gold,
            () => { levelRoot.SetActive(false); onNivelNext?.Invoke(); });
    }

    public void ShowLevelClear(int level, float meta, float money, float rep, System.Action next) {
        onNivelNext = next;
        tLevelReport.text =
            "Você bateu a meta de R$ " + meta.ToString("0") + " do Nível " + level + "!\n\n" +
            "Caixa: R$ " + money.ToString("0") + "   ·   Reputação: " + rep.ToString("0") + "\n\n" +
            "Prepare-se: o Nível " + (level + 1) + " é mais difícil.\n" +
            "Seu dinheiro e estoque continuam com você.";
        levelRoot.SetActive(true); levelRoot.transform.SetAsLastSibling();
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

    // ---------- HUD update ----------
    public void UpdateHUD() {
        var g = GameManager.I;
        float meta = GameConfig.MetaDoNivel(g.level);
        if (tMoney) tMoney.text = "R$ " + g.money.ToString("0");
        if (tMeta) tMeta.text = "Meta R$ " + meta.ToString("0");
        if (tDay) tDay.text = "Nível " + g.level + " · Dia " + g.day;
        if (tCust) tCust.text = "Clientes: " + g.customers.Count;
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
