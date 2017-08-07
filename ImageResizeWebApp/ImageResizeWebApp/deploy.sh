#!/bin/bash
# Docs: https://docs.microsoft.com/en-us/azure/app-service-web/scripts/app-service-cli-deploy-github
# Assumes user is logged in already via "az login"
# Alternatively this can be run from Azure Cloud Shell in the portal

# Replace the following URL with a public GitHub repo URL
gitrepo=https://github.com/Azure-Samples/integration-image-upload-resize-storage-functions
resourceGroup=imageResizer
webappname=imageResizerWeb
appServicePlanSKU=FREE
githubSouceControlToken=$GITHUB_TOKEN

# Create a resource group.
echo
echo "Creating Resource Group: $resourceGroup"
az group create --location westus --name $resourceGroup

# Create an App Service plan in STANDARD tier (minimum required by deployment slots).
echo
echo "Creating App Service Plan: $webappname using SKU: $appServicePlanSKU under Resource Group: $resourceGroup"
az appservice plan create --name $webappname \
                          --resource-group $resourceGroup \
                          --sku $appServicePlanSKU

# Create a web app.
echo
echo "Creating Web App: $webappname within App Service Plan: $webappname"
az webapp create --name $webappname \
                 --resource-group $resourceGroup \
                 --plan $webappname

# Deploy code from a public GitHub repository. 
echo
echo "Configuring deployment config for: $webappname"
az webapp deployment source config --name $webappname \
                                   --resource-group $resourceGroup \
                                   --repo-url $gitrepo \
                                   --git-token $githubSouceControlToken \
                                   --branch master \
                                   --manual-integration

# Browse to the production slot.
echo
echo "Browsing to: $webappname" 
az webapp browse --name $webappname \
                 --resource-group $resourceGroup


# Destroy resources with:
# az group delete --name $resourceGroup