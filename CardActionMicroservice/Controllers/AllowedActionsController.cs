using CardActionMicroservice.Business.Services;
using CardActionMicroservice.DataProviders;
using CardActionMicroservice.Models;
using CardActionMicroservice.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CardActionMicroservice.Controllers
{
    using FluentValidation;
    using Microsoft.AspNetCore.Mvc;

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
