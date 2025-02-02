+++
title = "2. Provision VM with Self-Hosted Runner"
weight = 2
date = 2025-02-02
draft = false
+++

## Goal

In this tutorial, you will learn how to:

1.	Provision an Azure Virtual Machine (VM) using the Azure CLI.
2.	Install and configure a GitHub self-hosted runner on the VM for Continuous Deployment (CD).
3.	Set up the necessary services to automatically deploy your application from GitHub Actions.

###Prerequisites

- Azure CLI installed and configured (az login).
- SSH key generated (ssh-keygen) and available at ~/.ssh/id_rsa.pub.
- A GitHub repository with a CI workflow already in place.

## Step-by-step Guide

### 1. Provision the Azure VM with Azure CLI

We’ll create an Ubuntu VM on Azure that will have a self-hosted runner installed in order to run a GitHub Action Workflow.

Steps:

1.	Create a cloud-init file that installs .Net Runtime and creates a service.

	> infra/cloud-init_dotnet.yaml
	
	```yaml
	#cloud-config
	
	# Install .Net Runtime 9.0
	runcmd:
	  # Register Microsoft repository (which includes .Net Runtime 9.0 package)
	  - wget -q https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
	  - dpkg -i packages-microsoft-prod.deb
	
	  # Install .Net Runtime 9.0
	  - apt-get update
	  - apt-get install -y aspnetcore-runtime-9.0
	
	# Create a service for the application
	write_files:
	  - path: /etc/systemd/system/Newsletter.service
	    content: |
	      [Unit]
	      Description=ASP.NET Web App running on Ubuntu
	
	      [Service]
	      WorkingDirectory=/opt/Newsletter
	      ExecStart=/usr/bin/dotnet /opt/Newsletter/Newsletter.dll
	      Restart=always
	      RestartSec=10
	      KillSignal=SIGINT
	      SyslogIdentifier=Newsletter
	      User=www-data
	      EnvironmentFile=/etc/Newsletter/.env
	
	      [Install]
	      WantedBy=multi-user.target      
	    owner: root:root
	    permissions: '0644'
	
	    # Create a directory for environment variables for the application
	  - path: /etc/Newsletter/.env
	    content: |
	      ASPNETCORE_ENVIRONMENT=Production
	      ASPNETCORE_URLS="http://*:5000"
	    owner: root:root
	    permissions: '0600'
	
	systemd:
	  units:
	    - name: Newsletter.service
	      enabled: true
	
	```

