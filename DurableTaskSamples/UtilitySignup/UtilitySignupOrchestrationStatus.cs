namespace DurableTaskSamples.UtilitySignup
{
    using System;

    /// <summary>
    /// This is used to keep track of Status of running orchestration.  <see cref="UtilitySignupOrchestration"/> orchestration
    /// creates an instance of it and use it to keep appending the current state.  Signup orchestration
    /// returns <see cref="UtilitySignupOrchestrationStatus"/> object back to framework by overriding <see cref="TaskOrchestration.OnGetStatus"/>.
    /// This object is then deserialized by the framework and then stored in instance store which is 
    /// made queryable through Durable Task client API.
    /// </summary>
    public class UtilitySignupOrchestrationStatus
    {
        public UtilitySignupOrchestrationStatus()
        {
        }

        public string Status { get; set; }

        public void AppendStatus(string message)
        {
            if (string.IsNullOrEmpty(this.Status))
            {
                this.Status = message;
            }
            else
            {
                this.Status += Environment.NewLine;
                this.Status += message;
            }

        }
    }
}
