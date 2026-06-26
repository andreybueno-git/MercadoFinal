using UnityEngine;

// GameConfig.cs — TODA a config/dados/balance de "Tudo Tem Preço" (Mercado Final).
// Data-driven: os sistemas leem daqui. Sem números mágicos espalhados pelo código.

[System.Serializable]
public class ProductDef {
    public string id, nome;
    public int icon;          // índice na grade de products.png
    public float buy, price;  // custo de compra, preço base de venda
    public bool perish;       // perecível?
    public int shelfLife;     // validade em dias (0 = não vence)
    public int unlockDay;     // dia a partir do qual aparece
    public ProductDef(string id, string nome, int icon, float buy, float price, bool perish, int shelfLife, int unlockDay) {
        this.id = id; this.nome = nome; this.icon = icon; this.buy = buy; this.price = price;
        this.perish = perish; this.shelfLife = shelfLife; this.unlockDay = unlockDay;
    }
}

[System.Serializable]
public class CustomerType {
    public string id, nome, desc;
    public float patience;   // segundos de paciência
    public float priceSens;  // sensibilidade a preço (0..1)
    public float tip;        // chance/valor de gorjeta
    public Color color;      // cor do sprite/realce
    public int icon;         // índice em chars.png (1..4; 0 = lojista)
    public CustomerType(string id, string nome, float patience, float priceSens, float tip, Color color, int icon, string desc) {
        this.id = id; this.nome = nome; this.patience = patience; this.priceSens = priceSens;
        this.tip = tip; this.color = color; this.icon = icon; this.desc = desc;
    }
}

[System.Serializable]
public class SupplierDef {
    public string id, nome, desc;
    public float costMult;   // multiplicador de custo
    public float quality;    // chance de vir bom (0..1)
    public float risk;       // risco (eventos ruins)
    public SupplierDef(string id, string nome, float costMult, float quality, float risk, string desc) {
        this.id = id; this.nome = nome; this.costMult = costMult; this.quality = quality; this.risk = risk; this.desc = desc;
    }
}

[System.Serializable]
public class ChoiceOption {
    public string label;
    public int money, rep;   // efeitos
    public string flag;      // flag de consequência (opcional)
    public string especial;  // efeito especial (opcional): "fiscalizar","precoAlta"
    public ChoiceOption(string label, int money, int rep, string flag = null, string especial = null) {
        this.label = label; this.money = money; this.rep = rep; this.flag = flag; this.especial = especial;
    }
}

[System.Serializable]
public class ChoiceEvent {
    public string id, titulo, texto;
    public ChoiceOption[] opcoes;
    public ChoiceEvent(string id, string titulo, string texto, ChoiceOption[] opcoes) {
        this.id = id; this.titulo = titulo; this.texto = texto; this.opcoes = opcoes;
    }
}

public static class GameConfig {
    // ---- Balance ----
    public const float StartMoney = 220f;
    public const float StartRep = 70f;        // 0..100
    public const float GoalMoney = 900f;       // meta de vitória
    public const float RepGameOver = 8f;       // reputação mínima
    public const float DailyExpenseBase = 40f;
    public const float DailyExpenseGrow = 12f;
    public const int ShelfCapacity = 8;
    public const int RestockChunk = 6;
    public const float PerishLossRep = 4f;
    public const float AngryLossRep = 6f;
    public const float SaleRepGain = 1f;
    public const float PlayerSpeed = 4.2f;     // unidades/seg (mundo)

    // ---- Dia / progressão ----
    public const float DayLengthSec = 70f;
    public const int CustomersBase = 6;
    public const int CustomersPerDay = 2;
    public const float SpawnEveryMin = 2.0f;
    public const float SpawnEveryMax = 5.0f;

    // ---- Mapa do mercado (16x9). # parede · . chão · S prateleira · R caixa · D porta · B fundo ----
    public static readonly string[] Map = {
        "################",
        "#B....SSSS.....#",
        "#B....SSSS.....#",
        "#.....SSSS.....#",
        "#..............#",
        "#....SSSS......#",
        "#....SSSS...R..#",
        "#..............#",
        "#######DD#######",
    };
    public const float Tile = 1f; // 1 unidade de mundo por tile

    // ---- Produtos ----
    public static readonly ProductDef[] Products = {
        new ProductDef("maca",     "Maçã",      0,  2.0f, 4.0f,  true,  3, 1),
        new ProductDef("banana",   "Banana",    1,  1.6f, 3.5f,  true,  2, 1),
        new ProductDef("pao",      "Pão",       2,  1.2f, 3.0f,  true,  1, 1),
        new ProductDef("leite",    "Leite",     3,  3.0f, 6.0f,  true,  4, 1),
        new ProductDef("enlatado", "Enlatado",  4,  3.5f, 7.0f,  false, 0, 1),
        new ProductDef("refri",    "Refri",     5,  3.2f, 7.5f,  false, 0, 2),
        new ProductDef("ovos",     "Ovos",      6,  4.0f, 8.0f,  true,  5, 2),
        new ProductDef("queijo",   "Queijo",    7,  5.5f, 11.0f, true,  6, 3),
        new ProductDef("arroz",    "Arroz",     8,  4.5f, 9.0f,  false, 0, 3),
        new ProductDef("tomate",   "Tomate",    9,  2.4f, 5.0f,  true,  3, 4),
        new ProductDef("agua",     "Água",      10, 1.5f, 4.0f,  false, 0, 4),
        new ProductDef("choco",    "Chocolate", 11, 3.0f, 8.0f,  false, 0, 5),
    };
    public const int ProductCols = 4, ProductRows = 3; // grade do products.png

