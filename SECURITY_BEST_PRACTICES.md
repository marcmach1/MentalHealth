# Segurança e Best Practices - AWS EC2 Deployment

## 🔐 Segurança

### 1. Gerenciamento de Secrets

**❌ NÃO FAZER:**
```json
{
  "MentalHealthAi": {
    "ApiKey": "sk-1234567890"
  }
}
```

**✅ FAZER: Usar AWS Secrets Manager**

```csharp
// Program.cs
using Amazon.SecretsManager;
using System.Text.Json;

if (!app.Environment.IsDevelopment())
{
    var secretClient = new AmazonSecretsManagerClient(Amazon.RegionEndpoint.USEast1);
    
    try
    {
        var request = new GetSecretValueRequest 
        { 
            SecretId = "mentalhealthsupport/openai-api-key" 
        };
        
        var response = await secretClient.GetSecretValueAsync(request);
        var secret = JsonDocument.Parse(response.SecretString);
        var apiKey = secret.RootElement.GetProperty("ApiKey").GetString();
        
        // Use apiKey na configuração
        builder.Configuration["MentalHealthAi:ApiKey"] = apiKey;
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Erro ao recuperar secrets");
    }
}
```

### 2. Network Security

```bash
# Security Group - Restritivo
aws ec2 authorize-security-group-ingress \
  --group-name mentalhealthsupport-sg \
  --protocol tcp --port 22 \
  --cidr YOUR_SPECIFIC_IP/32 \
  --region us-east-1

# Database Security Group (se usar RDS)
aws ec2 create-security-group \
  --group-name mentalhealthsupport-db-sg \
  --description "SG para banco de dados" \
  --vpc-id vpc-xxxxxxxx \
  --region us-east-1

aws ec2 authorize-security-group-ingress \
  --group-id sg-db-xxxxxxxx \
  --protocol tcp --port 5432 \
  --source-security-group-id sg-ec2-xxxxxxxx \
  --region us-east-1
```

### 3. VPC e Subnets (Recomendado)

```bash
# Criar VPC privada
aws ec2 create-vpc --cidr-block 10.0.0.0/16 --region us-east-1

# Criar subnets publica e privada
aws ec2 create-subnet \
  --vpc-id vpc-xxxxxxxx \
  --cidr-block 10.0.1.0/24 \
  --availability-zone us-east-1a

# Lançar EC2 em subnet pública, RDS em subnet privada
```

### 4. IAM Roles e Policies

```bash
# Criar role para EC2 acessar ECR e Secrets Manager
aws iam create-role --role-name MentalHealthSupportEC2Role \
  --assume-role-policy-document '{
    "Version": "2012-10-17",
    "Statement": [{
      "Effect": "Allow",
      "Principal": {"Service": "ec2.amazonaws.com"},
      "Action": "sts:AssumeRole"
    }]
  }'

# Attachar policies
aws iam attach-role-policy \
  --role-name MentalHealthSupportEC2Role \
  --policy-arn arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly

aws iam attach-role-policy \
  --role-name MentalHealthSupportEC2Role \
  --policy-arn arn:aws:iam::aws:policy/SecretsManagerReadWrite

# Criar instance profile
aws iam create-instance-profile --instance-profile-name MentalHealthSupportEC2Profile
aws iam add-role-to-instance-profile \
  --instance-profile-name MentalHealthSupportEC2Profile \
  --role-name MentalHealthSupportEC2Role

# Usar ao lançar instância:
# --iam-instance-profile Name=MentalHealthSupportEC2Profile
```

### 5. Certificados SSL/TLS

```bash
# Opção 1: AWS Certificate Manager (ACM) - Recomendado
aws acm request-certificate \
  --domain-name seu-dominio.com \
  --subject-alternative-names www.seu-dominio.com \
  --validation-method DNS \
  --region us-east-1

# Opção 2: Let's Encrypt (na EC2)
apt-get install -y certbot python3-certbot-nginx

certbot certonly --standalone \
  -d seu-dominio.com \
  -d www.seu-dominio.com \
  --non-interactive \
  --agree-tos \
  -m seu-email@example.com

# Auto-renovação
systemctl enable certbot.timer
systemctl start certbot.timer
```

