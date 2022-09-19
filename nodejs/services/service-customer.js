
const getCustomerCredit = customerId => {
    return generateRandomIntegerInRange(10,100);
}

const deductCredit = (customerCredit, amount) => {
    console.log(customerCredit);
    return customerCredit > parseFloat(amount) ? 0.0 : parseFloat(amount) - customerCredit;
}

const generateRandomIntegerInRange = (min, max) => {
    return Math.floor(Math.random() * (max - min + 1)) + min;
}
module.exports = { getCustomerCredit, deductCredit };