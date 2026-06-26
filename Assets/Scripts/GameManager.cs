using UnityEngine;
using System.Collections.Generic;

// GameManager.cs — o cérebro. Estado do jogo + máquina de telas + ciclo de dias +
// economia + clientes + escolhas morais + vitória/derrota. Singleton GameManager.I.
public enum Screen { Menu, Tutorial, Playing, DayEnd, GameOver, Victory }

// Cliente como objeto simples (visual = GameObject), atualizado pelo GameManager.
public class Customer {
    public CustomerType type;
    public string wantId;          // produto desejado
    public float patience, maxPatience;
    public bool served, leaving, angry;
    public GameObject go;
    public SpriteRenderer sr;
    public GameObject bubble;      // balão com o ícone do produto
    public SpriteRenderer bubbleIcon;
    public Vector3 target;
    public int slot;               // posição na fila
}

public class GameManager : MonoBehaviour {
    public static GameManager I;

    // ---- estado de run ----
    public Screen screen = Screen.Menu;
    public float money, rep;
    public int day;
    public float dayTime, dayLen;
    public float expenseToday;
    public Dictionary<string,int> backstock = new Dictionary<string,int>();   // almoxarifado
    public Dictionary<string,float> priceToday = new Dictionary<string,float>();
    public List<string> unlocked = new List<string>();
    public bool precoAltaHoje = false;     // escolha "subir preços"
    // flags de consequência permanente
    public bool flagRisco = false;
    // estatísticas / relatório
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

    // ===================== FLUXO =====================
    public void NewGame() {
        money = GameConfig.StartMoney; rep = GameConfig.StartRep; day = 1;
        backstock.Clear(); priceToday.Clear(); unlocked.Clear();
        flagRisco = false; expiredTotal = servedTotal = lostTotal = daysSurvived = 0;
        foreach (var p in GameConfig.Products) if (p.unlockDay <= 1) { unlocked.Add(p.id); backstock[p.id] = 10; }
        Market.I.Build();
        // abastece as prateleiras iniciais
        AutoStockShelves();
        SetScreen(Screen.Playing);
        StartDay();
    }

    public void SetScreen(Screen s) {
        screen = s;
        AudioManager.I.PlayMusic(s == Screen.Playing && money < GameConfig.DailyExpenseBase ? "crise" : (s == Screen.Playing ? "ambiente" : "ambiente"));
        if (s == Screen.Menu) AudioManager.I.PlayMusic("ambiente");
        UI.I.OnScreen(s);
        bool play = s == Screen.Playing;
        if (PlayerCtrl.I) PlayerCtrl.I.SetActive(play);
        ShowCustomers(play);
    }

