using BillPaymentProvider.Core.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace BillPaymentProvider.Extensions
{
    /// <summary>
    /// Extensions pour la configuration de Swagger
    /// </summary>
    public static class SwaggerExtensions
    {
        /// <summary>
        /// Configure Swagger pour l'application
        /// </summary>
        public static void ConfigureSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Egypt BillPayment API",
                    Version = "v1",
                    Description = "API pour le paiement de factures et recharges télécom en Égypte",
                    Contact = new OpenApiContact
                    {
                        Name = "Équipe BillPayment",
                        Email = "support@billpayment.eg"
                    }
                });

                // Ajout de la sécurité JWT dans Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Exemple: 'Bearer {token}'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                // Inclure les commentaires XML
                var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml");
                foreach (var xmlFile in xmlFiles)
                {
                    c.IncludeXmlComments(xmlFile);
                }

                // Ajouter les filtres pour les exemples
                c.SchemaFilter<SwaggerExampleSchemaFilter>();
                c.OperationFilter<SwaggerExampleOperationFilter>();
            });
        }

        /// <summary>
        /// Génère les commentaires XML pour les contrôleurs
        /// </summary>
        public static void GenerateXmlComments(this IServiceCollection services)
        {
            // Configurer la génération de fichiers XML pour les commentaires de documentation
            services.AddMvc()
                .AddJsonOptions(options => {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Garder les noms de propriétés tels quels
                });
        }        /// <summary>
        /// Configure Swagger UI avec personnalisation avancée
        /// </summary>
        public static void UseSwaggerWithUI(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Egypt BillPayment API v1");
                c.RoutePrefix = string.Empty; // Pour faire de Swagger la page d'accueil
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
                c.DefaultModelsExpandDepth(-1); // Cacher les modèles par défaut
                c.DisplayRequestDuration(); // Afficher la durée des requêtes
                c.EnableDeepLinking(); // Permettre les liens profonds
                c.DisplayOperationId(); // Afficher l'ID des opérations
                c.DefaultModelRendering(Swashbuckle.AspNetCore.SwaggerUI.ModelRendering.Example);
                c.EnableValidator(); // Activer le validateur JSON
                c.ShowExtensions(); // Montrer les extensions
                c.EnableFilter(); // Activer le filtre de recherche
                c.MaxDisplayedTags(50); // Nombre max de tags affichés
                  // Personnalisation CSS pour un meilleur look
                c.InjectStylesheet("/swagger-ui/custom.css");
                
                // Configuration simple pour éviter les problèmes de sérialisation
                c.ConfigObject.AdditionalItems.Add("syntaxHighlight", true);
                c.ConfigObject.AdditionalItems.Add("tryItOutEnabled", true);
            });
        }
    }

    /// <summary>
    /// Filtre pour ajouter des exemples de schémas dans Swagger
    /// </summary>
    public class SwaggerExampleSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(B3gServiceRequest))
            {
                // Exemple de requête de paiement de facture
                schema.Example = new OpenApiObject
                {
                    ["SessionId"] = new OpenApiString("12345678-1234-1234-1234-123456789012"),
                    ["ServiceId"] = new OpenApiString("bill_payment"),
                    ["UserName"] = new OpenApiString("test_user"),
                    ["Password"] = new OpenApiString("test_password"),
                    ["Language"] = new OpenApiString("fr"),
                    ["ChannelId"] = new OpenApiString("WEB"),
                    ["IsDemo"] = new OpenApiInteger(1),
                    ["ParamIn"] = new OpenApiObject
                    {
                        ["Operation"] = new OpenApiString("PAY"),
                        ["BillerCode"] = new OpenApiString("EGY-ELECTRICITY"),
                        ["CustomerReference"] = new OpenApiString("123456789"),
                        ["Amount"] = new OpenApiDouble(150.75)
                    }
                };
            }
            else if (context.Type == typeof(B3gServiceResponse))
            {
                // Exemple de réponse de paiement
                schema.Example = new OpenApiObject
                {
                    ["SessionId"] = new OpenApiString("12345678-1234-1234-1234-123456789012"),
                    ["ServiceId"] = new OpenApiString("bill_payment"),
                    ["StatusCode"] = new OpenApiString("000"),
                    ["StatusLabel"] = new OpenApiString("Paiement effectué avec succès"),
                    ["ParamOut"] = new OpenApiObject
                    {
                        ["TransactionId"] = new OpenApiString("9876543210abcdef"),
                        ["ReceiptNumber"] = new OpenApiString("REC202505150001"),
                        ["CustomerReference"] = new OpenApiString("123456789"),
                        ["BillerCode"] = new OpenApiString("EGY-ELECTRICITY"),
                        ["BillerName"] = new OpenApiString("Électricité d'Égypte"),
                        ["Amount"] = new OpenApiDouble(150.75),
                        ["PaymentDate"] = new OpenApiString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")),
                        ["Status"] = new OpenApiString("COMPLETED")
                    }
                };
            }
            else if (context.Type == typeof(Transaction))
            {
                // Exemple de transaction
                schema.Example = new OpenApiObject
                {
                    ["Id"] = new OpenApiInteger(1),
                    ["TransactionId"] = new OpenApiString("9876543210abcdef"),
                    ["BillerCode"] = new OpenApiString("EGY-ELECTRICITY"),
                    ["CustomerReference"] = new OpenApiString("123456789"),
                    ["Amount"] = new OpenApiDouble(150.75),
                    ["Status"] = new OpenApiString("COMPLETED"),
                    ["SessionId"] = new OpenApiString("12345678-1234-1234-1234-123456789012"),
                    ["ServiceId"] = new OpenApiString("bill_payment"),
                    ["ChannelId"] = new OpenApiString("WEB"),
                    ["ReceiptNumber"] = new OpenApiString("REC202505150001"),
                    ["CreatedAt"] = new OpenApiString(DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH:mm:ss")),
                    ["CompletedAt"] = new OpenApiString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                };
            }
            else if (context.Type == typeof(TransactionLog))
            {
                // Exemple de log de transaction
                schema.Example = new OpenApiObject
                {
                    ["Id"] = new OpenApiInteger(1),
                    ["TransactionId"] = new OpenApiString("9876543210abcdef"),
                    ["Action"] = new OpenApiString("PAYMENT"),
                    ["Details"] = new OpenApiString("Paiement EGY-ELECTRICITY pour 123456789 montant 150.75"),
                    ["NewStatus"] = new OpenApiString("COMPLETED"),
                    ["Timestamp"] = new OpenApiString(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                };
            }
            else if (context.Type == typeof(BillerConfiguration))
            {
                // Exemple de configuration de créancier
                schema.Example = new OpenApiObject
                {
                    ["Id"] = new OpenApiInteger(1),
                    ["BillerCode"] = new OpenApiString("EGY-ELECTRICITY"),
                    ["BillerName"] = new OpenApiString("Électricité d'Égypte"),
                    ["Description"] = new OpenApiString("Paiement des factures d'électricité"),
                    ["Category"] = new OpenApiString("ELECTRICITY"),
                    ["ServiceType"] = new OpenApiString("BILL_PAYMENT"),
                    ["CustomerReferenceFormat"] = new OpenApiString("^[0-9]{8,12}$"),
                    ["SpecificParams"] = new OpenApiString("{\"fixedFee\": 1.5, \"paymentDays\": \"1-25\"}"),
                    ["SimulateRandomErrors"] = new OpenApiBoolean(true),
                    ["ErrorRate"] = new OpenApiInteger(5),
                    ["ProcessingDelay"] = new OpenApiInteger(500),
                    ["IsActive"] = new OpenApiBoolean(true),
                    ["CreatedAt"] = new OpenApiString(DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd HH:mm:ss"))
                };
            }
        }
    }

    /// <summary>
    /// Filtre pour ajouter des exemples d'opérations dans Swagger
    /// </summary>
    public class SwaggerExampleOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            // Ajouter des exemples spécifiques à certaines opérations
            if (context.MethodInfo.Name == "Process" && context.MethodInfo.DeclaringType?.Name == "PaymentController")
            {
                // S'assurer que RequestBody et Content existent
                if (operation.RequestBody?.Content != null &&
                    operation.RequestBody.Content.ContainsKey("application/json"))
                {
                    // Exemple de requête pour le paiement
                    operation.RequestBody.Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                    {                        ["Paiement d'électricité"] = new OpenApiExample
                        {
                            Summary = "💡 Paiement de facture d'électricité",
                            Description = "Exemple complet pour payer une facture d'électricité avec tous les paramètres requis",
                            Value = new OpenApiObject
                            {
                                ["SessionId"] = new OpenApiString("12345678-1234-1234-1234-123456789012"),
                                ["ServiceId"] = new OpenApiString("bill_payment"),
                                ["UserName"] = new OpenApiString("api_user_demo"),
                                ["Password"] = new OpenApiString("Demo123!"),
                                ["Language"] = new OpenApiString("fr"),
                                ["ChannelId"] = new OpenApiString("WEB"),
                                ["IsDemo"] = new OpenApiInteger(1),
                                ["ParamIn"] = new OpenApiObject
                                {
                                    ["Operation"] = new OpenApiString("PAY"),
                                    ["BillerCode"] = new OpenApiString("EGY-ELECTRICITY"),
                                    ["CustomerReference"] = new OpenApiString("123456789"),
                                    ["Amount"] = new OpenApiDouble(150.75),
                                    ["BillNumber"] = new OpenApiString("ELEC-2025-001234"),
                                    ["PaymentReference"] = new OpenApiString("PAY-" + DateTime.Now.ToString("yyyyMMddHHmmss"))
                                }
                            }
                        },
                        ["Recharge télécom Orange"] = new OpenApiExample
                        {
                            Summary = "📱 Recharge téléphonique Orange",
                            Description = "Recharge de crédit téléphonique pour un numéro Orange Égypte",
                            Value = new OpenApiObject
                            {
                                ["SessionId"] = new OpenApiString("12345678-1234-1234-1234-123456789012"),
                                ["ServiceId"] = new OpenApiString("telecom_recharge"),
                                ["UserName"] = new OpenApiString("api_user_demo"),
                                ["Password"] = new OpenApiString("Demo123!"),
                                ["Language"] = new OpenApiString("fr"),
                                ["ChannelId"] = new OpenApiString("MOBILE"),
                                ["IsDemo"] = new OpenApiInteger(1),
                                ["ParamIn"] = new OpenApiObject
                                {
                                    ["Operation"] = new OpenApiString("PAY"),
                                    ["BillerCode"] = new OpenApiString("EGY-ORANGE"),
                                    ["PhoneNumber"] = new OpenApiString("0101234567"),
                                    ["Amount"] = new OpenApiDouble(100),
                                    ["RechargeType"] = new OpenApiString("CREDIT"),
                                    ["PaymentReference"] = new OpenApiString("ORANGE-" + DateTime.Now.ToString("yyyyMMddHHmmss"))
                                }
                            }
                        },
                        ["Recharge télécom Vodafone"] = new OpenApiExample
                        {
                            Summary = "📱 Recharge téléphonique Vodafone",
                            Description = "Recharge de crédit téléphonique pour un numéro Vodafone Égypte",
                            Value = new OpenApiObject
                            {
                                ["SessionId"] = new OpenApiString("12345678-1234-1234-1234-123456789012"),
                                ["ServiceId"] = new OpenApiString("telecom_recharge"),
                                ["UserName"] = new OpenApiString("api_user_demo"),
                                ["Password"] = new OpenApiString("Demo123!"),
                                ["Language"] = new OpenApiString("fr"),
                                ["ChannelId"] = new OpenApiString("MOBILE"),
                                ["IsDemo"] = new OpenApiInteger(1),
                                ["ParamIn"] = new OpenApiObject
                                {
                                    ["Operation"] = new OpenApiString("PAY"),
                                    ["BillerCode"] = new OpenApiString("EGY-VODAFONE"),
                                    ["PhoneNumber"] = new OpenApiString("0121234567"),
                                    ["Amount"] = new OpenApiDouble(50),
                                    ["RechargeType"] = new OpenApiString("CREDIT"),
                                    ["PaymentReference"] = new OpenApiString("VODAFONE-" + DateTime.Now.ToString("yyyyMMddHHmmss"))
                                }
                            }
                        },
                        ["Paiement facture d'eau"] = new OpenApiExample
                        {
                            Summary = "💧 Paiement de facture d'eau",
                            Description = "Paiement d'une facture du service des eaux égyptien",
                            Value = new OpenApiObject
                            {
                                ["SessionId"] = new OpenApiString("12345678-1234-1234-1234-123456789012"),
                                ["ServiceId"] = new OpenApiString("bill_payment"),
                                ["UserName"] = new OpenApiString("api_user_demo"),
                                ["Password"] = new OpenApiString("Demo123!"),
                                ["Language"] = new OpenApiString("fr"),
                                ["ChannelId"] = new OpenApiString("WEB"),
                                ["IsDemo"] = new OpenApiInteger(1),
                                ["ParamIn"] = new OpenApiObject
                                {
                                    ["Operation"] = new OpenApiString("PAY"),
                                    ["BillerCode"] = new OpenApiString("EGY-WATER"),
                                    ["CustomerReference"] = new OpenApiString("AB123456"),
                                    ["Amount"] = new OpenApiDouble(75.50),
                                    ["BillNumber"] = new OpenApiString("WATER-2025-005678"),
                                    ["PaymentReference"] = new OpenApiString("WATER-" + DateTime.Now.ToString("yyyyMMddHHmmss"))
                                }
                            }
                        },
                        ["Paiement facture de gaz"] = new OpenApiExample
                        {
                            Summary = "🔥 Paiement de facture de gaz",
                            Description = "Paiement d'une facture de gaz naturel égyptien",
                            Value = new OpenApiObject
                            {
                                ["SessionId"] = new OpenApiString("12345678-1234-1234-1234-123456789012"),
                                ["ServiceId"] = new OpenApiString("bill_payment"),
                                ["UserName"] = new OpenApiString("api_user_demo"),
                                ["Password"] = new OpenApiString("Demo123!"),
                                ["Language"] = new OpenApiString("fr"),
                                ["ChannelId"] = new OpenApiString("WEB"),
                                ["IsDemo"] = new OpenApiInteger(1),
                                ["ParamIn"] = new OpenApiObject
                                {
                                    ["Operation"] = new OpenApiString("PAY"),
                                    ["BillerCode"] = new OpenApiString("EGY-GAS"),
                                    ["CustomerReference"] = new OpenApiString("GAS789012"),
                                    ["Amount"] = new OpenApiDouble(89.25),
                                    ["BillNumber"] = new OpenApiString("GAS-2025-009876"),
                                    ["PaymentReference"] = new OpenApiString("GAS-" + DateTime.Now.ToString("yyyyMMddHHmmss"))
                                }
                            }
                        },                        ["Paiement multiple"] = new OpenApiExample
                        {
                            Summary = "📊 Paiement de plusieurs factures",
                            Description = "Exemple de paiement groupé pour électricité, eau et gaz en une seule transaction",
                            Value = new OpenApiObject
                            {
                                ["SessionId"] = new OpenApiString("12345678-1234-1234-1234-123456789012"),
                                ["ServiceId"] = new OpenApiString("bill_payment_multiple"),
                                ["UserName"] = new OpenApiString("api_user_demo"),
                                ["Password"] = new OpenApiString("Demo123!"),
                                ["Language"] = new OpenApiString("fr"),
                                ["ChannelId"] = new OpenApiString("WEB"),
                                ["IsDemo"] = new OpenApiInteger(1),
                                ["ParamIn"] = new OpenApiObject
                                {
                                    ["Operation"] = new OpenApiString("PAY_MULTIPLE"),
                                    ["Payments"] = new OpenApiArray
                                {
                                    new OpenApiObject
                                    {
                                        ["BillerCode"] = new OpenApiString("EGY-ELECTRICITY"),
                                        ["CustomerReference"] = new OpenApiString("123456789"),
                                        ["Amount"] = new OpenApiDouble(150.75),
                                        ["BillNumber"] = new OpenApiString("ELEC-2025-001234"),
                                        ["PaymentReference"] = new OpenApiString("MULTI-ELEC-" + DateTime.Now.ToString("yyyyMMddHHmmss"))
                                    },
                                    new OpenApiObject
                                    {
                                        ["BillerCode"] = new OpenApiString("EGY-WATER"),
                                        ["CustomerReference"] = new OpenApiString("AB123456"),
                                        ["Amount"] = new OpenApiDouble(75.50),
                                        ["BillNumber"] = new OpenApiString("WATER-2025-005678"),
                                        ["PaymentReference"] = new OpenApiString("MULTI-WATER-" + DateTime.Now.ToString("yyyyMMddHHmmss"))
                                    },
                                    new OpenApiObject
                                    {
                                        ["BillerCode"] = new OpenApiString("EGY-GAS"),
                                        ["CustomerReference"] = new OpenApiString("GAS789012"),
                                        ["Amount"] = new OpenApiDouble(89.25),
                                        ["BillNumber"] = new OpenApiString("GAS-2025-009876"),
                                        ["PaymentReference"] = new OpenApiString("MULTI-GAS-" + DateTime.Now.ToString("yyyyMMddHHmmss"))
                                    }
                                },
                                    ["TotalAmount"] = new OpenApiDouble(315.50),
                                    ["PaymentReference"] = new OpenApiString("MULTI-PAY-" + DateTime.Now.ToString("yyyyMMddHHmmss"))
                                }
                            }
                        }
                    };
                }
            }
            else if (context.MethodInfo.Name == "Inquire" && context.MethodInfo.DeclaringType?.Name == "InquiryController")
            {
                // S'assurer que RequestBody et Content existent
                if (operation.RequestBody?.Content != null &&
                    operation.RequestBody.Content.ContainsKey("application/json"))
                {                    // Exemple de requête pour la consultation
                    operation.RequestBody.Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                    {
                        ["Consultation facture d'électricité"] = new OpenApiExample
                        {
                            Summary = "💡 Consultation de facture d'électricité",
                            Description = "Vérifier le montant et les détails d'une facture d'électricité avant paiement",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-ELECTRICITY"),
                                ["CustomerReference"] = new OpenApiString("123456789")
                            }
                        },
                        ["Consultation facture d'eau"] = new OpenApiExample
                        {
                            Summary = "💧 Consultation de facture d'eau",
                            Description = "Vérifier le montant et les détails d'une facture d'eau avant paiement",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-WATER"),
                                ["CustomerReference"] = new OpenApiString("AB123456")
                            }
                        },
                        ["Consultation facture de gaz"] = new OpenApiExample
                        {
                            Summary = "🔥 Consultation de facture de gaz",
                            Description = "Vérifier le montant et les détails d'une facture de gaz avant paiement",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-GAS"),
                                ["CustomerReference"] = new OpenApiString("GAS789012")
                            }
                        },
                        ["Validation numéro Orange"] = new OpenApiExample
                        {
                            Summary = "📱 Validation de numéro Orange",
                            Description = "Vérifier qu'un numéro de téléphone Orange est valide et actif",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-ORANGE"),
                                ["PhoneNumber"] = new OpenApiString("0101234567")
                            }
                        },
                        ["Validation numéro Vodafone"] = new OpenApiExample
                        {
                            Summary = "📱 Validation de numéro Vodafone",
                            Description = "Vérifier qu'un numéro de téléphone Vodafone est valide et actif",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-VODAFONE"),
                                ["PhoneNumber"] = new OpenApiString("0121234567")
                            }
                        },
                        ["Validation numéro Etisalat"] = new OpenApiExample
                        {
                            Summary = "📱 Validation de numéro Etisalat",
                            Description = "Vérifier qu'un numéro de téléphone Etisalat est valide et actif",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-ETISALAT"),
                                ["PhoneNumber"] = new OpenApiString("0111234567")
                            }
                        }
                    };
                }
            }
            else if (context.MethodInfo.Name == "InquireMultiple" && context.MethodInfo.DeclaringType?.Name == "InquiryController")
            {
                // S'assurer que RequestBody et Content existent
                if (operation.RequestBody?.Content != null &&
                    operation.RequestBody.Content.ContainsKey("application/json"))
                {                    // Exemple de requête pour la consultation multiple
                    operation.RequestBody.Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                    {
                        ["Consultation factures multiples électricité"] = new OpenApiExample
                        {
                            Summary = "💡 Consultation de toutes les factures d'électricité d'un client",
                            Description = "Obtenir la liste de toutes les factures impayées d'un client pour l'électricité",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-ELECTRICITY"),
                                ["CustomerReference"] = new OpenApiString("123456789")
                            }
                        },
                        ["Consultation factures multiples eau"] = new OpenApiExample
                        {
                            Summary = "💧 Consultation de toutes les factures d'eau d'un client",
                            Description = "Obtenir la liste de toutes les factures impayées d'un client pour l'eau",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-WATER"),
                                ["CustomerReference"] = new OpenApiString("AB123456")
                            }
                        },
                        ["Consultation factures multiples gaz"] = new OpenApiExample
                        {
                            Summary = "🔥 Consultation de toutes les factures de gaz d'un client",
                            Description = "Obtenir la liste de toutes les factures impayées d'un client pour le gaz",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-GAS"),
                                ["CustomerReference"] = new OpenApiString("GAS789012")
                            }
                        }
                    };
                }
            }
            // Ajouter des exemples pour l'authentification
            else if (context.MethodInfo.Name == "Login" && context.MethodInfo.DeclaringType?.Name == "AuthController")
            {
                if (operation.RequestBody?.Content != null &&
                    operation.RequestBody.Content.ContainsKey("application/json"))
                {
                    operation.RequestBody.Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                    {
                        ["Connexion utilisateur standard"] = new OpenApiExample
                        {
                            Summary = "🔐 Connexion utilisateur API",
                            Description = "Authentification avec un compte utilisateur standard",
                            Value = new OpenApiObject
                            {
                                ["Username"] = new OpenApiString("api_user_demo"),
                                ["Password"] = new OpenApiString("Demo123!"),
                                ["RememberMe"] = new OpenApiBoolean(false)
                            }
                        },
                        ["Connexion administrateur"] = new OpenApiExample
                        {
                            Summary = "🔐 Connexion administrateur",
                            Description = "Authentification avec un compte administrateur",
                            Value = new OpenApiObject
                            {
                                ["Username"] = new OpenApiString("admin"),
                                ["Password"] = new OpenApiString("Admin123!"),
                                ["RememberMe"] = new OpenApiBoolean(true)
                            }
                        }
                    };
                }
            }
            
            // Ajouter des exemples pour la configuration des créanciers
            else if (context.MethodInfo.Name == "CreateBiller" && context.MethodInfo.DeclaringType?.Name == "BillerConfigController")
            {
                if (operation.RequestBody?.Content != null &&
                    operation.RequestBody.Content.ContainsKey("application/json"))
                {
                    operation.RequestBody.Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                    {
                        ["Nouveau créancier électricité"] = new OpenApiExample
                        {
                            Summary = "💡 Créer un créancier d'électricité",
                            Description = "Configuration d'un nouveau fournisseur d'électricité",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-ELEC-REGION-01"),
                                ["BillerName"] = new OpenApiString("Électricité Région du Caire"),
                                ["Description"] = new OpenApiString("Service d'électricité pour la région du Grand Caire"),
                                ["Category"] = new OpenApiString("ELECTRICITY"),
                                ["ServiceType"] = new OpenApiString("BILL_PAYMENT"),
                                ["CustomerReferenceFormat"] = new OpenApiString("^[0-9]{10}$"),
                                ["SpecificParams"] = new OpenApiString("{\"fixedFee\": 2.0, \"paymentDays\": \"1-28\", \"currency\": \"EGP\"}"),
                                ["SimulateRandomErrors"] = new OpenApiBoolean(false),
                                ["ErrorRate"] = new OpenApiInteger(0),
                                ["ProcessingDelay"] = new OpenApiInteger(300),
                                ["IsActive"] = new OpenApiBoolean(true)
                            }
                        },
                        ["Nouveau créancier télécom"] = new OpenApiExample
                        {
                            Summary = "📱 Créer un créancier télécom",
                            Description = "Configuration d'un nouveau fournisseur télécom",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-ETISALAT"),
                                ["BillerName"] = new OpenApiString("Etisalat Égypte"),
                                ["Description"] = new OpenApiString("Recharges et paiements Etisalat"),
                                ["Category"] = new OpenApiString("TELECOM"),
                                ["ServiceType"] = new OpenApiString("RECHARGE"),
                                ["CustomerReferenceFormat"] = new OpenApiString("^011[0-9]{8}$"),
                                ["SpecificParams"] = new OpenApiString("{\"minAmount\": 10, \"maxAmount\": 500, \"currency\": \"EGP\"}"),
                                ["SimulateRandomErrors"] = new OpenApiBoolean(true),
                                ["ErrorRate"] = new OpenApiInteger(3),
                                ["ProcessingDelay"] = new OpenApiInteger(200),
                                ["IsActive"] = new OpenApiBoolean(true)
                            }
                        }
                    };
                }
            }
            
            // Ajouter des exemples pour les recherches de transactions
            else if (context.MethodInfo.Name == "GetTransactions" && context.MethodInfo.DeclaringType?.Name == "TransactionController")
            {
                // Ajouter des exemples de paramètres de requête dans la description
                operation.Summary = "📊 Recherche et filtrage des transactions";
                operation.Description = @"
**Exemples de filtres disponibles :**

🔍 **Par date :** `?startDate=2025-06-01&endDate=2025-06-15`
💰 **Par montant :** `?minAmount=50&maxAmount=500`
🏢 **Par créancier :** `?billerCode=EGY-ELECTRICITY`
✅ **Par statut :** `?status=COMPLETED`
🆔 **Par référence client :** `?customerReference=123456789`
📱 **Par canal :** `?channelId=WEB`

**Exemples complets :**
- Toutes les transactions d'électricité du mois : `?billerCode=EGY-ELECTRICITY&startDate=2025-06-01&endDate=2025-06-30`
- Transactions échouées récentes : `?status=FAILED&startDate=2025-06-10`
- Gros montants sur mobile : `?minAmount=1000&channelId=MOBILE`
";
            }

            // Ajouter des tags et descriptions personnalisés
            if (operation.Tags?.Any() == true)
            {
                foreach (var tag in operation.Tags)
                {
                    switch (tag.Name)
                    {
                        case "Payment":
                            tag.Name = "💰 Paiements";
                            break;
                        case "Inquiry":
                            tag.Name = "🔍 Consultations";
                            break;
                        case "Auth":
                            tag.Name = "🔐 Authentification";
                            break;
                        case "Transaction":
                            tag.Name = "📊 Transactions";
                            break;
                        case "BillerConfig":
                            tag.Name = "⚙️ Configuration";
                            break;
                        case "Admin":
                            tag.Name = "👤 Administration";
                            break;
                    }
                }
            }
        }
    }
}
