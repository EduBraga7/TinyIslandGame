# 🏝️ Tiny Island - Documento de Design de Jogo (GDD)

> **Versão:** 1.0 (MVP Concluído)  
> **Gênero:** Micro-Aventura 3D Aconchegante (*Cozy Exploration*)  
> **Plataforma:** PC (Teclado e Controle)  
> **Engine & Ferramentas:** Unity 6 (URP) + Blender 5.2 (via MCP) + C#  
> **Status:** ✅ Entregue & Jogável  

---

## 1. Visão Geral & Premissa

**Tiny Island** é um micro-jogo 3D em visão isométrica focado em uma experiência agradável, intuitiva e sem frustrações. O jogador controla um pequeno explorador numa ilha paradisíaca tropical com um ciclo de jogo fechado: explorar o ambiente, reunir as moedas antigas perdidas e desvendar o baú lendário no topo da ilha.

### Pilares de Design:
* **Aconchego (*Cozy Game*):** Atmosfera relaxante, paleta de cores solar e vibrante, sem inimigos, sem contadores punitivos de tempo e sem morte punitiva.
* **Ciclo Fechado & Objetivo Claro:** Início, exploração guiada por feedback e tela de vitória comemorativa com opção de reiniciar a partida.
* **Fluidez e Feedback Instantâneo:** Movimentação responsiva com animações procedurais de passos e respiração, áudio procedural sintetizado em tempo real e efeitos visuais festivos.

---

## 2. Loop de Gameplay (*Core Loop*)

```
┌────────────────────────────────────────────────────────┐
│                   INÍCIO DA PARTIDA                    │
│      Spawn no centro da ilha com HUD de objetivo       │
└───────────────────────────┬────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│                 EXPLORAÇÃO & COLETA                    │
│   • Movimentação livre pelo arquipélago e ilhotas      │
│   • Pulo em pedras, conveses e encostas com tolerância │
│   • Coleta de 12 Moedas Douradas (som "plim!")         │
│   • Respawn seguro automático caso caia na água        │
└───────────────────────────┬────────────────────────────┘
                            │
                            ▼
┌────────────────────────────────────────────────────────┐
│                   VITÓRIA & REPLAY                     │
│   • Abertura animada do Baú no Altar das Ruínas        │
│   • Luz dourada, partículas festivas e fanfarra        │
│   • Painel de vitória com tempo e botão de reiniciar   │
└────────────────────────────────────────────────────────┘
```

---

## 3. Mecânicas de Jogo & Sistemas

### 3.1. Movimentação, Pulo e Controles
* **Suporte de Entrada:** Compatível com o novo **Input System do Unity 6** e teclado tradicional/gamepad:
  * **Teclado:** Teclas `W, A, S, D` ou `Setas Direcionais` para mover; `Espaço` para pular.
  * **Gamepad:** Alavanca analógica esquerda (`Left Stick`) para mover; Botão Sul (`A`) para pular.
* **Dinâmica de Movimento & Pulo:**
  * Movimentação no plano X/Z com colisão física via `CharacterController`.
  * Rotação suave (*Slerp*) virando o explorador na direção em que se desloca.
  * Gravidade calibrada com suporte a pulo ágil (`jumpHeight = 1.65m`) e tolerância de borda (*Coyote Time* de 0.15s) para saltos responsivos sobre pedras da praia e obstáculos.
* **Animação Procedural do Explorador:**
  * **Em Movimento:** Braços e pernas oscilam alternadamente através de senóides matemáticas, com leve salto vertical (*bobbing*) do tronco.
  * **Pulo no Ar:** Pose dinâmica e expressiva de salto com braços erguidos e pernas estendidas.
  * **Parado (Idle):** Leve movimento de respiração senoidal no torso, com membros retornando suavemente à pose neutra via interpolação.

### 3.2. Sistema de Queda e Respawn Seguro
* **Comportamento Real:** Se o jogador caminhar para fora dos limites da ilha e cair no oceano (coordenada `Y < -1.2`), o jogo executa um **respawn instantâneo**:
  * O jogador é reposicionado imediatamente no ponto seguro central da ilha em `(0, 1.5, -2)`.
  * A velocidade de queda vertical é zerada.
  * **Sem Punição:** Não há perda de vidas, não há tela de *Game Over* e as moedas já coletadas permanecem salvas.

### 3.3. Itens Coletáveis (Moedas Antigas)
* **Quantidade:** 12 moedas douradas espalhadas estrategicamente pelo arquipélago (Píer Sul, Veleiro ancorado, Acampamento, Bosque Tropical, Travessia de Pedras, Ilhota do Recife, Colina Norte, Farol Tropical, Mirante, Ponte de Madeira e Santuário Antigo).
* **Comportamento Visual:**
  * Rotação contínua no eixo vertical (90° por segundo).
  * Flutuação vertical oscilante procedural baseada em onda senoidal.
