namespace DurableTaskSamples.UtilitySignup
{
    using System;

    [Flags]
    public enum SignupChecks
    {
        PerformAddressCheck      = 1,
        PerformCreditCheck       = 2,
        PerformBankAccountCheck  = 4,

        All = 7
    }
}
