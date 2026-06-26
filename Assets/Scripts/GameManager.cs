using UnityEngine;
using System.Collections.Generic;

// GameManager.cs — cérebro de UM NÍVEL jogável. Roda o ciclo de dias, economia, clientes,
// escolhas morais. Quando bate a META do nível → carrega a próxima cena de nível (ou Fim na
// vitória). Quando perde (dinheiro 0 / reputação baixa) → carrega a cena Fim. Estado que
// passa entre níveis fica no Run (estático). Singleton GameManager.I (por cena).

// Cliente como objeto simples (visual = GameObject), atualizado pelo GameManager.
public class Customer {
    public CustomerType type;
    public string wantId;
    public float patience, maxPatience;
    public bool served, leaving, angry;
    public GameObject go;
    public SpriteRenderer sr;
    public GameObject bubble;
    public SpriteRenderer bubbleIcon;
    public Vector3 target;
    public int slot;
}

public class GameManager : MonoBehaviour {
    public static GameManager I;

    // ---- estado ----
    public bool running;               // jogabilidade ativa (substitui screen==Playing)
    public float money, rep;
    public int day, level;
    public float dayTime, dayLen;
    public float expenseToday;
    public Dictionary<string,int> backstock = new Dictionary<string,int>();
    public Dictionary<string,float> priceToday = new Dictionary<string,float>();
    public List<string> unlocked = new List<string>();
    public bool precoAltaHoje = false;
    public bool flagRisco = false;
    public int servedDay, lostDay; public float salesDay, tipsDay;
    public int servedTotal, lostTotal, expiredTotal, daysSurvived;

    // ---- clientes ----
    public List<Customer> customers = new List<Customer>();
    float spawnTimer; int toSpawnToday, spawnedToday;
    Transform customerRoot;

    // ---- eventos ----
    bool eventShownToday; float eventAt;
    public ChoiceEvent pendingEvent;

    void Awake() { I = this; }

    // ===================== INÍCIO DE NÍVEL =====================
    public void StartLevel() {
        level = Run.level;
        money = Run.money; rep = Run.rep; day = Mathf.Max(1, Run.day);
        backstock = new Dictionary<string,int>(Run.backstock);
        unlocked = new List<string>(Run.unlocked);
        servedTotal = Run.servedTotal; lostTotal = Run.lostTotal; expiredTotal = Run.expiredTotal;
        // primeira vez (começo do nível 1): garante os produtos iniciais
        if (unlocked.Count == 0) {
            foreach (var p in GameConfig.Products)
                if (p.unlockDay <= 1) { unlocked.Add(p.id); if (!backstock.ContainsKey(p.id)) backstock[p.id] = 10; }
        }
        flagRisco = false;
        Market.I.Build();
        AutoStockShelves();
        if (PlayerCtrl.I) PlayerCtrl.I.SetActive(true);
        ShowCustomers(true);
        StartDay();
        running = true;
    }

    void SaveToRun() {
        Run.money = money; Run.rep = rep; Run.day = day; Run.level = level;
        Run.backstock = new Dictionary<string,int>(backstock);
        Run.unlocked = new List<string>(unlocked);
        Run.servedTotal = servedTotal; Run.lostTotal = lostTotal; Run.expiredTotal = expiredTotal;
    }

    void StartDay() {
        dayTime = 0; dayLen = GameConfig.DayLengthSec;
        expenseToday = GameConfig.DailyExpenseBase + (day - 1) * GameConfig.DailyExpenseGrow;
        servedDay = lostDay = 0; salesDay = tipsDay = 0;
        precoAltaHoje = false; eventShownToday = false; eventAt = Random.Range(dayLen * 0.25f, dayLen * 0.7f);
        foreach (var p in GameConfig.Products) if (p.unlockDay == day && !unlocked.Contains(p.id)) { unlocked.Add(p.id); backstock[p.id] = 6; }
        RollPrices();
        toSpawnToday = GameConfig.CustomersBase + (day - 1) * GameConfig.CustomersPerDay;
        spawnedToday = 0; spawnTimer = 1.0f;
        ClearCustomers();
        AudioManager.I.PlayMusic("ambiente");
    }

