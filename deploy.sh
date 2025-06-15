#!/bin/bash

# 🚀 Script de déploiement BillPaymentProvider v2.0
# Utilisation: ./deploy.sh [environment]
# Environnements: development, staging, production

set -e

# Configuration
APP_NAME="BillPaymentProvider"
VERSION="2.0.0"
ENVIRONMENT=${1:-development}

echo "🚀 Déploiement de $APP_NAME v$VERSION - Environnement: $ENVIRONMENT"
echo "=================================================================="

# Vérification des prérequis
echo "📋 Vérification des prérequis..."
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET 8.0 SDK requis"
    exit 1
fi

if ! command -v git &> /dev/null; then
    echo "❌ Git requis"
    exit 1
fi

echo "✅ Prérequis validés"

# Nettoyage
echo "🧹 Nettoyage des artefacts précédents..."
rm -rf ./publish
find . -name "bin" -type d -exec rm -rf {} + 2>/dev/null || true
find . -name "obj" -type d -exec rm -rf {} + 2>/dev/null || true

# Restauration des dépendances
echo "📦 Restauration des dépendances..."
cd BillPaymentProvider
dotnet restore

# Tests de sécurité rapides
echo "🔒 Vérification de sécurité..."
if grep -r "password.*=.*admin" . --exclude-dir=bin --exclude-dir=obj 2>/dev/null; then
    echo "⚠️  Mots de passe en dur détectés - Vérifiez la configuration"
fi

# Configuration selon l'environnement
echo "⚙️  Configuration pour $ENVIRONMENT..."
case $ENVIRONMENT in
    "production")
        echo "🏭 Configuration PRODUCTION"
        BUILD_CONFIG="Release"
        
        # Vérifications production
        if [ ! -f "appsettings.Production.json" ]; then
            echo "⚠️  appsettings.Production.json manquant"
        fi
        
        echo "🔐 Points de sécurité PRODUCTION à vérifier:"
        echo "   - HTTPS activé"
        echo "   - Clés JWT sécurisées"
        echo "   - Base de données production configurée"
        echo "   - Logs externalisés"
        ;;
    "staging")
        echo "🧪 Configuration STAGING"
        BUILD_CONFIG="Release"
        ;;
    *)
        echo "🛠️  Configuration DEVELOPMENT"
        BUILD_CONFIG="Debug"
        ;;
esac

# Build
echo "🔨 Compilation en mode $BUILD_CONFIG..."
dotnet build --configuration $BUILD_CONFIG --no-restore

# Tests rapides
echo "🧪 Tests rapides..."
if [ -f "BillPaymentProvider.Tests/BillPaymentProvider.Tests.csproj" ]; then
    dotnet test --configuration $BUILD_CONFIG --no-build
else
    echo "⚠️  Aucun projet de test trouvé - Tests manuels recommandés"
fi

# Publication
echo "📦 Publication de l'application..."
dotnet publish --configuration $BUILD_CONFIG --output ../publish --no-build

# Vérification de la publication
echo "✅ Vérification de la publication..."
if [ ! -f "../publish/BillPaymentProvider.dll" ]; then
    echo "❌ Échec de la publication"
    exit 1
fi

# Migration de base de données (si nécessaire)
if [ "$ENVIRONMENT" != "development" ]; then
    echo "🗄️  Migration de base de données..."
    # dotnet ef database update --configuration $BUILD_CONFIG
    echo "⚠️  Migration manuelle requise si nécessaire"
fi

# Instructions de déploiement
cd ..
echo ""
echo "🎉 Publication réussie dans le dossier './publish'"
echo ""
echo "📋 Prochaines étapes selon l'environnement:"
echo ""

case $ENVIRONMENT in
    "production")
        echo "🏭 PRODUCTION:"
        echo "   1. Sauvegarder la base de données actuelle"
        echo "   2. Configurer les variables d'environnement:"
        echo "      export ASPNETCORE_ENVIRONMENT=Production"
        echo "      export ASPNETCORE_URLS=https://+:443;http://+:80"
        echo "   3. Configurer le reverse proxy (nginx/IIS)"
        echo "   4. Démarrer l'application:"
        echo "      cd publish && dotnet BillPaymentProvider.dll"
        echo "   5. Vérifier le health check:"
        echo "      curl https://yourdomain.com/health"
        ;;
    "staging")
        echo "🧪 STAGING:"
        echo "   1. Déployer sur serveur de staging"
        echo "   2. Configurer les variables d'environnement"
        echo "   3. Exécuter les tests d'intégration"
        echo "   4. Valider avec l'équipe QA"
        ;;
    *)
        echo "🛠️  DEVELOPMENT:"
        echo "   1. Lancer l'application:"
        echo "      cd publish && dotnet BillPaymentProvider.dll"
        echo "   2. Accéder à l'API:"
        echo "      http://localhost:5163"
        echo "   3. Documentation Swagger:"
        echo "      http://localhost:5163/swagger"
        ;;
esac

echo ""
echo "🔒 Utilisateurs par défaut:"
echo "   admin / Admin123!"
echo "   user / User123!"
echo "   manager / Manager123!"
echo ""
echo "📚 Documentation complète dans README.md"
echo ""
echo "✨ Déploiement terminé avec succès!"
