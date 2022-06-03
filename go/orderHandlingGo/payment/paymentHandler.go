package payment

import (
	"context"
	"errors"
	"fmt"
	"github.com/camunda/zeebe/clients/go/v8/pkg/entities"
	"github.com/camunda/zeebe/clients/go/v8/pkg/worker"
	"github.com/camunda/zeebe/clients/go/v8/pkg/zbc"
	"reflect"
	"strconv"
	"strings"
	"unicode"
)

var zbcClient zbc.Client

func StartPaymentHandler(client zbc.Client) {
	zbcClient = client
	client.NewJobWorker().JobType("credit-deduction").Handler(chargeCredit).Open()
	client.NewJobWorker().JobType("credit-card-charging").Handler(chargeCreditCard).Open()
	client.NewJobWorker().JobType("payment-completion").Handler(completePayment).Open()
}

func completePayment(client worker.JobClient, job entities.Job) {
	//print information about job
	fmt.Println("Complete Payment ...")
	fmt.Println("Job Type: " + job.GetType())
	jobKey := strconv.FormatInt(job.GetKey(), 10)
	fmt.Println("Job Key: " + jobKey)

	//get variables
	vars, _ := job.GetVariablesAsMap()
	orderId := reflect.ValueOf(vars["orderId"]).Convert(reflect.TypeOf(string(""))).String()

	ctx := context.Background()

	//start payment
	messageRequest, _ := zbcClient.NewPublishMessageCommand().MessageName("paymentCompletedMessage").CorrelationKey(orderId).VariablesFromMap(vars)
	messageRequest.Send(ctx)

	// complete job
	request, _ := client.NewCompleteJobCommand().JobKey(job.GetKey()).VariablesFromMap(vars)
	request.Send(ctx)
}

func chargeCredit(client worker.JobClient, job entities.Job) {
	//print information about job
	fmt.Println("Charge Credit ...")
	fmt.Println("Job Type: " + job.GetType())
	jobKey := strconv.FormatInt(job.GetKey(), 10)
	fmt.Println("Job Key: " + jobKey)

	//get variables
	vars, _ := job.GetVariablesAsMap()
	customerId := reflect.ValueOf(vars["customerId"]).Convert(reflect.TypeOf(string(""))).String()
	orderTotal := reflect.ValueOf(vars["orderTotal"]).Convert(reflect.TypeOf(float64(0))).Float()

	// deduct credit
	customerCredit := getCustomerCredit(customerId)
	openAmount := deductCredit(customerCredit, orderTotal)

	// set variables
	vars["customerCredit"] = customerCredit
	vars["openAmount"] = openAmount

	// complete job
	request, _ := client.NewCompleteJobCommand().JobKey(job.GetKey()).VariablesFromMap(vars)
	ctx := context.Background()
	request.Send(ctx)
}

type processVars struct {
	cardNumber string
}

func chargeCreditCard(client worker.JobClient, job entities.Job) {
	//print information about job
	fmt.Println("Charge Credit Card ...")
	fmt.Println("Job Type: " + job.GetType())
	jobKey := strconv.FormatInt(job.GetKey(), 10)
	fmt.Println("Job Key: " + jobKey)

	//get variables
	vars, _ := job.GetVariablesAsMap()

	cardNumber := reflect.ValueOf(vars["cardNumber"]).Convert(reflect.TypeOf(string(""))).String()
	cvc := reflect.ValueOf(vars["CVC"]).Convert(reflect.TypeOf(string(""))).String()
	expiryDate := reflect.ValueOf(vars["expiryDate"]).Convert(reflect.TypeOf(string(""))).String()
	amount := reflect.ValueOf(vars["openAmount"]).Convert(reflect.TypeOf(float64(0))).Float()

	err := chargeAmount(cardNumber, cvc, expiryDate, amount)
	if err != nil {
		// Error Handling
		fmt.Println("Invalid expiry date")
		errorCmd := zbcClient.NewThrowErrorCommand().JobKey(job.GetKey()).ErrorCode("creditCardChargeError")
		errorCmd = errorCmd.ErrorMessage("Payment failed - Invalid expiry date")
		errorCmd.Send(context.Background())

		//client.NewFailJobCommand().JobKey(job.GetKey()).Retries(0).ErrorMessage("Invalid expiry date").Send(context.Background())
	}

	client.NewCompleteJobCommand().JobKey(job.GetKey()).Send(context.Background())
}

func getCustomerCredit(customerId string) float64 {
	creditStr := strings.TrimFunc(customerId, func(r rune) bool { return !unicode.IsNumber(r) })
	credit, err := strconv.ParseFloat(creditStr, 64)
	if err != nil {
		panic(err)
	}
	return credit
}

func deductCredit(customerCredit, amountToDeduct float64) float64 {
	if customerCredit > amountToDeduct {
		return 0
	} else {
		return amountToDeduct - customerCredit
	}
}

func chargeAmount(cardNumber, cvc, expiryDate string, amount float64) error {

	if len(expiryDate) != 5 {
		return errors.New("invalid expiry date")
	}

	fmt.Println("CVC: " + cvc)
	fmt.Println("Expiry Date: " + expiryDate)
	fmt.Println("Card Number: " + cardNumber)
	fmt.Println("Amount: " + strconv.FormatFloat(amount, 'f', 2, 64))
	return nil
}
