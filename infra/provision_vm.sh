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
