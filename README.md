# API de Paiement de Factures et Recharges Télécom

Une API complète de simulation de paiements de factures et de recharges télécom, utilisant SQLite comme stockage de données.

## 📋 Sommaire

* [Installation](#-installation)
* [Fonctionnalités](#-fonctionnalités)
* [Données techniques](#-données-techniques)
* [Structure de l'API](#-structure-de-lapi)
* [Exemples d'utilisation](#-exemples-dutilisation)
* [Créanciers disponibles](#-créanciers-disponibles)
* [Notes importantes](#-notes-importantes)
* [Development](#-development)

## 🚀 Installation

### Prérequis

* .NET 8.0 SDK ou supérieur
* Un éditeur de code (Visual Studio, VS Code, Rider...)

### Configuration

1. Clonez le repository

```bash
git clone https://github.com/khalilbenaz/BillPaymentProvider.git
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

* **Paiement de factures** pour divers services (électricité, eau, gaz, téléphone...)
* **Recharges télécom** pour différents opérateurs
* **Support multi-facturier** configurable via base de données
* **Consultation de factures** multiples pour un client
* **Paiement multiple** en une seule transaction
* **Idempotence** des transactions via SessionId
* **Historique** consultable des transactions
* **Annulation** de transactions
* **Simulation d'erreurs** pour tester la robustesse des applications clientes

## 📊 Données techniques

* **Base URL** : `http://localhost:5163/`
* **API Version** : v1
* **Format** : JSON
* **Stockage** : SQLite
* **Swagger** : Documentation complète et tests via l'interface Swagger

## 🧩 Structure de l'API

### Point d'entrée unique

Toutes les opérations principales passent par un point d'entrée unique :

```
POST /api/Payment/process
```

La différenciation des opérations se fait via le paramètre `Operation` dans `ParamIn` :

* `INQUIRE` : Consultation d'une facture ou vérification d'un numéro
* `INQUIRE_MULTIPLE` : Consultation de plusieurs factures
* `PAY` : Paiement d'une facture ou recharge télécom
* `PAY_MULTIPLE` : Paiement de plusieurs factures
* `STATUS` : Vérification du statut d'une transaction
* `CANCEL` : Annulation d'une transaction

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

Voici des exemples détaillés des requêtes et réponses pour les principales opérations de l'API. Chaque exemple inclut une explication pour faciliter l'intégration.

### 1. Consultation d'une facture (INQUIRE)

Permet de récupérer les détails d'une facture spécifique.

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

**Explication**:
- Demande les détails d'une facture d'électricité pour le client `123456789`.
- `SessionId` assure l'idempotence.
- `BillerCode` identifie le créancier (ici, Électricité d'Égypte).

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

**Explication**:
- `StatusCode: "000"` indique un succès.
- `ParamOut` contient les détails de la facture (montant, échéance, etc.).
- Utilisé avant un paiement pour confirmer les informations.

### 2. Paiement d'une facture (PAY)

Effectue le paiement d'une facture spécifique.

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

**Explication**:
- Paie une facture d'électricité de 150.75.
- `Amount` doit correspondre au montant obtenu via INQUIRE.
- Nouveau `SessionId` pour cette transaction.

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

**Explication**:
- Confirme le succès du paiement.
- `TransactionId` et `ReceiptNumber` sont générés pour référence.
- `Status: "COMPLETED"` indique que le paiement est finalisé.

### 3. Consultation de plusieurs factures (INQUIRE_MULTIPLE)

Récupère toutes les factures disponibles pour un client.

#### Requête

```json
POST /api/Inquiry/inquire-multiple
{
  "BillerCode": "EGY-ELECTRICITY",
  "CustomerReference": "123456789"
}
```

**Explication**:
- Requête simplifiée envoyée à un endpoint spécifique.
- Demande toutes les factures pour le client `123456789` chez Électricité d'Égypte.

#### Réponse

```json
{
  "SessionId": "00000000-0000-0000-0000-000000000000",
  "ServiceId": "inquiry_multiple_service",
  "StatusCode": "000",
  "StatusLabel": "Factures trouvées (3)",
  "ParamOut": {
    "BillerCode": "EGY-ELECTRICITY",
    "CustomerReference": "123456789",
    "BillCount": 3,
    "Bills": [
      {
        "BillerCode": "EGY-ELECTRICITY",
        "BillerName": "Électricité d'Égypte",
        "CustomerReference": "123456789",
        "CustomerName": "Ahmed Mohamed",
        "DueAmount": 150.75,
        "DueDate": "2025-05-20",
        "BillPeriod": "Apr 2025 - May 2025",
        "BillNumber": "INV202505150001",
        "BillType": "Current",
        "BillIndex": 0
      },
      {
        "BillerCode": "EGY-ELECTRICITY",
        "BillerName": "Électricité d'Égypte",
        "CustomerReference": "123456789",
        "CustomerName": "Ahmed Mohamed",
        "DueAmount": 123.45,
        "DueDate": "2025-04-15",
        "BillPeriod": "Mar 2025",
        "BillNumber": "INV202504120035",
        "BillType": "Past",
        "BillIndex": 1
      }
    ]
  }
}
```

**Explication**:
- Retourne un tableau de factures dans `Bills`.
- `BillType` distingue les factures actuelles (`Current`) et passées (`Past`).
- `BillCount` indique le nombre total de factures.

### 4. Paiement de plusieurs factures (PAY_MULTIPLE)

Paie plusieurs factures en une seule transaction.

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

**Explication**:
- Paie deux factures pour différents créanciers et clients.
- `BillNumber` identifie chaque facture.
- `Payments` contient les détails de chaque paiement.

#### Réponse

```json
{
  "SessionId": "multiple-pay-12345678",
  "ServiceId": "bill_payment",
  "StatusCode": "000",
  "StatusLabel": "Paiements traités: 2 réussis, 0 échoués",
  "ParamOut": {
    "TotalPayments": 2,
    "SuccessCount": 2,
    "FailedCount": 0,
    "GlobalTransactionId": "abcdef1234567890",
    "Details": [
      {
        "StatusCode": "000",
        "StatusLabel": "Paiement Électricité d'Égypte effectué avec succès",
        "Details": {
        "TransactionId": "1122334455667788",
        "ReceiptNumber": "REC202505150010"
        }
      },
      {
        "StatusCode": "000",
        "StatusLabel": "Paiement Compagnie des Eaux d'Égypte effectué avec succès",
        "Details": {
        "TransactionId": "8877665544332211",
        "ReceiptNumber": "REC202505150011"
        }
      }
    ]
  }
}
```

**Explication**:
- Résumé global via `TotalPayments`, `SuccessCount`, et `FailedCount`.
- `GlobalTransactionId` identifie l'ensemble des paiements.
- `Details` fournit les résultats individuels de chaque paiement.

### 5. Validation d'un numéro de téléphone (INQUIRE pour recharge)

Vérifie un numéro de téléphone avant une recharge.

#### Requête

```json
POST /api/Payment/process
{
  "SessionId": "phone-check-12345678",
  "ServiceId": "telecom_recharge",
  "UserName": "test_user",
  "Password": "test_password",
  "Language": "fr",
  "ChannelId": "MOBILE",
  "IsDemo": 1,
  "ParamIn": {
    "Operation": "INQUIRE",
    "BillerCode": "EGY-ORANGE",
    "PhoneNumber": "0101234567"
  }
}
```

**Explication**:
- Valide un numéro pour une recharge Orange Égypte.
- Utilise `PhoneNumber` au lieu de `CustomerReference`.
- `BillerCode` désigne l'opérateur télécom.

#### Réponse

```json
{
  "SessionId": "phone-check-12345678",
  "ServiceId": "telecom_recharge",
  "StatusCode": "000",
  "StatusLabel": "Numéro validé",
  "ParamOut": {
    "BillerCode": "EGY-ORANGE",
    "BillerName": "Orange Égypte",
    "PhoneNumber": "0101234567",
    "AvailableAmounts": [10, 20, 50, 100, 200, 500],
    "MinAmount": 10,
    "MaxAmount": 500
  }
}
```

**Explication**:
- Confirme que le numéro est valide.
- `AvailableAmounts` liste les montants de recharge possibles.
- `MinAmount` et `MaxAmount` définissent les limites.

### 6. Recharge téléphonique (PAY pour recharge)

Effectue une recharge téléphonique.

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

**Explication**:
- Recharge le numéro `0101234567` avec 100.
- `Amount` doit être dans `AvailableAmounts` de l'INQUIRE.

#### Réponse

```json
{
  "SessionId": "phone-recharge-12345678",
  "ServiceId": "telecom_recharge",
  "StatusCode": "000",
  "StatusLabel": "Recharge Orange Égypte effectuée avec succès",
  "ParamOut": {
    "TransactionId": "orange1234567890",
    "ReceiptNumber": "REC202505150020",
    "PhoneNumber": "0101234567",
    "BillerCode": "EGY-ORANGE",
    "BillerName": "Orange Égypte",
    "Amount": 100,
    "RechargeDate": "2025-05-09 14:40:00",
    "Status": "COMPLETED"
  }
}
```

**Explication**:
- Confirme la recharge réussie.
- `TransactionId` et `ReceiptNumber` pour suivi.
- `RechargeDate` indique la date de traitement.

### 7. Vérification du statut d'une transaction (STATUS)

Vérifie l'état d'une transaction existante.

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

**Explication**:
- Vérifie le statut de la transaction `9876543210abcdef`.
- Utilisé pour confirmer le traitement d'une opération.

#### Réponse

```json
{
  "SessionId": "status-check-12345678",
  "ServiceId": "transaction_status",
  "StatusCode": "000",
  "StatusLabel": "Statut récupéré avec succès",
  "ParamOut": {
    "TransactionId": "9876543210abcdef",
    "Status": "COMPLETED",
    "BillerCode": "EGY-ELECTRICITY",
    "Amount": 150.75,
    "CreatedAt": "2025-05-09 12:30:00",
    "CompletedAt": "2025-05-09 12:30:00",
    "ReceiptNumber": "REC202505150001"
  }
}
```

**Explication**:
- Fournit le statut (`COMPLETED`) et les détails de la transaction.
- `CreatedAt` et `CompletedAt` indiquent les dates clés.

### 8. Annulation d'une transaction (CANCEL)

Tente d'annuler une transaction existante.

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

**Explication**:
- Demande l'annulation de la transaction `9876543210abcdef`.
- Possible uniquement dans les 24 heures.

#### Réponse (Succès)

```json
{
  "SessionId": "cancel-12345678",
  "ServiceId": "transaction_cancel",
  "StatusCode": "000",
  "StatusLabel": "Transaction annulée avec succès",
  "ParamOut": {
    "TransactionId": "9876543210abcdef",
    "Status": "CANCELLED",
    "BillerCode": "EGY-ELECTRICITY",
    "Amount": 150.75,
    "CancelledAt": "2025-05-09 14:45:00"
  }
}
```

**Explication**:
- Confirme l'annulation avec le statut `CANCELLED`.
- `CancelledAt` indique la date d'annulation.

#### Réponse (Échec)

```json
{
  "SessionId": "cancel-12345678",
  "ServiceId": "transaction_cancel",
  "StatusCode": "206",
  "StatusLabel": "Impossible d'annuler une transaction de plus de 24 heures",
  "ParamOut": {
    "ErrorMessage": "Impossible d'annuler une transaction de plus de 24 heures"
  }
}
```

**Explication**:
- Indique un échec avec `StatusCode: "206"`.
- `ErrorMessage` explique la raison.

### 9. Idempotence (requête dupliquée)

- Si une requête est envoyée plusieurs fois avec le même `SessionId`, le système retourne la même réponse sans exécuter l'opération à nouveau.
- **Point clé**: Assurez-vous que chaque nouvelle transaction utilise un `SessionId` unique.

### Points clés pour l'utilisation

1. **Validation préalable**: Utilisez `INQUIRE` avant `PAY` pour confirmer les montants et détails.
2. **Gestion des erreurs**: Vérifiez toujours `StatusCode` (`000` = succès, autre = erreur).
3. **`SessionId` vs `TransactionId`**:
   - `SessionId`: Fourni par le client pour l'idempotence.
   - `TransactionId`: Généré par le système pour identifier une transaction.

---

### Notes sur l'intégration

- Cette section inclut tous les cas d'utilisation principaux, avec des explications claires et des exemples complets.
- Les exemples respectent la structure standard de l'API décrite dans la section **Structure de l'API**.
- Pour tester, utilisez Swagger à `http://localhost:5163/` ou envoyez des requêtes via un client comme Postman.

## 🏢 Créanciers disponibles

### Factures

| Code            | Nom                         | Description                             |
| --------------- | --------------------------- | --------------------------------------- |
| EGY-ELECTRICITY | Électricité d'Égypte        | Paiement des factures d'électricité     |
| EGY-WATER       | Compagnie des Eaux d'Égypte | Paiement des factures d'eau             |
| EGY-GAS         | Gaz d'Égypte                | Paiement des factures de gaz            |
| EGY-TELECOM     | Télécom Égypte              | Paiement des factures de téléphone fixe |
| EGY-INTERNET    | Internet Égypte             | Paiement des factures internet          |

### Recharges télécom

| Code         | Nom             | Préfixes pris en charge |
| ------------ | --------------- | ----------------------- |
| EGY-ORANGE   | Orange Égypte   | 010, 012                |
| EGY-VODAFONE | Vodafone Égypte | 010, 011                |
| EGY-ETISALAT | Etisalat Égypte | 011, 015                |
| EGY-WE       | WE Égypte       | 015                     |

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
