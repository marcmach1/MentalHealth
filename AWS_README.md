# 🚀 MentalHealthSupport - AWS Deployment Guide

Seu projeto está configurado para ser publicado na AWS EC2 com Docker!

## 📋 Arquivos Criados

### 1. **QUICK_START_AWS.md** ⭐ COMECE AQUI
   - Guia rápido (5 passos)
   - Comandos prontos para copiar/colar
   - Architectural diagram
   - Estimativa de custos
   - **Tempo: ~30 minutos para primeira publicação**

### 2. **Docker Essentials**
   - `Dockerfile` - Containerização multi-stage (otimizada)
   - `.dockerignore` - Arquivos ignorados no build
   - `docker-compose.yml` - Ambiente local completo com Nginx
   - `nginx.conf` - Configuração de reverse proxy
   - **Uso: Testar localmente antes de fazer deploy**

### 3. **AWS Deployment Scripts**
   - `user-data.sh` - Script executado ao criar EC2 (configuração automática)
   - `deploy.sh` - Script interativo para deploy
   - **Uso: Automatizar infraestrutura e deployment**

### 4. **Configurações de Aplicação**
   - `appsettings.Production.json` - Variáveis de ambiente produção
   - **Nota: Chaves sensíveis não devem estar aqui. Use AWS Secrets Manager**

### 5. **Documentação Detalhada**
   - `AWS_DEPLOYMENT.md` - Guia completo com todos os passos
   - `SECURITY_BEST_PRACTICES.md` - Hardening e segurança
   - **Referência: Consulte quando tiver perguntas específicas**

## 🎯 Fluxo Recomendado

### Fase 1: Testar Localmente (10 minutos)
```bash
# Testar build Docker
docker build -t mentalhealthsupport:latest .

# Testar com docker-compose
docker-compose up

# Acesse: http://localhost
```

### Fase 2: Publicar na AWS (20-30 minutos)
```bash
# Siga o QUICK_START_AWS.md para:
# 1. Preparar ECR
# 2. Enviar imagem
# 3. Criar EC2
# 4. Deploy
```

### Fase 3: Certificar Segurança (15 minutos)
```bash
# Consulte SECURITY_BEST_PRACTICES.md para:
# - Configurar secrets
# - Setup SSL/TLS
# - Habilitar backups
```

## 📊 Arquitetura

```
Local Machine
    ↓ docker build
    ↓
ECR Registry (AWS)
    ↓ docker pull
    ↓
EC2 Instance
    ↓
Docker Container (ASP.NET Core)
    ↓
Nginx (Reverse Proxy)
    ↓
Internet (Port 80/443)
```

## ✅ Pré-requisitos Rápida Verificação

```bash
# AWS CLI
aws --version

# Docker
docker --version

# Conta AWS
aws sts get-caller-identity
```

## 🚨 Pontos Importantes

### ⚠️ ANTES de fazer deploy em PRODUÇÃO:

1. **Secrets**
   - ❌ NÃO armazene API keys em appsettings.json
   - ✅ USE AWS Secrets Manager

2. **Database**
   - ❌ NÃO use InMemoryRepository em produção
   - ✅ USE AWS RDS (PostgreSQL/MySQL)

3. **Segurança**
   - ✅ Habilite HTTPS/SSL
   - ✅ Configure Security Groups corretamente
   - ✅ Use Private Subnets para dados sensíveis

4. **High Availability**
   - ✅ Configure Auto Scaling Groups
   - ✅ Use Load Balancer
   - ✅ Multi-AZ deployment

## 💡 Dicas Úteis

### Obter Quick Info
```bash
# Account ID
aws sts get-caller-identity --query Account --output text

# EC2 Public IP
aws ec2 describe-instances --filters "Name=key-name,Values=mentalhealthsupport-key" --query 'Reservations[].Instances[].PublicIpAddress' --output text

# ECR Login (válido por 12 horas)
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin <account-id>.dkr.ecr.us-east-1.amazonaws.com
```

### SSH na EC2
```bash
ssh -i mentalhealthsupport-key.pem ubuntu@<EC2_IP>
```

### Ver Logs
```bash
# Na EC2
docker logs -f mentalhealthsupport
docker-compose logs -f
tail -f /var/log/nginx/access.log
```

## 📈 Próximas Etapas Recomendadas

1. **Banco de Dados** (Crítico para produção)
   - Migrar de InMemoryRepository para RDS
   - Configurar backups automatizados

2. **CI/CD** (Importante para produtividade)
   - GitHub Actions para deploy automático
   - AWS CodePipeline

3. **Monitoring** (Crítico para produção)
   - CloudWatch para logs
   - Alarmes para alertas
   - X-Ray para tracing

4. **Load Balancing** (Para escalabilidade)
   - AWS Application Load Balancer (ALB)
   - Auto Scaling Groups
   - Multi-AZ deployment

## 🆘 Suporte e Troubleshooting

### Docker não funciona localmente
```bash
# Reinicie Docker daemon
docker system prune -a
docker-compose down
docker-compose up
```

### Erro no Deploy na EC2
```bash
# SSH na instância e verifique logs
ssh -i mentalhealthsupport-key.pem ubuntu@<IP>
docker logs mentalhealthsupport
```

### ECR Login expirado
```bash
# Re-faça login (válido por 12 horas)
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com
```

## 📚 Recursos Úteis

### Documentação Oficial
- [AWS EC2 Documentation](https://docs.aws.amazon.com/ec2/)
- [Docker Documentation](https://docs.docker.com/)
- [ASP.NET Core on Linux](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx)
- [AWS Secrets Manager](https://docs.aws.amazon.com/secretsmanager/)

### Ferramentas Úteis
- [AWS CLI](https://docs.aws.amazon.com/cli/) - Controle AWS via terminal
- [Docker Desktop](https://www.docker.com/products/docker-desktop) - Teste local
- [AWS Management Console](https://console.aws.amazon.com) - Interface gráfica
- [PuTTY](https://www.putty.org/) - SSH alternativo (Windows)

## 💰 Estimativa de Custos (por mês)

| Recurso | Tipo | Custo |
|---------|------|-------|
| EC2 | t3.medium 24/7 | $28-35 |
| ECR | Armazenamento imagem | $0.10/GB |
| Data Out | Internet saída | $0.09/GB |
| **Total** | | **~$30-40** |

*Preços em USD, região us-east-1. Verifique com AWS Pricing Calculator*

## 🔐 Segurança - Checklist Rápido

- [ ] AWS Secrets Manager configurado
- [ ] EC2 Security Group restritivo
- [ ] SSH chave criada (não senhas)
- [ ] HTTPS/SSL habilitado
- [ ] Backups configurados
- [ ] CloudWatch habilitado
- [ ] Atualizações automáticas ativas

---

## 🚀 Começar Agora!

**👉 Leia primeiro: [QUICK_START_AWS.md](QUICK_START_AWS.md)**

Qualquer dúvida, consulte os arquivos de documentação específicos:
- Detalhes técnicos: `AWS_DEPLOYMENT.md`
- Segurança: `SECURITY_BEST_PRACTICES.md`
- Local testing: Use `docker-compose.yml`

**Sucesso no deployment! 🎉**

---

*Última atualização: 28 de Fevereiro de 2026*
*Versão do projeto: ASP.NET Core 9.0*
*Plataforma: AWS EC2 com Docker*
