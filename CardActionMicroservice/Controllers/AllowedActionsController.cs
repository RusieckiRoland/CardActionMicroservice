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
