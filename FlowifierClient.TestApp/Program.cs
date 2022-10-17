using dotenv.net;
using FlowifierClient;
using Newtonsoft.Json.Linq;
using System.Linq.Expressions;

internal class Program
{
    private static async Task Main(string[] args)
    {
        try
        {
            DotEnv.Load(options: new DotEnvOptions(ignoreExceptions: false, envFilePaths: new[] { ".env", "../.env" }, probeForEnv: true, probeLevelsToSearch: 10));
            var envVars = DotEnv.Read();

            if (!envVars.ContainsKey("ORGANIZATION_ID"))
            {
                Console.WriteLine("Environment variable ORGANIZATION_ID is not set!");
                return;
            }

            if (!envVars.ContainsKey("ORGANIZATION_TOKEN"))
            {
                Console.WriteLine("Environment variable ORGANIZATION_ID is not set!");
                return;
            }

            var organizationId = envVars["ORGANIZATION_ID"];
            var organizationToken = envVars["ORGANIZATION_TOKEN"];

            var fc = new Flowifier("http://127.0.0.1:8080", organizationId, organizationToken);

            Console.WriteLine("--- Get Workflows -------------------------------------------------");

            var workflows = await fc.GetWorkflows();

            foreach (var workflow in workflows)
            {
                Console.WriteLine($"{workflow.Id}: {workflow.Name}");
            }

            Console.WriteLine();
            Console.WriteLine("--- Get Workflow --------------------------------------------------");

            var singleWorkflow = await fc.GetWorkflow("62fb457b198284c3c5009001");
            Console.WriteLine($"{singleWorkflow.Id}: {singleWorkflow.Name}");

            Console.WriteLine();
            Console.WriteLine("--- Create Workflow Instance --------------------------------------");

            JObject contextObj = new JObject();
            contextObj.Add("firstname", ".NET Core");
            contextObj.Add("lastname", "Client Library");

            var WorkflowInstance = await fc.ExecuteWorkflow("62fb457b198284c3c5009001", contextObj);

            Console.WriteLine($"New Workflow Instance Id: {WorkflowInstance.Id}");
            var workflowInstanceId = WorkflowInstance.Id;

            if (workflowInstanceId is null)
            {
                throw new FlowifierException($"No workflow instance supplied.");
            }

            Console.WriteLine();
            Console.WriteLine("--- Get Workflow Instance Status ----------------------------------");

            var workflowInstanceStatus = "initial";

            do {
                workflowInstanceStatus = await fc.GetWorkflowInstanceStatus("62fb457b198284c3c5009001", workflowInstanceId);
                Console.WriteLine($"Workflow Instance Status: {workflowInstanceStatus}");

                Thread.Sleep(1000);
            } while (workflowInstanceStatus != "finished" && workflowInstanceStatus != "failed");

            if (workflowInstanceStatus == "finished") {

                Console.WriteLine();
                Console.WriteLine("--- Get Workflow Instance Status ----------------------------------");

                var workflowInstanceResult = await fc.GetWorkflowInstanceResult("62fb457b198284c3c5009001", workflowInstanceId);
                Console.WriteLine($"Workflow Instance Status: {workflowInstanceResult}");
            }

            Console.WriteLine();
            Console.WriteLine("--- FINISHED ------------------------------------------------------");
        }
        catch (FlowifierException ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex.Message);
        }
    }
}