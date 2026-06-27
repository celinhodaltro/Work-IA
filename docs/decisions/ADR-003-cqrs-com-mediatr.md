# ADR-003: CQRS com MediatR

## Contexto
Separar operações de leitura e escrita para melhor organização e escalabilidade.

## Decisão
MediatR para implementar CQRS com Commands e Queries separados, mais pipeline behaviors para validação e logging.

## Consequências
Positivas: Separação clara de preocupações, pipeline extensível.
Negativas: Indireção adicional, mais arquivos.

## Status
Aceito
