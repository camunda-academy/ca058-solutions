using System;

using CamundaTraining.Exceptions;
using Microsoft.Extensions.Logging;

namespace CamundaTraining.Services
{
    public class CreditCardService
    {
        public void ChargeAmount(string cardNumber, string cvc, string expiryDate, double amount)
        {
            if (string.IsNullOrEmpty(cardNumber) || string.IsNullOrEmpty(cvc) || string.IsNullOrEmpty(expiryDate) || amount <= 0){
                Console.WriteLine("Invalid input: cardNumber, cvc, expiryDate and amount are mandatory");
            }
            else{
                    Console.WriteLine("Credit card number: " + cardNumber +", CVC: "+cvc+", Expiry date: " + expiryDate + ", Order total: " + amount);
            }           
        }
    }
}
