using Azure;
using Azure.Messaging.EventGrid;
using Azure.Storage.Blobs;
using bike_project.Models;
using Newtonsoft.Json;
using System.Text;

namespace bike_project
{
    public class Helper
    {
        public static async Task<bool> UploadBlob(
            IConfiguration config, Staff staff)
        //IConfiguration is used to read data from app setting file
        {
            string blobConnString = config.GetConnectionString("StorAccConnString");

            BlobServiceClient client = new BlobServiceClient(blobConnString);
            //getting the container from the storage resource
            string container = config.GetValue<string>("Container");

            var containerClient = client.GetBlobContainerClient(container);

            string fileName = "ems.Staff." + Guid.NewGuid().ToString() + ".json";
            // Get a reference to a blob
            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            //memorystream
            using (var stream = new MemoryStream())
            {
                var serializer = JsonSerializer.Create(new JsonSerializerSettings());

                // Use the 'leave open' option to keep the memory stream open after the stream writer is disposed
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true))
                {
                    // Serialize the job to the StreamWriter
                    serializer.Serialize(writer, staff);
                }

                // Rewind the stream to the beginning
                stream.Position = 0;

                // Upload the job via the stream
                await blobClient.UploadAsync(stream, overwrite: true);
            }
            await PublishToEventGrid(config, staff);
            return true;
        }

        private static async Task PublishToEventGrid(

    IConfiguration config, Staff staff)

        {

            var endpoint = config.GetValue<string>("EventGridTopicEndpoint");

            var accessKey = config.GetValue<string>("EventGridAccessKey");


            EventGridPublisherClient client = new EventGridPublisherClient(

            new Uri(endpoint),

            new AzureKeyCredential(accessKey));


            var event1 = new EventGridEvent(

            "EMS",

            "EMS.StaffEvent",

            "1.0",

            JsonConvert.SerializeObject(staff));

            event1.Id = (new Guid()).ToString();

            event1.EventTime = DateTime.Now;

            //resource id

            //event1.Topic = "/subscriptions/73d972cd-c4c3-4ec5-9443-661a57525a5d/resourceGroups/rg-training/providers/Microsoft.EventGrid/topics/omsegt";

            event1.Topic = config.GetValue<string>("EventGridTopic");

            List<EventGridEvent> eventsList = new List<EventGridEvent>

                    {

                    event1

                    };
            await client.SendEventsAsync(eventsList);
        }


    }
}

