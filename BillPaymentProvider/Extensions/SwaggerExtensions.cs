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
        }

        /// <summary>
        /// Configure Swagger UI
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
                    {
                        ["Paiement d'électricité"] = new OpenApiExample
                        {
                            Summary = "Paiement de facture d'électricité",
                            Value = new OpenApiObject
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
                            }
                        },
                        ["Recharge télécom"] = new OpenApiExample
                        {
                            Summary = "Recharge téléphonique Orange",
                            Value = new OpenApiObject
                            {
                                ["SessionId"] = new OpenApiString("12345678-1234-1234-1234-123456789012"),
                                ["ServiceId"] = new OpenApiString("telecom_recharge"),
                                ["UserName"] = new OpenApiString("test_user"),
                                ["Password"] = new OpenApiString("test_password"),
                                ["Language"] = new OpenApiString("fr"),
                                ["ChannelId"] = new OpenApiString("MOBILE"),
                                ["IsDemo"] = new OpenApiInteger(1),
                                ["ParamIn"] = new OpenApiObject
                                {
                                    ["Operation"] = new OpenApiString("PAY"),
                                    ["BillerCode"] = new OpenApiString("EGY-ORANGE"),
                                    ["PhoneNumber"] = new OpenApiString("0101234567"),
                                    ["Amount"] = new OpenApiDouble(100)
                                }
                            }
                        },
                        ["Paiement multiple"] = new OpenApiExample
                        {
                            Summary = "Paiement de plusieurs factures",
                            Value = new OpenApiObject
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
                                    ["Operation"] = new OpenApiString("PAY_MULTIPLE"),
                                    ["Payments"] = new OpenApiArray
                                {
                                    new OpenApiObject
                                    {
                                        ["BillerCode"] = new OpenApiString("EGY-ELECTRICITY"),
                                        ["CustomerReference"] = new OpenApiString("123456789"),
                                        ["Amount"] = new OpenApiDouble(150.75),
                                        ["BillNumber"] = new OpenApiString("INV202505150001")
                                    },
                                    new OpenApiObject
                                    {
                                        ["BillerCode"] = new OpenApiString("EGY-WATER"),
                                        ["CustomerReference"] = new OpenApiString("AB123456"),
                                        ["Amount"] = new OpenApiDouble(75.50),
                                        ["BillNumber"] = new OpenApiString("INV202505150002")
                                    }
                                }
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
                {
                    // Exemple de requête pour la consultation
                    operation.RequestBody.Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                    {
                        ["Consultation facture"] = new OpenApiExample
                        {
                            Summary = "Consultation de facture d'électricité",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-ELECTRICITY"),
                                ["CustomerReference"] = new OpenApiString("123456789")
                            }
                        },
                        ["Validation téléphone"] = new OpenApiExample
                        {
                            Summary = "Validation de numéro Orange",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-ORANGE"),
                                ["PhoneNumber"] = new OpenApiString("0101234567")
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
                {
                    // Exemple de requête pour la consultation multiple
                    operation.RequestBody.Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                    {
                        ["Consultation factures multiples"] = new OpenApiExample
                        {
                            Summary = "Consultation de toutes les factures d'électricité d'un client",
                            Value = new OpenApiObject
                            {
                                ["BillerCode"] = new OpenApiString("EGY-ELECTRICITY"),
                                ["CustomerReference"] = new OpenApiString("123456789")
                            }
                        }
                    };
                }
            }
        }
    }
}
