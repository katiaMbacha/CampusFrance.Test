pipeline {
  agent any
  options { timestamps() }
  environment {
    // rends dotnet visible pour le process Jenkins
    PATH = "/usr/local/share/dotnet:/usr/local/bin:${PATH}"
    DOTNET_ROOT = "/usr/local/share/dotnet"

    DOTNET_CLI_TELEMETRY_OPTOUT='1'
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE='1'
    HEADLESS='1'
    DATA_DIR = "${WORKSPACE}/Data"
  }
  stages {
    stage('Checkout'){ steps { checkout scm } }
    stage('Env check'){
      steps {
        sh '''
          echo "whoami: $(whoami)"
          echo "PATH: $PATH"
          which dotnet || true
          dotnet --info || true
          "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" --version || true
          ls -la "$DATA_DIR" || true
        '''
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
          archiveArtifacts artifacts: 'TestResults/**/*, **/Screenshots/**/*, **/logs/**/*', onlyIfSuccessful: false
        }
      }
    }
  }
}
