package io.camunda.training.services;

public class CustomerService {

  public double getCustomerCredit(String customerId) {
    return Double.valueOf(customerId.substring(customerId.length() - 2));
  }

  public double deductCredit(double customerCredit, double amount) {
    return customerCredit > amount ? 0.0 : amount - customerCredit;
  }
}