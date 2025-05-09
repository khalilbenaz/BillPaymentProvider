using System.Collections.Generic;

namespace BillPaymentProvider.Utils
{
    /// <summary>
    /// Fournit des messages localisés pour les statuts et erreurs (fr, en, ar)
    /// </summary>
    public static class LocalizationHelper
    {
        private static readonly Dictionary<string, Dictionary<string, string>> Messages = new()
        {
            // Clé = code, valeur = dico langue:message
            ["000"] = new() {
                ["fr"] = "Opération réussie",
                ["en"] = "Operation successful",
                ["ar"] = "تمت العملية بنجاح"
            },
            ["101"] = new() {
                ["fr"] = "Paramètre manquant",
                ["en"] = "Missing parameter",
                ["ar"] = "معلمة مفقودة"
            },
            ["102"] = new() {
                ["fr"] = "Paramètre invalide",
                ["en"] = "Invalid parameter",
                ["ar"] = "معلمة غير صالحة"
            },
            ["103"] = new() {
                ["fr"] = "Montant invalide ou manquant",
                ["en"] = "Invalid or missing amount",
                ["ar"] = "المبلغ غير صالح أو مفقود"
            },
            ["104"] = new() {
                ["fr"] = "Référence client invalide",
                ["en"] = "Invalid customer reference",
                ["ar"] = "مرجع العميل غير صالح"
            },
            ["105"] = new() {
                ["fr"] = "Numéro de téléphone invalide",
                ["en"] = "Invalid phone number",
                ["ar"] = "رقم الهاتف غير صالح"
            },
            ["200"] = new() {
                ["fr"] = "Créancier introuvable",
                ["en"] = "Biller not found",
                ["ar"] = "لم يتم العثور على المفوتر"
            },
            ["201"] = new() {
                ["fr"] = "Service indisponible",
                ["en"] = "Service unavailable",
                ["ar"] = "الخدمة غير متوفرة"
            },
            ["202"] = new() {
                ["fr"] = "Facture introuvable",
                ["en"] = "Bill not found",
                ["ar"] = "لم يتم العثور على الفاتورة"
            },
            ["203"] = new() {
                ["fr"] = "Déjà payée",
                ["en"] = "Already paid",
                ["ar"] = "تم الدفع بالفعل"
            },
            ["204"] = new() {
                ["fr"] = "Fonds insuffisants",
                ["en"] = "Insufficient funds",
                ["ar"] = "رصيد غير كافٍ"
            },
            ["205"] = new() {
                ["fr"] = "Transaction introuvable",
                ["en"] = "Transaction not found",
                ["ar"] = "لم يتم العثور على المعاملة"
            },
            ["206"] = new() {
                ["fr"] = "Impossible d'annuler la transaction",
                ["en"] = "Cannot cancel transaction",
                ["ar"] = "لا يمكن إلغاء المعاملة"
            },
            ["500"] = new() {
                ["fr"] = "Erreur système",
                ["en"] = "System error",
                ["ar"] = "خطأ في النظام"
            },
            ["501"] = new() {
                ["fr"] = "Erreur base de données",
                ["en"] = "Database error",
                ["ar"] = "خطأ في قاعدة البيانات"
            },
            ["502"] = new() {
                ["fr"] = "Délai dépassé",
                ["en"] = "Timeout",
                ["ar"] = "انتهت المهلة"
            },
            ["503"] = new() {
                ["fr"] = "Erreur service externe",
                ["en"] = "External service error",
                ["ar"] = "خطأ في الخدمة الخارجية"
            },
        };

        public static string GetMessage(string code, string? lang)
        {
            lang = (lang ?? "fr").ToLower();
            if (Messages.TryGetValue(code, out var dict))
            {
                if (dict.TryGetValue(lang, out var msg))
                    return msg;
                if (dict.TryGetValue("fr", out var fallback))
                    return fallback;
            }
            return code;
        }
    }
}
