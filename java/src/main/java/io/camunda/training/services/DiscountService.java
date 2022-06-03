package io.camunda.training.services;

public class DiscountService {

  public double getDiscountedOrderTotal(Double orderTotal, Double discount) {
    return orderTotal - (orderTotal * discount / 100);
  }
}
