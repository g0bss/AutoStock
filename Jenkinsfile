// Jenkinsfile
pipeline {
    // 1. Agente: Onde o pipeline vai rodar?
    // 'agent any' diz ao Jenkins para rodar em qualquer agente disponível.
    // (Vamos precisar que este agente tenha o .NET 9 SDK, Docker e Docker Compose instalados)
    agent any
// --- ADICIONE ESTA SEÇÃO ---
    environment {
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = 'true'
    }
    // --- FIM DA NOVA SEÇÃO ---
    // 2. Stages: As etapas do nosso processo de CI/CD
    stages {
        
        // --- ESTÁGIO 1: CHECKOUT ---
        // Pega o código-fonte do seu repositório (ex: Git)
        stage('Checkout') {
            steps {
                echo 'Baixando o código-fonte...'
                // --- INÍCIO DA MUDANÇA ---
                // Isso vai 'tentar' rodar os testes. Se falhar,
                // vai marcar o estágio como UNSTABLE (amarelo)
                // em vez de FAILED (vermelho), e DEIXA O PIPELINE CONTINUAR.
                catchError(buildResult: 'UNSTABLE', stageResult: 'UNSTABLE') {
                    sh 'dotnet test'
                }
            }
        }

        // --- ESTÁGIO 2: BUILD (DevOps) ---
        // Compila o projeto .NET para garantir que não há erros de sintaxe
        // Usa os comandos do seu README.md
        stage('Build .NET') {
            steps {
                echo 'Restaurando dependências e compilando o projeto .NET...'
                // Precisamos do .NET 9 SDK instalado no agente Jenkins
                sh 'dotnet restore'
                sh 'dotnet build --no-restore'
            }
        }

        // --- ESTÁGIO 3: TEST (DevOps) ---
        // Roda a suíte de testes xUnit. Se um teste falhar, o pipeline para.
        stage('Run Tests') {
            steps {
                echo 'Executando os 47 testes automatizados...'
                sh 'dotnet test'
                // Você também pode arquivar os resultados dos testes
                // junit '**/TestResults/*.xml'
            }
        }

        // --- ESTÁGIO 4: SECURITY (DevSecOps) ---
        // Aqui começa a parte de DevSecOps. Vamos fazer duas verificações.
        stage('Security Scan (DevSecOps)') {
            parallel {
                // Verificação 1: Análise de Dependências (SCA)
                // Verifica se algum pacote NuGet que você usa tem vulnerabilidades conhecidas
                stage('Scan Dependencies') {
                    steps {
                        echo 'Verificando pacotes NuGet vulneráveis...'
                        // Comando nativo do .NET para checar vulnerabilidades
                        // '|| true' garante que o pipeline não pare se encontrar algo,
                        // mas ainda mostrará o aviso. Remova para parar o build.
                        sh 'dotnet list package --vulnerable || true'
                    }
                }
                
                // Verificação 2: Análise de Código Estático (SAST) - Simples
                // Uma verificação simples por "segredos" (senhas, chaves de API)
                // Para um trabalho, isso é uma ótima demonstração
                stage('Scan for Secrets') {
                    steps {
                        echo 'Verificando se há segredos "hard-coded" no código...'
                        // Instale 'trufflehog' no seu agente Jenkins: 'pip install trufflehog'
                        // Ele vai escanear o repositório por senhas
                        // '|| true' para não parar o pipeline
                        sh 'trufflehog filesystem . --fail || true'
                    }
                }
            }
        }

        // --- ESTÁGIO 5: PACKAGE (DevOps) ---
        // Constrói as imagens Docker conforme seu docker-compose.yml
        stage('Build Docker Images') {
            steps {
                echo 'Construindo as imagens Docker (App, Banco, etc)...'
                // Este é o comando do seu README.md!
                sh 'docker-compose build'
            }
        }
        
        // --- ESTÁGIO 6: DEPLOY (DevOps) ---
        // Para este trabalho, vamos "implantar" na própria máquina Jenkins
        // como uma simulação de um ambiente de Staging (Homologação)
        stage('Deploy (Simulated Staging)') {
            steps {
                echo 'Iniciando a aplicação AutoStock com Docker Compose...'
                // Sobe a aplicação em modo 'detached' (-d)
                sh 'docker-compose up -d'
                
                echo 'Aguardando 30 segundos para a aplicação iniciar...'
                sleep 30 // Dá um tempo para os containers subirem
                
                echo 'Verificando se os containers estão rodando...'
                sh 'docker-compose ps'
                
                echo 'Aplicação "implantada" com sucesso!'
                echo 'Acesse em http://localhost:8080 (na máquina Jenkins)'
            }
        }
    }
    
    // 3. Post-Actions: O que fazer no final
    // Isso garante que, mesmo que o pipeline falhe, ele limpe o ambiente
    post {
        always {
            echo 'Pipeline finalizado. Desligando os containers...'
            // Limpa o ambiente para a próxima execução
            sh 'docker-compose down'
        }
        success {
            echo 'Pipeline executado com SUCESSO!'
        }
        failure {
            echo 'Pipeline FALHOU. Verifique os logs.'
        }
    }
}
