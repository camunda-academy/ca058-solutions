const service_credit_card = require('../services/service-credit-card');
const CreditCardFormatError = require('../errors/credit-card-format-error');

const createWorker = (zbc) => {
    zbc.createWorker('credit-card-charging', (job) => {
        console.log('--- Executing worker credit-card-charging ---');
        console.log('Input variables');
        console.log(job.variables);
        const cardNumber = job.variables.cardNumber;
        const cvc = job.variables.cvc;
        const expiryDate = job.variables.expiryDate;
        const openAmount = job.variables.openAmount;
        try{
            service_credit_card.chargeAmount(cardNumber,  cvc,  expiryDate,  openAmount)
            job.complete()
        }
        catch(err){
            if(err instanceof CreditCardFormatError){
                console.log(err.message);                
                job.error("creditCardChargeError",err.message)
            }
            else{
                console.log(err.message);
                job.fail(`critical failure: ${err.message}`, job.retries-1);
            }
        }
 
        
    })// handler)
}		

module.exports = { createWorker };