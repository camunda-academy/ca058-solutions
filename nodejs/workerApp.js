const worker_deduct_credit = require('./workers/worker-deduct-credit');
const worker_credit_card_charging = require('./workers/worker-credit-card-charging');
const worker_discount = require('./workers/worker-discount');
var workerConnectionManager = require('./worker-connect');

workerConnectionManager.getConnection()
    .then(function(zbc){
        //print some information about the cluster
        workerConnectionManager.printTopology(zbc);

        //Execute workers
        worker_deduct_credit.createWorker(zbc);
        worker_credit_card_charging.createWorker(zbc);
        worker_discount.createWorker(zbc);
        //Close the connection when ctrl+c is pressed
        process.on('SIGINT', function() {
            console.log("Caught interrupt signal");        
            zbc.close().then(() => console.log('All workers closed'))
            process.exit();
        });   

    })



