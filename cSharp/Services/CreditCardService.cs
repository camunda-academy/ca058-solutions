using System;

using CamundaTraining.Exceptions;
using Microsoft.Extensions.Logging;

namespace CamundaTraining.Services
{
    public class CreditCardService
    {
        private readonly ILogger<CreditCardService> _logger;

        public CreditCardService(ILogger<CreditCardService> logger)
        {
            _logger = logger;
        }

        public void ChargeAmount(string cardNumber, string cvc, string expiryDate, double amount)
        {
            if (expiryDate.Length == 5)
            {
                _logger.LogInformation($"Credit card number: {cardNumber}, CVC: {cvc}, Expiry date: {expiryDate}, Order total: {amount}");
            }
            else
            {
                _logger.LogError($"The credit card's expiry date is invalid: {expiryDate}");

                throw new InvalidCreditCardException("Invalid credit card expiry date");
            }
        }
    }
}
