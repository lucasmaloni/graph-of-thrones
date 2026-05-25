# Graph of Thrones

## Visao geral
Graph of Thrones e um projeto em Godot 4 com C# que modela as Casas de Westeros como um grafo interativo em uma cena 2D.
Cada casa e um no físico (GreatHouse ou LordlyHouse) e cada relação política e uma aresta com intensidade no intervalo [-1, 1].
As intensidades influenciam a cor das conexoes e a simulação de forças, mantendo o grafo dinâmico e observável.

## Organização geral
- scenes: cenas Godot da simulação (MainScene, InitialWesteros, GreatHouse)
- scripts: logica C# do grafo e das regras (GraphController, Reasoner, Controller, DataManager, Edge, Triplets)
- scripts/data: dados em JSON usados para iniciar casas e conexoes (init.json, raw_data.json)
- assets/icons: sigils das casas para exibição visual

## Fluxo de montagem do grafo
1. GraphController carrega GreatHouses e LordlyHouses da cena e aplica metadados de scripts/data/init.json.
2. DataManager le scripts/data/raw_data.json e cria conexoes brutas com intensidade fixa.
3. Reasoner produz conexoes implicitas de vassalagem para completar o grafo com arestas iniciais.
4. O grafo e renderizado e simulado a cada frame, com repulsao global e atracao por intensidade.

## Integração de análise semântica para construção de um grafo dinâmico
A integracao semantica é baseada em regras de dominio que traduzem tipos de relacao em pesos numericos.
- Reasoner possui um ConnectionLookup que mapeia tipos semanticos (eVassaloDe, eCasadoCom, eInimigoDe) para intensidades.
- InferGreatHousesConnections transforma ConnectionTypes do init.json em pesos e atualiza as arestas entre Casas Grandes.
- InferKingdomRules gera Triplets (sujeito, predicado, objeto) a partir de LordlyHouse.VassalOf, criando inferencias de vassalagem.
- GraphController aplica as inferencias com CreateOrUpdateConnection, mudando intensidade, cor e comportamento fisico do grafo.
- Controller aciona essas inferencias quando o usuario solicita (botao RunReasoner).

## Como executar
Pre-requisitos:
- Godot 4.6 com suporte a C#
- .NET SDK compativel com net8.0

Passos:
1. Abrir o projeto no Godot.
2. Executar a cena principal definida no projeto.

Opcional para validar compilacao C#:
- rodar: ```dotnet build```