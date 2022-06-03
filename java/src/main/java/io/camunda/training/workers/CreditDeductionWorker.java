package io.camunda.training.workers;

import io.camunda.training.services.CustomerService;
import io.camunda.zeebe.client.api.response.ActivatedJob;
import io.camunda.zeebe.client.api.worker.JobClient;
import io.camunda.zeebe.spring.client.annotation.ZeebeWorker;
import java.util.Map;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.stereotype.Component;

@Component
public class CreditDeductionWorker {

  Logger LOGGER = LoggerFactory.getLogger(CreditDeductionWorker.class);

  @ZeebeWorker(type = "credit-deduction")
  public void handleCreditDeduction(final JobClient jobClient, final ActivatedJob job) {
    LOGGER.info("Task definition type: " + job.getType());

    Map<String, Object> variables = job.getVariablesAsMap();
    String customerId = variables.get("customerId").toString();
    Double orderTotal = Double.valueOf(variables.get("orderTotal").toString());

    CustomerService creditService = new CustomerService();
    double customerCredit = creditService.getCustomerCredit(customerId);
    double openAmount = creditService.deductCredit(customerCredit, orderTotal);

    variables.put("customerCredit", customerCredit);
    variables.put("openAmount", openAmount);

    jobClient.newCompleteCommand(job).variables(variables).send().join();
  }
}
