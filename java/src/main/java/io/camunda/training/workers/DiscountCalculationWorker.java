package io.camunda.training.workers;

import io.camunda.training.services.DiscountService;
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
public class DiscountCalculationWorker {

  Logger LOGGER = LoggerFactory.getLogger(DiscountCalculationWorker.class);

  @Autowired
  private ZeebeClient zeebeClient;

  @ZeebeWorker(type = "discount-application")
  public void handleDiscountCalculation(final JobClient jobClient, final ActivatedJob job) {
    LOGGER.info("Task definition type: " + job.getType());

    Map<String, Object> variables = job.getVariablesAsMap();
    Double orderTotal = Double.valueOf(variables.get("orderTotal").toString());
    Double discount = Double.valueOf(variables.get("discount").toString());

    double discountedAmount = new DiscountService().getDiscountedOrderTotal(orderTotal, discount);
    variables.put("discountedAmount", discountedAmount);

    jobClient.newCompleteCommand(job).variables(variables).send().join();
  }
}