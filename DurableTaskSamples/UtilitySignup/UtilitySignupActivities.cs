namespace DurableTaskSamples.UtilitySignup
{
    using System;
    using System.Threading.Tasks;

    /// <summary>
    /// Actual implementation of activities used by UtilitySignupOrchestration.  This is registered with <see cref="TaskHubWorker"/> 
    /// by using <see cref="TaskHubWorker.AddTaskActivitiesFromInterface"/> API.
    /// </summary>
    public class UtilitySignupActivities : IUtilitySignupActivities
    {
        const int MaxSleepInSeconds = 10;
        static Random random = new Random();
        static Random creditGenerator = new Random();
        static Random customerIdGenerator = new Random();

        public async Task<bool> ValidateAddress(CustomerAddress address)
        {
            Helpers.ConsoleWriteLineColor(ConsoleColor.Blue, string.Format("Validating Address: {0}", address.ToString()));

            int sleepInSeconds = random.Next(0, MaxSleepInSeconds);
            await Task.Delay(TimeSpan.FromSeconds(sleepInSeconds));

            Helpers.ConsoleWriteLineColor(ConsoleColor.Blue, string.Format("Address Validated: {0}", address.ToString()));

            return true;
        }

        public async Task<int> CreditCheck(string name, string provider)
        {
            Helpers.ConsoleWriteLineColor(ConsoleColor.DarkCyan, string.Format("Start Credit Check for '{0}' from Credit Agency '{1}'", name, provider));

            await DoSleep();

            int score = creditGenerator.Next(200, 850);

            Helpers.ConsoleWriteLineColor(ConsoleColor.DarkCyan, string.Format("Completed Credit Check for '{0}' from Credit Agency '{1}'.  Score: {2}",
                name, provider, score));

            return score;
        }

        public async Task<bool> ValidateBankAccount(string accountNumber)
        {
            Helpers.ConsoleWriteLineColor(ConsoleColor.DarkMagenta, string.Format("Validating Bank Account: {0}", accountNumber));

            await DoSleep();

            Helpers.ConsoleWriteLineColor(ConsoleColor.DarkMagenta, string.Format("Bank Account Validated: {0}", accountNumber));

            return true;
        }


        public async Task<string> SignupCustomer(string name, string accountNumber, CustomerAddress address, bool requiresManagerApproval)
        {
            Helpers.ConsoleWriteLineColor(ConsoleColor.Cyan, string.Format("Signing up customer: {0}", name));

            string customerId = name + "-" + customerIdGenerator.Next();
            await DoSleep();

            Helpers.ConsoleWriteLineColor(ConsoleColor.Cyan, string.Format("Customer '{0}' Signed Up with Id '{1}'", 
                name, customerId));

            return customerId;
        }

        async Task DoSleep()
        {
            int sleepInSeconds = random.Next(0, MaxSleepInSeconds);
            await Task.Delay(TimeSpan.FromSeconds(sleepInSeconds));
        }
    }
}
