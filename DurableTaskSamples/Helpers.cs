namespace DurableTaskSamples
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using DurableTask;
    using DurableTask.Test;
    using Newtonsoft.Json;
    using UtilitySignup;

    public static class Helpers
    {
        public static void DumpAllInstances(TaskHubClient client, int hours)
        {
            DateTime currentTime = DateTime.UtcNow;
            OrchestrationStateQuery statusQuery = new OrchestrationStateQuery();
            statusQuery
                .AddTimeRangeFilter(currentTime.Subtract(TimeSpan.FromHours(hours)), currentTime, OrchestrationStateTimeRangeFilterType.OrchestrationCreatedTimeFilter);

            DumpInstances(client.QueryOrchestrationStates(statusQuery));
        }

        public static void DumpAllInstancesForState(TaskHubClient client, OrchestrationStatus status, int hours)
        {
            DateTime currentTime = DateTime.UtcNow; 
            OrchestrationStateQuery statusQuery = new OrchestrationStateQuery();
            statusQuery
                .AddStatusFilter(status)
                .AddTimeRangeFilter(currentTime.Subtract(TimeSpan.FromHours(hours)), currentTime, OrchestrationStateTimeRangeFilterType.OrchestrationCreatedTimeFilter);

            DumpInstances(client.QueryOrchestrationStates(statusQuery));
        }

        public static void PrintOrchestrationStatus(TaskHubClient client, OrchestrationInstance instance)
        {
            OrchestrationState state = client.GetOrchestrationState(instance);
            if (state == null)
            {
                Helpers.ConsoleWriteLineColor(ConsoleColor.Red, 
                    string.Format("Orchestration instance '[InstanceId={0}, ExecutionId={1}]' not found.", instance.InstanceId, instance.ExecutionId));
                return;
            }

            string serializedState = state.Status;
            UtilitySignupOrchestrationStatus status = JsonConvert.DeserializeObject<UtilitySignupOrchestrationStatus>(serializedState);

            Helpers.ConsoleWriteLineColor(GetColorUsingState(state.OrchestrationStatus), 
                string.Format("Orchestration instance '[InstanceId={0}, ExecutionId={1}]' Status:", instance.InstanceId, instance.ExecutionId));
            Helpers.ConsoleWriteLineColor(GetColorUsingState(state.OrchestrationStatus), status.Status);
        }

        public static void PrintOrchestrationHistory(TaskHubClient client, OrchestrationInstance instance)
        {
            OrchestrationState state = client.GetOrchestrationState(instance);
            if (state == null)
            {
                Helpers.ConsoleWriteLineColor(ConsoleColor.Red, 
                    string.Format("Orchestration instance '[InstanceId={0}, ExecutionId={1}]' not found.", instance.InstanceId, instance.ExecutionId));

                return;
            }

            string history = client.GetOrchestrationHistory(instance);

            Helpers.ConsoleWriteLineColor(GetColorUsingState(state.OrchestrationStatus), 
                string.Format("Orchestration instance '[InstanceId={0}, ExecutionId={1}]' History:", instance.InstanceId, instance.ExecutionId));
            Helpers.ConsoleWriteLineColor(GetColorUsingState(state.OrchestrationStatus), history);
        }

        public static void ReplayInstance(TaskHubClient client, DebugTestHost replayer, OrchestrationInstance instance)
        {
            string history = client.GetOrchestrationHistory(instance);
            if (!string.IsNullOrEmpty(history))
            {

                Helpers.ConsoleWriteLineColor(ConsoleColor.Yellow, 
                    string.Format("Attach debugger to debug orchestration instance: [InstanceId={0}, ExecutionId={1}]", instance.InstanceId, instance.ExecutionId));

                Helpers.WaitForDebugger();

                replayer.ReplayOrchestration(typeof(UtilitySignupOrchestration), history);
            }
            else
            {
                Helpers.ConsoleWriteLineColor(ConsoleColor.Red, 
                    string.Format("Orchestration instance '[InstanceId={0}, ExecutionId={1}]' not found.", instance.InstanceId, instance.ExecutionId));
            }
        }

        static void DumpInstances(IEnumerable<OrchestrationState> states)
        {
            foreach (var state in states)
            {
                Helpers.ConsoleWriteLineColor(GetColorUsingState(state.OrchestrationStatus),
                    string.Format("InstanceId: {0}, ExecutionId: {1}, Status: {2}, StartedAt: {3}, Result: {4}",
                    state.OrchestrationInstance.InstanceId, state.OrchestrationInstance.ExecutionId, state.OrchestrationStatus, state.CreatedTime, state.Output));
            }
        }

        public static void WaitForDebugger()
        {
            DateTime start = DateTime.UtcNow;
            while (!Debugger.IsAttached)
            {
                Helpers.ConsoleWriteLineColor(ConsoleColor.Blue, "Waiting for debugger");

                Thread.Sleep(TimeSpan.FromSeconds(5));

                if ((DateTime.UtcNow - start) > TimeSpan.FromSeconds(120))
                {
                    Helpers.ConsoleWriteLineColor(ConsoleColor.Red, "Debugger did not attach. Continuing");
                    break;
                }
            }
        }

        static ConsoleColor GetColorUsingState(OrchestrationStatus status)
        {
            ConsoleColor color = Console.ForegroundColor;
            switch (status)
            {
                case OrchestrationStatus.Failed:
                    color = ConsoleColor.Red;
                    break;
                case OrchestrationStatus.Canceled:
                    color = ConsoleColor.Gray;
                    break;
                case OrchestrationStatus.Terminated:
                    color = ConsoleColor.DarkRed;
                    break;
                case OrchestrationStatus.Completed:
                case OrchestrationStatus.ContinuedAsNew:
                    color = ConsoleColor.Green;
                    break;
                case OrchestrationStatus.Running:
                    color = ConsoleColor.Yellow;
                    break;
                default:
                    break;
            }

            return color;
        }

        public static void ConsoleWriteLineColor(ConsoleColor color, string print)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(print);
            Console.ResetColor();
        }
    }
}
