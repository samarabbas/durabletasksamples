namespace DurableTaskSamples.UtilitySignup
{
    /// <summary>
    /// This is the input to <see cref="UtilitySignupOrchestration"/>.  This is passed to the instance when it is started using 
    /// <see cref="TaskHubClient.CreateOrchestrationInstance"/> API.  When <see cref="TaskHubWorker"/> picks up the instance
    /// it deserializes the payload and pass input to <see cref="UtilitySignupOrchestration.RunTask"/>.
    /// </summary>
    public class UtilitySignupOrchestrationInput
    {
        public string Name { get; set; }

        public string AccountNumber { get; set; }

        public CustomerAddress Address { get; set; }

        public int NumberOfCreditAgencies { get; set; }

        public SignupChecks Checks { get; set; }
    }
}
