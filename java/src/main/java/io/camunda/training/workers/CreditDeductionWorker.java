package io.camunda.training.workers;

import io.camunda.training.services.CustomerService;
import io.camunda.zeebe.client.api.response.ActivatedJob;
import io.camunda.zeebe.client.api.worker.JobClient;
import io.camunda.zeebe.spring.client.annotation.JobWorker;
import io.camunda.zeebe.spring.client.annotation.Variable;
import java.util.HashMap;
import java.util.Map;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

@Component
public class CreditDeductionWorker {

  Logger LOGGER = LoggerFactory.getLogger(CreditDeductionWorker.class);

  @JobWorker(type = "credit-deduction", autoComplete = false)
  public void handleCreditDeduction(final JobClient jobClient, final ActivatedJob job,
    @Variable String customerId, @Variable double orderTotal) {
    LOGGER.info("Task definition type: " + job.getType());

    CustomerService creditService = new CustomerService();
    double customerCredit = creditService.getCustomerCredit(customerId);
    double openAmount = creditService.deductCredit(customerCredit, orderTotal);

    Map<String, Object> variables = new HashMap<>();
    variables.put("openAmount", openAmount);
    variables.put("customerCredit", customerCredit);

    jobClient.newCompleteCommand(job)
      .variables(variables)
      .send().exceptionally(throwable -> {
        throw new RuntimeException("Could not complete job " + job, throwable);
      });
  }
}
