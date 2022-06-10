
const getDiscountedOrderTotal = (orderTotal, discount) => {
    return orderTotal - (orderTotal * discount / 100);
}

module.exports = { getDiscountedOrderTotal };