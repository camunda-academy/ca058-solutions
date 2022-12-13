package io.camunda.training.workers;

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

  @JobWorker(type = "credit-card-charging", autoComplete = false)
  public void handleCreditCardCharging(final JobClient jobClient, final ActivatedJob job,
    @Variable String cardNumber, @Variable String cvc, @Variable String expiryDate,
    @Variable Double openAmount) {
    LOGGER.info("Task definition type: " + job.getType());

    new CreditCardService().chargeAmount(cardNumber, cvc, expiryDate, openAmount);

    jobClient.newCompleteCommand(job)
      .send().exceptionally(throwable -> {
        throw new RuntimeException("Could not complete job " + job, throwable);
      });
  }
}