# Jungle — Parte 1: PlayerController + Movimento + StealthSystem (visão/ruído)

Este código vive na pasta `jungle-game/` do repositório `ampara`, num branch
próprio (`claude/charming-cannon-cbil6c`), separado do produto de saúde do
resto do repo. Não é um repositório dedicado (meu GitHub App não tem
permissão para criar repositórios novos), mas evita que você precise baixar
zip: a cada parte eu dou commit/push aqui e você só precisa `git pull`.

## O que está aqui

```
Assets/Jungle/Scripts/
  Core/
    CharacterStats.cs   -> ScriptableObject com os atributos de cada personagem
    CoverZone.cs         -> marca arbustos (bloqueio parcial + camuflagem)
    StealthSystem.cs      -> ruído emitido + LOS (linha de visão) reutilizável
  Player/
    PlayerController.cs  -> movimento top-down, sprint, agachar
    StaminaSystem.cs      -> stamina (drena no sprint, regenera com atraso)
  UI/
    DebugHUD.cs           -> HUD OnGUI provisório (sem arte) para testar
  Enemy/                  -> vazio de propósito, chega na Parte 2 (EnemyAI)
```

## ⚠️ Sobre os números de balanceamento

O arquivo `jungle_prototipo.html` não chegou até este ambiente (esta sessão
roda em um container isolado, sem acesso a anexos do chat — só a arquivos que
já estejam num repositório Git — e ele não está no `ampara`). Por isso os
valores abaixo são **estimativas
de design plausíveis**, não os números reais do protótipo. Estão todos
centralizados nos assets `CharacterStats` para você comparar e ajustar em
5 minutos assim que reabrir o protótipo — não deve ser necessário mexer em
código para recalibrar.

| Parâmetro | Valor inicial | Onde ajustar |
|---|---|---|
| Velocidade andando | 3.5 m/s | CharacterStats.walkSpeed |
| Velocidade correndo | 6.5 m/s | CharacterStats.sprintSpeed |
| Velocidade agachado | 1.8 m/s | CharacterStats.crouchSpeed |
| Stamina máxima | 100 | CharacterStats.maxStamina |
| Drenagem de stamina (sprint) | 20/s | CharacterStats.sprintDrainPerSecond |
| Regeneração de stamina | 12/s | CharacterStats.staminaRegenPerSecond |
| Atraso antes de regenerar | 0.75 s | CharacterStats.staminaRegenDelay |
| Raio de ruído — parado | 0 m | CharacterStats.idleNoiseRadius |
| Raio de ruído — andando | 4 m | CharacterStats.walkNoiseRadius |
| Raio de ruído — correndo | 9 m | CharacterStats.sprintNoiseRadius |
| Raio de ruído — agachado | 1.2 m | CharacterStats.crouchNoiseRadius |
| Camuflagem no arbusto | ×0.35 na suspeita | CoverZone.camouflageMultiplier |

## Recomendação para a IA do bot (Parte 2): NavMesh, não raycasting puro

Vou usar **NavMeshAgent** para patrulha e perseguição, com **raycasting
apenas para os checks de linha de visão/percepção** (que é exatamente o que
`StealthSystem.IsVisibleFrom(...)` já expõe, pronto para o `EnemyAI` chamar).

Por quê:
- O NavMesh já resolve contornar árvores/arbustos/cenário automaticamente —
  raycasting puro exigiria eu implementar A* ou steering manual do zero, sem
  ganho nenhum para uma fase estática (sem streaming de nível).
- Patrulha por waypoints é nativa (`SetDestination`), e perseguição também
  (basta atualizar o destino para a posição do jogador a cada frame).
- Raycasting continua insubstituível para UMA coisa: "o bot consegue *ver* o
  jogador agora?" — isso o NavMesh não responde, e é o que o `StealthSystem`
  já implementa de forma genérica (mesmo método será usado pelo bot e,
  futuramente, pelo fog-of-war do jogador).

## Passos manuais no Unity Editor (você precisa fazer isso — eu não consigo)

1. **Clonar o repositório** (uma vez só):
   `git clone -b claude/charming-cannon-cbil6c https://github.com/APPAmpara/ampara.git`
   A partir daqui, sempre que eu avançar uma parte, um `git pull` na pasta
   já traz os arquivos novos — sem zip, sem copiar nada manualmente entre sessões.
