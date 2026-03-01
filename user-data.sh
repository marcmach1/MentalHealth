#!/bin/bash
# User data script para AWS EC2 - Execute ao lançar a instância

set -e

echo "=========================================="
echo "MentalHealthSupport - EC2 Setup Script"
echo "=========================================="

# Atualizar sistema
echo "Atualizando pacotes..."
apt-get update
apt-get upgrade -y

# Instalar Docker
echo "Instalando Docker..."
apt-get install -y \
    apt-transport-https \
    ca-certificates \
    curl \
    gnupg \
    lsb-release

curl -fsSL https://download.docker.com/linux/ubuntu/gpg | gpg --dearmor -o /usr/share/keyrings/docker-archive-keyring.gpg
echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/docker-archive-keyring.gpg] https://download.docker.com/linux/ubuntu $(lsb_release -cs) stable" | tee /etc/apt/sources.list.d/docker.list > /dev/null
apt-get update
apt-get install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin

# Iniciar Docker
systemctl start docker
systemctl enable docker

# Instalar Docker Compose
echo "Instalando Docker Compose..."
curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
chmod +x /usr/local/bin/docker-compose

# Instalar AWS CLI
echo "Instalando AWS CLI..."
apt-get install -y awscli

# Instalar Nginx
echo "Instalando Nginx..."
apt-get install -y nginx

# Criar diretório de aplicação
mkdir -p /opt/mentalhealthsupport
cd /opt/mentalhealthsupport

# Criar arquivo de configuração Docker Compose
cat > docker-compose.yml << 'EOF'
version: '3.8'

services:
  mentalhealthsupport:
    image: ${REGISTRY}/${REPOSITORY}:latest
    container_name: mentalhealthsupport
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:80
    restart: always
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost/health"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
EOF

# Criar configuração Nginx
cat > /etc/nginx/sites-available/mentalhealthsupport << 'EOF'
server {
    listen 80 default_server;
    listen [::]:80 default_server;

    server_name _;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_buffering off;
        proxy_request_buffering off;
    }
}
EOF

# Habilitar site Nginx
rm -f /etc/nginx/sites-enabled/default
ln -sf /etc/nginx/sites-available/mentalhealthsupport /etc/nginx/sites-enabled/

# Testar configuração Nginx
nginx -t

# Iniciar Nginx
systemctl start nginx
systemctl enable nginx

# Criar script de deploy
cat > /usr/local/bin/deploy-mentalhealthsupport << 'EOF'
#!/bin/bash
set -e

REGISTRY="${1:-}"
REPOSITORY="${2:-mentalhealthsupport}"
AWS_REGION="${3:-us-east-1}"

if [ -z "$REGISTRY" ]; then
    echo "Uso: deploy-mentalhealthsupport <aws-account-id>.dkr.ecr.<region>.amazonaws.com <repository> [region]"
    exit 1
fi

cd /opt/mentalhealthsupport

echo "Fazendo login no ECR..."
aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$REGISTRY"

echo "Obtendo imagem..."
docker pull "$REGISTRY/$REPOSITORY:latest"

echo "Parando containers antigos..."
docker-compose down || true

echo "Iniciando novos containers..."
REGISTRY="$REGISTRY" REPOSITORY="$REPOSITORY" docker-compose up -d

echo "Aguardando aplicação inicializar..."
sleep 10

echo "Verificando logs..."
docker-compose logs -f &
sleep 5

echo "Deploy concluído com sucesso!"
EOF

chmod +x /usr/local/bin/deploy-mentalhealthsupport

echo "=========================================="
echo "Setup concluído!"
echo "=========================================="
echo ""
echo "Para fazer deploy, execute:"
echo "deploy-mentalhealthsupport <account-id>.dkr.ecr.<region>.amazonaws.com mentalhealthsupport <region>"
echo ""
echo "Exemplo:"
echo "deploy-mentalhealthsupport 123456789012.dkr.ecr.us-east-1.amazonaws.com mentalhealthsupport us-east-1"
