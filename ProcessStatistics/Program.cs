using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace ProcessStatistics
{
    class Program
    {
        
        class AppData
        {
            private object Locker = new object();
            public string ProcessPath { get; set; }

            public int SamplePeriodMs { get; set; }

            public List<string> ErrorLog { get; } = new List<string>();

            public bool RunEnable { get; set; }

            public ProcessDebugInfo ProcessInfo
            {
                get 
                { 
                    lock(Locker)
                    {
                        return mProcessInfo;
                    }
                }

                set
                {
                    lock(Locker)
                    {
                        mProcessInfo = value;
                    }
                }
            }

            private ProcessDebugInfo mProcessInfo;
        }

 
        private static void MeasureProcessRoutine(Object arg)
        {
            AppData data = arg as AppData;
            if (data == null) return;



            using (CsvWriter writer = new CsvWriter("Log_" + TimeToStr() + ".csv"))
            {

                using (ProcessObserver processObserver = new ProcessObserver(data.ProcessPath))
                {

                    CommonLibrary.OperationResult openResult = writer.Open();
                    if (!openResult)
                    {
                        data.ErrorLog.Add(openResult.Message);
                        data.RunEnable = false;
                        return;
                    }

                    if (processObserver.StartProcess())
                    {
                        CommonLibrary.OperationResult writeResult = writer.WriteLine(new List<string>() { data.ProcessPath });
                        if (!writeResult)
                        {
                            data.ErrorLog.Add(writeResult.Message);
                            writer.Close();
                            data.RunEnable = false;
                            return;
                        }

                        writeResult = writer.WriteLine(new List<string>()
                        {
                            nameof(ProcessDebugInfo.Time),
                            nameof(ProcessDebugInfo.CpuUsage),
                            nameof(ProcessDebugInfo.WorkingSet),
                            nameof(ProcessDebugInfo.PrivateBytes),
                            nameof(ProcessDebugInfo.HandleCount)
                        });

                        if (!writeResult)
                        {
                            data.ErrorLog.Add(writeResult.Message);
                            writer.Close();
                            data.RunEnable = false;
                            return;
                        }

                        while (data.RunEnable)
                        {
                            ProcessDebugInfo info;
                            CommonLibrary.OperationResult getResult = processObserver.TryGetDebugInfo(out info);
                            if (!getResult)
                            {
                                data.ErrorLog.Add(getResult.Message);
                                break;
                            }

                            data.ProcessInfo = info;

                            writeResult = writer.WriteLine(new List<string>()
                            {
                                info.Time.ToString(),
                                info.CpuUsage.ToString(),
                                info.WorkingSet.ToString(),
                                info.PrivateBytes.ToString(),
                                info.HandleCount.ToString()
                            });

                            if (!writeResult)
                            {
                                data.ErrorLog.Add(writeResult.Message);
                                break;
                            }

                            Thread.Sleep(data.SamplePeriodMs);
                        }
                        processObserver.StopProcess();
                    }
                    else
                    {
                        data.ErrorLog.Add("Error starting process " + data.ProcessPath);
                    }
                    data.RunEnable = false;
                }

            }
            
        }

        private static string TimeToStr()
        {
            var time = DateTime.Now;
            return time.Hour.ToString() + "_" + time.Minute.ToString() + "_" + time.Second.ToString();
        }

        

        static void Main(string[] args)
        {
            //
            foreach(string arg in args)
            {
                if ((arg == "--h") || (arg == "-h") || (arg == "-help"))
                {
                    PrintHelpMessage();
                    return;
                }
            }


            if (args.Length>0)
            {
                AppData appData = new AppData();
                appData.ProcessPath = args[0];
                appData.RunEnable = true;
                appData.SamplePeriodMs = 1000;//default
                if (args.Length>1)
                {
                    int periodMsec = 1000;
                    if (!int.TryParse(args[1],out periodMsec))
                    {
                        PrintHelpMessage();
                        return;
                    }
                    if (periodMsec<=0)
                    {
                        PrintHelpMessage();
                        return;
                    }
                    appData.SamplePeriodMs = periodMsec;
                }

                Console.WriteLine("Process " + appData.ProcessPath + " observing...");
                Console.WriteLine();

                Thread t1 = new Thread(MeasureProcessRoutine);
                t1.Start(appData);
                
                
                PrintKeyControlMessage();

                bool printDataState = false;
                int printDelimiter = 0;
                while (appData.RunEnable)
                {

                    while (!Console.KeyAvailable)
                    {
                        if (!appData.RunEnable) break;

                        if (printDataState)
                        {
                            printDelimiter++;
                            if (printDelimiter >= 4)
                            {
                                PrintProcessInfo(appData.ProcessPath, appData.ProcessInfo);
                                PrintKeyControlMessage();
                                printDelimiter = 0;
                            }
                        }

                        Thread.Sleep(250);
                    }

                    if (appData.RunEnable)
                    {
                        var key = Console.ReadKey(true);

                        if (key.KeyChar == 'x' || key.KeyChar == 'X')
                        {
                            appData.RunEnable = false;
                        }
                        else if (key.KeyChar == 'p' || key.KeyChar == 'P')
                        {
                            printDataState = !printDataState;
                            printDelimiter = 0;
                        }
                    }
                }

                t1.Join();
                PrintErrorLog(appData.ErrorLog);
                return;
            }
            PrintHelpMessage();
        }


        static void PrintKeyControlMessage()
        {
            
            Console.WriteLine("press p to enable/disable print process data");
            Console.WriteLine("press x to stop process and exit");
            Console.WriteLine();
            
        }
    
        static void PrintHelpMessage()
        {
            Console.WriteLine("ProcessStatistics.exe");
            Console.WriteLine("Input format:");
            Console.WriteLine("ProcessStatistics [\"exe filename\"] [sample interval in ms(default = 1000)]");
        }

        static void PrintErrorLog(IEnumerable<string> messages)
        {
            foreach(var it in messages)
            {
                Console.WriteLine(it);
            }
        }

        static void PrintProcessInfo(string path, ProcessDebugInfo info)
        {
            Console.WriteLine(info.Time.ToLongTimeString() + 
                " "+path+" Cpu Load(%)=" + info.CpuUsage+
                " Working Set="+ info.WorkingSet+
                " Private bytes="+info.PrivateBytes+
                " Handle count="+info.HandleCount);
            Console.WriteLine();
        }
    
    }
}
