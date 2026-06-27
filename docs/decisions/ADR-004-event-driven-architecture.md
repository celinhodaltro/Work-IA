# ADR-004: Arquitetura Orientada a Eventos

## Contexto
Agentes precisam reagir a eventos do workspace em tempo real.

## Decisão
Sistema de eventos com Event Bus (RabbitMQ para produção, InMemory para desenvolvimento), eventos de domínio puros (sem dependência de MediatR), e Event Store para auditoria.

## Consequências
Positivas: Desacoplamento total entre produtores e consumidores, escalabilidade.
Negativas: Complexidade de consistência eventual.

## Status
Aceito
