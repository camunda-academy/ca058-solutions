package io.camunda.training.workers;

import io.camunda.training.exceptions.InvalidCreditCardException;
import io.camunda.training.services.CreditCardService;
import io.camunda.zeebe.client.ZeebeClient;
import io.camunda.zeebe.client.api.response.ActivatedJob;
import io.camunda.zeebe.client.api.worker.JobClient;
import io.camunda.zeebe.spring.client.annotation.ZeebeWorker;
import java.util.Map;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

@Component
public class CreditCardChargingWorker {

  Logger LOGGER = LoggerFactory.getLogger(CreditCardChargingWorker.class);

  @Autowired
  private ZeebeClient zeebeClient;

  @ZeebeWorker(type = "credit-card-charging")
  public void handleCreditCardCharging(final JobClient jobClient, final ActivatedJob job) {
    LOGGER.info("Task definition type: " + job.getType());

    Map<String, Object> variables = job.getVariablesAsMap();
    String cardNumber = variables.get("cardNumber").toString();
    String cvc = variables.get("cvc").toString();
    String expiryDate = variables.get("expiryDate").toString();
    Double amount = Double.valueOf(variables.get("openAmount").toString());

    try {
      new CreditCardService().chargeAmount(cardNumber, cvc, expiryDate, amount);

      jobClient.newCompleteCommand(job).send().join();
    } catch (InvalidCreditCardException e) {
      zeebeClient.newThrowErrorCommand(job).errorCode("creditCardChargeError").send().join();
    } catch (Exception e) {
      jobClient.newFailCommand(job).retries(3).errorMessage(e.getMessage()).send().join();
    }
  }
}
