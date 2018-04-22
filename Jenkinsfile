node('windows&&unity') {

	def version = "1.0.0"
	def lastCommitHash = ''
	def productVersion = ''

    stage('Checkout') {
        def scmVars = checkout scm
		def temp = scmVars.GIT_COMMIT

        lastCommitHash = temp.substring(0, 7)
        echo "Last commit hash: ${lastCommitHash}"

		currentBuild.displayName = "${version}-${env.BUILD_ID}-${lastCommitHash}"

		productVersion = "${version}-${env.BUILD_NUMBER}-${lastCommitHash}"
    }
    //stage('Register Unity (Disabled)') {
	    // try{
	    //  	withCredentials([usernamePassword(credentialsId: 'unity-login', passwordVariable: 'PASSWORD', usernameVariable: 'USERNAME')]) {
	    //  		bat '''%UNITYEXE% -quit -nographics -silent-crashes -batchmode -username \'%USERNAME%\' -password \'%PASSWORD%\' -logFile registerLog.txt '''
	    //  	}
	    // } finally {
	    //  	bat '''type registerLog.txt '''
	    // }
    //}
    // stage('Unit tests') { }
    stage('Build All Systems') {
	    try{
	    	withCredentials([usernamePassword(credentialsId: 'unity-login', passwordVariable: 'PASSWORD', usernameVariable: 'USERNAME')]) {
				bat '''%UNITYEXE% -quit -nographics -silent-crashes -batchmode -projectPath %WORKSPACE% -executeMethod Builder.ManySystemBuilder.build -logFile stdout.txt '''
	    	}
	 	} finally {
	    	bat '''type stdout.txt '''
	    }   	
    }
    // stage('Sonar') { }
    stage('Archive') {
    	withEnv(["VERSION=${productVersion}"]) {
	    	withCredentials([usernamePassword(credentialsId: 'nexus-credentials', passwordVariable: 'PASSWORD', usernameVariable: 'USERNAME')]) {
	    		bat '''echo WebPlayer'''
	    		bat '''gradlew.bat publish -Pversion="%VERSION%" -PjenkinsUser="%USERNAME%" -PjenkinsPassword="%PASSWORD%" -PbuildName="webgl" -PunityOutput="webgl/residentracing" ''' 
	    		bat '''gradlew.bat publish -Pversion="latest" -PjenkinsUser="%USERNAME%" -PjenkinsPassword="%PASSWORD%" -PbuildName="webgl" -PunityOutput="webgl/residentracing" ''' 

	    		bat '''echo OSX'''
	    		bat '''gradlew.bat publish -Pversion="%VERSION%" -PjenkinsUser="%USERNAME%" -PjenkinsPassword="%PASSWORD%" -PbuildName="osx" -PunityOutput="osxuniversal" ''' 
	    		bat '''gradlew.bat publish -Pversion="latest" -PjenkinsUser="%USERNAME%" -PjenkinsPassword="%PASSWORD%" -PbuildName="osx" -PunityOutput="osxuniversal" ''' 

	    		bat '''echo Linux'''
	    		bat '''gradlew.bat publish -Pversion="%VERSION%" -PjenkinsUser="%USERNAME%" -PjenkinsPassword="%PASSWORD%" -PbuildName="linux" -PunityOutput="linuxuniversal" ''' 
	    		bat '''gradlew.bat publish -Pversion="latest" -PjenkinsUser="%USERNAME%" -PjenkinsPassword="%PASSWORD%" -PbuildName="linux" -PunityOutput="linuxuniversal" ''' 

	    		bat '''echo Win'''
	    		bat '''gradlew.bat publish -Pversion="%VERSION%" -PjenkinsUser="%USERNAME%" -PjenkinsPassword="%PASSWORD%" -PbuildName="win" -PunityOutput="windows" ''' 
	    		bat '''gradlew.bat publish -Pversion="latest" -PjenkinsUser="%USERNAME%" -PjenkinsPassword="%PASSWORD%" -PbuildName="win" -PunityOutput="windows" ''' 

	    		bat '''echo Win64'''
	    		bat '''gradlew.bat publish -Pversion="%VERSION%" -PjenkinsUser="%USERNAME%" -PjenkinsPassword="%PASSWORD%" -PbuildName="win64" -PunityOutput="windows64" ''' 
	    		bat '''gradlew.bat publish -Pversion="latest" -PjenkinsUser="%USERNAME%" -PjenkinsPassword="%PASSWORD%" -PbuildName="win64" -PunityOutput="windows64" ''' 
	    	}
	    }
    }
}