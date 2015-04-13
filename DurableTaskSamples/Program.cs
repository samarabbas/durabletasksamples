namespace DurableTaskSamples
{
    using System;
    using System.Configuration;
    using System.Threading.Tasks;
    using DurableTask;
    using DurableTask.Test;
    using UtilitySignup;

    class Program
    {
        static Options options = new Options();

        static void Main(string[] args)
        {
            if (CommandLine.Parser.Default.ParseArgumentsStrict(args, options))
            {
                string servicebusConnectionString = Program.GetSetting("ServiceBusConnectionString");
                string storageConnectionString = Program.GetSetting("StorageConnectionString");
                string taskHubName = ConfigurationManager.AppSettings["taskHubName"];

                TaskHubClient taskHubClient = new TaskHubClient(taskHubName, servicebusConnectionString, storageConnectionString);
                TaskHubWorker taskHub = new TaskHubWorker(taskHubName, servicebusConnectionString, storageConnectionString);

                if (options.CreateHub)
                {
                    taskHub.CreateHub();
                }

                if (!string.IsNullOrWhiteSpace(options.StartInstance))
                {
                    OrchestrationInstance instance = Program.StartOrchestrationInstance(taskHubClient, options.StartInstance, options.InstanceId, options.Parameters);

                    Console.WriteLine("Workflow Instance Started: " + instance);
                }
                else if (!string.IsNullOrWhiteSpace(options.Signal))
                {
                    if (string.IsNullOrWhiteSpace(options.InstanceId))
                    {
                        throw new ArgumentException("instantceId");
                    }
                    if (options.Parameters == null || options.Parameters.Length != 1)
                    {
                        throw new ArgumentException("parameters");
                    }

                    Program.RaiseEvent(taskHubClient, options.InstanceId, options.Signal, options.Parameters[0]);
                }
                else if (!string.IsNullOrWhiteSpace(options.MonitorCommand))
                {
                    Program.Monitor(taskHubClient, options.MonitorCommand, options.Parameters);
                }
                else if (options.RunSimulation)
                {
                    Program.RunSimulation(taskHubClient, options.Parameters);
                }

                if (!options.SkipWorker)
                {
                    Program.StartWorker(taskHub);
                }
            }
        }

        public static void StartWorker(TaskHubWorker taskHub)
        {
            try
            {
                Console.WriteLine("Starting DurableTask worker...");
                IUtilitySignupActivities utilitySignupActivities = new UtilitySignupActivities();
                taskHub.AddTaskActivitiesFromInterface<IUtilitySignupActivities>(utilitySignupActivities);
                taskHub.AddTaskOrchestrations(typeof(UtilitySignupOrchestration));

                taskHub.Start();

                Console.WriteLine("DurableTask worker successfully started.  Press any key to quit.");
                Console.ReadLine();

                taskHub.Stop(true);
            }
            catch (Exception)
            {
                // silently eat any unhandled exceptions.
            }
        }

        public static OrchestrationInstance StartOrchestrationInstance(TaskHubClient client, string name, string instanceId, string[] parameters)
        {
            OrchestrationInstance instance = null;
            switch (name)
            {
                case "Signup":
                    if (parameters == null || parameters.Length < 2)
                    {
                        throw new ArgumentException("Signup parameters not provided.");
                    }

                    UtilitySignupOrchestrationInput signupInput = new UtilitySignupOrchestrationInput
                    {
                        Name = parameters[0],
                        AccountNumber = parameters[1],
                        NumberOfCreditAgencies = parameters.Length > 2 ? int.Parse(parameters[2]) : 0,
                        Address = new CustomerAddress
                        {
                            Street = "One Microsoft Way",
                            City = "Redmond",
                            State = "WA",
                            Zip = 98052,
                        },
                        Checks = SignupChecks.All,
                    };
                    instance = client.CreateOrchestrationInstance(typeof(UtilitySignupOrchestration), instanceId, signupInput);
                    break;
                default:
                    throw new Exception("Unsupported Orchestration Name: " + name);
            }

            return instance;
        }

        public static void RaiseEvent(TaskHubClient client, string instanceId, string signalName, string signalValue)
        {
            OrchestrationInstance instance = new OrchestrationInstance { InstanceId = instanceId };
            client.RaiseEvent(instance, signalName, signalValue);
        }

        public static void Monitor(TaskHubClient client, string command, string[] parameters)
        {
            switch (command)
            {
                case "Dump":
                    int hours = 5;
                    if (parameters != null && parameters.Length == 1)
                    {
                        hours = int.Parse(parameters[0]);
                    }

                    Helpers.DumpAllInstances(client, hours);
                    break;
                case "DumpStatus":
                    int h = 5;
                    if (parameters != null && (parameters.Length == 1 || parameters.Length == 2))
                    {
                        if (parameters != null && parameters.Length == 2)
                        {
                            h = int.Parse(parameters[1]);
                        }

                        Helpers.DumpAllInstancesForState(client, (OrchestrationStatus)Enum.Parse(typeof(OrchestrationStatus), parameters[0]), h);
                    }
                    else
                    {
                        throw new ArgumentException("DumpStatus parameters not provided.  Please specify orchestration status to query for.");
                    }
                    break;
                case "GetStatus":
                    if (parameters != null && parameters.Length == 2)
                    {
                        OrchestrationInstance instance = new OrchestrationInstance
                        {
                            InstanceId = parameters[0],
                            ExecutionId = parameters[1],
                        };

                        Helpers.PrintOrchestrationStatus(client, instance);
                    }
                    else
                    {
                        throw new ArgumentException("GetStatus parameters not provided.");
                    }
                    break;
                case "GetHistory":
                    if (parameters != null && parameters.Length == 2)
                    {
                        OrchestrationInstance instance = new OrchestrationInstance
                        {
                            InstanceId = parameters[0],
                            ExecutionId = parameters[1],
                        };

                        Helpers.PrintOrchestrationHistory(client, instance);
                    }
                    else
                    {
                        throw new ArgumentException("History parameters not provided.");
                    }
                    break;
                case "Replay":
                    if (parameters != null && parameters.Length == 2)
                    {
                        OrchestrationInstance instance = new OrchestrationInstance
                        {
                            InstanceId = parameters[0],
                            ExecutionId = parameters[1],
                        };

                        DebugTestHost replayer = new DebugTestHost();
                        replayer.AddTaskOrchestrations(typeof(UtilitySignupOrchestration));

                        Helpers.ReplayInstance(client, replayer, instance);
                    }
                    else
                    {
                        throw new ArgumentException("Replay parameters not provided.");
                    }
                    break;
                default:
                    throw new ArgumentException("Unsupported monitoring command: " + command);
            }
        }

        public static void RunSimulation(TaskHubClient client, string[] parameters)
        {
            int numberOfInstances = 1;
            int delayInSeconds = 2;

            if (parameters != null && parameters.Length == 2)
            {
                numberOfInstances = int.Parse(parameters[0]);
                delayInSeconds = int.Parse(parameters[1]);
            }

            Task.Factory.StartNew(async () =>
                {
                    Random random = new Random();
                    
                    for (int i = 0; i < numberOfInstances; i++)
                    {
                        UtilitySignupOrchestrationInput signupInput = new UtilitySignupOrchestrationInput
                        {
                            Name = "TestName-" + i.ToString(),
                            AccountNumber = "TestAccount-" + i.ToString(),
                            NumberOfCreditAgencies = random.Next(0, 4),
                            Address = new CustomerAddress
                            {
                                Street = "One Microsoft Way",
                                City = "Redmond",
                                State = "WA",
                                Zip = 98052,
                            },
                            Checks = SignupChecks.All,
                        };

                        client.CreateOrchestrationInstance(typeof(UtilitySignupOrchestration), "instance-" + i.ToString(), signupInput);

                        await Task.Delay(delayInSeconds * 1000);
                    }
                });
            
        }

        public static string GetSetting(string name)
        {
            string value = Environment.GetEnvironmentVariable("DurableTaskSamples_" + name);
            if (string.IsNullOrEmpty(value))
            {
                value = ConfigurationManager.AppSettings.Get(name);
            }
            return value;
        }
    }
}