2. **Criar o projeto Unity**: Unity Hub → New Project → template *3D (URP ou
   Built-in, tanto faz)* → nome "Jungle". Depois, copie (ou crie um link
   simbólico, se preferir) a pasta `ampara/jungle-game/Assets/Jungle/` para
   dentro da pasta `Assets/` desse projeto Unity. Esse é o único passo manual
   de "arquivo" que sobra — o resto do fluxo (puxar código novo) já fica
   automático via git.
3. **Layers**: Edit → Project Settings → Tags and Layers → crie uma layer
   chamada `Obstacle` (ex: User Layer 8).
4. **Cena de teste**:
   - `GameObject > 3D Object > Plane` — isso é o chão.
   - `GameObject > 3D Object > Capsule` — isso é o jogador. Renomeie para
     `Player`. Posicione em (0, 1, 0).
   - `GameObject > 3D Object > Cylinder` — vira uma "árvore". Coloque a
     layer dele como `Obstacle`. Deixe o Collider padrão (NÃO marcar
     "Is Trigger" — precisa ser sólido para bloquear fisicamente E bloquear
     o raycast de visão).
   - `GameObject > 3D Object > Sphere` (achatada no eixo Y) — vira um
     "arbusto". No Collider dela, marque **Is Trigger = true**.
5. **Configurar o Player**:
   - Adicione os componentes (Add Component): `Character Controller`,
     `PlayerController`, `StaminaSystem`, `StealthSystem`.
   - No `PlayerController`, arraste um asset `CharacterStats` no campo Stats
     (ver passo 6).
   - No `StealthSystem`, no campo **Obstacle Mask**, marque a layer
     `Obstacle` criada no passo 3.
   - (Opcional, só para testar LOS sem bot ainda) crie um Cubo vazio na
     cena, posicione longe do Player, e arraste-o no campo **Debug Watcher**
     do `StealthSystem`. Selecione o Player na hierarquia e olhe a Scene
     View: uma linha vermelha aparece quando o "cubo" enxergaria o jogador,
     cinza quando está bloqueada (ex: atrás da árvore).
6. **Criar os 3 CharacterStats**: botão direito na pasta
   `Assets/Jungle/ScriptableObjects` → `Create > Jungle > Character Stats`.
   Crie um para Macaco, um para Caçador, um para Índio. Preencha os campos
   (os valores da tabela acima servem de ponto de partida para os três; as
   diferenças assimétricas entre personagens — melee vs. à distância,
   audição direcional do Índio — entram na Parte 3, quando o combate e a
   percepção especial forem implementados).
7. **Câmera top-down**: selecione a `Main Camera`, mude Projection para
   **Orthographic**, posicione bem acima do Player (ex: (0, 12, 0)), rotação
   (90, 0, 0) olhando reto para baixo. Ajuste o Size ortográfico até
   enquadrar a área de teste.
8. **HUD de debug**: crie um GameObject vazio chamado `DebugHUD`, adicione o
   componente `DebugHUD`, arraste o Player nos três campos (Player, Stamina,
   Stealth — os três componentes estão no mesmo GameObject do Player).
9. **Input**: nada a configurar — o controle usa teclado direto (WASD/setas
   para mover, Shift esquerdo para correr, Ctrl esquerdo ou C para agachar),
   sem depender do Input Manager nem do pacote novo Input System.
10. **Testar (Play)**: ande, corra (veja a stamina cair e o raio de ruído
    subir no HUD), agache (raio de ruído cai perto de zero), entre no
    "arbusto" (Cobertura muda para Bush e o multiplicador de camuflagem
    aparece), e fique atrás/na frente do "cubo" de debug watcher para
    confirmar que a árvore bloqueia a linha vermelha.

## O que NÃO está nesta parte (de propósito)

`EnemyAI`, `DetectionMeter` e `TensionManager` ficam para a Parte 2, como
você pediu — para você validar esta base primeiro. O `StealthSystem` já foi
desenhado para que o `EnemyAI` só precise chamar
`stealthSystem.IsVisibleFrom(...)` sem precisar reescrever nada aqui.
