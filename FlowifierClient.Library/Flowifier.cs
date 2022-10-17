using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

/// <summary>This is Namespace</summary>
namespace FlowifierClient
{
    /// <summary>
    /// This is class
    /// </summary>
    public class Flowifier
    {
        public string AppUrl { get; private set; }
        public string OrganizationId { get; private set; }
        public string OrganizationToken { get; private set; }

        public Flowifier(string appUrl, string organizationId, string organizationToken)
        {
            this.AppUrl = appUrl;
            this.OrganizationId = organizationId;
            this.OrganizationToken = organizationToken;
        }

        public Flowifier(string organizationId, string organizationToken)
        {
            this.AppUrl = @"https://app.flowifier.com";
            this.OrganizationId = organizationId;
            this.OrganizationToken = organizationToken;
        }

        private static void VerifyResponse(RestResponse response)
        {
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.Created)
            {
                throw new FlowifierException($"Request failed with status code {response.StatusCode}.");
            }
        }

        private static JSend DeserializeBody(RestResponse response)
        {
            if (response.Content is null)
            {
                throw new FlowifierException($"No data received.");
            }

            if (response.ContentType is null || response.RawBytes is null)
            {
                throw new FlowifierException($"No content type received");
            }

            JSend? jSend = null;

            if (response.ContentType.Equals("application/json"))
            {
                jSend = JsonConvert.DeserializeObject<JSend>(response.Content);

            } else if (response.ContentType.Equals("application/json"))
            {
                MemoryStream ms = new MemoryStream(response.RawBytes);

                using (BsonDataReader reader = new BsonDataReader(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();

                    jSend = serializer.Deserialize<JSend>(reader);
                }
            } else
            {
                throw new FlowifierException($"Unhandled content type '{response.ContentType}' received");
            }

            if (jSend is null)
            {
                throw new FlowifierException($"Deserializing of JSON response context failed.");
            }

            return jSend;
        }

        private static JSend VerifyResponseAndDeserializeBody(RestResponse response)
        {
            VerifyResponse(response);
            return DeserializeBody(response);
        }

        private RestRequest GenerateRestRequest(Method method, Uri uri, Body? body = null)
        {
            var request = new RestRequest(uri, method);
            request.AddHeader("accept", "application/json");
            request.AddHeader("authorization", $"Bearer {this.OrganizationToken}");

            if (body!=null && (method == Method.Post || method == Method.Put || method == Method.Patch))
            {
                if (body.ContentType == Body.ContentTypes.JSON)
                {
                    request.AddHeader("Content-Type", "application/json");
                    request.AddParameter("application/json", body.JSON, ParameterType.RequestBody);
                }
                else if (body.ContentType == Body.ContentTypes.BSON)
                {
                    request.AddHeader("Content-Type", "application/bson");
                    request.AddParameter("application/bson", body.BSON, ParameterType.RequestBody);
                } else
                {
                    throw new FlowifierException($"Unsupported body type '{body.ContentType}'");
                }
            }

            return request;
        }

        private async Task<RestResponse> ExecuteRestRequest(Method method, Uri uri, Body? body = null)
        {
            var client = new RestClient();

            var request = GenerateRestRequest(method, uri, body);

            var response = await client.ExecuteAsync(request).ConfigureAwait(false);

            return response;
        }

        /*private string objectToJSONString(JObject jObj)
        {
            return jObj.ToString(Formatting.None);
        }

        private string objectToBSONString(JObject jObj)
        {
            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, jObj);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        private Body objectToBSONBody(JObject jObj)
        {
            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, jObj);
            }

            return ms.ToArray();
        }*/

        public async Task<Workflow[]> GetWorkflows()
        {
            var uri = new Uri($"{this.AppUrl}/api/v1/organizations/{this.OrganizationId}/workflows");

            var response = await ExecuteRestRequest(Method.Get, uri).ConfigureAwait(false);

            JSend jSend = VerifyResponseAndDeserializeBody(response);

            if (jSend is null || jSend.Data is null || jSend.Data.data is null)
            {
                throw new FlowifierException($"Expected data not supplied.");
            }

            Workflow[] workflows = (Workflow[])jSend.Data.data.ToObject<Workflow[]>();

            return workflows;
        }

        public async Task<Workflow> GetWorkflow(string workflowId)
        {
            var uri = new Uri($"{this.AppUrl}/api/v1/organizations/{this.OrganizationId}/workflows/{workflowId}");

            var response = await ExecuteRestRequest(Method.Get, uri).ConfigureAwait(false);

            var jSend = VerifyResponseAndDeserializeBody(response);

            if (jSend is null || jSend.Data is null || jSend.Data.data is null)
            {
                throw new FlowifierException($"Expected data not supplied.");
            }

            Workflow workflow = (Workflow)jSend.Data.data.ToObject<Workflow>();

            return workflow;
        }

        public async Task<WorkflowInstance> ExecuteWorkflow(Workflow workflow, JObject? context)
        {
            if (workflow.Id is null)
            {
                throw new FlowifierException($"No workflows identifier supplied");
            }

            return await ExecuteWorkflow(workflow.Id, context).ConfigureAwait(false);
        }

        public async Task<WorkflowInstance> ExecuteWorkflow(string workflowId, JObject? context)
        {
            JObject reqBodyObj = new JObject();
            reqBodyObj.Add("name", "Instance Name");
            reqBodyObj.Add("context", (context == null ? new JObject() : context));

            var uri = new Uri($"{this.AppUrl}/api/v1/organizations/{this.OrganizationId}/workflows/{workflowId}/instances");

            //var body = reqBodyObj.ToString(Formatting.None);
            var body = Body.ObjectToBSONBody(reqBodyObj);

            var response = await ExecuteRestRequest(Method.Post, uri, body).ConfigureAwait(false);

            var jSend = VerifyResponseAndDeserializeBody(response);

            if (jSend is null || jSend.Data is null || jSend.Data.workflowInstance is null)
            {
                throw new FlowifierException($"Expected data not supplied.");
            }

            WorkflowInstance workflowInstance = (WorkflowInstance)jSend.Data.workflowInstance.ToObject<WorkflowInstance>();

            return workflowInstance;
        }

        public async Task<string> GetWorkflowInstanceStatus(string workflowId, string workflowInstanceId)
        {
            var uri = new Uri($"{this.AppUrl}/api/v1/organizations/{this.OrganizationId}/workflows/{workflowId}/instances/{workflowInstanceId}");

            var response = await ExecuteRestRequest(Method.Get, uri).ConfigureAwait(false);

            var jSend = VerifyResponseAndDeserializeBody(response);

            if (jSend is null || jSend.Data is null || jSend.Data.data is null)
            {
                throw new FlowifierException($"Expected data not supplied.");
            }

            WorkflowInstance workflowInstance = (WorkflowInstance)jSend.Data.data.ToObject<WorkflowInstance>();

            if (workflowInstance.Status is null)
            {
                throw new FlowifierException($"No workflow instance status in response.");
            }

            return workflowInstance.Status;
        }

        public async Task<dynamic> GetWorkflowInstanceResult(string workflowId, string workflowInstanceId)
        {
            var uri = new Uri($"{this.AppUrl}/api/v1/organizations/{this.OrganizationId}/workflows/{workflowId}/instances/{workflowInstanceId}/result");

            var response = await ExecuteRestRequest(Method.Get, uri).ConfigureAwait(false);

            var jSend = VerifyResponseAndDeserializeBody(response);

            if (jSend is null || jSend.Data is null)
            {
                throw new FlowifierException($"Expected data not supplied.");
            }

            dynamic workflowInstanceResult = jSend.Data.result;

            return workflowInstanceResult;
        }
    }
}