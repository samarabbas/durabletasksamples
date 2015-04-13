namespace DurableTaskSamples.UtilitySignup
{
    /// <summary>
    /// This can be used if you want to send signals to running instance of an Orchestration.  <see cref=" TaskHubClient.RaiseEvent"/> 
    /// and <see cref="TaskOrchestration.OnEvent"/>.  This is not used in this sample.
    /// </summary>
    public class UtilitySignupOrchestrationEvent
    {
    }
}