### 6. HTTPS no Nginx

```nginx
server {
    listen 443 ssl http2;
    server_name seu-dominio.com;

    ssl_certificate /etc/letsencrypt/live/seu-dominio.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/seu-dominio.com/privkey.pem;

    # Segurança SSL
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers 'ECDHE-ECDSA-AES128-GCM-SHA256:ECDHE-RSA-AES128-GCM-SHA256:ECDHE-ECDSA-AES256-GCM-SHA384';
    ssl_prefer_server_ciphers on;
    ssl_session_cache shared:SSL:10m;
    ssl_session_timeout 10m;
    ssl_session_tickets off;
    ssl_stapling on;
    ssl_stapling_verify on;

    # HSTS
    add_header Strict-Transport-Security "max-age=31536000; includeSubDomains" always;

    location / {
        proxy_pass http://mentalhealthsupport;
        # ... resto da configuração
    }
}

# Redirecionar HTTP para HTTPS
server {
    listen 80;
    server_name seu-dominio.com;
    return 301 https://$server_name$request_uri;
}
```

## 🛡️ Hardening da EC2

### 1. Atualizações de Segurança

```bash
# Em /etc/apt/apt.conf.d/50unattended-upgrades
APT::Periodic::Update-Package-Lists "1";
APT::Periodic::Download-Upgradeable-Packages "1";
APT::Periodic::AutocleanInterval "7";
APT::Periodic::Unattended-Upgrade "1";
APT::Periodic::Reboot "1";
APT::Periodic::RebootTime "02:00";

# Habilitar
apt-get install -y unattended-upgrades
systemctl enable unattended-upgrades
```

### 2. Firewall (UFW)

```bash
# Habilitar firewall
ufw enable

# Rules padrão
ufw default deny incoming
ufw default allow outgoing

# Liberar portas específicas
ufw allow 22/tcp
ufw allow 80/tcp
ufw allow 443/tcp

# Status
ufw status numbered
```

### 3. Fail2Ban (Proteção contra Brute Force)

```bash
# Instalar
apt-get install -y fail2ban

# Configurar
cp /etc/fail2ban/jail.conf /etc/fail2ban/jail.local

# /etc/fail2ban/jail.local
[DEFAULT]
maxretry = 5
findtime = 10m
bantime = 1h

[sshd]
enabled = true

[nginx-http-auth]
enabled = true

[nginx-limit-req]
enabled = true

# Iniciar
systemctl enable fail2ban
systemctl start fail2ban
```

### 4. Monitoramento com CloudWatch

```bash
# Instalar CloudWatch Agent
wget https://s3.amazonaws.com/amazoncloudwatch-agent/ubuntu/amd64/latest/amazon-cloudwatch-agent.deb
dpkg -i -E ./amazon-cloudwatch-agent.deb

# Configurar (arquivo JSON)
cat > /opt/aws/amazon-cloudwatch-agent/etc/config.json << 'EOF'
{
  "metrics": {
    "namespace": "MentalHealthSupport",
    "metrics_collected": {
      "cpu": {
        "measurement": [
          {
            "name": "cpu_usage_idle",
            "rename": "CPU_IDLE",
            "unit": "Percent"
          }
        ],
        "metrics_collection_interval": 60
      },
      "disk": {
        "measurement": [
          {
            "name": "used_percent",
            "rename": "DISK_USED",
            "unit": "Percent"
          }
        ],
        "metrics_collection_interval": 60,
        "resources": [
          "/"
        ]
      },
      "mem": {
        "measurement": [
          {
            "name": "mem_used_percent",
            "rename": "MEM_USED",
            "unit": "Percent"
          }
        ],
        "metrics_collection_interval": 60
      }
    }
  },
  "logs": {
    "logs_collected": {
      "files": {
        "collect_list": [
          {
            "file_path": "/var/log/nginx/access.log",
            "log_group_name": "/aws/ec2/mentalhealthsupport/nginx",
            "log_stream_name": "access"
          },
          {
            "file_path": "/var/log/nginx/error.log",
            "log_group_name": "/aws/ec2/mentalhealthsupport/nginx",
            "log_stream_name": "error"
          }
        ]
      }
    }
  }
}
EOF

# Iniciar agent
/opt/aws/amazon-cloudwatch-agent/bin/amazon-cloudwatch-agent-ctl \
  -a query -m ec2 -c file:/opt/aws/amazon-cloudwatch-agent/etc/config.json -s
```

