


class CreditCardFormatError extends Error {
    constructor(message) {
      super(message);
      Object.setPrototypeOf(this, CreditCardFormatError.prototype);
      this.name = "CreditCardFormatError";
      
    }
  }

module.exports = CreditCardFormatError 