    // ---- Tipos de cliente ----
    public static readonly CustomerType[] CustomerTypes = {
        new CustomerType("apressado","Apressado", 9f,  0.6f, 0.05f, new Color(0.23f,0.47f,0.76f), 1, "Sai rápido se demorar."),
        new CustomerType("educado",  "Educado",   16f, 0.5f, 0.20f, new Color(0.23f,0.62f,0.36f), 2, "Paciente e dá gorjeta."),
        new CustomerType("reclamao", "Reclamão",  11f, 1.0f, 0.0f,  new Color(0.82f,0.28f,0.23f), 3, "Odeia preço alto."),
        new CustomerType("fiel",     "Fiel",      18f, 0.3f, 0.10f, new Color(0.55f,0.36f,0.78f), 4, "Volta sempre; tolera muito."),
        new CustomerType("suspeito", "Suspeito",  12f, 0.7f, 0.0f,  new Color(0.42f,0.42f,0.42f), 1, "Pode tentar dar golpe."),
    };
    public const int CharCols = 5, CharRows = 1; // grade do chars.png (0=lojista,1..4=clientes)

    // ---- Fornecedores ----
    public static readonly SupplierDef[] Suppliers = {
        new SupplierDef("honesto",     "Fornecedor Honesto",     1.00f, 0.98f, 0.0f,  "Justo e confiável."),
        new SupplierDef("caro",        "Fornecedor Caro",        1.35f, 1.00f, 0.0f,  "Caro, mas sempre ótimo."),
        new SupplierDef("rapido",      "Fornecedor Rápido",      1.15f, 0.90f, 0.05f, "Entrega na hora."),
        new SupplierDef("clandestino", "Fornecedor Clandestino", 0.60f, 0.65f, 0.30f, "Barato… e arriscado."),
    };

    // ---- Eventos de escolha moral ----
    public static readonly ChoiceEvent[] Events = {
        new ChoiceEvent("fome", "Cliente com fome",
            "Uma cliente sem dinheiro pede um pão para o filho. O que você faz?",
            new[] {
                new ChoiceOption("Dar de graça", -3, +10),
                new ChoiceOption("Cobrar metade", +1, +4),
                new ChoiceOption("Recusar", 0, -8),
            }),
        new ChoiceEvent("ilegal", "Oferta suspeita",
            "O fornecedor clandestino oferece um lote barato de procedência duvidosa.",
            new[] {
                new ChoiceOption("Aceitar (lucro)", +60, -6, "risco"),
                new ChoiceOption("Recusar", 0, +3),
            }),
        new ChoiceEvent("fiscal", "Fiscalização surpresa",
            "A vigilância aparece! Se houver produtos vencidos na prateleira, leva multa.",
            new[] {
                new ChoiceOption("Deixar fiscalizar", 0, 0, null, "fiscalizar"),
                new ChoiceOption("Suborno (R$40)", -40, -10),
            }),
        new ChoiceEvent("troco", "Troco a mais",
            "Você deu R$20 de troco a mais para um cliente que já saiu.",
            new[] {
                new ChoiceOption("Correr e avisar", 0, +8),
                new ChoiceOption("Deixar pra lá", -20, 0),
            }),
        new ChoiceEvent("alta", "Escassez na cidade",
            "Faltou mercadoria na cidade. Dá pra subir os preços hoje.",
            new[] {
                new ChoiceOption("Subir preços (+30%)", 0, -5, null, "precoAlta"),
                new ChoiceOption("Manter justo", 0, +5),
            }),
    };

    // ---- Paleta ----
    public static Color Hex(string h) { Color c; ColorUtility.TryParseHtmlString(h, out c); return c; }
    public static readonly Color Floor   = Hex("#d9c39a");
    public static readonly Color Floor2  = Hex("#cdb589");
    public static readonly Color Wall     = Hex("#6b4a30");
    public static readonly Color WallTop  = Hex("#8a6440");
    public static readonly Color Shelf    = Hex("#9c6e44");
    public static readonly Color ShelfTop = Hex("#b98a55");
    public static readonly Color Register = Hex("#caa15a");
    public static readonly Color Paper    = Hex("#f4e9d6");
    public static readonly Color Ink      = Hex("#211a17");
    public static readonly Color Good     = Hex("#3a9d5d");
    public static readonly Color Warn     = Hex("#f4b942");
    public static readonly Color Bad      = Hex("#d2483a");
    public static readonly Color Gold     = Hex("#f4b942");

    public static ProductDef Product(string id) { foreach (var p in Products) if (p.id == id) return p; return null; }
}
