package main

import (
	"github.com/camunda/zeebe/clients/go/v8/pkg/zbc"
	"orderHandlingGo/order"
	Payment "orderHandlingGo/payment"
	"os"
)

var readyClose = make(chan int)

func main() {

	client, err := zbc.NewClient(&zbc.ClientConfig{
		GatewayAddress: os.Getenv("ZEEBE_ADDRESS"),
	})
	if err != nil {
		panic(err)
	}

	Payment.StartPaymentHandler(client)
	order.StartOrderHandler(client)

	<-readyClose
}
