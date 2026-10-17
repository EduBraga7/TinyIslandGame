# 🏝️ Tiny Island - Documento de Design de Jogo (GDD) & Escopo do MVP

> **Versão:** 1.0 (MVP)  
> **Gênero:** Micro-Aventura 3D Aconchegante (*Cozy Exploration*)  
> **Plataforma:** PC (Teclado e Controle)  
> **Engine & Ferramentas:** Unity 6 (URP) + Blender 5.2 (via MCP) + C#  
> **Status:** Em Desenvolvimento Ativo  

---

## 1. Visão Geral do Jogo

**Tiny Island** é um micro-jogo 3D em visão isométrica onde o jogador assume o papel de um pequeno explorador numa ilha paradisíaca tropical. O foco é uma experiência agradável, leve e recompensadora de exploração com ciclo fechado: **Começo, Meio e Fim**.

---

## 2. Ciclo de Vida do Projeto (Começo, Meio e Fim do Desenvolvimento)

Para garantir que o nosso primeiro projeto seja **concluído com sucesso** e não fique abandonado pela metade, dividimos a produção em três etapas macro:

```
┌──────────────────────────────────────────────┐
│          1. O COMEÇO DO PROJETO              │
│    (Setup, Automação & Prova de Conceito)    │  ✅ CONCLUÍDO
└──────────────────────┬───────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────┐
│           2. O MEIO DO PROJETO               │
│      (Assets 3D, Regras de Jogo & Juice)     │  🔄 ESTAMOS AQUI
└──────────────────────┬───────────────────────┘
                       │
                       ▼
┌──────────────────────────────────────────────┐
│            3. O FIM DO PROJETO               │
│   (Menu, Polimento, Build .EXE & Entrega)    │  🎯 META FINAL
└──────────────────────────────────────────────┘
```

### 🏁 1. O Começo (Fundação & Prototipagem) — [100% Concluído]
* **Setup de Ferramentas:** Unity 6 (URP) e Blender 5.2 configurados com ponte MCP.
* **Mecânica Básica:** Movimentação fluida do jogador em 8 direções e câmera isométrica suave.
* **Ambiente Inicial:** Superfície plana corrigida com `MeshCollider` e mar com respawn automático.
* **Critério de Saída desta fase:** Boneco se movimentando com controles responsivos.

### 🔨 2. O Meio (Produção de Assets & Regras) — [Em Andamento]
* **Modelagem 3D Autoral:** Criação de palmeiras, moedas douradas e do baú do tesouro no Blender.
* **Regras de Jogo:** Coletar todas as 6 moedas espalhadas pela ilha para abrir o baú do tesouro.
* **Game Feel & Feedback:** Efeitos visuais de brilho/estrelas e efeitos sonoros ("plim!" de moedas, fanfarra de vitória).
* **Critério de Saída desta fase:** Jogo 100% jogável do começo ao fim dentro do Editor.

### 📦 3. O Fim (Polimento, Build .EXE & Entrega) — [A Concluir]
* **Menus & Telas:** Tela inicial simples ("Jogar") e tela de encerramento com tempo de vitória e replay.
* **Limpeza & Otimização:** Verificação de bugs, limpeza de scripts não usados e ajustes finais de iluminação.
* **Geração da Build Standalone (.exe):** Exportação do jogo compilado em binário executável para Windows.
* **Entrega:** Pacote `.zip` com o jogo pronto para você enviar para qualquer amigo jogar no computador dele sem precisar de Unity ou Blender instalados!
* **Critério de Saída desta fase:** Projeto 100% encerrado, entregue e jogável.

---

## 3. O Loop de Gameplay (A Experiência do Jogador)

```
[ COMEÇO: Chegada na Ilha ]
           │
           ▼
[ MEIO: Exploração & Coleta das 6 Moedas ]
           │
           ▼
[ FIM: Destrancar o Baú Lendário & Vitória! ]
```

### 🎬 O Começo (Introdução & Spawn)
* O jogador nasce no centro da ilha (área de grama segura).
* A câmera assume o ângulo isométrico suave a 45 graus.
* Um objetivo claro surge na tela: **"Encontre as 6 Moedas Antigas perdidas na ilha para abrir o Baú do Tesouro!"**

### 🧭 O Meio (Gameplay & Exploração)
* **Movimentação:** O jogador caminha livremente pelas praias de areia e pelo platô de grama, desviando dos troncos das palmeiras.
* **Segurança:** Caso o jogador caia no oceano, ele não morre de forma punitiva: uma onda o devolve suavemente para o centro da ilha (*respawn*).
* **Coleta:** O jogador localiza as 6 moedas de ouro 3D que flutuam e giram pelo cenário.
* **Feedback:** A cada moeda coletada:
  * A moeda desaparece com efeito visual e sonoro de coleta.
  * O placar na tela atualiza (`🪙 Moedas: X / 6`).

