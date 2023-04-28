namespace CamundaTraining.Services
{
    public class CustomerService
    {
        public double GetCustomerCredit(string customerId)
        {
            return double.Parse(customerId.Substring(customerId.Length - 2));
        }

        public double DeductCredit(double customerCredit, double amount)
        {
            return customerCredit > amount ? 0.0 : amount - customerCredit;
        }
    }
}