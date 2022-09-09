import asyncio
from pyzeebe import ZeebeWorker, ZeebeClient, Job, create_camunda_cloud_channel

async def trigger_boundary_event(exception: Exception, job: Job):
    # set error status takes an error message and an error code. The corresponding BPMN error is thrown.
    await job.set_error_status(f"Failed to handle job {job}. Error: {str(exception)}", "creditCardChargeError")

def get_customer_credit(customerId):
    return int(customerId[-2:]) # get the last two characters of the customer ID and convert them to int

def deduct_credit(customerCredit, amountToDeduct):
    return max(0.0, amountToDeduct - customerCredit) # calculate the openAmount

# main function called by the program
async def main():
    # create a channel to Camunda 8 SaaS
    channel = create_camunda_cloud_channel(
            client_id= "",
            client_secret="",
            cluster_id= "",
            region="",
            ) # connect to your cloud instance
    
     # Create a worker object to register your workers
    worker = ZeebeWorker(channel)
    zeebe_client = ZeebeClient(channel)

    # Create a worker for task type "credit-deduction"
    @worker.task(task_type="credit-deduction")
    def deduct_amount(customerId, orderTotal) -> dict: # parameters are matched to process variables, i.e., customerCredit holds the value of the process variable 'customerCredit'. If no such variable exists, an exception occurs.
        customerCredit = get_customer_credit(customerId)
        openAmount = deduct_credit(customerCredit, orderTotal)
        return {"customerCredit": max(0.0, customerCredit - orderTotal), "openAmount": openAmount}
    
    # Create a worker for task type "credit-card-charging"
    @worker.task(task_type="credit-card-charging", exception_handler=trigger_boundary_event)
    def charge_card(openAmount, expiryDate) -> dict:
        if len(expiryDate)==5:
            print(f"Charging {openAmount} from credit card")
        else:
            print(f"Failed to charge credit card. Invalid expiration date {expiryDate}")
            # We raise an exception and delegate raising the BPMN error to the exception handler
            raise RuntimeError(f"Failed to charge credit card. Invalid expiration date {expiryDate}")
        return {}
        
    @worker.task(task_type="payment-completion")
    async def complete_payment(orderId):
        # Publishes a message. We need to add an await, otherwise python will complain
        await zeebe_client.publish_message(name="paymentCompletedMessage", correlation_key=orderId)
        
    @worker.task(task_type="payment-invocation")
    async def start_payment(job : Job): # We can work directly on the job object
        # Publish a message with an empy correlation key (start events do not support correlation keys)
        await zeebe_client.publish_message(name="paymentRequestMessage", correlation_key="", variables=job.variables) # include all variables in the message
        
            
    @worker.task(task_type="discount-application")
    async def calculate_discount(discount, orderTotal):
        discountedAmount = orderTotal - (orderTotal * discount / 100)
        return {"discountedAmount": discountedAmount}
    
    await worker.work() # start the workers


asyncio.run(main())