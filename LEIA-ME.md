# Tudo Tem Preço — Mercado Final (Unity 2D)

Simulador de gerência de mercadinho em **pixel art top-down**, feito em **Unity (C#)** seguindo o GDD.
Arte e áudio gerados na **Higgsfield**. Todo o jogo é montado **por código** (runtime-bootstrap) —
não precisa configurar prefabs nem cena à mão.

## ▶ Como abrir e jogar (passo a passo)

1. Instale a **Unity** (pelo Unity Hub) — recomendado **Unity 6** ou **2022.3 LTS**.
2. No Unity Hub: **New project → modelo "2D (Core)"** → crie o projeto.
3. **Copie a pasta `Assets`** desta entrega PARA DENTRO da pasta `Assets` do projeto novo
   (junte/aceite substituir). Isto leva os scripts (`Assets/Scripts`, `Assets/Editor`) e os
   assets (`Assets/Resources/art` e `Assets/Resources/audio`).
4. Espere a Unity compilar. No menu superior vai aparecer **"Tudo Tem Preço"** →
   clique em **"Criar Cena do Jogo"** (cria e abre a cena automaticamente).
5. Aperte **PLAY ▶**. Pronto, tá jogando.

> **Se o personagem não andar / botões não responderem:**
> Edit → Project Settings → **Player** → Other Settings → **Active Input Handling = Both**
> (ou "Input Manager (Old)"). Depois feche e abra o Play de novo. O jogo usa o Input clássico.

## 🎮 Controles
- **WASD / setas** — andar pelo mercado.
- **E / Espaço** — interagir (contextual): **atender** um cliente, **repor** uma prateleira,
  ou **comprar** estoque (perto das caixas do fundo).

## 🕹️ Como o jogo funciona (resumo do GDD)
- Clientes entram pela porta querendo um produto (mostrado num **balão**). Têm **paciência**.
- Vá até o cliente e aperte **E** para **vender** (precisa do produto numa prateleira).
- Aperte **E** perto de uma **prateleira** para **repor** do estoque do fundo.
- Aperte **E** perto das **caixas do fundo** para **comprar** de **fornecedores**
  (honesto / caro / rápido / clandestino — cada um com preço e risco).
- **Produtos perecíveis vencem** — venda/repõe antes ou perde reputação.
- Os **preços mudam todo dia**. Cada dia tem **despesas** (sobem com o tempo).
- **Eventos morais** aparecem (cliente sem dinheiro, oferta ilegal, fiscalização…) —
  cada escolha mexe em **dinheiro** e **reputação**.
- **Game Over:** o dinheiro zera ou a reputação despenca. **Vitória:** chegar a
  **R$ 900** mantendo a reputação.

## 🖥️ Gerar o .exe (Windows)
File → **Build Settings** → Plataforma **Windows** → **Build**. (A cena já entra na build
quando você usa o menu "Criar Cena do Jogo".)

## 📂 Estrutura dos scripts
| Arquivo | Papel |
|---|---|
| `Bootstrap.cs` | Ponto de entrada — monta câmera, managers, jogador, UI |
| `GameConfig.cs` | Todos os dados/balance (produtos, clientes, fornecedores, eventos, mapa) |
| `GameManager.cs` | Cérebro — telas, dias, economia, clientes, escolhas, vitória/derrota |
| `Market.cs` | Monta a sala/prateleiras do mapa + colisão + validade |
| `PlayerCtrl.cs` | Movimento top-down + interação |
| `AssetDB.cs` | Carrega sprites/áudio da Higgsfield (de `Resources`) |
| `AudioManager.cs` | Música (ambiente/crise) + SFX |
| `UI.cs` | HUD + 4 telas + diálogo de evento + painel de compra (uGUI por código) |
| `Util.cs` | Helpers (sprites de fallback) |
| `Editor/SetupTool.cs` | Menu "Criar Cena do Jogo" |

## ⚠️ Observações honestas
- **Eu (assistente) não consigo rodar o Editor da Unity aqui**, então não pude dar Play.
  Se aparecer **qualquer erro de compilação** quando você abrir, me manda o erro que eu
  corrijo na hora.
- Os **sprites de personagens/produtos** foram gerados como folhas e são fatiados por código
  numa grade. Se algum ficar "cortado" estranho, dá pra eu regerar individualmente ou ajustar
  o fatiamento — é só pedir.
- A **trilha de crise** (bgm_crise) é opcional: se não existir, toca a música ambiente.

Créditos: Pedro Henrique Araújo Ferreira · Gabriel Silva de Miranda · Rayane Araújo Teles · Andrey Bueno Isoton.
