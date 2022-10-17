using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowifierClient
{   
    public class WorkflowInstance
    {
        public string? Id { get; set; }
        public string? Name { get; set; }

        public string? Status { get; set; }

        [JsonProperty("organization")]
        public string? OrganizationId { get; set; }

        [JsonProperty("workflow")]
        public string? WorkflowId { get; set; }
    }
}
