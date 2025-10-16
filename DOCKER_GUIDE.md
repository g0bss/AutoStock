# 🐳 Guia Completo do Docker - AutoStock

## 📋 Pré-requisitos

### 1. Instalar Docker no Windows

1. **Baixar Docker Desktop:**
   - Acesse: https://www.docker.com/products/docker-desktop
   - Baixe a versão para Windows

2. **Instalar Docker Desktop:**
   - Execute o instalador baixado
   - Siga as instruções na tela
   - **IMPORTANTE:** Certifique-se de habilitar o WSL 2 (Windows Subsystem for Linux)

3. **Verificar Instalação:**
   - Abra o PowerShell ou CMD
   - Execute: `docker --version`
   - Execute: `docker-compose --version`

### 2. Verificar se o Docker está Rodando
- Certifique-se de que o Docker Desktop está aberto e rodando
- Você deve ver o ícone do Docker na bandeja do sistema

## 🚀 Como Executar o Projeto

### Passo 1: Navegar até a pasta do projeto
```bash
cd C:\Users\gabfe\code\projetoDS
```

### Passo 2: Executar com Docker Compose
```bash
docker-compose up --build
```

**O que esse comando faz:**
- `docker-compose up`: Inicia todos os serviços definidos no docker-compose.yml
- `--build`: Reconstrói as imagens se necessário

### Passo 3: Aguardar a Inicialização
O processo pode levar alguns minutos na primeira execução. Você verá logs similares a:
```
autostock_db     | ... database system is ready to accept connections
autostock_api    | ... Application started. Press Ctrl+C to shut down.
autostock_pgadmin| ... Application startup complete.
```

## 🌐 Acessando a Aplicação

### AutoStock (Aplicação Principal)
- **URL:** http://localhost:8080
- **Credenciais padrão:**
  - Usuário: `admin`
  - Senha: `Admin123!`

### Swagger (Documentação da API)
- **URL:** http://localhost:8080/swagger
- Documentação interativa da API REST

### pgAdmin (Gerenciador de Banco)
- **URL:** http://localhost:5050
- **Credenciais:**
  - Email: `admin@autostock.com`
  - Senha: `admin123`

#### Conectar ao Banco no pgAdmin:
1. Clique em "Add New Server"
2. **General Tab:**
   - Name: `AutoStock`
3. **Connection Tab:**
   - Host: `autostock_db`
   - Port: `5432`
   - Database: `AutoStock`
   - Username: `admin`
   - Password: `Admin123!`

## 📱 Funcionalidades Disponíveis

### Interface Web
- **Dashboard:** Visão geral do sistema
- **Veículos:** CRUD completo de veículos
- **Clientes:** Gerenciamento de clientes
- **Fabricantes:** Cadastro de marcas
- **Usuários:** Controle de acesso
- **Movimentações:** Histórico de transações

### API REST
- **Autenticação:** JWT Token
- **Endpoints:** Todos os CRUDs documentados no Swagger
- **Formato:** JSON
- **Segurança:** Headers de autorização necessários

## 🛠️ Comandos Úteis

### Para parar a aplicação:
```bash
docker-compose down
```

### Para ver logs em tempo real:
```bash
docker-compose logs -f
```

### Para ver apenas logs da API:
```bash
docker-compose logs -f autostock_api
```

### Para reconstruir apenas a API:
```bash
docker-compose build autostock_api
docker-compose up autostock_api
```

### Para limpar dados do banco (CUIDADO!):
```bash
docker-compose down -v
docker-compose up --build
```

### Para acessar o container da API:
```bash
docker exec -it autostock_api bash
```

## 🔧 Estrutura dos Containers

### autostock_db (PostgreSQL)
- **Porta:** 5432
- **Banco:** AutoStock
- **Volume:** Dados persistentes em `postgres_data`

### autostock_pgadmin (pgAdmin)
- **Porta:** 5050
- **Interface:** Web para gerenciar banco

### autostock_api (Aplicação .NET)
- **Porta:** 8080
- **Ambiente:** Development
- **Build:** A partir do Dockerfile na raiz

## 🐛 Solução de Problemas

### Problema: "Port already in use"
**Solução:**
```bash
# Verificar o que está usando a porta
netstat -ano | findstr :8080
netstat -ano | findstr :5432
netstat -ano | findstr :5050

# Parar processos se necessário
docker-compose down
```

### Problema: Warning sobre "version is obsolete"
**Solução:**
- ✅ **JÁ CORRIGIDO!** O atributo `version` foi removido do docker-compose.yml
- Este warning não afeta o funcionamento, mas foi eliminado para limpeza

### Problema: "service refers to undefined network"
**Solução:**
- ✅ **JÁ CORRIGIDO!** Todas as redes foram padronizadas para `autostock_network`
- Todos os serviços agora usam a mesma rede consistentemente

### Problema: "Database connection failed"
**Solução:**
1. Verificar se o container do banco está rodando:
   ```bash
   docker-compose ps
   ```
2. Aguardar mais tempo para o banco inicializar
3. Verificar logs do banco:
   ```bash
   docker-compose logs autostock_db
   ```

### Problema: Aplicação não abre no navegador
**Solução:**
1. Verificar se todos os containers estão "Up":
   ```bash
   docker-compose ps
   ```
2. Aguardar a aplicação carregar completamente
3. Tentar acessar: http://localhost:8080 (não HTTPS)

### Problema: "No space left on device"
**Solução:**
```bash
# Limpar imagens não utilizadas
docker system prune -a

# Limpar volumes órfãos
docker volume prune
```

## 📊 Status dos Containers

Para verificar se tudo está funcionando:
```bash
docker-compose ps
```

Saída esperada:
```
Name                 State    Ports
autostock_api        Up       0.0.0.0:8080->8080/tcp
autostock_db         Up       0.0.0.0:5432->5432/tcp
autostock_pgadmin    Up       0.0.0.0:5050->80/tcp
```

## 🎯 Primeiro Acesso

1. Execute: `docker-compose up --build`
2. Aguarde todos os containers subirem
3. Acesse: http://localhost:8080
4. Faça login com: admin / Admin123!
5. Explore as funcionalidades!

---

**✅ Projeto AutoStock pronto para uso!**

*Sistema completo de gestão de estoque automotivo desenvolvido em .NET com Angular, PostgreSQL e Docker.*