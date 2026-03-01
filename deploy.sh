#!/bin/bash

# Script de Deploy - MentalHealthSupport para AWS EC2

set -e

# Cores para output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m'

# Configurações
AWS_REGION="us-east-1"
REPOSITORY_NAME="mentalhealthsupport"
IMAGE_NAME="$REPOSITORY_NAME:latest"
ACCOUNT_ID=""
EC2_KEY_PATH=""
EC2_IP=""

echo -e "${YELLOW}=== MentalHealthSupport - AWS Deployment Script ===${NC}"

# Função para apresentar menu
show_menu() {
    echo -e "\n${YELLOW}Escolha uma opção:${NC}"
    echo "1. Build Docker localmente"
    echo "2. Push para AWS ECR"
    echo "3. Criar/Atualizar repositório ECR"
    echo "4. Deploy em EC2 existente"
    echo "5. Executar tudo (1-4)"
    echo "6. Sair"
    read -p "Opção: " option
}

# Função para validar entrada
validate_aws_account() {
    if [ -z "$ACCOUNT_ID" ]; then
        read -p "Digite seu AWS Account ID: " ACCOUNT_ID
    fi
}

# 1. Build Docker
build_docker() {
    echo -e "\n${YELLOW}[1] Iniciando build Docker...${NC}"
    
    if docker build -t "$IMAGE_NAME" . ; then
        echo -e "${GREEN}✓ Build concluído com sucesso!${NC}"
        return 0
    else
        echo -e "${RED}✗ Erro no build Docker${NC}"
        return 1
    fi
}

# 2. Push para ECR
push_to_ecr() {
    validate_aws_account
    
    echo -e "\n${YELLOW}[2] Fazendo push para AWS ECR...${NC}"
    
    ECR_REPO="$ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME"
    
    # Login ECR
    echo "Login no ECR..."
    if aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com" ; then
        echo -e "${GREEN}✓ Login ECR bem-sucedido${NC}"
    else
        echo -e "${RED}✗ Erro no login ECR${NC}"
        return 1
    fi
    
    # Tag image
    echo "Tagging image..."
    docker tag "$IMAGE_NAME" "$ECR_REPO:latest"
    docker tag "$IMAGE_NAME" "$ECR_REPO:$(date +%Y%m%d_%H%M%S)"
    
    # Push
    echo "Push para ECR..."
    if docker push "$ECR_REPO" ; then
        echo -e "${GREEN}✓ Push concluído: $ECR_REPO${NC}"
        return 0
    else
        echo -e "${RED}✗ Erro no push${NC}"
        return 1
    fi
}

# 3. Criar repositório ECR
create_ecr_repo() {
    validate_aws_account
    
    echo -e "\n${YELLOW}[3] Criando/Verificando repositório ECR...${NC}"
    
    if aws ecr describe-repositories \
        --repository-names "$REPOSITORY_NAME" \
        --region "$AWS_REGION" &>/dev/null ; then
        echo -e "${GREEN}✓ Repositório já existe${NC}"
    else
        echo "Criando novo repositório..."
        if aws ecr create-repository \
            --repository-name "$REPOSITORY_NAME" \
            --region "$AWS_REGION" ; then
            echo -e "${GREEN}✓ Repositório criado com sucesso${NC}"
        else
            echo -e "${RED}✗ Erro ao criar repositório${NC}"
            return 1
        fi
    fi
}

# 4. Deploy em EC2
deploy_to_ec2() {
    validate_aws_account
    
    echo -e "\n${YELLOW}[4] Deploy para EC2...${NC}"
    
    read -p "Digite o IP público da instância EC2: " EC2_IP
    read -p "Digite o caminho da chave SSH (ex: ./mentalhealthsupport-key.pem): " EC2_KEY_PATH
    
    if [ ! -f "$EC2_KEY_PATH" ]; then
        echo -e "${RED}✗ Arquivo de chave SSH não encontrado: $EC2_KEY_PATH${NC}"
        return 1
    fi
    
    # Criar script de deploy remoto
    echo "Preparando script de deploy remoto..."
    
    cat > /tmp/deploy-ec2.sh << 'EOF'
#!/bin/bash
set -e

AWS_REGION="$1"
ACCOUNT_ID="$2"
REPOSITORY_NAME="$3"

echo "Atualizando container..."
aws ecr get-login-password --region "$AWS_REGION" | docker login --username AWS --password-stdin "$ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"

docker pull "$ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:latest"

echo "Parando container antigo..."
docker stop mentalhealthsupport || true
docker rm mentalhealthsupport || true

echo "Iniciando novo container..."
docker run -d \
  --name mentalhealthsupport \
  -p 80:80 \
  -p 443:443 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:80 \
  "$ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com/$REPOSITORY_NAME:latest"

echo "Deploy concluído!"
docker logs -f mentalhealthsupport
EOF
    
    # Fazer upload e executar
    echo "Enviando e executando script na EC2..."
    
    scp -i "$EC2_KEY_PATH" -o StrictHostKeyChecking=no /tmp/deploy-ec2.sh "ec2-user@$EC2_IP:/tmp/"
    
    ssh -i "$EC2_KEY_PATH" -o StrictHostKeyChecking=no "ec2-user@$EC2_IP" \
        "chmod +x /tmp/deploy-ec2.sh && /tmp/deploy-ec2.sh $AWS_REGION $ACCOUNT_ID $REPOSITORY_NAME"
    
    echo -e "${GREEN}✓ Deploy concluído!${NC}"
    echo -e "Acesse: http://$EC2_IP"
}

# Loop principal
while true; do
    show_menu
    
    case $option in
        1)
            build_docker
            ;;
        2)
            push_to_ecr
            ;;
        3)
            create_ecr_repo
            ;;
        4)
            deploy_to_ec2
            ;;
        5)
            create_ecr_repo && build_docker && push_to_ecr
            echo -e "\n${YELLOW}Próximos passos:${NC}"
            echo "1. Crie uma instância EC2"
            echo "2. Execute: ./deploy.sh (opção 4)"
            ;;
        6)
            echo -e "${YELLOW}Saindo...${NC}"
            exit 0
            ;;
        *)
            echo -e "${RED}Opção inválida${NC}"
            ;;
    esac
done
