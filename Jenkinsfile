pipeline {
    agent any

    environment {
        DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    }

    stages {
        stage('Checkout') {
            steps {
                echo 'Pulling latest code from GitHub...'
                checkout scm
            }
        }

        stage('Restore') {
            steps {
                echo 'Restoring dependencies...'
                dir('backend') {
                    bat 'dotnet restore'
                }
            }
        }

        stage('Build') {
            steps {
                echo 'Building the project...'
                dir('backend') {
                    bat 'dotnet build --no-restore'
                }
            }
        }

        stage('Test') {
            steps {
                echo 'Running tests...'
                dir('tests') {
                    bat 'dotnet test --no-build --verbosity normal'
                }
            }
        }

        stage('Deploy') {
            steps {
                echo 'Deploying application...'
                dir('backend') {
                    bat 'start /B dotnet run'
                }
            }
        }
    }

    post {
        success {
            echo 'Pipeline passed! Application deployed successfully.'
        }
        failure {
            echo 'Pipeline failed! Deployment blocked.'
        }
    }
}
