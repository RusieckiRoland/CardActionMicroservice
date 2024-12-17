
// --- AllCode.cs ---


// --- Program.cs ---
using CardActionMicroservice.Infrastructure;
using CardActionMicroservice.Models;
using CardActionMicroservice.Validators;
using CardActionService.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Text;

var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<CardDetailsValidator>();
builder.Services.AddFluentValidationAutoValidation();
var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "ActionsConfiguration.json");
var jsonContent = File.ReadAllText(configFilePath, Encoding.UTF8);
builder.Services.AddSingleton<IRuleLoader>(_ => new JsonRuleLoader(jsonContent));

//Automatic request validation - through binding
builder.Services.AddValidatorsFromAssemblyContaining<CardRequestValidator>();
//Provider data validation - controler body
builder.Services.AddScoped<IValidator<CardDetails>, CardDetailsValidator>();

builder.Services.AddBusinessServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();




// --- AllowedActionsService.cs ---
using CardActionMicroservice.Business.Strategies;
using CardActionMicroservice.Models;

namespace CardActionMicroservice.Business.Services
{
    public class AllowedActionsService
    {
        private readonly IEnumerable<IActionStrategy> _strategies;

        public AllowedActionsService(IEnumerable<IActionStrategy> strategies)
        {
            _strategies = strategies;
        }

        /// <summary>
        /// Determines the list of allowed actions for a card by applying a set of strategies.
        /// </summary>
        /// <param name="cardDetails">Details of the card including status, type, and pin state.</param>
        /// <returns>A list of allowed actions after filtering out blocked actions.</returns>


        public IEnumerable<string> GetAllowedActions(CardDetails cardDetails)
        {
            var blockedActions = _strategies
                .Where(strategy => strategy.IsApplicable(cardDetails))
                .SelectMany(strategy => strategy.GetBlockedActions(cardDetails))
                .Distinct();

            var allActions = Enumerable.Range(1, 13).Select(i => $"ACTION{i}");
            return allActions.Except(blockedActions);
        }
    }
}


// --- BusinessStrategiesFactory.cs ---
using CardActionMicroservice.Infrastructure;

namespace CardActionMicroservice.Business.Strategies
{
    public static class BusinessStrategiesFactory
    {
        /// <summary>
        /// Creates a list of action strategies based on rules loaded from the IRuleLoader.
        /// </summary>
        /// <param name="ruleLoader">The rule loader providing card type, status, and pin rules.</param>
        /// <returns>List of IActionStrategy implementations.</returns>

        public static List<IActionStrategy> CreateStrategies(IRuleLoader ruleLoader)
        {
            var (cardTypeRules, cardStatusRules, pinRules) = ruleLoader.LoadRules();

            return new List<IActionStrategy>
        {
            new CardTypeStrategy(cardTypeRules),
            new CardStatusStrategy(cardStatusRules),
            new PinStrategy(pinRules)
        };
        }
    }

}


// --- CardStatusStrategy.cs ---
using CardActionMicroservice.Models.Enums;
using CardActionMicroservice.Models;

namespace CardActionMicroservice.Business.Strategies
{
    public class CardStatusStrategy : IActionStrategy
    {
        private readonly Dictionary<CardStatus, List<string>> _cardStatusRules;

        public CardStatusStrategy(Dictionary<CardStatus, List<string>> cardStatusRules)
        {
            _cardStatusRules = cardStatusRules;
        }

        public bool IsApplicable(CardDetails cardDetails) 
            => _cardStatusRules.ContainsKey(cardDetails.CardStatus);

        public IEnumerable<string> GetBlockedActions(CardDetails cardDetails) 
            => _cardStatusRules[cardDetails.CardStatus];
    }
}


// --- CardTypeStrategy.cs ---
using CardActionMicroservice.Models;
using CardActionMicroservice.Models.Enums;

namespace CardActionMicroservice.Business.Strategies
{
    public class CardTypeStrategy : IActionStrategy
    {
        private readonly Dictionary<CardType, List<string>> _cardTypeRules;

