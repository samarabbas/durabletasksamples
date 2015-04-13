namespace DurableTaskSamples.UtilitySignup
{
    using System.Linq;
    using System.Threading.Tasks;
    using DurableTask;

    /// <summary>
    /// This is the main user orchestration.  RunTask is the entry point for any user orchestration.  Users are expected to start an instance of this 
    /// orchestration through <see cref="TaskHubClient.CreateOrchestrationInstance"/> and any input parameters provided as part of CreateOrchestrationInstance
    /// are passed as input parameter to RunTask method.  This is registered with <see cref="TaskHubWorker"/> 
    /// by using <see cref="TaskHubWorker.AddTaskOrchestrations"/> API.
    /// </summary>
    public class UtilitySignupOrchestration : TaskOrchestration<string, UtilitySignupOrchestrationInput, UtilitySignupOrchestrationEvent, UtilitySignupOrchestrationStatus>
    {
        readonly string[] CreditAgencies = { "Experian", "Equifax", "TransUnion" };

        IUtilitySignupActivities activityClient = null;
        bool requiresManagerApproval = false;
        UtilitySignupOrchestrationStatus status = new UtilitySignupOrchestrationStatus();

        public override async Task<string> RunTask(OrchestrationContext context, UtilitySignupOrchestrationInput input)
        {
            this.activityClient = context.CreateClient<IUtilitySignupActivities>();

            bool[] results = null;
            try
            {
                // start three tasks in parallel and wait for all of them to finish
                results = await Task.WhenAll<bool>(
                    PerformAddressCheckAsync(input.Checks, input.Address),
                    PerformCreditCheckAsync(input.Name, input.NumberOfCreditAgencies),
                    PerformBankAccountCheckAsync(input.Checks, input.AccountNumber));

                this.status.AppendStatus("All Checks Completed.");
            }
            catch
            {
                // Error handling if any of the tasks throws an exception.
                // Just append it to instance status and rethrow to cause the orchestration instance to fail.
                this.status.AppendStatus("Checks Failed.");
                throw;
            }

            string customerId = "Rejected";
            if (results != null && results.All(r => r)) 
            {
                // Check to see if all checks succeeded for the customer and then signup user for utility service
                customerId = await this.activityClient.SignupCustomer(
                    input.Name, input.AccountNumber, input.Address, this.requiresManagerApproval);
            }

            return customerId;
        }

        async Task<bool> PerformAddressCheckAsync(SignupChecks checks, CustomerAddress address)
        {
            bool result = true;
            // Check to see if Address check is requested for this application
            if ((checks & SignupChecks.PerformAddressCheck) == SignupChecks.PerformAddressCheck)
            {
                this.status.AppendStatus("PerformAddressCheck: Starting.");
                result = await this.activityClient.ValidateAddress(address);
                this.status.AppendStatus("PerformAddressCheck: Completed with result: " + result);
            }

            return result;
        }

        async Task<bool> PerformCreditCheckAsync(string name, int numberOfCreditAgencies)
        {
            this.status.AppendStatus("PerformCreditCheckAsync: Starting.");
            bool result = false;
            int totalScore = 0;
            int count = 0;
            for (; count < numberOfCreditAgencies; count++)
            {
                // Run credit check for each of the requested agencies
                totalScore += await this.activityClient.CreditCheck(name, CreditAgencies[count]);
            }

            // Compute average credit score.
            // I have intentionally leave a bug where "DivideByZero" exception is thrown if number of credit checks requested is 0
            // This will cause the workflow instance to fail and we can later debug the failed instance by using the Replay command
            // in this sample.
            int averageScore = totalScore / count;
            if (averageScore > 500) 
            {
                result = true;
                if (averageScore < 700) 
                {
                    this.requiresManagerApproval = true;
                }
            }

            this.status.AppendStatus("PerformCreditCheckAsync: Completed with result: " + result);

            return result;
        }

        async Task<bool> PerformBankAccountCheckAsync(SignupChecks checks, string accountNumber)
        {
            bool result = true;
            if ((checks & SignupChecks.PerformBankAccountCheck) == SignupChecks.PerformBankAccountCheck)
            {
                this.status.AppendStatus("PerformBankAccountCheckAsync: Starting.");
                result = await this.activityClient.ValidateBankAccount(accountNumber);
                this.status.AppendStatus("PerformBankAccountCheckAsync: Completed with result: " + result);
            }

            return result;
        }

        public override UtilitySignupOrchestrationStatus OnGetStatus()
        {
            // Returning status of current orchestration instance.
            // This is made availble by querying the instance store with InstanceId and ExecutionId
            return status;
        }

        public override void OnEvent(OrchestrationContext context, string name, UtilitySignupOrchestrationEvent input)
        {
        }
    }
}
