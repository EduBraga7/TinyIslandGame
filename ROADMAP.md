# 🗺️ Roadmap & Planos Futuros - Tiny Island

> **Status do Projeto:** Versão `1.0.0` (MVP Concluído)  
> **Filosofia de Atualizações:** Incrementos pequenos, divertidos e focados em aprendizado prático — sem sobrecarga (*overengineering*)!

---

## 📌 Próximos Passos (Visão Geral)

```
┌─────────────────────────┐       ┌─────────────────────────┐       ┌─────────────────────────┐
│     v1.0 (Atual)        │  ──►  │     v1.1 (Próxima)      │  ──►  │     v1.2 (Futuro)       │
│  MVP Entregue & Jogável │       │  Menu, Pulo & Áudio     │       │ Cenário Vivo & Detalhes │
└─────────────────────────┘       └─────────────────────────┘       └─────────────────────────┘
```

---

## 🎯 v1.1 — Atualização de Acolhimento & Controles (Foco Imediato)

*O objetivo desta versão é dar uma sensação mais completa de jogo finalizado logo ao abrir, além de expandir a mobilidade do explorador.*

- [ ] **🎬 Tela de Menu Inicial (Start Screen):**
  - Estado inicial antes de começar a partida com o título estilizado do jogo.
  - Câmera em órbita suave (*slow rotate*) contemplando a ilha.
  - Botão interativo **"Começar Expedição"** que faz a câmera transicionar suavemente para a posição de gameplay.
  - Créditos com o seu nome como criador.
- [ ] **🦘 Mecânica e Animação de Pulo:**
  - Acionamento pela tecla `Espaço` (ou botão sul no controle / `A`).
  - Lógica no `PlayerController` permitindo pular sobre pequenas pedras da praia.
  - Animação procedural: leve compressão ao tocar o chão e esticamento no ápice do salto (*Squash & Stretch*).
- [ ] **🌊 Áudio Ambiente Suave:**
  - Gerador de som de mar contínuo (ruído branco filtrado ou ondas quebrando na areia).
  - Vento suave na vegetação para aumentar a sensação de relaxamento (*cozy game*).

---

## 🌴 v1.2 — Cenário Vivo & Polimento Visual

*O objetivo desta versão é transformar a ilha em um ambiente ainda mais vibrante, rico e charmoso.*

- [ ] **🏖️ Detalhes e Vida na Praia:**
  - Conchinhas do mar e pequenos pedaços de madeira encalhados na areia.
  - Efeito visual de espuma suave na linha onde a água encosta na praia.
  - Pequeno barquinho a remo ancorado ou encalhado na beira da areia (o barco da chegada do explorador).
- [ ] **🌿 Natureza & Micro-Adornos:**
  - Pequenas flores tropicais coloridas espalhadas pelo platô de grama.
  - Folhas das palmeiras com leve balanço procedural ao vento.
  - Gaivotas voando em círculos no céu ou pequenos siris caminhando na praia.
- [ ] **✨ Partículas de Coleta Aprimoradas:**
  - Efeito de brilho estelar ao encostar em cada uma das moedas.

---

## 🌅 v1.3 — Atmosfera & Recursos Opcionais

*Ideias de expansão quando as versões anteriores estiverem concluídas.*

- [ ] **🕒 Iluminação de Pôr do Sol (*Golden Hour*):**
  - Opção no menu ou tecla para alternar entre dia ensolarado e fim de tarde dourado.
  - Tochas acendendo ao redor do baú do tesouro.
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