        public CardTypeStrategy(Dictionary<CardType, List<string>> cardTypeRules)
        {
            _cardTypeRules = cardTypeRules;
        }

        public bool IsApplicable(CardDetails cardDetails) => 
            _cardTypeRules.ContainsKey(cardDetails.CardType);

       

        public IEnumerable<string> GetBlockedActions(CardDetails cardDetails) =>
            _cardTypeRules[cardDetails.CardType];
    }
}


// --- IActionStrategy.cs ---
using CardActionMicroservice.Models;

namespace CardActionMicroservice.Business.Strategies
{
    public interface IActionStrategy
    {
        bool IsApplicable(CardDetails cardDetails);
        IEnumerable<string> GetBlockedActions(CardDetails cardDetails);
    }
}


// --- PinStrategy.cs ---
using CardActionMicroservice.Models.Enums;
using CardActionMicroservice.Models;

namespace CardActionMicroservice.Business.Strategies
{
    public class PinStrategy : IActionStrategy
    {
        private readonly Dictionary<(bool IsPinSet, CardStatus), List<string>> _pinRules;

        public PinStrategy(Dictionary<(bool, CardStatus), List<string>> pinRules)
        {
            _pinRules = pinRules;
        }

        public bool IsApplicable(CardDetails cardDetails)
        {
            return _pinRules.ContainsKey((cardDetails.IsPinSet, cardDetails.CardStatus));
        }

        public IEnumerable<string> GetBlockedActions(CardDetails cardDetails)
      => _pinRules[(cardDetails.IsPinSet, cardDetails.CardStatus)];
    }
}


// --- AllowedActionsController.cs ---
using CardActionMicroservice.Business.Services;
using CardActionMicroservice.DataProviders;
using CardActionMicroservice.Models;

namespace CardActionMicroservice.Controllers
{
    using FluentValidation;
    using Microsoft.AspNetCore.Mvc;

    // <summary>
    /// Retrieves a list of allowed actions for a given card based on its details.
    /// </summary>
    /// <param name="request">
    /// The request object containing the user ID and card number.
    /// Both parameters are required for retrieving card details.
    /// </param>
    /// <returns>
    /// Returns an HTTP 200 OK response with the list of allowed actions if the request is valid.
    /// Returns HTTP 400 BadRequest if the input validation fails or the card details are invalid.
    /// Returns HTTP 404 NotFound if the specified card is not found for the user.
    /// </returns>
    /// <remarks>
    /// - This endpoint uses the POST method, as per the decision made in ADR-1, 
    ///   to ensure that sensitive card details are not exposed in URLs or logs.
    /// - POST was chosen over GET to prioritize data security.
    /// 
    /// Sample Request:
    /// POST /api/AllowedActions
    /// {
    ///     "UserId": "User1",
    ///     "CardNumber": "Card11"
    /// }
    /// 
    /// Sample Response:
    /// HTTP 200 OK
    /// {
    ///     "UserId": "User1",
    ///     "CardNumber": "Card11",
    ///     "AllowedActions": [ "ACTION1", "ACTION2", "ACTION3" ]
    /// }
    /// </remarks>

    [ApiController]
    [Route("api/[controller]")]
    public class AllowedActionsController : ControllerBase
    {
        private readonly ICardProvider _cardProvider;
        private readonly AllowedActionsService _allowedActionsService;
        private readonly IValidator<CardDetails> _cardDetailsValidator;

        public AllowedActionsController(
            ICardProvider cardProvider,
            AllowedActionsService allowedActionsService,
            IValidator<CardDetails> cardDetailsValidator)
        {
            _cardProvider = cardProvider;
            _allowedActionsService = allowedActionsService;
            _cardDetailsValidator = cardDetailsValidator;
        }

        [HttpPost]
        public async Task<IActionResult> GetAllowedActions([FromBody] CardRequest request)
        {
          
            var cardDetails = await _cardProvider.GetCardDetails(request.UserId, request.CardNumber);
            if (cardDetails == null)
            {
                return NotFound($"Card {request.CardNumber} for User {request.UserId} not found.");
            }

           
            var validationResult = await _cardDetailsValidator.ValidateAsync(cardDetails);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }

           
            var allowedActions = _allowedActionsService.GetAllowedActions(cardDetails);

