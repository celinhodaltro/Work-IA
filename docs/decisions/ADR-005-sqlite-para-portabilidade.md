# ADR-005: SQLite para Portabilidade

## Contexto
O projeto deve ser portátil — o usuário deve conseguir rodar com `dotnet run` sem configurar bancos externos.

## Decisão
SQLite como banco de dados principal. Sem necessidade de servidor de banco. Docker opcional apenas para RabbitMQ/Redis.

## Consequências
Positivas: Portabilidade total, zero configuração, arquivo .db único.
Negativas: Limitações de concorrência, não adequado para alta escala.

## Status
Aceito
