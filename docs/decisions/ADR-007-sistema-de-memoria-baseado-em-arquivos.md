# ADR-007: Sistema de Memória Baseado em Arquivos

## Contexto
Agentes precisam persistir e recuperar conhecimento entre sessões.

## Decisão
Sistema de memória híbrido: SQLite para metadados e busca, sistema de arquivos (`.agent-memory/`) para conteúdo completo. Compatível com formato do OpenCode.

## Consequências
Positivas: Compatibilidade com ecossistema OpenCode, backup simples, portabilidade.
Negativas: Performance inferior a banco dedicado para buscas complexas.

## Status
Aceito
