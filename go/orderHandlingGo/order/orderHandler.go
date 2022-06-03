package order

import (
	"context"
	"fmt"
	"github.com/camunda/zeebe/clients/go/v8/pkg/entities"
	"github.com/camunda/zeebe/clients/go/v8/pkg/worker"
	"github.com/camunda/zeebe/clients/go/v8/pkg/zbc"
	"github.com/segmentio/ksuid"
	"reflect"
	"strconv"
)

var zbcClient zbc.Client

func StartOrderHandler(client zbc.Client) {
	zbcClient = client
	client.NewJobWorker().JobType("payment-invocation").Handler(invokePayment).Open()
	client.NewJobWorker().JobType("discount-reduction").Handler(reduceDiscount).Open()
}

func invokePayment(client worker.JobClient, job entities.Job) {
	//print information about job
	fmt.Println("Invoke Payment ...")
	fmt.Println("Job Type: " + job.GetType())
	jobKey := strconv.FormatInt(job.GetKey(), 10)
	fmt.Println("Job Key: " + jobKey)

	//get variables
	vars, _ := job.GetVariablesAsMap()

	//generate OrderId
	orderId := ksuid.New().String()
	vars["orderId"] = orderId

	ctx := context.Background()

	//start payment
	messageRequest, _ := zbcClient.NewPublishMessageCommand().MessageName("paymentRequestMessage").CorrelationKey(orderId).VariablesFromMap(vars)
	messageRequest.Send(ctx)

	// complete job
	request, _ := client.NewCompleteJobCommand().JobKey(job.GetKey()).VariablesFromMap(vars)
	request.Send(ctx)
}

func reduceDiscount(client worker.JobClient, job entities.Job) {
	//print information about job
	fmt.Println("Reduce Discount ...")
	fmt.Println("Job Type: " + job.GetType())
	jobKey := strconv.FormatInt(job.GetKey(), 10)
	fmt.Println("Job Key: " + jobKey)

	//get variables
	vars, _ := job.GetVariablesAsMap()
	orderTotal := reflect.ValueOf(vars["discount"]).Convert(reflect.TypeOf(float64(0))).Float()
	discount := reflect.ValueOf(vars["discount"]).Convert(reflect.TypeOf(float64(0))).Float()

	vars["orderTotal"] = orderTotal * (100 - discount) / 100

	// complete job
	request, _ := client.NewCompleteJobCommand().JobKey(job.GetKey()).VariablesFromMap(vars)
	request.Send(context.Background())
}
