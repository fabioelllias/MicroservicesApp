# 📘 Roadmap de Melhorias para Microserviços .NET

## ✅ Visão Geral

Este projeto implementa uma arquitetura de microserviços em .NET 8 com:

- MassTransit + RabbitMQ para mensageria
- PostgreSQL com EF Core
- Docker Compose com WSL2
- Debug remoto via VSDBG

Agora, avançaremos com melhorias para produção.

---

## 🔐 Segurança

| Item                         | Descrição                                                                 |
|------------------------------|---------------------------------------------------------------------------|
| ✅ **JWT Auth**              | Autenticação com tokens, usando IdentityServer ou Auth0                   |
| ✅ **CORS**                  | Restringir origens confiáveis por ambiente                                |
| ✅ **Rate Limiting**         | Controle de chamadas com `AspNetCoreRateLimit` ou gateway                 |
| ✅ **Segredos seguros**      | Configuração via variáveis de ambiente ou Azure Key Vault                 |

---

## 📈 Observabilidade e Monitoramento

| Item                           | Descrição                                                            |
|--------------------------------|------------------------------------------------------------------------|
| ✅ **Logging estruturado**     | Serilog com sinks para console, arquivos e fallback para Elasticsearch |
| ✅ **Tracing distribuído**     | OpenTelemetry com exportação para **Jaeger** via OTLP                 |
| ✅ **Jaeger UI funcional**     | Visualização de spans HTTP com erros e latência                       |
| ⏳ **Métricas Prometheus**     | (Planejado) Exposição de métricas HTTP com `prometheus-net` + Grafana |
| ✅ **Health Checks**           | `AspNetCore.Diagnostics.HealthChecks` exposto via `/health`           |
| ⏳ **Logs correlacionados**    | (Planejado) Incluir `TraceId` e `SpanId` nos logs do Serilog          |

---

## ⚙️ Resiliência e Qualidade

| Item                            | Descrição                                                                 |
|----------------------------------|---------------------------------------------------------------------------|
| ✅ **Polly avançado**            | Retry com jitter, circuit breaker, timeout para chamadas externas         |
| ✅ **Dead-letter monitoring**    | Monitoramento ativo das filas `_error` do RabbitMQ                        |
| ⏳ **Transactional outbox**     | (Planejado) Garante consistência eventual entre DB e mensagens            |
| ✅ **Fallback handlers**        | Mensagens críticas podem ser enviadas para log alternativo ou quarentena  |

---

## 🧪 Testes e Garantia de Qualidade

| Item                          | Descrição                                                      |
|-------------------------------|----------------------------------------------------------------|
| ✅ **xUnit + Moq**            | Testes unitários de services, consumers e lógica de domínio    |
| ✅ **Testes de integração**   | Uso de `Testcontainers` para PostgreSQL e RabbitMQ             |
| ✅ **Cobertura de código**    | Ferramentas como Coverlet + ReportGenerator                    |
| ✅ **Validações automáticas** | FluentValidation e integrações nos endpoints                   |

---

## 🚀 Dev Experience e CI/CD

| Item                          | Descrição                                                        |
|-------------------------------|------------------------------------------------------------------|
| ✅ **Hot reload com watch**  | `dotnet watch run` com suporte a file polling                   |
| ✅ **VSDBG no Docker**        | Debug remoto ativado com `sourceFileMap` e attach               |
| ✅ **CI/CD**                  | GitHub Actions ou Azure Pipelines com build/test/deploy         |
| ✅ **Ambientes separados**    | `docker-compose.override.dev.yml` e `docker-compose.override.prod.yml`

---

## 📦 Infraestrutura e Deploy

| Item                           | Descrição                                                         |
|--------------------------------|-------------------------------------------------------------------|
| ✅ **Docker Compose Prod**     | Separação de ambiente com volumes e restrições                    |
| ⏳ **Kubernetes (futuro)**     | Manifestos com Helm, Ingress, HorizontalPodAutoscaler             |
| ⏳ **Config centralizada**     | Considerar uso de Dapr ou Azure App Configuration no futuro       |

---

## 📋 Prioridades

### 🔹 Fase 1 – Curto prazo
- [x] Logging com Serilog
- [x] Health checks + Swagger
- [x] Criação de usuários com JWT
- [x] Retry e fallback com Polly
- [x] Tracing com OpenTelemetry + Jaeger

### 🔹 Fase 2 – Médio prazo
- [ ] Correlacionar logs com TraceId/SpanId
- [ ] Métricas com Prometheus + Grafana
- [ ] Testes de integração com Testcontainers
- [ ] CI/CD com GitHub Actions

### 🔹 Fase 3 – Longo prazo
- [ ] Transactional Outbox
- [ ] Service Mesh ou API Gateway
- [ ] Kubernetes com autoscaling
- [ ] Monitoramento ativo de DLQs