    void RollPrices() {
        priceToday.Clear();
        foreach (var p in GameConfig.Products) {
            float v = p.price * Random.Range(0.85f, 1.25f);
            priceToday[p.id] = Mathf.Round(v * 2f) / 2f;
        }
    }

    public float PriceOf(string id) {
        float bas = priceToday.ContainsKey(id) ? priceToday[id] : (GameConfig.Product(id)?.price ?? 0f);
        return precoAltaHoje ? Mathf.Round(bas * 1.3f * 2f) / 2f : bas;
    }

    // ===================== UPDATE =====================
    void Update() {
        if (!running || pendingEvent != null || (UI.I != null && UI.I.modal)) return;
        float dt = Time.deltaTime;
        dayTime += dt;

        if (spawnedToday < toSpawnToday) {
            spawnTimer -= dt;
            if (spawnTimer <= 0f) { SpawnCustomer(); spawnedToday++; spawnTimer = Random.Range(GameConfig.SpawnEveryMin, GameConfig.SpawnEveryMax); }
        }
        UpdateCustomers(dt);

        if (!eventShownToday && dayTime >= eventAt) { eventShownToday = true; TriggerEvent(); }

        if (dayTime >= dayLen && customers.Count == 0 && spawnedToday >= toSpawnToday) EndDay();

        UI.I.UpdateHUD();
    }

    // ===================== CLIENTES =====================
    void SpawnCustomer() {
        var t = GameConfig.CustomerTypes[Random.Range(0, GameConfig.CustomerTypes.Length)];
        var c = new Customer { type = t, maxPatience = t.patience, patience = t.patience };
        c.wantId = unlocked[Random.Range(0, unlocked.Count)];
        c.go = new GameObject("Cliente_" + t.id);
        if (customerRoot == null) { customerRoot = new GameObject("Clientes").transform; }
        c.go.transform.SetParent(customerRoot);
        var vis = new GameObject("vis"); vis.transform.SetParent(c.go.transform, false);
        c.sr = vis.AddComponent<SpriteRenderer>();
        c.sr.sprite = AssetDB.I.Char(t.icon); c.sr.color = c.sr.sprite ? Color.white : t.color;
        c.sr.sortingOrder = 50;
        var canim = vis.AddComponent<CharAnim>(); canim.sr = c.sr;
        if (!c.sr.sprite) { c.sr.sprite = Util.SolidSprite(t.color); canim.baseScale = 0.8f; }
        c.bubble = new GameObject("balao"); c.bubble.transform.SetParent(c.go.transform);
        c.bubble.transform.localPosition = new Vector3(0, 0.9f, 0);
        var bg = c.bubble.AddComponent<SpriteRenderer>(); bg.sprite = Util.SolidSprite(Color.white); bg.sortingOrder = 60; c.bubble.transform.localScale = Vector3.one * 0.6f;
        var iconGo = new GameObject("icon"); iconGo.transform.SetParent(c.bubble.transform); iconGo.transform.localPosition = new Vector3(0,0,-0.01f);
        c.bubbleIcon = iconGo.AddComponent<SpriteRenderer>(); c.bubbleIcon.sprite = AssetDB.I.Product(GameConfig.Product(c.wantId).icon); c.bubbleIcon.sortingOrder = 61; iconGo.transform.localScale = Vector3.one * 1.4f;
        if (!c.bubbleIcon.sprite) { c.bubbleIcon.sprite = Util.SolidSprite(GameConfig.Good); }
        c.go.transform.position = Market.I.DoorWorld() + Vector3.up * 0.2f;
        c.slot = NextSlot();
        c.target = Market.I.QueueWorld(c.slot);
        customers.Add(c);
    }

    int NextSlot() { int s = 0; var used = new HashSet<int>(); foreach (var c in customers) if (!c.leaving) used.Add(c.slot); while (used.Contains(s)) s++; return s; }

