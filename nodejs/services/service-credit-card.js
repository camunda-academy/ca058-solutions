
const CreditCardFormatError  = require('../errors/credit-card-format-error');


const chargeAmount = (cardNumber,  cvc,  expiryDate,  amount) => {
    if(cardNumber.length==5){
        console.log(`Customer charged - Credit card number: ${cardNumber}, CVC: ${cvc}, Expiry date: ${expiryDate}, Amount charged for the order: ${amount}`);
    }
    else{
        throw new CreditCardFormatError(`The credit card's expiry date is invalid: ${expiryDate}`)
    }
}

module.exports = { chargeAmount };