# Guia de Deploy - MentalHealthSupport no AWS EC2

## Pré-requisitos

- Conta AWS ativa
- AWS CLI instalado e configurado (`aws configure`)
- Docker instalado (local)
- Git configurado

## 1. Preparar a Imagem Docker

### 1.1 Build local para testar
```bash
docker build -t mentalhealthsupport:latest .
docker run -p 8080:80 mentalhealthsupport:latest
```

### 1.2 Fazer push para AWS ECR (Elastic Container Registry)

```bash
# Criar repositório ECR
aws ecr create-repository --repository-name mentalhealthsupport --region us-east-1

# Login no ECR
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com

# Tag da imagem
docker tag mentalhealthsupport:latest YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/mentalhealthsupport:latest

# Push para ECR
docker push YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/mentalhealthsupport:latest
```

## 2. Configurar SSH no AWS EC2

### 2.1 Criar ou usar Key Pair existente
```bash
# Para criar nova chave
aws ec2 create-key-pair --key-name mentalhealthsupport-key --region us-east-1 --query 'KeyMaterial' --output text > mentalhealthsupport-key.pem
chmod 400 mentalhealthsupport-key.pem
```

### 2.2 Criar Security Group
```bash
# Criar security group
aws ec2 create-security-group --group-name mentalhealthsupport-sg --description "SecurityGroup para MentalHealthSupport" --region us-east-1

# Liberar porta HTTP (80)
aws ec2 authorize-security-group-ingress --group-name mentalhealthsupport-sg --protocol tcp --port 80 --cidr 0.0.0.0/0 --region us-east-1

# Liberar porta HTTPS (443)
aws ec2 authorize-security-group-ingress --group-name mentalhealthsupport-sg --protocol tcp --port 443 --cidr 0.0.0.0/0 --region us-east-1

# Liberar SSH (22)
aws ec2 authorize-security-group-ingress --group-name mentalhealthsupport-sg --protocol tcp --port 22 --cidr YOUR_IP/32 --region us-east-1
```

## 3. Lançar uma Instância EC2

### 3.1 Comando para lançar EC2
```bash
aws ec2 run-instances \
  --image-id ami-0c55b159cbfafe1f0 \
  --instance-type t3.medium \
  --key-name mentalhealthsupport-key \
  --security-groups mentalhealthsupport-sg \
  --region us-east-1 \
  --user-data file://user-data.sh
```

### 3.2 Arquivo user-data.sh
Crie um arquivo `user-data.sh` na raiz do projeto:

```bash
#!/bin/bash
set -e

# Update system
apt-get update
apt-get upgrade -y

# Install Docker
apt-get install -y docker.io
systemctl start docker
systemctl enable docker

# Install Docker Compose
curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose

# Login no ECR e pull da imagem
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com
docker pull YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/mentalhealthsupport:latest

# Criar network Docker
docker network create mentalhealthsupport-network || true

# Parar container antigo se existir
docker stop mentalhealthsupport || true
docker rm mentalhealthsupport || true

# Executar novo container
docker run -d \
  --name mentalhealthsupport \
  --network mentalhealthsupport-network \
  -p 80:80 \
  -p 443:443 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:80 \
  YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com/mentalhealthsupport:latest

# Instalar Nginx como reverse proxy (opcional, recomendado)
apt-get install -y nginx

# Criar configuração Nginx
cat > /etc/nginx/sites-available/mentalhealthsupport << 'EOF'
server {
    listen 80;
    server_name _;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host \$host;
        proxy_cache_bypass \$http_upgrade;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
    }
}
EOF

# Habilitar site
ln -sf /etc/nginx/sites-available/mentalhealthsupport /etc/nginx/sites-enabled/
rm -f /etc/nginx/sites-enabled/default

# Testar e iniciar Nginx
nginx -t
systemctl start nginx
systemctl enable nginx
```

## 4. Gerenciar Secrets com AWS Secrets Manager

### 4.1 Criar secret para OpenAI API Key
```bash
aws secretsmanager create-secret \
  --name mentalhealthsupport/openai-key \
  --secret-string '{"api_key":"YOUR_OPENAI_KEY"}' \
  --region us-east-1
```

### 4.2 Modificar aplicação para ler secrets
Atualize o código C# to Program.cs:

```csharp
// Adicionar ao Program.cs
if (!app.Environment.IsDevelopment())
{
    var client = new Amazon.SecretsManager.AmazonSecretsManagerClient();
    var secret = await client.GetSecretValueAsync(new Amazon.SecretsManager.Model.GetSecretValueRequest 
    { 
        SecretId = "mentalhealthsupport/openai-key" 
    });
    var jsonSecret = JsonDocument.Parse(secret.SecretString);
    var apiKey = jsonSecret.RootElement.GetProperty("api_key").GetString();
    // Use apiKey na configuração
}
```

## 5. Conectar e Gerenciar a Instância

### 5.1 Obter IP da instância
```bash
aws ec2 describe-instances \
  --filters "Name=tag:Name,Values=mentalhealthsupport" \
  --region us-east-1 \
  --query 'Reservations[].Instances[].PublicIpAddress' \
  --output text
```

### 5.2 SSH na instância
```bash
ssh -i mentalhealthsupport-key.pem ec2-user@EC2_PUBLIC_IP
```

## 6. Configurar HTTPS com Let's Encrypt

Na instância EC2:

```bash
# Instalar Certbot
apt-get install -y certbot python3-certbot-nginx

# Obter certificado
certbot certonly --nginx -d seu-dominio.com

# Renovação automática
systemctl enable certbot.timer
systemctl start certbot.timer
```

## 7. Monitoramento

### 7.1 Logs do Docker
```bash
docker logs -f mentalhealthsupport
```

### 7.2 CloudWatch
Na console AWS, configure CloudWatch para monitorar:
- CPU utilization
- Network in/out
- Disk I/O
- Memory usage

## 8. Escalabilidade com Auto Scaling

Para crescimento automático:

```bash
# Criar AMI da instância atual
aws ec2 create-image --instance-id i-xxxxxxxxx --name mentalhealthsupport-ami

# Criar launch template
aws ec2 create-launch-template --launch-template-name mentalhealthsupport-template

# Criar Auto Scaling Group
aws autoscaling create-auto-scaling-group \
  --auto-scaling-group-name mentalhealthsupport-asg \
  --launch-template-name mentalhealthsupport-template
```

## Dicas de Produção

✅ Usar RDS para banco de dados (em vez de In-Memory)
✅ Configurar backups automáticos
✅ Usar CloudFront para CDN
✅ Implementar WAF (Web Application Firewall)
✅ Ativar VPC para isolamento de rede
✅ Usar ELB (Elastic Load Balancer) para distribuir tráfego
✅ Logs centralizados com CloudWatch
✅ Monitoramento com CloudWatch Alarms

## Variáveis de Ambiente Importantes

```bash
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
MentalHealthAi__ApiKey=YOUR_KEY_FROM_SECRETS_MANAGER
```

## Troubleshooting

**Problema:** Container não inicia
```bash
docker logs mentalhealthsupport
```

**Problema:** Porta 80 já está em uso
```bash
sudo lsof -i :80
sudo kill -9 PID
```

**Problema:** Erro de permissões ECR
```bash
aws ecr get-login-password --region us-east-1 | docker login --username AWS --password-stdin YOUR_ACCOUNT_ID.dkr.ecr.us-east-1.amazonaws.com
```