    void UpdateCustomers(float dt) {
        for (int i = customers.Count - 1; i >= 0; i--) {
            var c = customers[i];
            Vector3 p = c.go.transform.position;
            Vector3 d = c.target - p; float dist = d.magnitude;
            if (dist > 0.05f) c.go.transform.position = p + d.normalized * Mathf.Min(GameConfig.PlayerSpeed * 0.7f * dt, dist);
            bool atQueue = !c.leaving && dist < 0.2f;
            if (atQueue && !c.served) {
                c.patience -= dt;
                float f = c.patience / c.maxPatience;
                if (c.sr) c.sr.color = Color.Lerp(GameConfig.Bad, (c.sr.sprite && c.sr.sprite.name.StartsWith("art")) ? Color.white : c.type.color, f);
                if (c.patience <= 0f) CustomerAngry(c);
            }
            if (c.leaving) { if (Vector3.Distance(c.go.transform.position, Market.I.DoorWorld()) < 0.3f) { Object.Destroy(c.go); customers.RemoveAt(i); } }
        }
    }

    public bool TryServeNearest(Vector3 from) {
        Customer best = null; float bd = 1.6f;
        foreach (var c in customers) { if (c.served || c.leaving) continue; float dd = Vector3.Distance(from, c.go.transform.position); if (dd < bd) { bd = dd; best = c; } }
        if (best == null) return false;
        return ServeCustomer(best);
    }

    bool ServeCustomer(Customer c) {
        var shelf = Market.I.ShelfWith(c.wantId);
        if (shelf == null) { UI.I.Float(c.go.transform.position, "Sem estoque!", GameConfig.Bad); return false; }
        shelf.Take(1);
        float price = PriceOf(c.wantId);
        var prod = GameConfig.Product(c.wantId);
        float caro = Mathf.Clamp01((price - prod.price) / prod.price);
        float repGain = GameConfig.SaleRepGain * (1f - caro * c.type.priceSens);
        float tip = (Random.value < c.type.tip) ? price * 0.15f : 0f;
        money += price + tip; salesDay += price; tipsDay += tip; servedDay++; servedTotal++;
        rep = Mathf.Clamp(rep + repGain, 0, 100);
        c.served = true; c.leaving = true; c.target = Market.I.DoorWorld();
        if (c.bubble) c.bubble.SetActive(false);
        if (PlayerCtrl.I != null) PlayerCtrl.I.ShowServe();
        AudioManager.I.Sfx(AssetDB.I.sfxCaixa);
        UI.I.Float(c.go.transform.position, "+R$" + (price + tip).ToString("0.0") , GameConfig.Good);
        return true;
    }

    void CustomerAngry(Customer c) {
        c.angry = true; c.leaving = true; c.target = Market.I.DoorWorld();
        if (c.bubble) c.bubble.SetActive(false);
        rep = Mathf.Clamp(rep - GameConfig.AngryLossRep, 0, 100);
        lostDay++; lostTotal++;
        UI.I.Float(c.go.transform.position, "Foi embora! -rep", GameConfig.Bad);
    }

    void ClearCustomers() { foreach (var c in customers) if (c.go) Object.Destroy(c.go); customers.Clear(); }
    void ShowCustomers(bool v) { foreach (var c in customers) if (c.go) c.go.SetActive(v); }

    // ===================== ESTOQUE / COMPRA =====================
    public void OpenBuyPanel() { UI.I.ShowBuyPanel(); }

    public bool BuyProduct(string id, int qty, SupplierDef sup) {
        var prod = GameConfig.Product(id);
        float cost = prod.buy * sup.costMult * qty;
        if (money < cost) { UI.I.Toast("Sem dinheiro p/ comprar"); return false; }
        money -= cost;
        if (!backstock.ContainsKey(id)) backstock[id] = 0;
        backstock[id] += qty;
        AudioManager.I.Sfx(AssetDB.I.sfxDing);
        return true;
    }

