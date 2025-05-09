# API de Paiement de Factures et Recharges Télécom

Une API complète de simulation de paiements de factures et de recharges télécom, utilisant SQLite comme stockage de données.

## 📋 Sommaire

- [Installation](#-installation)
- [Fonctionnalités](#-fonctionnalités)
- [Données techniques](#-données-techniques)
- [Structure de l'API](#-structure-de-lapi)
- [Exemples d'utilisation](#-exemples-dutilisation)
- [Créanciers disponibles](#-créanciers-disponibles)
- [Notes importantes](#-notes-importantes)
- [Development](#-development)

## 🚀 Installation

### Prérequis
- .NET 8.0 SDK ou supérieur
- Un éditeur de code (Visual Studio, VS Code, Rider...)

### Configuration
1. Clonez le repository
```bash
git clone https://github.com/votre-user/BillPaymentProvider.git
cd BillPaymentProvider
```

2. Restaurez les packages
```bash
dotnet restore
```

3. Lancez l'application
```bash
dotnet run
```

4. Accédez à Swagger pour explorer et tester l'API
```
http://localhost:5163/
```

## ✨ Fonctionnalités

- **Paiement de factures** pour divers services (électricité, eau, gaz, téléphone...)
- **Recharges télécom** pour différents opérateurs
- **Support multi-facturier** configurable via base de données
- **Consultation de factures** multiples pour un client
- **Paiement multiple** en une seule transaction
- **Idempotence** des transactions via SessionId
- **Historique** consultable des transactions
- **Annulation** de transactions
- **Simulation d'erreurs** pour tester la robustesse des applications clientes

## 📊 Données techniques

- **Base URL** : `http://localhost:5163/`
- **API Version** : v1
- **Format** : JSON
- **Stockage** : SQLite
- **Swagger** : Documentation complète et tests via l'interface Swagger

## 🧩 Structure de l'API

### Point d'entrée unique

Toutes les opérations principales passent par un point d'entrée unique :
```
POST /api/Payment/process
```

La différenciation des opérations se fait via le paramètre `Operation` dans `ParamIn` :
- `INQUIRE` : Consultation d'une facture ou vérification d'un numéro
- `INQUIRE_MULTIPLE` : Consultation de plusieurs factures
- `PAY` : Paiement d'une facture ou recharge télécom
- `PAY_MULTIPLE` : Paiement de plusieurs factures
- `STATUS` : Vérification du statut d'une transaction
- `CANCEL` : Annulation d'une transaction

### Structure de requête standard

```json
{
  "SessionId": "string",       // Identifiant unique de session (pour idempotence)
  "ServiceId": "string",       // Identifiant du service demandé
  "UserName": "string",        // Nom d'utilisateur pour authentification
  "Password": "string",        // Mot de passe pour authentification
  "Language": "string",        // Langue préférée pour les messages (fr, en, ar)
  "ChannelId": "string",       // Canal utilisé (WEB, MOBILE, CASH, etc.)
  "IsDemo": 0,                 // Mode démonstration (0=production, 1=démo)
  "ParamIn": {                 // Paramètres spécifiques à l'opération
    "Operation": "string",     // Type d'opération (INQUIRE, PAY, etc.)
    // Autres paramètres selon l'opération...
  }
}
```

### Structure de réponse standard

```json
{
  "SessionId": "string",       // Identifiant de session (même que requête)
  "ServiceId": "string",       // Identifiant de service (même que requête)
  "StatusCode": "string",      // Code de statut (000=succès)
  "StatusLabel": "string",     // Message décrivant le statut
  "ParamOut": {                // Résultat de l'opération
    // Dépend de l'opération...
  },
  "ParamOutJson": null         // Paramètres au format JSON (pour systèmes legacy)
}
```

## 📝 Exemples d'utilisation

### 1. Consultation d'une facture d'électricité

#### Requête
```json
POST /api/Payment/process
{
  "SessionId": "12345678-1234-1234-1234-123456789012",
  "ServiceId": "bill_payment",
  "UserName": "test_user",
  "Password": "test_password",
  "Language": "fr",
  "ChannelId": "WEB",
  "IsDemo": 1,
  "ParamIn": {
    "Operation": "INQUIRE",
    "BillerCode": "EGY-ELECTRICITY",
    "CustomerReference": "123456789"
  }
}
```

#### Réponse
```json
{
  "SessionId": "12345678-1234-1234-1234-123456789012",
  "ServiceId": "bill_payment",
  "StatusCode": "000",
  "StatusLabel": "Facture trouvée",
  "ParamOut": {
    "BillerCode": "EGY-ELECTRICITY",
    "BillerName": "Électricité d'Égypte",
    "CustomerReference": "123456789",
    "CustomerName": "Ahmed Mohamed",
    "DueAmount": 150.75,
    "DueDate": "2025-05-20",
    "BillPeriod": "Apr 2025 - May 2025",
    "BillNumber": "INV202505150001"
  }
}
```

### 2. Paiement d'une facture

#### Requête
```json
POST /api/Payment/process
{
  "SessionId": "87654321-4321-4321-4321-210987654321",
  "ServiceId": "bill_payment",
  "UserName": "test_user",
  "Password": "test_password",
  "Language": "fr",
  "ChannelId": "WEB",
  "IsDemo": 1,
  "ParamIn": {
    "Operation": "PAY",
    "BillerCode": "EGY-ELECTRICITY",
    "CustomerReference": "123456789",
    "Amount": 150.75
  }
}
```

#### Réponse
```json
{
  "SessionId": "87654321-4321-4321-4321-210987654321",
  "ServiceId": "bill_payment",
  "StatusCode": "000",
  "StatusLabel": "Paiement Électricité d'Égypte effectué avec succès",
  "ParamOut": {
    "TransactionId": "9876543210abcdef",
    "ReceiptNumber": "REC202505150001",
    "CustomerReference": "123456789",
    "BillerCode": "EGY-ELECTRICITY",
    "BillerName": "Électricité d'Égypte",
    "Amount": 150.75,
    "PaymentDate": "2025-05-09 14:30:00",
    "Status": "COMPLETED"
  }
}
```

### 3. Paiement de plusieurs factures

#### Requête
```json
POST /api/Payment/process
{
  "SessionId": "multiple-pay-12345678",
  "ServiceId": "bill_payment",
  "UserName": "test_user",
  "Password": "test_password",
  "Language": "fr",
  "ChannelId": "WEB",
  "IsDemo": 1,
  "ParamIn": {
    "Operation": "PAY_MULTIPLE",
    "Payments": [
      {
        "BillerCode": "EGY-ELECTRICITY",
        "CustomerReference": "123456789",
        "Amount": 150.75,
        "BillNumber": "INV202505150001"
      },
      {
        "BillerCode": "EGY-WATER",
        "CustomerReference": "AB123456",
        "Amount": 75.50,
        "BillNumber": "INV202505150002"
      }
    ]
  }
}
```

### 4. Recharge téléphonique

#### Requête
```json
POST /api/Payment/process
{
  "SessionId": "phone-recharge-12345678",
  "ServiceId": "telecom_recharge",
  "UserName": "test_user",
  "Password": "test_password",
  "Language": "fr",
  "ChannelId": "MOBILE",
  "IsDemo": 1,
  "ParamIn": {
    "Operation": "PAY",
    "BillerCode": "EGY-ORANGE",
    "PhoneNumber": "0101234567",
    "Amount": 100
  }
}
```

### 5. Vérification du statut d'une transaction

#### Requête
```json
POST /api/Payment/process
{
  "SessionId": "status-check-12345678",
  "ServiceId": "transaction_status",
  "UserName": "test_user",
  "Password": "test_password",
  "Language": "fr",
  "ChannelId": "WEB",
  "IsDemo": 1,
  "ParamIn": {
    "Operation": "STATUS",
    "TransactionId": "9876543210abcdef"
  }
}
```

### 6. Annulation d'une transaction

#### Requête
```json
POST /api/Payment/process
{
  "SessionId": "cancel-12345678",
  "ServiceId": "transaction_cancel",
  "UserName": "test_user",
  "Password": "test_password",
  "Language": "fr",
  "ChannelId": "WEB",
  "IsDemo": 1,
  "ParamIn": {
    "Operation": "CANCEL",
    "TransactionId": "9876543210abcdef"
  }
}
```

## 🏢 Créanciers disponibles

### Factures

| Code | Nom | Description |
|------|-----|-------------|
| EGY-ELECTRICITY | Électricité d'Égypte | Paiement des factures d'électricité |
| EGY-WATER | Compagnie des Eaux d'Égypte | Paiement des factures d'eau |
| EGY-GAS | Gaz d'Égypte | Paiement des factures de gaz |
| EGY-TELECOM | Télécom Égypte | Paiement des factures de téléphone fixe |
| EGY-INTERNET | Internet Égypte | Paiement des factures internet |

### Recharges télécom

| Code | Nom | Préfixes pris en charge |
|------|-----|-------------------------|
| EGY-ORANGE | Orange Égypte | 010, 012 |
| EGY-VODAFONE | Vodafone Égypte | 010, 011 |
| EGY-ETISALAT | Etisalat Égypte | 011, 015 |
| EGY-WE | WE Égypte | 015 |

## ⚠️ Notes importantes

1. **Idempotence** : Le système garantit l'idempotence des transactions via le `SessionId`. Si une requête est envoyée plusieurs fois avec le même `SessionId`, la transaction ne sera exécutée qu'une seule fois.

2. **Erreurs simulées** : Le système peut simuler des erreurs aléatoires pour tester la robustesse des applications clientes.

3. **Recharges télécom** : Pour les recharges télécom, le système valide le préfixe du numéro de téléphone en fonction de l'opérateur.

4. **Annulation** : Les transactions ne peuvent être annulées que dans les 24 heures suivant leur création.

## 🛠 Development

### Structure du projet

```
BillPaymentSimulatorEgyptApi/
│
├── Controllers/           - Points d'entrée API
├── Core/                  - Modèles et interfaces
├── Data/                  - Accès aux données (SQLite)
├── Infrastructure/        - Logging et utilitaires
├── Middleware/            - Gestion d'erreurs et idempotence
├── Providers/             - Provider générique pour tous types de paiement
├── Services/              - Services métier
└── Utils/                 - Classes utilitaires
```

### Ajout d'un nouveau créancier

Les créanciers sont configurés directement dans la base de données SQLite. Pour ajouter un nouveau créancier :

1. Utilisez l'API d'administration pour ajouter un créancier
```
POST /api/BillerConfig
```

2. Ou ajoutez directement dans la base SQLite
```sql
INSERT INTO BillerConfigurations (...) VALUES (...);
```

### Tests

Pour exécuter les tests unitaires :
```bash
dotnet test
```

Développé avec ❤️ pour simuler le système de paiement de factures et recharges télécom en Égypte.
