# Créanciers et services disponibles dans l'API

## 📋 Liste des créanciers supportés

Cette API simule les paiements pour différents types de créanciers et services. Voici la liste complète des services disponibles :

### ⚡ Électricité

| Service ID | Nom du service | Type | Zones |
|------------|----------------|------|-------|
| `ELEC_SENELEC` | SENELEC | Facture électricité | National |
| `ELEC_PREPAID` | SENELEC Prépayé | Recharge électricité | National |

### 💧 Eau

| Service ID | Nom du service | Type | Zones |
|------------|----------------|------|-------|
| `WATER_SDE` | SDE - Sénégalaise des Eaux | Facture eau | National |
| `WATER_PREPAID` | SDE Prépayé | Recharge eau | Urbain |

### 📞 Télécommunications

#### Opérateurs mobiles

| Service ID | Nom du service | Type | Montants |
|------------|----------------|------|----------|
| `MOBILE_ORANGE` | Orange Sénégal | Recharge mobile | 500, 1000, 2000, 5000, 10000 XOF |
| `MOBILE_FREE` | Free Sénégal | Recharge mobile | 500, 1000, 2000, 5000, 10000 XOF |
| `MOBILE_EXPRESSO` | Expresso | Recharge mobile | 500, 1000, 2000, 5000, 10000 XOF |

#### Internet et forfaits

| Service ID | Nom du service | Type | Description |
|------------|----------------|------|-------------|
| `INTERNET_ORANGE` | Orange Internet | Forfait internet | Forfaits data mensuel |
| `INTERNET_FREE` | Free Internet | Forfait internet | Forfaits data mensuel |
| `TV_CANAL` | Canal+ | Abonnement TV | Bouquets TV satellite |

### 🏦 Services financiers

| Service ID | Nom du service | Type | Description |
|------------|----------------|------|-------------|
| `BANK_TRANSFER` | Virement bancaire | Transfert | Entre comptes bancaires |
| `MOBILE_MONEY` | Mobile Money | Transfert | Orange Money, Free Money |

### 🏛️ Services publics

| Service ID | Nom du service | Type | Description |
|------------|----------------|------|-------------|
| `TAX_LOCAL` | Impôts locaux | Taxe | Taxes municipales |
| `TAX_VEHICLE` | Vignette véhicule | Taxe | Vignette automobile |
| `FINE_TRAFFIC` | Amendes circulation | Amende | Contraventions routières |

### 🎓 Éducation

| Service ID | Nom du service | Type | Description |
|------------|----------------|------|-------------|
| `SCHOOL_FEES` | Frais scolaires | Frais | Universités et écoles |
| `EXAM_FEES` | Frais d'examen | Frais | Concours et examens |

### 🏥 Santé

| Service ID | Nom du service | Type | Description |
|------------|----------------|------|-------------|
| `HEALTH_INSURANCE` | Assurance santé | Cotisation | Mutuelles de santé |
| `HOSPITAL_BILLS` | Frais hospitaliers | Facture | Hôpitaux publics et privés |

## 📊 Format des références client

Chaque type de service a son format de référence client spécifique :

### Électricité (SENELEC)
- **Format** : `ELEC-XXXXXXXXXX` (10 chiffres après ELEC-)
- **Exemple** : `ELEC-1234567890`

### Eau (SDE)
- **Format** : `WATER-XXXXXXXXX` (9 chiffres après WATER-)
- **Exemple** : `WATER-123456789`

### Mobile (Recharges)
- **Format** : `+221XXXXXXXXX` ou `77XXXXXXXX` (numéro de téléphone)
- **Exemple** : `+221771234567` ou `771234567`

### Services publics
- **Format** : `TAX-XXXXXXXXXX` (10 chiffres)
- **Exemple** : `TAX-1234567890`

## 💰 Montants et limites

### Montants standard de recharge mobile
```json
{
  "montants_disponibles": [500, 1000, 2000, 5000, 10000, 15000, 20000],
  "devise": "XOF",
  "minimum": 500,
  "maximum": 100000
}
```

### Limites par transaction
```json
{
  "limite_quotidienne": 500000,
  "limite_mensuelle": 2000000,
  "montant_minimum": 100,
  "montant_maximum": 100000
}
```

## 🔧 Configuration des créanciers

Les créanciers sont configurés dans la base de données SQLite via la table `BillerConfigurations` :

```sql
SELECT * FROM BillerConfigurations WHERE IsActive = 1;
```

### Exemple de configuration créancier

```json
{
  "ServiceId": "MOBILE_ORANGE",
  "ServiceName": "Orange Sénégal - Recharge",
  "BillerType": "Telecom",
  "IsActive": true,
  "MinAmount": 500,
  "MaxAmount": 100000,
  "Currency": "XOF",
  "ValidatorPattern": "^(\\+221)?[76][0-9]{8}$",
  "Description": "Recharge de crédit Orange Sénégal",
  "SupportedOperations": ["INQUIRE", "PAY", "STATUS"],
  "ProcessingTimeSeconds": 5,
  "SuccessRate": 95.5
}
```

## 🧪 Services de test

Pour faciliter les tests, des services de simulation sont disponibles :

### Service de test - Succès
- **Service ID** : `TEST_SUCCESS`
- **Comportement** : Toujours succès
- **Temps de traitement** : 1 seconde

### Service de test - Échec
- **Service ID** : `TEST_FAILURE`
- **Comportement** : Toujours échec
- **Code erreur** : `INSUFFICIENT_FUNDS`

### Service de test - Timeout
- **Service ID** : `TEST_TIMEOUT`
- **Comportement** : Timeout après 30 secondes
- **Code erreur** : `TIMEOUT`

## 📱 Exemples d'utilisation par service

### Recharge mobile Orange

```json
{
  "SessionId": "session-123",
  "ServiceId": "MOBILE_ORANGE",
  "UserName": "admin",
  "Operation": "PAY",
  "ParamIn": {
    "PhoneNumber": "771234567",
    "Amount": 1000,
    "CustomerReference": "Recharge crédit"
  }
}
```

### Paiement facture électricité

```json
{
  "SessionId": "session-456",
  "ServiceId": "ELEC_SENELEC",
  "UserName": "admin", 
  "Operation": "PAY",
  "ParamIn": {
    "BillNumber": "ELEC-1234567890",
    "Amount": 15000,
    "CustomerReference": "Facture octobre 2025"
  }
}
```

### Consultation facture eau

```json
{
  "SessionId": "session-789",
  "ServiceId": "WATER_SDE",
  "UserName": "admin",
  "Operation": "INQUIRE", 
  "ParamIn": {
    "BillNumber": "WATER-123456789",
    "CustomerReference": "Consultation facture"
  }
}
```

## 🔄 Mise à jour des créanciers

Pour ajouter un nouveau créancier, utiliser l'endpoint d'administration :

```bash
POST /api/admin/billers
Authorization: Bearer <admin-token>

{
  "ServiceId": "NEW_SERVICE",
  "ServiceName": "Nouveau Service",
  "BillerType": "Utility",
  "MinAmount": 1000,
  "MaxAmount": 50000,
  "Currency": "XOF",
  "IsActive": true
}
```

---

*Documentation mise à jour le 15 juin 2025*