            return Ok(new
            {
                UserId = request.UserId,
                CardNumber = request.CardNumber,
                AllowedActions = allowedActions
            });
        }
    }

}


// --- ICardProvider.cs ---
using CardActionMicroservice.Models;

namespace CardActionMicroservice.DataProviders
{
    public interface ICardProvider
    {

        /// <summary>
        /// Retrieves card details for a specified user and card number.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cardNumber">The number of the card.</param>
        /// <returns>
        /// The details of the card if found; otherwise, null.
        /// </returns>
        /// <remarks>Simulates a database call with Task.Delay for testing purposes.</remarks>
        Task<CardDetails?> GetCardDetails(string userId, string cardNumber);
    }
}


// --- InMemoryProvider.cs ---
using CardActionMicroservice.Models.Enums;
using CardActionMicroservice.Models;

namespace CardActionMicroservice.DataProviders
{
    public class InMemoryCardProvider : ICardProvider
    {
        private readonly Dictionary<string, Dictionary<string, CardDetails>> _userCards;

        public InMemoryCardProvider()
        {
            _userCards = CreateSampleUserCards();
        }

        public async Task<CardDetails?> GetCardDetails(string userId, string cardNumber)
        {
            // Symulacja opóźnienia (jak przy wywołaniu API lub bazy danych)
            await Task.Delay(100);

            if (_userCards.TryGetValue(userId, out var cards) &&
                cards.TryGetValue(cardNumber, out var cardDetails))
            {
                return cardDetails;
            }

            return null; 
        }

        private static Dictionary<string, Dictionary<string, CardDetails>> CreateSampleUserCards()
        {
            var userCards = new Dictionary<string, Dictionary<string, CardDetails>>();
            for (var i = 1; i <= 3; i++)
            {
                var cards = new Dictionary<string, CardDetails>();
                var cardIndex = 1;
                foreach (CardType cardType in Enum.GetValues(typeof(CardType)))
                {
                    foreach (CardStatus cardStatus in Enum.GetValues(typeof(CardStatus)))
                    {
                        var cardNumber = $"Card{i}{cardIndex}";
                        cards.Add(cardNumber,
                        new CardDetails(
                        CardNumber: cardNumber,
                        CardType: cardType,
                        CardStatus: cardStatus,
                        IsPinSet: cardIndex % 2 == 0));
                        cardIndex++;
                    }
                }
                var userId = $"User{i}";
                userCards.Add(userId, cards);
            }
            return userCards;
        }

    }

}


// --- ServiceCollectionExtensions.cs ---
using CardActionMicroservice.Business.Services;
using CardActionMicroservice.Business.Strategies;
using CardActionMicroservice.DataProviders;
using CardActionMicroservice.Infrastructure;

namespace CardActionService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBusinessServices(this IServiceCollection services)
        {
            
            services.AddSingleton(provider =>
            {
                var ruleLoader = provider.GetRequiredService<IRuleLoader>();              
                var strategies = BusinessStrategiesFactory.CreateStrategies(ruleLoader);

                return strategies;
            });
            
            services.AddSingleton<AllowedActionsService>();
            services.AddSingleton<ICardProvider, InMemoryCardProvider>();

            return services;
        }

    }
}





// --- IRuleLoader.cs ---
using CardActionMicroservice.Models.Enums;

namespace CardActionMicroservice.Infrastructure
{
    public interface IRuleLoader
    {
        (Dictionary<CardType, List<string>> cardTypeRules, Dictionary<CardStatus,
            List<string>> cardStatusRules, Dictionary<(bool, CardStatus), List<string>> pinRules) LoadRules();
    }
}


// --- JsonRuleLoader.cs ---
using CardActionMicroservice.Models.Enums;
using System.Text.Json;