## 📊 Monitoramento e Logging

### 1. Docker Logging

```bash
# Ver logs detalhados
docker logs -f --tail 100 mentalhealthsupport

# Limpar logs antigos
docker container prune -f

# Configure log rotation em docker-compose.yml
services:
  mentalhealthsupport:
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"
```

### 2. Application Insights (.NET)

```bash
# Instalar NuGet package
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry();
```

### 3. Alertas CloudWatch

```bash
# Criar alarme para CPU alta
aws cloudwatch put-metric-alarm \
  --alarm-name MentalHealthSupport-HighCPU \
  --alarm-description "Alerta quando CPU excede 80%" \
  --metric-name CPUUtilization \
  --namespace AWS/EC2 \
  --statistic Average \
  --period 300 \
  --threshold 80 \
  --comparison-operator GreaterThanThreshold \
  --evaluation-periods 2 \
  --alarm-actions arn:aws:sns:us-east-1:ACCOUNT:mentalhealthsupport-alerts
```

## 🔄 Backups e Disaster Recovery

### 1. EBS Snapshots (Automático)

```bash
# Criar snapshot rule
aws dlm create-lifecycle-policy \
  --execution-role-arn arn:aws:iam::ACCOUNT:role/service-role/AWSDataLifecycleManagerDefaultRole \
  --description "Daily snapshots of MentalHealthSupport volumes" \
  --state ENABLED \
  --policy-details file://policy.json
```

### 2. Database Backups (RDS)

```bash
# Criar backup manual
aws rds create-db-snapshot \
  --db-instance-identifier mentalhealthsupport-db \
  --db-snapshot-identifier mentalhealthsupport-backup-$(date +%Y%m%d)

# Enable automated backups (7 dias retenção)
aws rds modify-db-instance \
  --db-instance-identifier mentalhealthsupport-db \
  --backup-retention-period 7 \
  --apply-immediately
```

## 🚀 Otimização de Performance

### 1. Caching

```bash
# Nginx caching
location ~* \.(jpg|jpeg|png|gif|ico|css|js)$ {
    expires 365d;
    add_header Cache-Control "public, immutable";
}

# API caching
location /api/ {
    proxy_cache_valid 200 10m;
    proxy_cache_key "$scheme$request_method$host$request_uri";
    add_header X-Cache-Status $upstream_cache_status;
}
```

### 2. Compressão

```bash
# Já configurado no nginx.conf
gzip on;
gzip_comp_level 6;
gzip_types text/plain text/css application/json application/javascript;
```

### 3. Connection Pooling

```csharp
// Services/Repository
var connectionString = "Server=db;Database=mentalhealthsupport;Max Pool Size=20;";
```

## 📝 Checklist de Segurança

- [ ] AWS Secrets Manager configurado
- [ ] Security Groups restritivos
- [ ] VPC com subnets públicas/privadas
- [ ] IAM Roles com permissões mínimas
- [ ] SSL/TLS habilitado (HTTPS)
- [ ] Certificados válidos (ACM ou Let's Encrypt)
- [ ] Firewall (UFW) habilitado
- [ ] Fail2Ban instalado e configurado
- [ ] CloudWatch Agent instalado
- [ ] Alarmes CloudWatch configurados
- [ ] Backups automatizados habilitados
- [ ] Logs centralizados (CloudWatch/S3)
- [ ] Atualizações automáticas habilitadas
- [ ] SSH com key pair (sem senha)
- [ ] Testes de segurança realizados (OWASP ZAP)

## 🔗 Recursos Adicionais

- [AWS Security Best Practices](https://docs.aws.amazon.com/security/)
- [OWASP ASP.NET Core Security](https://cheatsheetseries.owasp.org/cheatsheets/DotNet_Security_Cheat_Sheet.html)
- [Docker Security](https://docs.docker.com/engine/security/)
- [Nginx Security](https://nginx.org/en/docs/http/ngx_http_ssl_module.html)