    public void RestockShelf(Shelf sh) {
        string id = sh.productId;
        if (string.IsNullOrEmpty(id)) { id = FirstStocked(); if (id == null) { UI.I.Toast("Almoxarifado vazio"); return; } }
        if (!backstock.ContainsKey(id) || backstock[id] <= 0) { UI.I.Toast("Sem " + GameConfig.Product(id).nome + " no fundo"); return; }
        int can = Mathf.Min(GameConfig.RestockChunk, GameConfig.ShelfCapacity - sh.count, backstock[id]);
        if (can <= 0) { UI.I.Toast("Prateleira cheia"); return; }
        backstock[id] -= can; sh.SetProduct(id, sh.count + can, day);
        AudioManager.I.Sfx(AssetDB.I.sfxDing);
    }

    string FirstStocked() { foreach (var kv in backstock) if (kv.Value > 0) return kv.Key; return null; }

    void AutoStockShelves() {
        var shelves = Market.I.shelves; int si = 0;
        foreach (var id in unlocked) {
            if (si >= shelves.Count) break;
            int put = Mathf.Min(GameConfig.ShelfCapacity, backstock.ContainsKey(id) ? backstock[id] : 0);
            if (put > 0) { shelves[si].SetProduct(id, put, day); backstock[id] -= put; si++; }
        }
    }

    // ===================== EVENTOS MORAIS =====================
    void TriggerEvent() {
        var ev = GameConfig.Events[Random.Range(0, GameConfig.Events.Length)];
        pendingEvent = ev;
        AudioManager.I.Sfx(AssetDB.I.sfxDing);
        UI.I.ShowEvent(ev);
    }

    public void ResolveChoice(ChoiceEvent ev, ChoiceOption opt) {
        money += opt.money; rep = Mathf.Clamp(rep + opt.rep, 0, 100);
        if (!string.IsNullOrEmpty(opt.flag) && opt.flag == "risco") flagRisco = true;
        if (opt.especial == "precoAlta") precoAltaHoje = true;
        if (opt.especial == "fiscalizar") {
            int venc = Market.I.CountExpired(day);
            if (venc > 0) { float multa = 25f + venc * 10f; money -= multa; rep = Mathf.Clamp(rep - venc * 3f, 0, 100); UI.I.Toast("Multa por vencidos: -R$" + multa.ToString("0")); }
            else { rep = Mathf.Clamp(rep + 4, 0, 100); UI.I.Toast("Tudo certo na fiscalização! +rep"); }
        }
        pendingEvent = null;
        UI.I.HideEvent();
    }

    // ===================== FIM DO DIA / NÍVEL / FIM =====================
    void EndDay() {
        running = false;
        int venc = Market.I.ExpireOldStock(day);
        if (venc > 0) { rep = Mathf.Clamp(rep - venc * GameConfig.PerishLossRep, 0, 100); expiredTotal += venc; }
        money -= expenseToday;
        daysSurvived = day;
        SaveToRun();

        // derrota
        if (money <= 0f || rep < GameConfig.RepGameOver) {
            Run.victory = false;
            Run.motivo = money <= 0f ? "O dinheiro acabou — o mercado faliu." : "A reputação ficou baixa demais.";
            Flow.Ir(Flow.Fim);
            return;
        }

        float meta = GameConfig.MetaDoNivel(level);
        if (money >= meta) {
            if (level >= GameConfig.MaxLevel) { Run.victory = true; Flow.Ir(Flow.Fim); return; }
            // passou de nível → próxima cena (carrega dinheiro/rep/estoque)
            UI.I.ShowLevelClear(level, meta, money, rep, () => {
                Run.day = day + 1; Run.level = level + 1;
                Flow.Ir(Flow.NivelDoLevel(level + 1));
            });
            return;
        }

        // ainda não bateu a meta → próximo dia no mesmo nível
        UI.I.ShowDayReport(day, level, meta, servedDay, lostDay, salesDay, tipsDay, expenseToday, venc, money);
    }

    public void NextDay() { day++; SaveToRun(); StartDay(); ClearCustomers(); running = true; }
}
