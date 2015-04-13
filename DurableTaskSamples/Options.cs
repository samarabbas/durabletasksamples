namespace DurableTaskSamples
{
    using CommandLine;
    using CommandLine.Text;

    class Options
    {
        [Option('c', "create-hub", DefaultValue = false,
            HelpText = "Create Orchestration Hub.")]
        public bool CreateHub { get; set; }

        [Option('s', "start-instance", DefaultValue = null,
            HelpText = "Start new instance.  Supported Orchestrations: 'Signup'.")]
        public string StartInstance { get; set; }

        [Option('i', "instance-id",
            HelpText = "Instance id for new orchestration instance.")]
        public string InstanceId { get; set; }

        [OptionArray('p', "params",
            HelpText = "Parameters for new instance.")]
        public string[] Parameters { get; set; }

        [Option('n', "signal-name",
            HelpText = "Instance id to send signal")]
        public string Signal { get; set; }

        [Option('w', "skip-worker", DefaultValue = false,
            HelpText = "Don't start worker")]
        public bool SkipWorker { get; set; }

        [Option('m', "monitor-command", DefaultValue = null,
            HelpText = "Monitoring commands.  Supported commands are: 'Dump DumpStatus GetStatus GetHistory Replay'.")]
        public string MonitorCommand { get; set; }

        [Option('r', "run-simulation", DefaultValue = false,
            HelpText = "Run simulation for UtilitySignupOrchestration")]
        public bool RunSimulation { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            // this without using CommandLine.Text
            //  or using HelpText.AutoBuild

            var help = new HelpText
            {
                Heading = new HeadingInfo("DurableTaskSamples", "1.0"),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };
            help.AddPreOptionsLine("Usage: DurableTaskSamples.exe -c -s Signup -p <name> <accountId>");
            help.AddPreOptionsLine("Usage: DurableTaskSamples.exe -c -s Signup -p <name> <accountId> <numberOfCreditAgencies>");
            help.AddPreOptionsLine("Usage: DurableTaskSamples.exe -w -m Dump -p <hours>");
            help.AddPreOptionsLine("Usage: DurableTaskSamples.exe -w -m DumpStatus -p <Running> <hours>");
            help.AddPreOptionsLine("Usage: DurableTaskSamples.exe -w -m DumpStatus -p <Completed> <hours>");
            help.AddPreOptionsLine("Usage: DurableTaskSamples.exe -w -m DumpStatus -p <Failed> <hours>");
            help.AddPreOptionsLine("Usage: DurableTaskSamples.exe -w -m GetStatus -p <instanceId> <executionId>");
            help.AddPreOptionsLine("Usage: DurableTaskSamples.exe -w -m GetHistory -p <instanceId> <executionId>");
            help.AddPreOptionsLine("Usage: DurableTaskSamples.exe -w -m Replay -p <instanceId> <executionId>");
            help.AddPreOptionsLine("Usage: DurableTaskSamples.exe -r -p <numberOfInstances> <delayInSeconds>");
            help.AddOptions(this);
            return help;
        }
    }
}