### 🏆 O Fim (Condição de Vitória & Encerramento)
* Ao coletar a **6ª moeda**:
  * A câmera faz um foco dramático no **Baú do Tesouro**.
  * A tampa do baú se abre com animação.
  * Fogos de artifício ou partículas de estrelas douradas explodem do baú.
  * Tela de vitória estilizada: **"Parabéns! Você descobriu o Segredo de Tiny Island!"** com tempo de jogo e botão para jogar novamente.

---

## 3. Arquitetura Técnica & Pipeline com IA

Este projeto utiliza uma esteira moderna de desenvolvimento assistida por IA via **MCP (Model Context Protocol)**:

```
┌──────────────────┐       MCP Protocol       ┌───────────────────┐
│     BLENDER      │ ◄──────────────────────► │    ANTIGRAVITY    │
│ (Modelagem 3D &  │                          │ (Gerente & Coder) │
│  Exportação FBX) │                          └─────────┬─────────┘
└────────┬─────────┘                                    │
         │ .FBX (Modelos)                               │ Scripts C# / Cena
         ▼                                              ▼
┌─────────────────────────────────────────────────────────────────┐
│                          UNITY 6 (URP)                          │
│        (Física, Iluminação, Gameplay, Câmera e Interface)       │
└─────────────────────────────────────────────────────────────────┘
```

### Componentes de Código:
| Script C# | Responsabilidade |
| :--- | :--- |
| **`PlayerController.cs`** | Movimentação em 8 direções, rotação suave, gravidade e respawn aquático. |
| **`CameraFollow.cs`** | Câmera isométrica cinemática com perseguição suave (*SmoothDamp*). |
| **`CollectibleItem.cs`** | Animação procedural de flutuação (*Sin*) e rotação de moedas/itens. |
| **`TreasureChest.cs`** | Baú interativo com estados: *Trancado* -> *Abrindo* -> *Aberto / Vitória*. |
| **`IslandGameManager.cs`** | Máquina de estados do jogo (*Intro*, *Playing*, *Victory*), contagem e interface. |
| **`IslandSceneBuilder.cs`** | Construtor automatizado no Unity Editor para montar o cenário em 1 segundo. |

---

## 4. Checklist & Roadmap do MVP

### Fase 1: Fundação & Ambientes (✅ Concluído)
- [x] Inicialização do projeto no Unity 6 com Universal Render Pipeline (URP).
- [x] Configuração dos servidores MCP no Antigravity (`unity-mcp` e `mcp-for-blender`).
- [x] Criação do gerador automatizado de cena (`IslandSceneBuilder.cs`).
- [x] Chão plano com `MeshCollider` (eliminando a bolha de colisão do cilindro).
- [x] Movimentação do jogador em WASD/Setas com colisão e rotação suave.
- [x] Câmera isométrica em terceira pessoa.

### Fase 2: Assets 3D via Blender MCP (🔄 Em Progresso)
- [x] Modelagem da **Palmeira Tropical** no Blender e exportação FBX.
- [x] Modelagem da **Moeda Dourada** estilizada no Blender e exportação FBX.
- [ ] Modelagem do **Baú do Tesouro** (base + tampa separada para animar).
- [ ] Modelagem de pedras e detalhes decorativos para a praia.
- [ ] Modelagem do avatar do jogador (substituir a cápsula).

### Fase 3: Ciclo Completo de Gameplay (Próximo Passo)
- [x] Sistema de coleta e contagem de moedas no placar.
- [ ] Adicionar o **Baú do Tesouro** na cena (posição nobre na ilha).
- [ ] Programar a animação de abertura da tampa do baú.
- [ ] Vincular a vitória: coletar 6 moedas -> disparar abertura do baú.
- [ ] Tela de Vitória ("Fim de Jogo") na interface.

### Fase 4: Polimento, Áudio & Sensação de Jogo (*Game Juice*)
- [ ] Efeito de partículas de estrelas ao pegar uma moeda.
- [ ] Efeito de partículas ao abrir o baú (fogos / confetes de vitória).
- [ ] Efeitos sonoros:
  * Passos leves na areia/grama.
  * Som de coleta ("plim!").
  * Som de vitória / fanfarra ao abrir o baú.
  * Som ambiente de ondas do mar.
- [ ] Ajuste de iluminação URP (sombra suave e clima ensolarado).

---

## 5. Como Executar e Testar

1. **No Blender:** O servidor MCP deve estar ativo na porta padrão (`9876`) para modelagem em tempo real.
2. **No Unity:** 
   * A qualquer momento, clique em **`TinyIsland` > `🏝️ Construir Ilha Completa`** no menu do topo para sincronizar os modelos e reconstruir o cenário.
   * Aperte **Play (▶)** para testar o jogo completo.
