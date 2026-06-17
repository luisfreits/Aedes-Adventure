# Aedes Adventure

Aedes Adventure é um jogo educacional focado na conscientização e combate à dengue, desenvolvido como projeto semestral para a disciplina de **Projetos Práticos em Tecnologia da Informação** e apresentado na **Info Week**, durante a exposição de Projetos Integradores, no Centro Universitário de Patos de Minas (UNIPAM).

O projeto une mecânicas de jogos de plataforma e ação com elementos pedagógicos, instruindo o jogador sobre as formas de prevenção e combate aos vetores da doença enquanto avança pelos desafios.

---

## Recursos e Mecânicas do Jogo

* **Estrutura de Níveis:** O jogo conta com 3 níveis principais de dificuldade progressiva.
* **Modo Speedrun:** Modo focado em desempenho que contabiliza o tempo de conclusão. Foi projetado para aumentar o fator replay do software e auxiliar na fixação do conhecimento por meio da repetição assistida.
* **Movimentação Avançada:** O personagem possui mecânica de pulo duplo (limite máximo de 2 pulos consecutivos).
* **Sistema de Diálogos:** Integração de narrativa e dados educativos utilizando o plugin Dialogic 2.
* **Objetivo Principal:** O jogador deve localizar e eliminar os focos de mosquitos espalhados pelo mapa que funcionam como geradores (*spawners*) de inimigos, assimilando conteúdos informativos durante o processo.

### Interfaces Específicas
* **Tela de Run Setup:** Atua como intermediária entre o menu principal e o modo Speedrun, fornecendo o campo necessário para o jogador inserir seu nome antes de iniciar a partida cronometrada.
* **Tela de Ranking:** Interface dedicada a exibir a classificação global dos jogadores com base nos tempos salvos no banco de dados.

### Combat e Bestiário (Inimigos)
* **Ataque Base:** O combate padrão consiste em ataques normais de curto alcance ou na eliminação de ameaças ao pular diretamente sobre os inimigos (mecanica de pisada).
* **Pernilongo Albopictus:** Inimigo com movimentação padrão pelo cenário que causa dano por contato direto.
* **Aedes Albopictus:** Possui o mesmo comportamento de movimentação do pernilongo comum, mas executa investidas rápidas (*dash*) na direção do jogador.
* **Aedes Aegypti (Chefe Final):** Posicionado no encerramento da jornada, o chefe gera mosquitos menores que avançam contra o personagem. 
* **Mecânica de Parry:** Exclusiva para o confronto final contra o chefe, permitindo ao jogador rebater os projéteis/mosquitos gerados pelo Aedes Aegypti diretamente contra ele.

---

## Controles do Jogo

| Comando | Tecla correspondente | Função |
| :--- | :--- | :--- |
| Mover para a Esquerda | **A** | Movimenta o personagem para a esquerda. |
| Mover para a Direita | **D** | Movimenta o personagem para a direita. |
| Pular | **Espaço** | Executa o pulo básico e o pulo duplo (máximo de 2). |
| Atacar | **Q** | Realiza o ataque normal de curto alcance. |
| Parry | **p** | Realiza a defesa perfeita e reflete o projétil. |

---

## Arquitetura Tecnológica

O ecossistema do projeto foi dividido em duas camadas principais: o cliente (jogo) e o servidor de persistência (API e Banco de Dados).

### Cliente (Jogo)
* **Engine:** Godot Engine 4.6.3.
* **Linguagem de Programação:** C# (Ambiente de execução .NET).
* **Ambiente de Desenvolvimento (IDE):** Visual Studio Code.

### Infraestrutura e Backend (API e Banco de Dados)
* **Tecnologia do Servidor:** Node.js (desenvolvido em JavaScript).
* **Hospedagem da API:** Vercel.
* **Banco de Dados:** PostgreSQL hospedado e gerenciado na plataforma Aiven.
* **Segurança:** A API atua como uma camada intermediária de isolamento, protegendo o banco de dados contra acessos diretos maliciosos e vulnerabilidades de injeção de código (*SQL Injection*).
* **Sincronização:** Utilizado no Modo Speedrun para computar e atualizar, em tempo real, um ranking global. O sistema salva automaticamente o nome do jogador e o tempo exato de conclusão da partida. Não há necessidade de chaves adicionais de autenticação por parte do usuário final para a execução e computação das runs de velocidade.

---

## Distribuição

O jogo está preparado para distribuição web e desktop.
* **Plataforma de Hospedagem:** Itch.io (Link pendente).

---

## Universidade e Desenvolvedores

* **Backend e Game Dev:** Luis Freitas ([luisfreits](https://github.com/luisfreits)).
* **Design de Níveis e Arte 2D:** Lucas Gabriel ([lucasgts-crtl](https://github.com/lucasgts-crtl)).
* **Instituição:** Centro Universitário de Patos de Minas (UNIPAM).
