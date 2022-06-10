var PropertiesReader = require('properties-reader');
var properties = PropertiesReader('config/application.properties');
const ZB = require('zeebe-node');
const { Duration } = require('zeebe-node');

const clientId = properties.get("ZEEBE_CLIENT_ID");
const clientSecret = properties.get("ZEEBE_CLIENT_SECRET");
const clusterId = properties.get("ZEEBE_CLUSTER_ID");
const clusterRegion = properties.get("ZEEBE_CLUSTER_REGION");

const getConnection = async () => {
    return new ZB.ZBClient({
        camundaCloud: {
            clientId,
            clientSecret,
            clusterId,
            clusterRegion				
        },
        longPoll: Duration.seconds.of(60)
    });
}
const printTopology = async zbc =>{
	const topology = await zbc.topology()
	console.log(JSON.stringify(topology, null, 2))
}

const closeConnection = async zbc => {
    console.log('Closing client...')
    zbc.close().then(() => console.log('All workers closed'))
}

module.exports = {
    getConnection,
    printTopology,   
    closeConnection  
}



