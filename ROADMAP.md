# 🗺️ Roadmap & Planos Futuros - Tiny Island

> **Status do Projeto:** Versão `1.2.0` (v1.2 — Direção de Arte, Espuma de Borda & Paisagem Sonora Entregues! 🎨🎶)  
> **Filosofia de Atualizações:** Incrementos pequenos, divertidos e focados em aprendizado prático — sem sobrecarga (*overengineering*)!

---

## 📌 Próximos Passos (Visão Geral)

```
┌─────────────────────────┐       ┌─────────────────────────┐       ┌─────────────────────────┐
│     v1.0 (Entregue)     │  ──►  │     v1.1 (Concluída)    │  ──►  │     v1.2 (Em Curso)     │
│  MVP Arquipélago 3D     │       │  Menu, Pulo & Áudio     │       │ Arte, Espuma & Música   │
└─────────────────────────┘       └─────────────────────────┘       └─────────────────────────┘
```

---

## 🎯 v1.1 — Atualização de Acolhimento & Controles (Concluída ✅)

*O objetivo desta versão foi dar uma sensação mais completa de jogo finalizado logo ao abrir, além de expandir a mobilidade do explorador e o aconchego sonoro.*

- [x] **🎬 Tela de Menu Inicial (Start Screen):**
  - Estado inicial antes de começar a partida com o título estilizado do jogo e animação de flutuação.
  - Câmera em órbita suave (*slow rotate*) contemplando a ilha em 360°.
  - Botão interativo **"Começar Expedição"** com transição cinematográfica fluida para o gameplay.
  - Modal estilizado **"Como Jogar"** com controles e objetivos.
  - Menu de Pausa minimalista (ESC) e atalhos rápidos via teclado/gamepad.
  - Créditos com o nome de Edu Braga como criador.
- [x] **🦘 Mecânica e Animação de Pulo:**
  - Acionamento pela tecla `Espaço` (ou botão sul no controle / `A`).
  - Lógica no `PlayerController` permitindo pular sobre pequenas pedras da praia e colinas com *Coyote Time*.
  - Animação procedural de salto no ar e efeitos sonoros de pulo e aterrissagem suave.
- [x] **🌊 Áudio Ambiente Suave:**
  - Gerador de som de mar contínuo em loop procedural perfeito com ondas suaves e brisa tropical.
  - Efeitos de pulo ágil e aterrissagem suave na areia sintetizados em tempo real.

---

## 🎨 v1.2 — Direção de Arte, Espuma Costeira & Paisagem Sonora (Concluído ✅)

*O objetivo desta versão foi dar o salto de apresentação estética e aconchego sensorial de Tiny Island.*

- [x] **🌊 Espuma de Borda Costeira Dinâmica (`WaterShorelineFoam`):**
  - Anéis estilizados de espuma suave nas bordas de areia da praia principal, ilhota do santuário, ilhota do recife e pedras do mar.
  - Respiração senoidal de maré simulando o quebrar e recuo das ondas.
  - Modulação contínua de escala e transparência (ao recuar fica tênue, ao quebrar na praia fica branca e nítida).
  - Material `M_EspumaMar` com renderização translúcida dedicada (`renderQueue = 3050`) e emissão suave ao sol.
- [x] **🎶 Trilha Sonora Tropical Aconchegante Procedural (`CozyIslandMelody`):**
  - Composição original de 16 compassos a 84 BPM (~45.7s) em loop contínuo com cross-fade perfeito.
  - Camada cantante de Kalimba (Thumb Piano com chime de sino), dedilhado de Ukulele/Violão em colcheias e contrabaixo acústico suave.
  - Sintetizado 100% em código C# em runtime sem arquivos externos ou peso de build.
  - Integrado ao controle global de áudio e mudo da interface.
- [x] **✨ Pós-Processamento URP Calibrado & Iluminação Artística:**
  - Configuração de `GlobalVolume` na cena com perfil persistente `SampleSceneProfile`.
  - **Bloom suave:** Realce dourado nas moedas de ouro, lanterna do farol e fogueira.
  - **Color Adjustments:** Saturação quente tropical (+18%), contraste aprimorado e filtro solar sem aspecto "lavado".
  - **White Balance:** Temperatura aconchegante de verão (+8°C de calor solar).
  - **Vignette:** Escurecimento sutil nas bordas estilo diorama/tilt-shift.
  - **Transição Cinematográfica Sunset:** O `IslandAtmosphereManager` agora interpola suavemente os parâmetros de pós-processamento para um dourado alaranjado rico ao alternar entre Dia e Pôr do Sol!
- [ ] **🏖️ Detalhes e Micro-Adornos na Areia:**
  - Conchinhas do mar e pequenos pedaços de madeira encalhados na areia.
  - Pequeno barquinho a remo ancorado ou encalhado na beira da areia.
- [ ] **🌿 Natureza & Micro-Adornos:**
  - Folhas das palmeiras com leve balanço procedural ao vento.
  - Gaivotas voando em círculos no céu ou pequenos siris caminhando na praia.

---

## 🌅 v1.3 — Atmosfera & Recursos Opcionais

*Ideias de expansão quando as versões anteriores estiverem concluídas.*

- [x] **🕒 Iluminação de Pôr do Sol (*Golden Hour*):**
  - Opção no menu inicial, menu de pausa e atalho `T` para alternar entre dia ensolarado e fim de tarde dourado.
  - Transição cinematográfica suave com sol baixo, sombras longas, luz do Farol com rotação contínua do feixe pelo mar e fogueira radiante.
- [ ] **📸 Modo Foto / Pause:**
  - Tecla `ESC` para pausar o cronômetro, ocultar o HUD e permitir mover a câmera livremente para capturar screenshots bonitas.
- [ ] **📦 Build Executável & Publicação WebGL:**
  - Gerar build para WebGL (jogável no navegador via itch.io) ou `.exe` empacotado.

---

## 💡 Caixa de Ideias & Brainstorming (Backlog Aberto)

*Espaço livre para anotar qualquer inspiração futura sem a obrigação de implementar agora:*

* [ ] *Sistema de conquistas locais (ex: "Coletar todas as moedas em menos de 20 segundos").*
* [ ] *Minigame opcional de chacoalhar a palmeira para derrubar um coco.*
* [ ] *Modo noite estrelada com vaga-lumes flutuando na grama.*
* [ ] *Pegadas na areia temporárias atrás do jogador.*

---

## 📝 Dica de Gamedev para o Roadmap:
> **Regra de Ouro:** Escolha **uma única caixinha `[ ]` por vez**. Implemente, teste, dê um commit no Git e sinta o prazer de marcar o `[x]` antes de começar a próxima tarefa!
