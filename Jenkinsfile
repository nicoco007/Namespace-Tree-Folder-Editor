pipeline {
  agent {
    node {
      label 'windows && vs-15'
    }
    
  }
  stages {
    stage('Build') {
      steps {
        bat 'nuget restore'
        bat 'msbuild /p:Configuration=Release'
        archiveArtifacts 'Namespace Tree Folder Editor/bin/Release/**'
        cleanWs(cleanWhenAborted: true, cleanWhenFailure: true, cleanWhenNotBuilt: true, cleanWhenSuccess: true, cleanWhenUnstable: true)
      }
    }
  }
}