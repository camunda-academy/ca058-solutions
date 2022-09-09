import asyncio
from pyzeebe import ZeebeWorker, create_camunda_cloud_channel

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

    # Create a worker for task type "credit-deduction"
    @worker.task(task_type="credit-deduction")
    def deduct_amount(customerCredit, orderTotal) -> dict: # parameters are matched to process variables, i.e., customerCredit holds the value of the process variable 'customerCredit'. If no such variable exists, an exception occurs.
        return {"customerCredit": max(0.0, customerCredit - orderTotal), "openAmount": max(orderTotal - customerCredit, 0,0)}
    
    # Create a worker for task type "credit-card-charging"
    @worker.task(task_type="credit-card-charging")
    def charge_card(remainingAmount) -> dict:
        print(f"Charging {remainingAmount} from credit card")
    
    await worker.work() # start the workers


asyncio.run(main())