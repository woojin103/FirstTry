using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FirstTry
{
    class WorkflowCLR
    {
        public List<WorkflowCLRAssembly> References { get; set; }

        public WorkflowCLR(string file)
        {
            this.References = new List<WorkflowCLRAssembly>();
            var doc = XElement.Load(file);
            var query = from attr in doc.Attributes() where attr.IsNamespaceDeclaration && attr.Value.ToLower().StartsWith("clr-namespace") select attr.Value;
            foreach (var assemblyName in query)
            {
                this.References.Add(new WorkflowCLRAssembly(assemblyName));
            }
        }

        public WorkflowCLR LoadWorkflow()
        {
            foreach (var workflowCLR in References)
            {
                workflowCLR.LoadWorkflow();
            }

            return this;
        }

        internal static WorkflowCLR LoadWorkflow(string name)
        {
            return new WorkflowCLR(name).LoadWorkflow();
        }
    }
}
