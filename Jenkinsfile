pipeline {
  agent any
  stages {
    stage('Clean') {
        steps {
        catchError(buildResult: 'SUCCESS', stageResult: 'FAILURE') {
          sh 'docker stop $(docker ps -a -q -f "name=apibackend")'
          sh 'docker rm apibackend'
          sh 'docker rmi apibackend:v1.0'
            }
         }
      }
    stage('Build') {
      steps {
        sh 'docker build -t apibackend:v1.0 .'
      }
    }
    stage('Run') {
      steps {
        sh 'docker run -d --network jasperserver_reportsnet --name apibackend -p 8082:80 apibackend:v1.0'
      }
    }
  }
}