* **Detecção e Coleta:**
  * Colisor do tipo gatilho (`SphereCollider` com `isTrigger = true`).
  * Ao ser tocada pelo explorador, a moeda é removida da cena, incrementa a pontuação no `IslandGameManager` e dispara o som procedural de coleta ("plim!").

### 3.4. Baú do Tesouro & Condição de Vitória
* **Posição:** Altar central do Santuário das Ruínas Antigas na Ilhota Leste.
* **Composição:** Modelo 3D com madeira rústica, reforços de ferro, moedas douradas, barras de ouro e gemas lapidadas (rubis, esmeraldas e safiras).
* **Mecânica de Abertura:**
  * O baú permanece trancado até a coleta da **12ª moeda**.
  * Ao atingir a 12ª moeda, a tampa rotaciona suavemente com interpolação `SmoothStep` até o ângulo de abertura (-100°).
  * Uma luz pontual dourada (`TreasureGlowLight`) acende de dentro do baú, iluminando a pilha de tesouro.
  * Um sistema de partículas (`ChestOpenParticles`) emite uma rajada cônica de partículas douradas comemorativas.
  * Uma fanfarra triunfante em acorde ascendente é sintetizada e executada.

### 3.5. Interface de Usuário (HUD & Menus)
* **Tela de Menu Inicial (Start Screen):**
  * Título dinâmico: `"🏝️ TINY ISLAND"` com animação suave de flutuação vertical senoidal e subtítulo `"Uma Micro-Aventura Tropical"`.
  * Câmera em órbita suave panorâmica de 360° ao redor da ilha, destacando as águas tropicais, palmeiras, moedas douradas e o explorador em idle.
  * Botão Primário: `"▶ COMEÇAR EXPEDIÇÃO"` (dourado luminoso com bordas arredondadas e hover dinâmico).
  * Ao iniciar: dispara arpeggio ascendente acolhedor sintetizado, faz fade-out gradual da UI e transiciona a câmera suavemente (`Mathf.SmoothStep`) da órbita panorâmica para a visão isométrica de perseguição do explorador.
  * Botão Secundário: `"📜 Como Jogar"` — abre janela modal com instruções de controles e objetivo.
  * Botão de Configuração Rápida: `"🔊 Som: Ativado / 🔇 Som: Desativado"` — alterna o áudio global do jogo.
  * Botão de Saída: `"🚪 Sair do Jogo"` — aciona confirmação segura antes de fechar o jogo.
  * Atalho de conveniência: Início instantâneo ao pressionar `Espaço`, `Enter` ou botão sul do controle (`A`). Pressionar `ESC` abre o diálogo de saída.
  * Créditos autorais com o nome de Edu Braga no rodapé.
* **HUD Superior Esquerdo:**
  * Mostrador contínuo de moedas: `🪙 Moedas: X / 12` em painel de vidro escuro translúcido com acabamento arredondado.
  * Mensagem de orientação em jogo: `"Colete todas para abrir o Baú do Tesouro!"`.
  * Mensagem pós-abertura: `"✨ O Baú do Tesouro foi aberto!"`.
  * Indicador de cronômetro e dica `[ESC] Menu` no canto superior direito.
* **Menu de Pausa & Opções (Acionado com a tecla ESC ou Start no controle):**
  * Congela o cronômetro da expedição e exibe uma central com as seguintes opções:
    1. `"▶ Continuar Expedição"` — retoma a partida imediatamente.
    2. `"🔊 Som: Ativado / Desativado"` — controle de mudo em tempo real.
    3. `"📜 Controles & Objetivo"` — consulta do guia do jogador.
    4. `"🔄 Reiniciar Ilha"` — recarrega a cena atual e repõe as moedas.
    5. `"🏠 Menu Principal"` — retorna à tela inicial com a câmera em órbita.
    6. `"🚪 Sair do Jogo"` — confirmação e fechamento da aplicação.
* **Painel de Conquista (Tela de Vitória):**
  * Surge centralizada na tela assim que o objetivo é cumprido.
  * Título: `🏆 PARABÉNS! 🏆`.
  * Mensagem: `"Você descobriu o Segredo de Tiny Island!"`.
  * Tempo de Expedição: exibe o tempo total da partida em segundos com uma casa decimal (ex: `Tempo de Expedição: 24.3 segundos`).
  * Botões Interativos: `"🔄 Jogar Novamente"`, `"🏠 Menu Inicial"` e `"🚪 Sair do Jogo"`.

---

## 4. Ambiente, Câmera e Arte

