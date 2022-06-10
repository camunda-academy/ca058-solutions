
const service_customer = require('../services/service-customer');

const createWorker = (zbc) => {
		zbc.createWorker('credit-deduction', (job) => {
			console.log('--- Executing worker credit-deduction ---');
			console.log('Input variables');
			console.log(job.variables);
			const customerId = job.variables.customerId;
			const orderTotal = job.variables.orderTotal;
			var customerCredit = service_customer.getCustomerCredit(customerId);
			var openAmount = service_customer.deductCredit(customerCredit, orderTotal);
			var variables = {"customerCredit":customerCredit, "openAmount":openAmount};
			console.log('Output variables');
			console.log(variables);
			job.complete(variables);		
		})
}		

module.exports = { createWorker };
	

