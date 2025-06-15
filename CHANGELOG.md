# Changelog - BillPaymentProvider

Toutes les modifications notables de ce projet seront documentées dans ce fichier.

## [2.0.0] - 2025-06-15

### 🔒 Sécurité - BREAKING CHANGES
- **Migration BCrypt** : Remplacement complet de SHA256 par BCrypt pour le hashage des mots de passe
- **JWT sécurisé** : Implémentation d'un service JWT centralisé avec validation robuste
- **Protection anti-brute force** : Système de limitation des tentatives de connexion
- **Validation avancée** : Intégration de FluentValidation pour toutes les entrées
- **Headers de sécurité** : Middleware automatique pour headers de sécurité

### 🏗️ Architecture
- **Configuration typée** : Remplacement de la configuration loose par des classes typées
- **Injection de dépendances** : Refactoring complet avec extensions modulaires
- **Clean Architecture** : Séparation claire des responsabilités en couches
- **Middleware pipeline** : Gestion centralisée et ordonnée des middleware

### 📊 Observabilité
- **Serilog structuré** : Remplacement du logging basique par Serilog avec format JSON
- **Multi-fichiers** : Logs séparés par type (application, erreurs, audit)
- **Health checks** : Surveillance en temps réel de la base de données et services
- **Audit trail** : Traçabilité complète de toutes les actions utilisateur

### 🛠️ Qualité du code
- **Extensions modulaires** : Réorganisation en extensions spécialisées
- **Services centralisés** : JwtService, UserService, BruteForceProtection
- **Utilities** : UserMigrationUtility pour la gestion des migrations
- **Documentation** : README complet et guides d'amélioration

### 🧪 Tests et validation
- **Migration validée** : 3 utilisateurs migrés avec succès de SHA256 vers BCrypt
- **Endpoints testés** : Tous les endpoints validés avec authentification
- **Performance** : Tests de charge et de sécurité effectués
- **Suite de tests** : Documentation complète des tests de validation

### 📁 Nouveaux fichiers
- `Services/JwtService.cs` - Gestion centralisée JWT
- `Configuration/Settings.cs` - Classes de configuration typée
- `Extensions/LoggingExtensions.cs` - Configuration Serilog
- `Utils/UserMigrationUtility.cs` - Utilitaire de migration
- `Controllers/AdminController.cs` - Administration système
- `IMPROVEMENTS.md` - Guide détaillé des améliorations
- `TESTS_VALIDATION.md` - Documentation des tests

### 🔧 Modifications
- `Program.cs` - Refactoring complet avec nouvelles extensions
- `Extensions/ServiceCollectionExtensions.cs` - Modularisation des services
- `Controllers/AuthController.cs` - Sécurisation avec BCrypt et JWT
- `Services/UserService.cs` - Migration vers BCrypt
- `appsettings.json` - Nouvelle configuration typée
- `BillPaymentProvider.csproj` - Ajout dépendances sécurité et logging

### ⬆️ Dépendances ajoutées
- `BCrypt.Net-Next` - Hashage sécurisé des mots de passe
- `Serilog.AspNetCore` - Logging structuré
- `Serilog.Sinks.File` - Logging vers fichiers
- `Microsoft.Extensions.Diagnostics.HealthChecks` - Health checks

### 🗑️ Supprimé
- `Microsoft.AspNetCore.RateLimiting` - Pas supporté dans cette version .NET

### 👤 Utilisateurs par défaut (mots de passe mis à jour)
- **admin** / Admin123! (Administrateur)
- **user** / User123! (Utilisateur standard)
- **manager** / Manager123! (Gestionnaire)

### 🚀 Migration
Pour migrer depuis la version 1.x :
1. Tous les utilisateurs existants sont automatiquement migrés vers BCrypt
2. Les nouveaux mots de passe par défaut doivent être utilisés
3. Les endpoints d'administration nécessitent maintenant une authentification Admin
4. La configuration doit être mise à jour avec les nouveaux paramètres JWT et Security

---

## [1.0.0] - 2025-05-09

### Ajouté
- Version initiale de l'API de paiement
- Support SQLite
- Endpoints de base pour paiements et consultations
- Documentation Swagger
- Système d'authentification basique avec SHA256