    void StartDay() {
        dayTime = 0; dayLen = GameConfig.DayLengthSec;
        expenseToday = GameConfig.DailyExpenseBase + (day - 1) * GameConfig.DailyExpenseGrow;
        servedDay = lostDay = 0; salesDay = tipsDay = 0;
        precoAltaHoje = false; eventShownToday = false; eventAt = Random.Range(dayLen * 0.25f, dayLen * 0.7f);
        // desbloqueia produtos novos
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
            float v = p.price * Random.Range(0.85f, 1.25f);    // oscilação diária
            priceToday[p.id] = Mathf.Round(v * 2f) / 2f;        // arredonda p/ .50
        }
    }

    public float PriceOf(string id) {
        float bas = priceToday.ContainsKey(id) ? priceToday[id] : (GameConfig.Product(id)?.price ?? 0f);
        return precoAltaHoje ? Mathf.Round(bas * 1.3f * 2f) / 2f : bas;
    }

    // ===================== UPDATE =====================
    void Update() {
        if (screen != Screen.Playing || pendingEvent != null || (UI.I != null && UI.I.modal)) return;
        float dt = Time.deltaTime;
        dayTime += dt;

        // spawn de clientes
        if (spawnedToday < toSpawnToday) {
            spawnTimer -= dt;
            if (spawnTimer <= 0f) { SpawnCustomer(); spawnedToday++; spawnTimer = Random.Range(GameConfig.SpawnEveryMin, GameConfig.SpawnEveryMax); }
        }
        UpdateCustomers(dt);

        // evento moral do dia
        if (!eventShownToday && dayTime >= eventAt) { eventShownToday = true; TriggerEvent(); }

        // fim do dia: acabou o tempo E não há mais clientes pendentes
        if (dayTime >= dayLen && customers.Count == 0 && spawnedToday >= toSpawnToday) EndDay();

        UI.I.UpdateHUD();
    }

    // ===================== CLIENTES =====================
    void SpawnCustomer() {
        var t = GameConfig.CustomerTypes[Random.Range(0, GameConfig.CustomerTypes.Length)];
        var c = new Customer { type = t, maxPatience = t.patience, patience = t.patience };
        // quer um produto que ESTÁ desbloqueado
        c.wantId = unlocked[Random.Range(0, unlocked.Count)];
        c.go = new GameObject("Cliente_" + t.id);
        if (customerRoot == null) { customerRoot = new GameObject("Clientes").transform; }
        c.go.transform.SetParent(customerRoot);
        // sprite num filho "vis" pra o CharAnim animar (bob/respiração) sem brigar com o
        // movimento do cliente (que vive no c.go). O balão fica filho do c.go (firme em cima).
        var vis = new GameObject("vis"); vis.transform.SetParent(c.go.transform, false);
        c.sr = vis.AddComponent<SpriteRenderer>();
        c.sr.sprite = AssetDB.I.Char(t.icon); c.sr.color = c.sr.sprite ? Color.white : t.color;
        c.sr.sortingOrder = 50;
        var canim = vis.AddComponent<CharAnim>(); canim.sr = c.sr;
        if (!c.sr.sprite) { // fallback procedural: quadrado colorido
            c.sr.sprite = Util.SolidSprite(t.color);
            canim.baseScale = 0.8f;
        }
        // balão com o ícone do produto desejado
        c.bubble = new GameObject("balao"); c.bubble.transform.SetParent(c.go.transform);
        c.bubble.transform.localPosition = new Vector3(0, 0.9f, 0);
        var bg = c.bubble.AddComponent<SpriteRenderer>(); bg.sprite = Util.SolidSprite(Color.white); bg.sortingOrder = 60; c.bubble.transform.localScale = Vector3.one * 0.6f;
        var iconGo = new GameObject("icon"); iconGo.transform.SetParent(c.bubble.transform); iconGo.transform.localPosition = new Vector3(0,0,-0.01f);
        c.bubbleIcon = iconGo.AddComponent<SpriteRenderer>(); c.bubbleIcon.sprite = AssetDB.I.Product(GameConfig.Product(c.wantId).icon); c.bubbleIcon.sortingOrder = 61; iconGo.transform.localScale = Vector3.one * 1.4f;
        if (!c.bubbleIcon.sprite) { c.bubbleIcon.sprite = Util.SolidSprite(GameConfig.Good); }
        // posição: entra pela porta, vai p/ um slot na frente da caixa
        c.go.transform.position = Market.I.DoorWorld() + Vector3.up * 0.2f;
        c.slot = NextSlot();
        c.target = Market.I.QueueWorld(c.slot);
        customers.Add(c);
    }

    int NextSlot() { int s = 0; var used = new HashSet<int>(); foreach (var c in customers) if (!c.leaving) used.Add(c.slot); while (used.Contains(s)) s++; return s; }

    void UpdateCustomers(float dt) {
        for (int i = customers.Count - 1; i >= 0; i--) {
            var c = customers[i];
            // anda até o alvo
            Vector3 p = c.go.transform.position;
            Vector3 d = c.target - p; float dist = d.magnitude;
            if (dist > 0.05f) c.go.transform.position = p + d.normalized * Mathf.Min(GameConfig.PlayerSpeed * 0.7f * dt, dist);
            // se já chegou na fila, conta paciência
            bool atQueue = !c.leaving && dist < 0.2f;
            if (atQueue && !c.served) {
                c.patience -= dt;
                float f = c.patience / c.maxPatience;
                if (c.sr) c.sr.color = Color.Lerp(GameConfig.Bad, (c.sr.sprite && c.sr.sprite.name.StartsWith("art")) ? Color.white : c.type.color, f);
                if (c.patience <= 0f) CustomerAngry(c);
            }
            // saindo → some ao chegar na porta
            if (c.leaving) { if (Vector3.Distance(c.go.transform.position, Market.I.DoorWorld()) < 0.3f) { Object.Destroy(c.go); customers.RemoveAt(i); } }
        }
    }

    // chamado pelo jogador ao interagir perto de um cliente OU da caixa
    public bool TryServeNearest(Vector3 from) {
        Customer best = null; float bd = 1.6f;
        foreach (var c in customers) { if (c.served || c.leaving) continue; float dd = Vector3.Distance(from, c.go.transform.position); if (dd < bd) { bd = dd; best = c; } }
        if (best == null) return false;
        return ServeCustomer(best);
    }

    bool ServeCustomer(Customer c) {
        // precisa ter o produto em ALGUMA prateleira
        var shelf = Market.I.ShelfWith(c.wantId);
        if (shelf == null) { UI.I.Float(c.go.transform.position, "Sem estoque!", GameConfig.Bad); return false; }
        shelf.Take(1);
        float price = PriceOf(c.wantId);
        // sensibilidade a preço: cliente caro-sensível pode reclamar (menos gorjeta/rep)
        var prod = GameConfig.Product(c.wantId);
        float caro = Mathf.Clamp01((price - prod.price) / prod.price); // quão acima do base
        float repGain = GameConfig.SaleRepGain * (1f - caro * c.type.priceSens);
        float tip = (Random.value < c.type.tip) ? price * 0.15f : 0f;
        money += price + tip; salesDay += price; tipsDay += tip; servedDay++; servedTotal++;
        rep = Mathf.Clamp(rep + repGain, 0, 100);
        c.served = true; c.leaving = true; c.target = Market.I.DoorWorld();
        if (c.bubble) c.bubble.SetActive(false);
        if (PlayerCtrl.I != null) PlayerCtrl.I.ShowServe(); // lojista segura a sacola ao entregar
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
    // jogador interage no FUNDO (B) → compra um lote do fornecedor selecionado
    public void OpenBuyPanel() { UI.I.ShowBuyPanel(); }

    public bool BuyProduct(string id, int qty, SupplierDef sup) {
        var prod = GameConfig.Product(id);
        float cost = prod.buy * sup.costMult * qty;
        if (money < cost) { UI.I.Toast("Sem dinheiro p/ comprar"); return false; }
        money -= cost;
        // qualidade: lote ruim pode vir já perto de vencer (simplificado: reduz validade)
        if (!backstock.ContainsKey(id)) backstock[id] = 0;
        backstock[id] += qty;
        AudioManager.I.Sfx(AssetDB.I.sfxDing);
        return true;
    }

    // jogador interage numa PRATELEIRA → repõe do almoxarifado
    public void RestockShelf(Shelf sh) {
        // se a prateleira está vazia, escolhe um produto do almoxarifado; senão completa o mesmo
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

    // ===================== FIM DO DIA / VITÓRIA / DERROTA =====================
    void EndDay() {
        // produtos vencidos nas prateleiras → reputação
        int venc = Market.I.ExpireOldStock(day);
        if (venc > 0) { rep = Mathf.Clamp(rep - venc * GameConfig.PerishLossRep, 0, 100); expiredTotal += venc; }
        money -= expenseToday;
        daysSurvived = day;
        // checa fim de jogo
        if (money <= 0f || rep < GameConfig.RepGameOver) { SetScreen(Screen.GameOver); return; }
        if (money >= GameConfig.GoalMoney && rep >= 40f) { SetScreen(Screen.Victory); return; }
        // relatório do dia
        UI.I.ShowDayReport(day, servedDay, lostDay, salesDay, tipsDay, expenseToday, venc);
        SetScreen(Screen.DayEnd);
    }

    public void NextDay() { day++; SetScreen(Screen.Playing); StartDay(); ClearCustomers(); }

    public void BackToMenu() { ClearCustomers(); SetScreen(Screen.Menu); }
}
