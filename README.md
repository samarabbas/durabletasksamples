Durable Task Framework Samples
==============================

This sample is provided to show various capabilites of Durable Task Framework available at:
https://github.com/affandar/durabletask

This sample requires you to provide your ServiceBus and Azure storage connection string.  There are following 2 ways:
* Set the environment variables 'DurableTaskSamples_ServiceBusConnectionString' and 'DurableTaskSamples_StorageConnectionString'.
* Update the App.Config to provide values for 'ServiceBusConnectionString' and 'StorageConnectionString' application settings.

This sample implements a signup workflow for customers of utility company.  Here are the steps of workflow:
* Workflow starts 3 tasks in parallel (PerformAddressCheckAsync, PerformCreditCheckAsync, PerformBankAccountCheckAsync)
* Once all Checks are completed then it Signs up the customer by invoking 'SignupCustomer' only if all checks were successful
* The workflow returns the Id for customer after signup or returns 'REJECTED' if any of the checks failed.
* PerformCreditCheckAsync is a data-driven asynchronous process which relies on the 'NumberOfCreditAgencies' value passed in as input to workflow.
** Workflow is hard-coded with 3 credit agencies where it loops through each credit agency to do a credit check upto 'NumberOfCreditAgencies' value passed as input.
** Then it computes the average score.
** if score is less than 500 then CreditCheck is marked as failed.
** if score is greater than 500 then it is successful but if it is less then 700 it is marked to have manager approval.

Other interesting features:
* State of the workflow is continously updated as it is making progress by appending status messages to UtilitySignupOrchestrationStatus instance.  This is returned as status for the orchestration and can be queried through client API to monitor the state of orchestration.
* An instance of Signup workflow is started through StartOrchestrationInstance API by providing input as instance of UtilitySignupOrchestrationInput object.

The following commands are provided to run the sample:
  -c, --create-hub         (Default: False) Create Orchestration Hub.

  -s, --start-instance     (Default: ) Start new instance.  Suported
                           Orchestrations: 'Signup'.

  -i, --instance-id        Instance id for new orchestration instance.

  -p, --params             Parameters for new instance.

  -n, --signal-name        Instance id to send signal

  -w, --skip-worker        (Default: False) Don't start worker

  -m, --monitor-command    (Default: ) Monitoring commands.  Supported commands
                           are: 'Dump DumpStatus GetStatus GetHistory Replay'.

  -r, --run-simulation     (Default: False) Run simulation for
                           UtilitySignupOrchestration

  --help                   Display this help screen.

Usage: DurableTaskSamples.exe -c -s Signup -p <name> <accountId> <numberOfCreditAgencies>
Usage: DurableTaskSamples.exe -w -m Dump -p <hours>
Usage: DurableTaskSamples.exe -w -m DumpStatus -p <Running> <hours>
Usage: DurableTaskSamples.exe -w -m DumpStatus -p <Completed> <hours>
Usage: DurableTaskSamples.exe -w -m DumpStatus -p <Failed> <hours>
Usage: DurableTaskSamples.exe -w -m GetStatus -p <instanceId> <executionId>
Usage: DurableTaskSamples.exe -w -m GetHistory -p <instanceId> <executionId>
Usage: DurableTaskSamples.exe -w -m Replay -p <instanceId> <executionId>
Usage: DurableTaskSamples.exe -r -p <numberOfInstances> <delayInSeconds>

  

