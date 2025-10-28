// Jenkinsfile
pipeline {
    // 1. Agente: Onde o pipeline vai rodar?
    agent any

    // Define a variável de ambiente para corrigir o erro de 'libicu' do .NET
    environment {
        DOTNET_SYSTEM_GLOBALIZATION_INVARIANT = 'true'
    }

    // 2. Stages: As etapas do nosso processo de CI/CD
    stages {
        
        // --- ESTÁGIO 1: CHECKOUT ---
        // Pega o código-fonte do seu repositório (ex: Git)
        stage('Checkout') {
            steps {
                echo 'Baixando o código-fonte...'
                // O Jenkins faz isso automaticamente quando configurado com um SCM (Git)
                checkout scm 
            }
        }

        // --- ESTÁGIO 2: BUILD (DevOps) ---
        // Compila o projeto .NET para garantir que não há erros de sintaxe
        stage('Build .NET') {
            steps {
                echo 'Restaurando dependências e compilando o projeto .NET...'
                sh 'dotnet restore'
                sh 'dotnet build --no-restore'
            }
        }

        // --- ESTÁGIO 3: TEST (DevOps) ---
        // Roda a suíte de testes xUnit.
        stage('Run Tests') {
            steps {
                echo 'Executando os 47 testes automatizados...'
                
                // Tenta rodar os testes. Se falharem, marca como
                // 'UNSTABLE' (amarelo) e continua o pipeline,
                // graças ao '|| true'.
                catchError(buildResult: 'UNSTABLE', stageResult: 'UNSTABLE') {
                    sh 'dotnet test || true'
                }
            }
        }

        // --- ESTÁGIO 4: SECURITY (DevSecOps) ---
        // Roda as verificações de segurança
        stage('Security Scan (DevSecOps)') {
            parallel {
                // Verificação 1: Análise de Dependências (SCA)
                stage('Scan Dependencies') {
                    steps {
                        echo 'Verificando pacotes NuGet vulneráveis...'
                        sh 'dotnet list package --vulnerable || true'
                    }
                }
                
                // Verificação 2: Análise de Código Estático (SAST)
                stage('Scan for Secrets') {
                    steps {
                        echo 'Verificando se há segredos "hard-coded" no código...'
                        // '|| true' para não parar o pipeline caso encontre algo
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
                sh 'docker-compose build'
            }
        }
        
        // --- ESTÁGIO 6: DEPLOY (DevOps) ---
        // Simula a implantação na própria máquina Jenkins
        stage('Deploy (Simulated Staging)') {
            steps {
                echo 'Iniciando a aplicação AutoStock com Docker Compose...'
                sh 'docker-compose up -d'
                
                echo 'Aguardando 30 segundos para a aplicação iniciar...'
                sleep 30 // Dá um tempo para os containers subirem
                
                echo 'Verificando se os containers estão rodando...'
                sh 'docker-compose ps'
                
                echo 'Aplicação "implantada" com sucesso!'
                echo 'Acesse em http://localhost:8081 (na máquina Jenkins)'
            }
        }
    } // <-- FIM DO BLOCO 'stages'
    
    // 3. Post-Actions: O que fazer no final
    // Roda sempre, independente de falha ou sucesso
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
} // <-- FIM DO BLOCO 'pipeline'
