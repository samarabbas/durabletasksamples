namespace DurableTaskSamples.UtilitySignup
{
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for all activities used by UtilitySignupOrchestration.  This is registered with <see cref="TaskHubWorker"/> 
    /// by using <see cref="TaskHubWorker.AddTaskActivitiesFromInterface"/> API.
    /// </summary>
    public interface IUtilitySignupActivities
    {
        Task<bool> ValidateAddress(CustomerAddress address);

        Task<int> CreditCheck(string name, string provider);

        Task<bool> ValidateBankAccount(string accountNumber);

        Task<string> SignupCustomer(string name, string accountNumber, CustomerAddress address, bool requiresManagerApproval);
    }
}
