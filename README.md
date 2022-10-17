# Flowifier Client Library for .NET Core

This library gives you a seemless integration of the [Flowifier application](http://flowifier.com) into your code.

It allows you to:
- List all workflows for a specific organisation
- Retrieving the workflow details of a specific workflow
- Executing a workflow by creating a workflow instance
- Retrieving the workflow instance status
- Retrieving the workflow instance result if successful finished

## Table of Contents
- [Usage](#usage)
- [Installation](#installation)

## Usage

### Retrieving all workflows

This example shows you how to list all workflows for your organization.

```csharp
var organizationId = '<YOUR-ORGANIZATION-ID>';
var organizationToken = '<YOUR-ORGANIZATION-ACCESS-TOKEN>';

var flowifier = new Flowifier(organizationId, organizationToken);

var workflows = await flowifier.GetWorkflows();

foreach (var workflow in workflows)
{
    Console.WriteLine($"{workflow.Id}: {workflow.Name}");
}
```

This example shows you how to list execute a workflow.

```csharp
var organizationId = '<YOUR-ORGANIZATION-ID>';
var organizationToken = '<YOUR-ORGANIZATION-ACCESS-TOKEN>';
var workflowId = '<YOUR-WORKFLOW-ID>';

var flowifier = new Flowifier(organizationId, organizationToken);

JObject contextObj = new JObject();
contextObj.Add("name", "John Doe");
contextObj.Add("age", 31);

var WorkflowInstance = await flowifier.ExecuteWorkflow(workflowId, contextObj);
Console.WriteLine($"New Workflow Instance Id: {WorkflowInstance.Id}");
```

## Installation

`dotnet add package Flowifier.Client`