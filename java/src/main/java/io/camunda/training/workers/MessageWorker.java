package io.camunda.training.workers;

import io.camunda.zeebe.client.ZeebeClient;
import io.camunda.zeebe.client.api.response.ActivatedJob;
import io.camunda.zeebe.client.api.worker.JobClient;
import io.camunda.zeebe.spring.client.annotation.ZeebeWorker;
import java.util.Map;
import java.util.Random;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

@Component
public class MessageWorker {

  Logger LOGGER = LoggerFactory.getLogger(MessageWorker.class);

  @Autowired
  private ZeebeClient zeebeClient;

  @ZeebeWorker(type = "payment-invocation")
  public void handlePaymentInvocation(final JobClient jobClient, final ActivatedJob job) {
    LOGGER.info("Task definition type: " + job.getType());

    Map<String, Object> variables = job.getVariablesAsMap();
    String orderId = generateRandomOrderId(6);
    variables.put("orderId", orderId);

    zeebeClient.newPublishMessageCommand()
        .messageName("paymentRequestMessage")
        .correlationKey(orderId)
        .variables(variables)
        .send().join();

    jobClient.newCompleteCommand(job).variables(variables).send().join();
  }

  @ZeebeWorker(type = "payment-completion")
  public void handlePaymentCompletion(final JobClient jobClient, final ActivatedJob job) {
    LOGGER.info("Task definition type: " + job.getType());

    Map<String, Object> variables = job.getVariablesAsMap();

    zeebeClient.newPublishMessageCommand()
        .messageName("paymentCompletedMessage")
        .correlationKey(variables.get("orderId").toString())
        .send().join();

    jobClient.newCompleteCommand(job).send().join();
  }

  // Generates a random order ID with a given length, consisting of letters and/or digits
  private String generateRandomOrderId(int length) {
    var stringBuilder = new StringBuilder();
    var random = new Random();

    for (int i = 0; i < length; ++i) {
      boolean appendChar = random.nextBoolean();

      if (appendChar) {
        stringBuilder.append((char) ('A' + random.nextInt(26)));
      } else {
        stringBuilder.append(random.nextInt(9));
      }
    }

    return stringBuilder.toString();
  }
}
