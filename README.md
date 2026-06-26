# Tudo Tem Preço — Mercado Final

Jogo desenvolvido **durante a aula** (disciplina de Desenvolvimento de Jogos), em **Unity 6 (C#)**.

## 📝 Resumo do projeto (do GDD)

**Tudo Tem Preço — Mercado Final** é um **simulador de gerência de mercadinho** em **pixel art top-down 2D**. Você comanda uma lojinha de bairro: anda pelo mercado em 4 direções, **atende clientes** que chegam querendo um produto específico (cada um com sua paciência), **repõe as prateleiras**, **compra estoque de fornecedores** (honesto, caro, rápido ou clandestino) e cuida da **validade** dos produtos perecíveis.

A economia é **dinâmica** (os preços oscilam a cada dia) e surgem **eventos de escolha moral** (cliente sem dinheiro, oferta ilegal, fiscalização surpresa…), onde cada decisão mexe no **dinheiro** e na **reputação**.

O jogo tem **3 níveis jogáveis**, cada um com uma **meta de dinheiro** crescente — o dinheiro e a reputação **passam de um nível para o outro**. Você **vence** ao cumprir as metas dos 3 níveis sem deixar a reputação despencar, e **perde** (Game Over) se o dinheiro zerar ou a reputação ficar baixa demais.

## 🎮 Mecânicas implementadas (conforme o GDD)

- Movimento top-down (WASD / setas) com colisão por tiles.
- Interação contextual (E / Espaço): **atender** cliente, **repor** prateleira, **comprar** estoque.
- Clientes com tipos distintos (apressado, educado, reclamão, fiel, suspeito), paciência e gorjeta.
- Estoque + **fornecedores** com custo/qualidade/risco + **validade** (produtos vencem).
- **Economia dinâmica**: preços diferentes a cada dia; despesas diárias.
- **Eventos de escolha moral** que afetam dinheiro e reputação.
- Progressão por **dias** dentro de cada nível e por **3 níveis** (metas crescentes).
- HUD (dinheiro, meta, nível/dia, reputação, tempo do dia, clientes).
- 6 cenas: **Menu**, **Como Jogar**, **Nível 1/2/3** e **Fim** (vitória / game over) com **Jogar de novo**.
- Áudio: música ambiente e efeitos (caixa registradora, confirmação) + som nos botões.

## 🕹️ Como jogar

WASD/setas para andar · **E** ou **Espaço** para atender cliente / repor prateleira / comprar no fundo. Bata a meta de dinheiro de cada nível antes que o tempo/dias acabem.

## ▶️ Como abrir no Unity

1. Abra o projeto no **Unity 6** (testado na **6000.0.77f1**).
2. Abra a cena `Assets/Scenes/Menu.unity`.
3. Aperte **Play ▶**.

> As 6 cenas ficam em `Assets/Scenes/`. Se precisar recriá-las, use o menu **Tudo Tem Preço → Criar Cenas do Jogo**.

## 👥 Equipe

- **Pedro Henrique A. Ferreira**
- **Gabriel Miranda**
- **Andrey Bueno Isoton**
- **Rayane Araújo Teles**

## 🎨 Direitos dos autores / créditos de assets

Todos os **sprites** (personagens, produtos, key art) e o **áudio** (música e efeitos sonoros)
foram **gerados pela própria equipe com ferramentas de IA generativa (Higgsfield)** — não são
obras de terceiros nem material protegido por copyright de outros autores.

- **Sprites / arte:** gerados com IA generativa (Higgsfield, modelo *nano_banana_pro*) e tratados
  pela equipe (remoção de fundo, fatiamento de animação).
- **Áudio (música e SFX):** gerados com IA generativa (Higgsfield).
- **Fontes:** fonte interna (built-in) da Unity (*LegacyRuntime / Arial*).
- **Motor:** Unity 6 (Unity Technologies).

Nenhum sprite, música ou efeito sonoro de terceiros foi utilizado.

---
*Projeto acadêmico, sem fins comerciais.*
