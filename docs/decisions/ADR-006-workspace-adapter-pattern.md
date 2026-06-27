# ADR-006: Workspace Adapter Pattern

## Contexto
Suportar múltiplos ecossistemas (OpenCode, Claude Code, Cursor) sem dependência direta.

## Decisão
Interface IWorkspaceAdapter com implementações específicas para cada plataforma. OpenCode como adaptador primário, Claude Code como secundário.

## Consequências
Positivas: Desacoplamento total de plataformas, extensível via plugins.
Negativas: Complexidade de manutenção de múltiplos adaptadores.

## Status
Aceito
