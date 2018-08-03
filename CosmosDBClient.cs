using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Azure;

namespace EventReader
{
    class CosmosDBClient<T> where T : class
    {
        private string DatabaseId;
        private string CollectionId;
        private DocumentClient docClient;

        //TODO: Azure KeyVault
        private String Endpoint = "https://mythical.documents.azure.com:443/";
        private Uri docUri;

        private String lastStatusMsg;

        public CosmosDBClient()
        {

        }
        public Boolean Connect(String endpoint, String databaseId, String CollId, String key)
        {
            //TODO: Replace with KeyVault
            //Ideally a simple way to automate KeyVault would be awesome...for e.g.
            /*
             * String endpoint = "https://mythical.documents.azure.com:443/"
             * String authKey = MyCompany.GetKey(Me)
             * DocumentClient client = new DocumentClient(endpointUri,authKey)
             * 
             * The Client automatically gets the Key to be used to Connect to a Secure Data Store....
             * In the DevOps pipeline, Applications are registered and access is granted in real-time
             * Every app has to implement EntperiseAuth Library... GetAToken => Access, Revoke
             * DevOps pipeline will not pass if the app does not implement the required DevSecLibrary.... 
            //SecDev
            */
            var authKey = key;

            docClient = new DocumentClient(new Uri(endpoint), authKey);
            docUri = UriFactory.CreateDocumentUri(databaseId, CollId, key);

            this.DatabaseId = databaseId;
            this.CollectionId = CollId;
            this.Endpoint = endpoint;
            return false;
        }


        //Calling : var items = await DocumentDBRepository<Item>.GetItemsAsync(d => !d.Completed);

        //TODO: What does this function express mean?
        public async Task<IEnumerable<T>> GetItemsAsync(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = docClient.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId),
                new FeedOptions { MaxItemCount = -1, EnableCrossPartitionQuery = true })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }
            return results;
        }

        public async Task<T> GetItemAsync()
        {
            String sqlString = null;
            SqlParameterCollection spColl = new SqlParameterCollection();

            var query = new SqlQuerySpec(sqlString, spColl);

            var document = docClient.CreateDocumentQuery<T>(docUri, query).AsEnumerable().FirstOrDefault();
            return document;
        }

        public async Task<T> GetItemAsync(string paramaterName, String value)
        {
            try
            {
                //UriFactory.CreateDocumentUri(DatabaseId, CollectionId, id)
                Document document = await docClient.ReadDocumentAsync(docUri);
                return (T)(dynamic)document;
            }
            catch (DocumentClientException e)

            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)

                {

                    return null;

                }

                else

                {

                    throw;

                }

            }

        }

        public async Task<Document> CreateItemAsync(T item)
        {
            lastStatusMsg = "";
            try
            {
                //Document retItem =  await docClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item);
                Document retItem = await docClient.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), item);
                return retItem;
            }
            catch (Microsoft.Azure.Documents.DocumentClientException ex)
            {
                lastStatusMsg = ex.Message;
                if (ex.StatusCode == System.Net.HttpStatusCode.Conflict) return null;

                throw ex;
            }
        }

        public String GetLastMsg()
        {
            return lastStatusMsg;
        }



    }
}