namespace CardActionMicroservice.Infrastructure
{
        public class JsonRuleLoader : IRuleLoader
    {
        private readonly string _jsonContent;

        public JsonRuleLoader(string jsonContent)
        {
            _jsonContent = jsonContent;
        }

        /// <summary>
        /// Loads rules for card type, status, and pin from JSON content.
        /// </summary>
        /// <returns>
        /// Tuple containing:  
        /// - Card type rules.  
        /// - Card status rules.  
        /// - Pin rules (converted to a structured format).
        /// </returns>


        public (Dictionary<CardType, List<string>>, Dictionary<CardStatus, List<string>>, Dictionary<(bool, CardStatus), List<string>>)
        LoadRules()
        {
        
            var jsonConfig = JsonSerializer.Deserialize<JsonElement>(_jsonContent);

            var cardTypeRules = jsonConfig.GetProperty("CardTypeRules")
                .Deserialize<Dictionary<CardType, List<string>>>() ?? new Dictionary<CardType, List<string>>();

            var cardStatusRules = jsonConfig.GetProperty("CardStatusRules")
                .Deserialize<Dictionary<CardStatus, List<string>>>() ?? new Dictionary<CardStatus, List<string>>();

            var pinRules = jsonConfig.GetProperty("PinRules")
                .Deserialize<Dictionary<string, Dictionary<string, List<string>>>>() ?? new Dictionary<string, Dictionary<string, List<string>>>();

            var pinConvertedRules = new Dictionary<(bool, CardStatus), List<string>>();
            foreach (var pinRule in pinRules)
            {
                bool isPinSet = pinRule.Key == "WithPin";
                foreach (var statusRule in pinRule.Value)
                {
                    if (Enum.TryParse<CardStatus>(statusRule.Key, out var status))
                    {
                        pinConvertedRules[(isPinSet, status)] = statusRule.Value;
                    }
                }
            }

            return (cardTypeRules, cardStatusRules, pinConvertedRules);
        }
    }


}


// --- CardDetails.cs ---
using CardActionMicroservice.Models.Enums;


namespace CardActionMicroservice.Models
{
    public record CardDetails(string CardNumber, CardType CardType, CardStatus CardStatus, bool IsPinSet);
}


// --- CardRequest.cs ---
namespace CardActionMicroservice.Models
{
    public class CardRequest
    {
        public string UserId { get; set; }
        public string CardNumber { get; set; }
    }
}


// --- CardStatus.cs ---
namespace CardActionMicroservice.Models.Enums
{
    public enum CardStatus
    {
        Ordered,
        Inactive,
        Active,
        Restricted,
        Blocked,
        Expired,
        Closed
    }
}


// --- CardType.cs ---
namespace CardActionMicroservice.Models.Enums
{
    public enum CardType
    {
        Prepaid,
        Debit,
        Credit
    }
}


// --- .NETCoreApp,Version=v8.0.AssemblyAttributes.cs ---
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]


// --- CardActionMicroservice.AssemblyInfo.cs ---
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("CardActionMicroservice")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+3b08c89a025809428c7bb62d90f000912802a6ff")]
[assembly: System.Reflection.AssemblyProductAttribute("CardActionMicroservice")]
[assembly: System.Reflection.AssemblyTitleAttribute("CardActionMicroservice")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

// Generated by the MSBuild WriteCodeFragment class.



// --- CardActionMicroservice.GlobalUsings.g.cs ---
// <auto-generated/>
global using global::Microsoft.AspNetCore.Builder;
global using global::Microsoft.AspNetCore.Hosting;
global using global::Microsoft.AspNetCore.Http;
global using global::Microsoft.AspNetCore.Routing;
global using global::Microsoft.Extensions.Configuration;
global using global::Microsoft.Extensions.DependencyInjection;
global using global::Microsoft.Extensions.Hosting;
global using global::Microsoft.Extensions.Logging;
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Net.Http.Json;
global using global::System.Threading;
global using global::System.Threading.Tasks;


// --- CardActionMicroservice.MvcApplicationPartsAssemblyInfo.cs ---
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartAttribute("FluentValidation.AspNetCore")]
[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartAttribute("Swashbuckle.AspNetCore.SwaggerGen")]

