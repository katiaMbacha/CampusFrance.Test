pipeline {
  agent any
  options { timestamps() }
  environment {
    DOTNET_CLI_TELEMETRY_OPTOUT='1'
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'
    HEADLESS='1'
    DATA_DIR = "${WORKSPACE}/Data"
  }
  stages {
    stage('Checkout'){ steps { checkout scm } }

    stage('Env check'){
      steps {
        sh 'dotnet --info || true'
        sh '"/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" --version || echo "Chrome non trouvé"'
        sh 'ls -la "$DATA_DIR" || true'
      }
    }

    stage('Restore'){ steps { sh 'dotnet restore CampusFrance.Test.csproj' } }
    stage('Build'){ steps { sh 'dotnet build CampusFrance.Test.csproj --configuration Release --no-restore' } }
    stage('Test'){
      steps {
        sh '''
          dotnet test CampusFrance.Test.csproj \
            --configuration Release --no-build \
            --logger "trx;LogFileName=test_results.trx" \
            --results-directory ./TestResults
        '''
      }
      post {
        always {
          // Si le plugin MSTest n'est pas installé, commente cette ligne ou installe-le.
          mstest testResultsFilePattern: 'TestResults/*.trx', keepLongStdio: true, failOnError: false
          archiveArtifacts artifacts: 'TestResults/**/*, **/Screenshots/**/*, **/logs/**/*', onlyIfSuccessful: false
        }
      }
    }
  }
  post {
    success { echo '✅ Tests OK (macOS)' }
    failure { echo '❌ Échec — ouvre les TRX/artefacts' }
  }
}
