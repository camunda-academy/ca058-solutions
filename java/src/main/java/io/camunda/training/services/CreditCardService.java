package io.camunda.training.services;

import io.camunda.training.exceptions.InvalidCreditCardException;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

@Component
public class CreditCardService {

  Logger LOGGER = LoggerFactory.getLogger(CreditCardService.class);

  public void chargeAmount(String cardNumber, String cvc, String expiryDate, Double amount)
    throws InvalidCreditCardException {
    if (expiryDate.length() == 5) {
      LOGGER.info(
        "Credit card number: " + cardNumber + ", CVC: " + cvc + ", Expiry date: " + expiryDate
          + ", Order total: " + amount);
    } else {
      LOGGER.error("The credit card's expiry date is invalid: " + expiryDate);

      throw new InvalidCreditCardException("Invalid credit card expiry date");
    }
  }
}