### 4.1. Cenário e Level Design
* **Ilha Base:** Cilindro de areia tropical de 26m de diâmetro com `MeshCollider` plano (garantindo superfície de caminhada plana, sem distorções de colisores esféricos/cápsula).
* **Platô Superior:** Camada elevada de grama verde de 21m de diâmetro com `MeshCollider` plano.
* **Oceano:** Plano aquático estilizado (100x100m) em tom azul-turquesa que delimita o horizonte da ilha.
* **Vegetação e Adornos:**
  * **4 Palmeiras Tropicais:** Troncos curvos com copa de folhas e `CapsuleCollider` sólido para evitar atravessamentos indesejados.
  * **6 Pedras de Praia:** Dispostas nas bordas de areia com `MeshCollider` convexo para composição visual e obstáculos naturais.

### 4.2. Câmera Isométrica
* **Comportamento:** Acompanhamento em terceira pessoa suave com amortecimento via `Vector3.SmoothDamp`.
* **Posicionamento:** Offset fixo de `(0, 6.2, -5.5)` com ângulo apontado ligeiramente acima do tronco do personagem, oferecendo visibilidade ampla e sem obstruções da ilha inteira.

### 4.3. Paleta Visual (Universal Render Pipeline - URP)
* Materiais estilizados com acabamentos metálicos e de rugosidade calibrados para visual vibrante:
  * Areia quente dourada e grama verde viva.
  * Água cristalina com alto brilho especular.
  * Ouro polido reflexivo para as moedas e tesouros.
  * Madeira nobre e ferro forjado para o baú.
  * Gemas com cores saturadas (rubi vermelho, esmeralda verde, safira azul).

---

## 5. Arquitetura Técnica & Pipeline

### 5.1. Componentes de Software (C#)
| Script | Localização | Responsabilidade |
| :--- | :--- | :--- |
| **`PlayerController.cs`** | `Assets/Scripts/` | Leitura de controles, movimentação física via `CharacterController`, animações procedurais de passos/idle e teletransporte de respawn aquático. |
| **`CameraFollow.cs`** | `Assets/Scripts/` | Perseguição suave da câmera em perspectiva isométrica com amortecimento (`SmoothDamp`). |
| **`CollectibleItem.cs`** | `Assets/Scripts/` | Animação procedural de flutuação senoidal e rotação de moedas, além da detecção de coleta com gatilho. |
| **`TreasureChest.cs`** | `Assets/Scripts/` | Controle de estados do baú, corrotina de rotação da tampa com `SmoothStep`, disparo da luz dourada e sistema de partículas. |
| **`IslandGameManager.cs`** | `Assets/Scripts/` | Singleton de gerenciamento do estado da partida, contagem de moedas, cronômetro, sintetizador de áudio procedural e desenho do HUD/vitória via `OnGUI`. |
| **`IslandSceneBuilder.cs`** | `Assets/Editor/` | Ferramenta de automação no Editor Unity para sincronização de modelos 3D, materiais, colisores e reconstrução da cena em 1 clique. |

### 5.2. Áudio Procedural em Tempo Real
O projeto não depende de arquivos de áudio externos (`.wav`/`.mp3`), gerando ondas sonoras via código matemático em tempo de execução:
* **Som de Moeda:** Arpeggio agudo de duas frequências (nota B5 a 987.77 Hz seguida de E6 a 1318.51 Hz) com decaimento suave em 0.22 segundos.
* **Fanfarra de Vitória:** Acorde triunfante ascendente em 4 notas (C5 -> E5 -> G5 -> C6) sintetizado com decaimento exponencial ao abrir o baú.

### 5.3. Pipeline Blender 3D + Unity via MCP
Todos os modelos 3D autorais foram gerados e calibrados no Blender via automação MCP e exportados como `.FBX` para a pasta `Assets/Models/`:
* `PlayerExplorer.fbx`: Avatar do explorador com hierarquia modular para permitir animação procedural das articulações.
* `TreasureChest.fbx`: Baú com base sólida, tampa separada com pivô articulado e tesouro interno.
* `Coin.fbx`: Moeda estilizada com relevo e espessura.
* `PalmTree.fbx`: Palmeira com tronco segmentado e copa tropical.
* `Rock.fbx`: Pedras de praia estilizadas com faces facetadas (*low-poly* suave).

---

## 6. Como Executar e Testar

1. **No Unity Editor:**
   * Abra a cena principal em `Assets/Scenes/`.
   * Caso queira remontar o cenário ou restaurar os objetos com todos os materiais e componentes, use o menu:  
     👉 **`TinyIsland` > `🏝️ Construir Ilha Completa`**
   * Pressione o botão **Play (▶)** para iniciar a partida.
2. **Controles:**
   * Use **W, A, S, D** ou **Setas** para caminhar.
   * Colete as **12 moedas** espalhadas pelo arquipélago (farol, píer, barco, ruínas, colinas e recife).
   * Veja o baú se abrir e comemore a vitória com o tempo final de expedição!

