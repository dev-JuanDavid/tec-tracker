using Quartz;
using System;
using System.Threading.Tasks;
using Queue.Controllers;

namespace Queue.Jobs
{
    public class Alerts : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                OperationController oc = new OperationController();
                oc.alert();
            }
            catch (Exception ex)
            {

            }
        }
    }
}