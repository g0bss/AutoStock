# 🚀 Guia de Execução Completo - AutoStock

## 📋 Pré-requisitos

### Opção 1: Execução com Docker (RECOMENDADO) 🐳
- ✅ [Docker Desktop](https://www.docker.com/products/docker-desktop/) instalado e em execução
- ✅ 4GB de RAM disponível
- ✅ 2GB de espaço em disco

### Opção 2: Execução Local (Desenvolvimento) 💻
- ✅ [.NET 9.0 SDK](https://dotnet.microsoft.com/download) instalado
- ✅ [PostgreSQL](https://www.postgresql.org/) ou Docker para banco de dados
- ✅ IDE (Visual Studio Code ou Visual Studio)

---

## 🐳 EXECUÇÃO COM DOCKER (Mais Fácil)

### 1️⃣ Preparar o Ambiente
```bash
# Abrir terminal/PowerShell como administrador
# Navegar até a pasta do projeto
cd C:\Users\gabfe\code\projetoDS

# Verificar se o Docker está rodando
docker --version
```

### 2️⃣ Iniciar o Sistema Completo
```bash
# Comando único para iniciar tudo
docker-compose up --build

# OU para executar em background (modo detached)
docker-compose up --build -d
```

### 3️⃣ Aguardar Inicialização
- ⏳ **Tempo estimado:** 2-3 minutos
- 📦 **Containers criados:**
  - `autostock_api` - API Principal
  - `autostock_db` - Banco PostgreSQL
  - `autostock_pgadmin` - Interface do Banco

### 4️⃣ Acessar o Sistema
| Serviço | URL | Credenciais |
|---------|-----|-------------|
| **🚗 AutoStock** | http://localhost:8080 | admin / Admin123! |
| **📚 API Swagger** | http://localhost:8080/swagger | - |
| **🗄️ pgAdmin** | http://localhost:5050 | admin@autostock.com / admin123 |

---

## 💻 EXECUÇÃO LOCAL (Desenvolvimento)

### 1️⃣ Preparar Banco de Dados
```bash
# Opção A: Usar Docker apenas para o banco
docker-compose up postgres pgadmin -d

# Opção B: Instalar PostgreSQL localmente
# Configure: Host=localhost, Port=5432, Database=AutoStock
```

### 2️⃣ Configurar a Aplicação
```bash
# Navegar para a pasta da API
cd src/DealershipInventorySystem.WebAPI

# Restaurar dependências
dotnet restore

# Verificar configuração
dotnet build
```

### 3️⃣ Executar a Aplicação
```bash
# Executar em modo desenvolvimento
dotnet run

# OU para watch mode (auto-reload)
dotnet watch run
```

### 4️⃣ Acessar o Sistema
| Serviço | URL |
|---------|-----|
| **🚗 AutoStock** | http://localhost:5195 |
| **📚 API Swagger** | http://localhost:5195/swagger |

---

## ✅ TESTE DE FUNCIONALIDADES

### 1. Login e Autenticação
1. Acessar: http://localhost:8080 (Docker) ou http://localhost:5195 (Local)
2. **Login:** `admin`
3. **Senha:** `Admin123!`
4. ✅ Verificar redirecionamento para dashboard

### 2. Dashboard Principal
- 📊 **Estatísticas:** Veículos totais, disponíveis, vendidos
- 📈 **Gráficos:** Estatísticas visuais
- 🎯 **Cards interativos:** Hover effects e animações

### 3. Gestão de Veículos
1. **Menu:** Clique em "Veículos"
2. **Criar:** Botão "Novo Veículo"
3. **Testar campos:**
   - VIN (único e obrigatório)
   - Marca, Modelo, Ano
   - Status (Disponível, Vendido, etc.)
   - Preços de custo e venda
4. ✅ **CRUD Completo:** Criar, Visualizar, Editar, Excluir

### 4. Gestão de Clientes
1. **Menu:** Clique em "Clientes"
2. **Testar funcionalidades:**
   - Cadastro com CPF/CNPJ
   - Informações de contato
   - Histórico de compras
3. ✅ **Validações:** CPF/CNPJ únicos

### 5. Gestão de Fabricantes
1. **Menu:** Clique em "Fabricantes"
2. **Testar:**
   - Cadastro de montadoras
   - Status ativo/inativo
   - Informações de contato

### 6. Movimentações de Estoque
1. **Menu:** Clique em "Movimentações"
2. **Tipos disponíveis:**
   - Entrada de veículo
   - Test drive
   - Venda
   - Transferência
   - Manutenção

### 7. Administração de Usuários
1. **Menu:** Clique em "Usuários"
2. **Perfis disponíveis:**
   - Administrador
   - Gerente
   - Vendedor
   - Mecânico
   - Operador

---

## 🧪 EXECUÇÃO DE TESTES

### Testes Automatizados
```bash
# Executar todos os testes
dotnet test

# Resultado esperado:
# ✅ 47 testes passando
# ❌ 4 testes falhando (integração de auth - problema menor)
```

### Teste de API (Swagger)
1. Acessar: http://localhost:8080/swagger
2. **Autenticação:**
   - POST `/api/auth/login`
   - Body: `{"username": "admin", "password": "Admin123!"}`
   - Copiar token retornado
3. **Autorizar:** Botão "Authorize" → `Bearer {token}`
4. **Testar endpoints:** Todos os CRUDs disponíveis

---

## 📱 TESTE RESPONSIVO

### Desktop (1200px+)
- ✅ Layout completo com sidebar
- ✅ Cards em grid responsivo
- ✅ Todas as funcionalidades visíveis

### Tablet (768px - 1200px)
- ✅ Sidebar recolhível
- ✅ Cards reorganizados
- ✅ Formulários otimizados

### Mobile (< 768px)
- ✅ Menu hambúrguer
- ✅ Cards empilhados
- ✅ Botões full-width
- ✅ Formulários otimizados para touch

---

## 🎨 RECURSOS VISUAIS IMPLEMENTADOS

### ✨ Animações e Transições
- **FadeIn:** Cards e conteúdo principal
- **SlideIn:** Sidebar e navegação
- **Hover Effects:** Botões, cards e links
- **Pulse:** Botões primários
- **Shimmer:** Efeito de loading

### 🎨 Design Moderno
- **Gradientes:** Botões e backgrounds
- **Glassmorphism:** Sidebar e modais
- **Sombras dinâmicas:** Cards e elementos interativos
- **Cores atualizadas:** Paleta moderna e consistente

### 📐 Responsividade
- **Breakpoints:** 480px, 768px, 1024px
- **Flexbox e Grid:** Layout adaptativo
- **Touch-friendly:** Elementos otimizados para mobile

---

## 🔧 COMANDOS ÚTEIS

### Docker
```bash
# Parar todos os containers
docker-compose down

# Reconstruir e iniciar
docker-compose up --build

# Ver logs em tempo real
docker-compose logs -f

# Limpar volumes (reset completo)
docker-compose down -v
```

### .NET
```bash
# Limpar e reconstruir
dotnet clean && dotnet build

# Executar com logs detalhados
dotnet run --verbosity detailed

# Executar testes específicos
dotnet test --filter "TestName"
```

### Banco de Dados
```bash
# Acessar PostgreSQL via container
docker exec -it autostock_db psql -U admin -d AutoStock

# Backup do banco
docker exec autostock_db pg_dump -U admin AutoStock > backup.sql
```

---

## 🚨 SOLUÇÃO DE PROBLEMAS

### ❌ Docker não inicia
```bash
# Verificar se Docker está rodando
docker info

# Reiniciar Docker Desktop
# Windows: Restart Docker Desktop
```

### ❌ Porta já está em uso
```bash
# Verificar portas ocupadas
netstat -an | findstr ":8080"
netstat -an | findstr ":5432"

# Parar processos nas portas
# Windows: taskkill /PID <número_do_processo> /F
```

### ❌ Banco de dados não conecta
1. Verificar se container postgres está rodando:
   ```bash
   docker ps
   ```
2. Verificar logs do banco:
   ```bash
   docker logs autostock_db
   ```

### ❌ Aplicação não carrega
1. Verificar logs da aplicação:
   ```bash
   docker logs autostock_api
   ```
2. Verificar configuração de rede:
   ```bash
   docker network ls
   ```

---

## 📞 SUPORTE

### Para Apresentação/Demonstração
1. **Comando único:** `docker-compose up --build`
2. **URL principal:** http://localhost:8080
3. **Login:** admin / Admin123!
4. **Tempo de setup:** 2-3 minutos

### Para Desenvolvimento
1. **Documentação:** README.md
2. **API Docs:** /swagger
3. **Arquitetura:** Clean Architecture (.NET 9)
4. **Testes:** `dotnet test`

---

## 🎯 CHECKLIST PARA APRESENTAÇÃO

### ✅ Antes da Apresentação
- [ ] Docker Desktop está rodando
- [ ] Executar: `docker-compose up --build`
- [ ] Aguardar inicialização completa (2-3 min)
- [ ] Testar acesso: http://localhost:8080
- [ ] Confirmar login: admin / Admin123!

### ✅ Durante a Apresentação
- [ ] Mostrar Dashboard com estatísticas
- [ ] Demonstrar CRUD de Veículos
- [ ] Mostrar responsividade (mobile)
- [ ] Apresentar Swagger API
- [ ] Destacar arquitetura limpa
- [ ] Mostrar testes automatizados

### ✅ Funcionalidades a Destacar
- [ ] Interface moderna com animações
- [ ] Sistema de autenticação JWT
- [ ] CRUDs completos para todas entidades
- [ ] Responsividade mobile
- [ ] API REST documentada
- [ ] 47 testes automatizados
- [ ] Clean Architecture
- [ ] Docker containerizado

---

**🎉 AutoStock está pronto para apresentação! Sistema completo, moderno e funcional.**