2.	Create a provisioning script that provisions the VM (with the cloud-init config).

	> infra/provision_vm.sh
	
	```bash
	#!/bin/bash
	
	set -e  # Exit immediately if a command exits with a non-zero status.
	
	# Variables - adjust these as needed
	RESOURCE_GROUP="NewsletterRG"
	LOCATION="northeurope"          # Change to your preferred Azure region
	
	VM_NAME="NewsletterVM"
	ADMIN_USERNAME="azureuser"
	SSH_KEY_PATH="$HOME/.ssh/id_rsa.pub"  # Path to your SSH public key
	PORT=5000
	
	# Function to check prerequisites
	check_prerequisites() {
	  # Check if the SSH key exists
	  if [ ! -f "$SSH_KEY_PATH" ]; then
	    echo "SSH key not found at $SSH_KEY_PATH. Please generate an SSH key (e.g., with 'ssh-keygen')."
	    exit 1
	  fi
	  echo "✔ SSH key found at $SSH_KEY_PATH"
	
	  # Check if the Azure CLI is installed
	  if ! command -v az &> /dev/null; then
	    echo "Azure CLI is not installed. Please install it first."
	    exit 1
	  fi
	  echo "✔ Azure CLI is installed."
	
	  # Check if the Azure CLI is logged in
	  if [ -z "$(az account show --query id -o tsv)" ]; then
	    echo "Azure CLI is not logged in. Please run 'az login' first."
	    exit 1
	  fi
	  echo "✔ Azure CLI is logged in."
	}
	
	# Run the prerequisite checks
	check_prerequisites
	
	# Create a resource group
	echo "Creating resource group '$RESOURCE_GROUP' in region '$LOCATION'..."
	az group create \
	    --name "$RESOURCE_GROUP" \
	    --location "$LOCATION"
	
	# Create the Ubuntu VM
	echo "Creating Ubuntu VM '$VM_NAME'..."
	az vm create \
	  --resource-group "$RESOURCE_GROUP" \
	  --location northeurope \
	  --name "$VM_NAME" \
	  --image Ubuntu2204 \
	  --size Standard_B1s \
	  --admin-username "$ADMIN_USERNAME" \
	  --generate-ssh-keys \
	  --custom-data @cloud-init_dotnet.yaml
	
	# Open port 5000 on the VM to allow incoming traffic
	echo "Opening port '$PORT' on the VM '$VM_NAME' ..."
	az vm open-port \
	  --resource-group "$RESOURCE_GROUP" \
	  --name "$VM_NAME" \
	  --port $PORT \
	  --priority 1001
	
	echo "Setup complete. Your VM '$VM_NAME' is ready and port '$PORT' is open."
	echo "Connect to your VM with: ssh $ADMIN_USERNAME@$(az vm show -d -g $RESOURCE_GROUP -n $VM_NAME --query publicIps -o tsv)"
	```
	
	> Explanation:
	> 
	> - Resource Group: NewsletterRG2 groups all related resources.
	> - VM Configuration: Ubuntu 22.04 LTS, using SSH authentication.
	> - Port 5000: Opened to allow HTTP traffic for the deployed app.
	> - Custom Initialization: Uses cloud-init_dotnet.yaml to install .NET and configure the
	> - The cloud-init script installs the .NET runtime and sets up the application as a systemd service.
	

3. Execute Provisioning Script: Change the script's permission with `chmod +x provision_vm.sh` and run it to set up your VM.

	```bash
	chmod +x provision_vm.sh
	./provision_vm.sh
	```

### 3. Install the GitHub Actions Runner on the VM

Once the VM is up and running, you’ll install and configure the GitHub Actions runner.

Steps:

1. Configure Runner on GitHub:

	- Navigate to your repository's settings, find the Actions tab, and set up a new self-hosted runner following GitHub's instructions:
    - select the **Settings** tab
    - Select **Actions -> Runners** in the side menu
    - Click **New self-hosted runner**
    - Select **Linux** (Architecture: x64)

2. Install Runner on VM:

	- SSH into the VM using the command output from the provisioning script:
			
		```bash
		ssh azureuser@<VM_PUBLIC_IP>
		```
			
	- Run the code from Github
	- Press `<Enter>`to accept all default values in the configuration wizard
			
		```bash
		# The code from Github
			
		...
			
		./run.sh
		```

> Purpose: This registers your VM as a self-hosted runner, ready to deploy your application in the CD pipeline.

### 4. Update the GitHub Workflow for Continuous Deployment

Now, we’ll modify the GitHub Actions workflow to deploy the application using the self-hosted runner.

> .github/workflows/cicd.yaml