// Generated by the MSBuild WriteCodeFragment class.



// --- CardDetailsValidator.cs ---
using CardActionMicroservice.Models;
using FluentValidation;

namespace CardActionMicroservice.Validators
{
    public class CardDetailsValidator : AbstractValidator<CardDetails>
    {
        public CardDetailsValidator()
        {
            RuleFor(card => card.CardNumber)
                .NotEmpty().WithMessage("Card number is required.");
            RuleFor(card => card.CardType)
                .IsInEnum().WithMessage("Invalid card type.");
            RuleFor(card => card.CardStatus)
                .IsInEnum().WithMessage("Invalid card status.");
            RuleFor(card => card.IsPinSet)
                .NotNull().WithMessage("Pin set status is required.");
        }
    }
}


// --- CardRequestValidator.cs ---
using CardActionMicroservice.Models;
using FluentValidation;

namespace CardActionMicroservice.Validators
{

    public class CardRequestValidator : AbstractValidator<CardRequest>
    {
        public CardRequestValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId cannot be empty.");

            RuleFor(x => x.CardNumber)
                .NotEmpty().WithMessage("CardNumber cannot be empty.");
        }
    }

}


// --- AllowedActionsServiceTests.cs ---
using CardActionMicroservice.Business.Services;
using CardActionMicroservice.Models.Enums;
using CardActionMicroservice.Models;
using Xunit;
using FluentAssertions;
using System.Text.Json;
using CardActionMicroservice.Infrastructure;
using CardActionMicroservice.Business.Strategies;


namespace CardActionMicroservice.Tests.Business.Services
{
    public class AllowedActionsServiceTests
    {
        [Fact]
        public void GetAllowedActions_ReturnsCorrectActions_BasedOnJsonStrategiesAndCards()
        {
            // Arrange
            var jsonContent = GetJsonContentForFirstConfiguration();
            var ruleLoader = new JsonRuleLoader(jsonContent);
            var strategyList = BusinessStrategiesFactory.CreateStrategies(ruleLoader);

            var allowedActionService = new AllowedActionsService(strategyList);

            var cardDetails = new CardDetails("1234567890123456", CardType.Prepaid, CardStatus.Ordered, false);            

            // Act
            var allowedActions = allowedActionService.GetAllowedActions(cardDetails);



            // Assert
            var allActions = JsonSerializer.Deserialize<JsonElement>(jsonContent)
                                   .GetProperty("AllActions")
                                   .Deserialize<List<string>>();
            //Na podstawie tabeli
            var blockedActions = new List<string> { "ACTION1", "ACTION2", "ACTION5", "ACTION6" };                       
          
            var expectedActions = allActions?.Except(blockedActions);

            allowedActions.Should().BeEquivalentTo(expectedActions);
        }

        private string GetJsonContentForFirstConfiguration()
        {
            return @"{
           ""AllActions"": [
             ""ACTION1"",
             ""ACTION2"",
             ""ACTION3"",
             ""ACTION4"",
             ""ACTION5"",
             ""ACTION6"",
             ""ACTION7"",
             ""ACTION8"",
             ""ACTION9"",
             ""ACTION10"",
             ""ACTION11"",
             ""ACTION12"",
             ""ACTION13""
           ],
           ""CardTypeRules"": {
               ""Prepaid"": [ ""ACTION5"" ],
               ""Debit"": [ ""ACTION6"" ],
               ""Credit"": []
           },
           ""CardStatusRules"": {
               ""Ordered"": [ ""ACTION1"", ""ACTION2"" ],
               ""Inactive"": [ ""ACTION1"" ],
               ""Active"": [ ""ACTION2"" ],
               ""Restricted"": [ ""ACTION1"", ""ACTION2"", ""ACTION6"", ""ACTION7"" ],
               ""Blocked"": [ ""ACTION6"", ""ACTION7"" ],
               ""Expired"": [ ""ACTION10"", ""ACTION11"", ""ACTION12"", ""ACTION13"" ],
               ""Closed"": [ ""ACTION10"", ""ACTION11"", ""ACTION12"", ""ACTION13"" ]
           },
           ""PinRules"": {
              ""NoPin"": {
                ""Ordered"": [ ""ACTION6"" ],
                ""Inactive"": [ ""ACTION6"" ],
                ""Active"": [ ""ACTION6"" ]
              },
              ""WithPin"": {
                ""Ordered"": [ ""ACTION7"" ],
                ""Blocked"": [ ""ACTION6"", ""ACTION7"" ]
             }
            }
           }";

        }
    }

}


