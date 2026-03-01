# Quick Start - Deploy AWS EC2

## ⚡ 5 Passos Rápidos

### Passo 1: Preparar Localmente
```bash
# Fazer login na AWS
aws configure

# Testar Docker
docker build -t mentalhealthsupport:latest .
docker run -p 8080:80 mentalhealthsupport:latest
# Acesse http://localhost:8080
```

### Passo 2: Preparar Repositório ECR
```bash
# Obter seu Account ID
ACCOUNT_ID=$(aws sts get-caller-identity --query Account --output text)
echo $ACCOUNT_ID

# Criar repositório
aws ecr create-repository --repository-name mentalhealthsupport --region us-east-1

# Login no ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com
```

### Passo 3: Enviar Imagem para ECR
```bash
# Tag
docker tag mentalhealthsupport:latest $ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/mentalhealthsupport:latest

# Push
docker push $ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/mentalhealthsupport:latest
```

### Passo 4: Criar Instância EC2
```bash
# Criar chave SSH
aws ec2 create-key-pair --key-name mentalhealthsupport-key --region us-east-1 --query 'KeyMaterial' --output text > mentalhealthsupport-key.pem
chmod 400 mentalhealthsupport-key.pem

# Criar security group
aws ec2 create-security-group \
  --group-name mentalhealthsupport-sg \
  --description "Security group para MentalHealthSupport" \
  --region us-east-1

# Liberar portas
aws ec2 authorize-security-group-ingress --group-name mentalhealthsupport-sg --protocol tcp --port 80 --cidr 0.0.0.0/0 --region us-east-1
aws ec2 authorize-security-group-ingress --group-name mentalhealthsupport-sg --protocol tcp --port 443 --cidr 0.0.0.0/0 --region us-east-1
aws ec2 authorize-security-group-ingress --group-name mentalhealthsupport-sg --protocol tcp --port 22 --cidr YOUR_IP/32 --region us-east-1
```

### Passo 5: Lançar Instância com User Data
```bash
aws ec2 run-instances \
  --image-id ami-0c55b159cbfafe1f0 \
  --instance-type t3.medium \
  --key-name mentalhealthsupport-key \
  --security-groups mentalhealthsupport-sg \
  --user-data file://user-data.sh \
  --region us-east-1
```

## 🎯 Quando a Instância Estiver Pronta

```bash
# Obter IP público
aws ec2 describe-instances \
  --filters "Name=key-name,Values=mentalhealthsupport-key" \
  --region us-east-1 \
  --query 'Reservations[].Instances[].PublicIpAddress' \
  --output text

# SSH na instância (aguarde 2-3 minutos para user-data completar)
ssh -i mentalhealthsupport-key.pem ubuntu@<EC2_PUBLIC_IP>

# Na instância, fazer deploy
deploy-mentalhealthsupport $ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com mentalhealthsupport us-east-1
```

## ✅ Checklist

- [ ] AWS CLI instalado e configurado
- [ ] Docker instalado localmente
- [ ] Build local testado
- [ ] ECR repositório criado
- [ ] Imagem enviada para ECR
- [ ] EC2 Key Pair criado
- [ ] Security Group configurado
- [ ] EC2 instância lançada
- [ ] Aplicação acessível em http://<EC2_IP>

## 📊 Arquitetura

```
┌──────────────────┐
│   Local Machine  │
│                  │
│ ┌──────────────┐ │
│ │ Docker Build │ │
│ └──────┬───────┘ │
│        │         │
└────────┼─────────┘
         │
         v
    ┌─────────────────────────────────┐
    │   AWS (us-east-1)              │
    │                                 │
    │  ┌──────────────────────────┐   │
    │  │   ECR Repository         │   │
    │  │   mentalhealthsupport    │   │
    │  └───────────┬──────────────┘   │
    │              │                   │
    │              v                   │
    │  ┌──────────────────────────┐   │
    │  │   EC2 Instance           │   │
    │  │                          │   │
    │  │  ┌─────────────────────┐ │   │
    │  │  │   Docker Container  │ │   │
    │  │  │   (ASP.NET Core)    │ │   │
    │  │  └─────┬───────────────┘ │   │
    │  │        │                 │   │
    │  │  ┌─────┴───────────────┐ │   │
    │  │  │   Nginx             │ │   │
    │  │  │   (Reverse Proxy)   │ │   │
    │  │  └─────┬───────────────┘ │   │
    │  └────────┼────────────────┘   │
    │           │                     │
    │    ┌──────┴──────────┐         │
    │    │ Port 80 / 443   │         │
    │    │ (Public Internet)│        │
    └────┼──────┬──────────┘─────────┘
         │      │
         v      v
      HTTP/HTTPS Traffic
```

## 💰 Custos Estimados (por mês)

- **EC2 t3.medium**: ~$28-35 (24/7)
- **ECR**: ~$0.10 por GB armazenado
- **Data Transfer**: ~$0.09 por GB (após free tier)
- **Total**: ~$30-40 (estimado)

## 🔧 Variáveis de Ambiente Importantes

Na instância EC2, você pode definir:

```bash
export ASPNETCORE_ENVIRONMENT=Production
export ASPNETCORE_URLS=http://+:80
export OPENAI_API_KEY=your_key_here  # Não recomendado em produção
```

**Melhor prática**: Use AWS Secrets Manager

```bash
# Criar secret
aws secretsmanager create-secret \
  --name mentalhealthsupport/openai-api-key \
  --secret-string '{"ApiKey":"sua_chave_aqui"}' \
  --region us-east-1

# No código C#, recuperar:
# var client = new SecretsManagerClient();
# var response = await client.GetSecretValueAsync(...);
```

## 🚨 Troubleshooting Comum

### Container não inicia
```bash
ssh -i mentalhealthsupport-key.pem ubuntu@<IP>
docker logs mentalhealthsupport
docker ps
```

### Nginx está em erro
```bash
nginx -t
sudo systemctl restart nginx
curl http://localhost:5000
```

### ECR login expirou
```bash
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin $ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com
```

### Verificar logs da aplicação
```bash
docker-compose logs -f mentalhealthsupport
```

## 📚 Próximos Passos

1. **Banco de Dados**
   - Substituir `InMemoryMentalHealthRepository` por RDS (PostgreSQL/MySQL)
   - [Ver documentação](https://docs.aws.amazon.com/rds/)

2. **SSL/TLS**
   - Configurar HTTPS com Let's Encrypt
   - ACM (AWS Certificate Manager)

3. **Monitoramento**
   - CloudWatch para logs
   - X-Ray para tracing
   - SNS para alertas

4. **Escalabilidade**
   - Auto Scaling Groups
   - Elastic Load Balancer (ELB)
   - RDS Multi-AZ

5. **CI/CD**
   - GitHub Actions
   - AWS CodePipeline
   - Automated deployments

## 🆘 Suporte

- [AWS EC2 Documentation](https://docs.aws.amazon.com/ec2/)
- [Docker Documentation](https://docs.docker.com/)
- [ASP.NET Core Deployment](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/)
- [AWS CLI Reference](https://docs.aws.amazon.com/cli/)
