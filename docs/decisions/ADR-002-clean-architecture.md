# ADR-002: Clean Architecture como Padrão Arquitetural

## Contexto
Definir a arquitetura de software para garantir separação de responsabilidades, testabilidade e manutenibilidade.

## Decisão
Clean Architecture com 4 camadas: Domain, Application, Infrastructure, Presentation. Domain puro sem dependências externas.

## Consequências
Positivas: Isolamento de regras de negócio, facilidade para testes, substituição de infrastructure.
Negativas: Maior número de projetos e arquivos, overhead inicial de configuração.

## Status
Aceito