```yaml
name: CI/CD Pipeline # Name of the GitHub Actions workflow

on:
  push: # Trigger the workflow on push events
    branches:
      - main # Only trigger on pushes to the 'main' branch
  workflow_dispatch: # Enable manual triggering of the workflow

jobs:
  build:
    runs-on: ubuntu-latest # Use the latest Ubuntu runner

    steps:
    - name: Checkout repository # Check out the repository to the GitHub Actions runner
      uses: actions/checkout@v4

    - name: Setup .NET SDK # Set up the .NET SDK according to the specified version
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.0.x' # Specify the .NET version to use

    - name: Restore, build and publish # Restore dependencies, build, and publish the app
      run: |
        echo "Restore the NuGet packages without using the cache"
        dotnet restore --no-cache

        echo "Build the app in Release configuration without restoring dependencies"
        dotnet build --configuration Release --no-restore
        
        echo "Publish the app to the 'publish' directory in the repository root"
        dotnet publish ./Newsletter.csproj --configuration Release --no-restore --output ./publish

    - name: Upload app artifacts # Upload the published app artifacts to the GitHub artifact repository
      uses: actions/upload-artifact@v4
      with:
        name: app-artifacts # Name the artifact 'app-artifacts'
        path: publish # Specify the path to the 'publish' directory in the repository root

  deploy:
    runs-on: self-hosted # Use a self-hosted runner for deployment (that runs on the Azure VM)
    needs: build # Ensure the build job completes successfully before running this job

    steps:
    - name: Download the artifacts from Github (from the build job) # Download the build artifacts
      uses: actions/download-artifact@v4
      with:
        name: app-artifacts # Specify the name of the artifact to download

    - name: Stop the application service # Stop the running application service
      run: sudo systemctl stop Newsletter.service        

    - name: Deploy the application # Deploy the new version of the application
      run: |
        echo "Remove the existing application directory"
        sudo rm -Rf /opt/Newsletter || true
        echo "Copy the new build to the application directory"
        sudo cp -r /home/azureuser/actions-runner/_work/Newsletter/Newsletter/ /opt/Newsletter

    - name: Start the application service # Start the application service
      run: sudo systemctl start Newsletter.service

```

### 5. Prepare appsettings.Production.json for Prod environment

We’ll need to prepare the settings for the Production environment (in which the Azure VM operates). Since we don´t have a MongoDB yet we want to use the in-memory database for the time being. In .Net you can have one appsettings.json file for each environment (Development, Test, Production, etc). In the service file that we decalared in the cloud-init script we have set the environment to Production with this line: `Environment=ASPNETCORE_ENVIRONMENT=Production`

Steps:

1.	Create a file `appsettings.Production.json` in the root of your project. Add the following section:

> appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "AzureKeyVault": {
    "VaultUri": ""
  },
  "MongoDbSettings": {
    "ConnectionString": "SET PROD CONNECTION STRING",
    "DatabaseName": "NewsletterProdDb",
    "CollectionName": "Subscribers"
  },
  "DatabaseToUse": "InMemoryDb"
}
```

> Purpose:
> 
> - This is the Production settings.
> - Note how the InMemoryDb is used `"DatabaseToUse": "InMemoryDb"`

### 6. Test and Verify the Deployment

1. Push a Code Change to the main branch:

	```bash
	git add .
	git commit -m "Add Github Action CD Workflow"
	git push origin main
	```

2.	Go to the Actions Tab in your GitHub repository to monitor the workflow.
3.	Verify the Application:

	Open your browser and navigate to:
	
	```
	http://<VM_PUBLIC_IP>:5000
	``` 
	
	You should see your deployed ASP.NET application running!

### 7. Run the runner as a service

In a real scenario you should run the runner as a service in the background. You can do this by running the following commands:

```bash
sudo ./svc.sh install azureuser
sudo ./svc.sh start
```

You can follow the logs with `journalctl` (use autocomplete with `<tab>` to find your service)

```bash
sudo journalctl -u actions.runner.<GithubOrganization>-<GithubRepo>.<Runner>.service -f
```

## Summary

In this tutorial, we:

1.	Provisioned an Azure VM with Azure CLI to host a self-hosted GitHub Actions runner.
2.	Configured the .NET runtime and set up the app as a systemd service.
3.	Installed and registered a GitHub self-hosted runner for deployment.
4.	Updated the CI/CD workflow to deploy the app automatically using the self-hosted runner.
5.	Verified the deployment by accessing the application through the VM’s public IP.

# You now have a CI/CD pipeline to an Azure VM! 🚀