// --- InMemoryProviderTests.cs ---
using CardActionMicroservice.DataProviders;
using Xunit;

namespace CardActionMicroservice.Tests.DataProvider
{
    public class InMemoryProviderTests
    {
        [Fact]
        public async Task GetCardDetails_Returns_Existing()
        {
            var provider = new InMemoryCardProvider();
            var details = await provider.GetCardDetails("User1", "Card11");

            Assert.NotNull(details);
        }



        [Theory]
        [InlineData("User1", "Card11", 0, 0, false)] // Prepaid, Ordered, NoPin
        [InlineData("User1", "Card14", 0, 3, true)]  // Prepaid, Restricted, WithPin
        [InlineData("User1", "Card18", 1, 0, true)]  // Debit, Ordered, WithPin
        [InlineData("User2", "Card219", 2, 4, false)]// Credit, Expired, NoPin
        [InlineData("User3", "Card317", 2, 2, false)]// Credit, Inactive, NoPin
        public async Task GetCardDetails_Returns_CorrectDetails(string user, string cardNumber, int expectedCardType, int expectedCardStatus, bool expectedIsPinSet)
        {
            // Arrange
            var provider = new InMemoryCardProvider();

            // Act
            var details = await provider.GetCardDetails(user, cardNumber);

            // Assert
            Assert.NotNull(details);
            Assert.Equal(cardNumber, details.CardNumber);
            Assert.Equal(expectedCardType, (int)details.CardType);
            Assert.Equal(expectedCardStatus, (int)details.CardStatus);
            Assert.Equal(expectedIsPinSet, details.IsPinSet);
        }
        [Fact]
        public async Task GetCardDetails_Returns_Null_ForEmpty()
        {
            var provider = new InMemoryCardProvider();
            var details = await provider.GetCardDetails("FakeUser", "Card11");

            Assert.Null(details);
        }
    }
}


// --- .NETCoreApp,Version=v8.0.AssemblyAttributes.cs ---
// <autogenerated />
using System;
using System.Reflection;
[assembly: global::System.Runtime.Versioning.TargetFrameworkAttribute(".NETCoreApp,Version=v8.0", FrameworkDisplayName = ".NET 8.0")]


// --- CardActionMicroservice.Tests.AssemblyInfo.cs ---
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Reflection;

[assembly: System.Reflection.AssemblyCompanyAttribute("CardActionMicroservice.Tests")]
[assembly: System.Reflection.AssemblyConfigurationAttribute("Debug")]
[assembly: System.Reflection.AssemblyFileVersionAttribute("1.0.0.0")]
[assembly: System.Reflection.AssemblyInformationalVersionAttribute("1.0.0+4c0b5615651dabcc3fbc765aff8e40d3e05da5ed")]
[assembly: System.Reflection.AssemblyProductAttribute("CardActionMicroservice.Tests")]
[assembly: System.Reflection.AssemblyTitleAttribute("CardActionMicroservice.Tests")]
[assembly: System.Reflection.AssemblyVersionAttribute("1.0.0.0")]

// Generated by the MSBuild WriteCodeFragment class.



// --- CardActionMicroservice.Tests.GlobalUsings.g.cs ---
// <auto-generated/>
global using global::System;
global using global::System.Collections.Generic;
global using global::System.IO;
global using global::System.Linq;
global using global::System.Net.Http;
global using global::System.Threading;
global using global::System.Threading.Tasks;

