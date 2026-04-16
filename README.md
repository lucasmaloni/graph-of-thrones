# Graph of Thrones

## Visao geral
Graph of Thrones e um projeto em Godot 4 com C# que representa as Casas de Westeros como um grafo interativo.
Cada casa e um no fisico na cena, e as relacoes politicas sao arestas com intensidade variavel.

## Status do projeto
Projeto em desenvolvimento ativo.

O sistema ja possui uma base funcional para:
- carregar casas e relacoes a partir de JSON
- desenhar conexoes entre casas com espessura e cor por intensidade
- aplicar simulacao fisica de atracao e repulsao entre nos
- exibir sigilos das casas dinamicamente

Ainda faltam partes importantes para chegar na visao final, incluindo refinamentos de gameplay, regras de simulacao semantica e camada de interacao mais rica.

## Objetivo final
O objetivo final e entregar um simulador visual de dinamicas politicas em Westeros, no qual:
- as Casas Grandes e vassalas sejam modeladas como uma rede viva
- lealdade, influencia, conflitos e aliancas evoluam ao longo do tempo
- eventos alterem o estado do grafo de forma rastreavel
- o usuario consiga observar e explorar mudancas de poder em tempo real

Em resumo: transformar a estrutura narrativa de Game of Thrones em um grafo dinamico, explicavel e interativo.

## Arquitetura atual (resumo)
- GraphController: monta o grafo, cria conexoes, desenha arestas e aplica forcas
- WesterosHouse: base das casas, configuracao fisica e carregamento de sigilos
- GreatHouse e LordlyHouse: especializacoes do dominio
- DataManager: leitura de dados em scripts/data/init.json e scripts/data/database.json

## Como executar
Pre-requisitos:
- Godot 4.6 com suporte a C#
- .NET SDK compativel com net8.0

Passos:
1. Abrir o projeto no Godot.
2. Executar a cena principal definida no projeto.

Opcional para validar compilacao C#:
- rodar: dotnet build

## Estrutura principal
- scenes: cenas da simulacao
- scripts: logica C# da simulacao
- scripts/data: dados iniciais e conexoes em JSON
- assets/icons: sigilos das casas

## Proximos passos
- completar cobertura de casas e consistencia entre cena e dados JSON
- introduzir sistema de eventos narrativos e mudancas de lealdade
- melhorar UX de exploracao do grafo (filtros, destaque, painel de detalhes)
- adicionar testes para carregamento de dados e regras de relacao
