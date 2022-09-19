
const service_discount = require('../services/service-discount');

const createWorker = (zbc) => {
		zbc.createWorker('discount-application', (job) => {
			console.log('Executing worker discount-application')
			console.log(job.variables)
			const orderTotal = parseFloat(job.variables.orderTotal);
			const discount = parseFloat(job.variables.discount);
            const discountAmounted = service_discount.getDiscountedOrderTotal(orderTotal, discount);
			var variables = {"discountedAmount":discountAmounted};
			job.complete(variables);
			
		})// handler)
}		

module.exports = { createWorker };