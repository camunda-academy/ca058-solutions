package io.camunda.training.workers;

import io.camunda.training.exceptions.InvalidCreditCardException;
import io.camunda.training.services.CreditCardService;
import io.camunda.zeebe.client.api.response.ActivatedJob;
import io.camunda.zeebe.client.api.worker.JobClient;
import io.camunda.zeebe.spring.client.annotation.JobWorker;
import io.camunda.zeebe.spring.client.annotation.Variable;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

@Component
public class CreditCardChargingWorker {

  Logger LOGGER = LoggerFactory.getLogger(CreditCardChargingWorker.class);

  private final CreditCardService creditCardService;

  public CreditCardChargingWorker(CreditCardService creditCardService) {
    this.creditCardService = creditCardService;
  }

  @JobWorker(type = "credit-card-charging", autoComplete = false)
  public void handleCreditCardCharging(final JobClient jobClient, final ActivatedJob job,
    @Variable String cardNumber, @Variable String cvc, @Variable String expiryDate,
    @Variable Double openAmount) {
    LOGGER.info("Task definition type: " + job.getType());

    try {
      creditCardService.chargeAmount(cardNumber, cvc, expiryDate, openAmount);

      jobClient.newCompleteCommand(job)
        .send().exceptionally(throwable -> {
          throw new RuntimeException("Could not complete job " + job, throwable);
        });
    } catch (InvalidCreditCardException e) {
      jobClient.newThrowErrorCommand(job)
        .errorCode("invalidExpiryDateError")
        .errorMessage("Invalid expiry date: " + expiryDate)
        .send().exceptionally(throwable -> {
          throw new RuntimeException("Could not throw BPMN error during job " + job, throwable);
        });;
    } catch (Exception e) {
      jobClient.newFailCommand(job)
        .retries(job.getRetries() - 1)
        .errorMessage("An error has occured: " + e.getMessage())
        .send().exceptionally(throwable -> {
          throw new RuntimeException("Could not fail job " + job, throwable);
        });;;
    }
  }
}