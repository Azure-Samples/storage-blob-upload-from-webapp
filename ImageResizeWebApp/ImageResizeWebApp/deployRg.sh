#!/bin/bash
# Docs: https://docs.microsoft.com/en-us/azure/app-service-web/scripts/app-service-cli-deploy-github
#       https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-azure-function-azure-cli
# Assumes user is logged in already via "az login"
# Alternatively this can be run from Azure Cloud Shell in the portal

## General settings ##
# Replace the following URL with a private/public GitHub repo URL
webappGitrepo=https://github.com/Azure-Samples/integration-image-upload-resize-storage-functions
funcGitrepo=https://github.com/Azure-Samples/function-image-upload-resize

## App Service (Web App) settings ##
webAppName=imageResizerWeb

## Github Personal Access Token ##
# This deploy script requires setting a "Personal Access Token with Github" with "repo" privileges for private repo (while in testing mode)
# NOTE: When you create a Personal Access Token, GitHub will only show it once and you'll have to regenerate and update the token on the service
# Guide: https://help.github.com/articles/creating-a-personal-access-token-for-the-command-line/
# Do not commit your GitHub Personal Access Token to git
#    Instead, add it as a local environment variable called "GITHUB_TOKEN", the script will reference that env variable
githubSouceControlToken=$GITHUB_TOKEN
[ -z "$GITHUB_TOKEN" ] && echo "Need to set GITHUB_TOKEN as an environment variable" && exit 1;

## Function Settings ##
# Function name has been lowercased to be used in Storage name creation
functionName=imageresizefunc

## Storage Settings ##
# Substitute your own globally unique storage account name.
# Storage account names must be between 3 and 24 characters 
#    in length and may contain numbers and lowercase letters only.
storageName="$functionName$RANDOM"
queueName=imageprocessing
imagesContainerName=images
thumbnailsContainerName=thumbnails

# Create a resource group.
echo
echo "Creating Resource Group 'myResourceGroup' within region 'westus'"
az group create --location westus \
                --name myResourceGroup

# Create a storage account (Functions require a storage account)
echo
echo "Creating Storage Account $storageName within myResourceGroup"
az storage account create --name $storageName \
                          --location westus \
                          --resource-group myResourceGroup \
                          --sku Standard_LRS

# Find the storage key for newly created storage account. Uses JMESPath syntax for --query
echo
echo "Finding first storage API key for Storage Account $storageName"
storageAccountKey=$(az storage account keys list --resource-group myResourceGroup --account-name $storageName --query [0].value)
# Remove leading and trailing double quotes
storageAccountKey=$(sed -e 's/^"//' -e 's/"$//' <<<"$storageAccountKey")
echo $storageAccountKey

# Find storage connection string. Uses JMESPath syntax for --query
echo
echo "Finding storage connection string for Storage Account $storageName"
storageConnectionString=$(az storage account show-connection-string --resource-group myResourceGroup --name $storageName --query connectionString)
# Remove leading and trailing double quotes
storageConnectionString=$(sed -e 's/^"//' -e 's/"$//' <<<"$storageConnectionString")
echo $storageConnectionString
                          
# Create blog storage containers for images
echo
echo "Creating blob storage containers: $imagesContainerName"
az storage container create --name $imagesContainerName \
                            --account-name $storageName \
                            --account-key $storageAccountKey \
                            --public-access container

# Create blog storage containers for images
echo
echo "Creating blob storage containers: $thumbnailsContainerName"
az storage container create --name $thumbnailsContainerName \
                            --account-name $storageName \
                            --account-key $storageAccountKey \
                            --public-access container

# Create queue
echo
echo "Creating a queue within storage account: $storageName"
az storage queue create --name $queueName \
                        --account-name $storageName \
                        --account-key $storageAccountKey

# Create a function app
echo 
echo "Creating a function app $functionName within myResourceGroup"
az functionapp create --name $functionName \
                      --storage-account $storageName \
                      --resource-group myResourceGroup \
                      --consumption-plan-location westus

# Deploy functiona app code from a private GitHub repository. 
# GitHub Personal Access Token is only required for private repos and can be removed for public repos
echo
echo "Configuring deployment config for Function App: $functionName"
az functionapp deployment source config --name $functionName \
                                        --resource-group myResourceGroup \
                                        --repo-url $funcGitrepo \
                                        --git-token $githubSouceControlToken \
                                        --branch master \
                                        --manual-integration

# Set function app settings
echo
echo "Configuring Function App $functionName environment settings"
az functionapp config appsettings set --name $functionName \
                                      --resource-group myResourceGroup \
                                      --settings STORAGE_CONNECTION_STRING=$storageConnectionString QUEUE=$queueName


# Create an App Service plan
echo
echo "Creating App Service Plan: $webAppName using SKU FREE"
az appservice plan create --name $webAppName \
                          --resource-group myResourceGroup \
                          --sku FREE

# Create a web app.
echo
echo "Creating Web App: $webAppName within App Service Plan: $webAppName"
az webapp create --name $webAppName \
                 --resource-group myResourceGroup \
                 --plan $webAppName

# Deploy webapp code from a private GitHub repository. 
# GitHub Personal Access Token is only required for private repos and can be removed for public repos
echo
echo "Configuring deployment config for WebApp: $webAppName"
az webapp deployment source config --name $webAppName \
                                   --resource-group myResourceGroup \
                                   --repo-url $webappGitrepo \
                                   --git-token $githubSouceControlToken \
                                   --branch master \
                                   --manual-integration

# Configure app settings
echo 
echo "Configuring Web App $webAppName environment settings"
az webapp config appsettings set --name $webAppName \
                                 --resource-group myResourceGroup \
                                 --settings AzureStorageConfig__AccountName=$storageName AzureStorageConfig__AccountKey=$storageAccountKey AzureStorageConfig__QueueName=$queueName AzureStorageConfig__ImageContainer=$imagesContainerName AzureStorageConfig__ThumbnailContainer=$thumbnailsContainerName

# Browse to web app.
echo
echo "Browsing to: $webAppName" 
az webapp browse --name $webAppName \
                 --resource-group myResourceGroup