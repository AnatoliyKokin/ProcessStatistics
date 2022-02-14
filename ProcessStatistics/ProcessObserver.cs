using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace ProcessStatistics
{

    class ProcessObserver : IDisposable
    {


        object locker = new object();
        public ProcessObserver(string fileName)
        {
            mProcess = new Process();
            mProcess.StartInfo.UseShellExecute = false;
            mProcess.StartInfo.FileName = fileName;
            mProcess.StartInfo.CreateNoWindow = true;
            
        }

        public CommonLibrary.OperationResult StartProcess()
        {
            try
            {
                mProcess.Start();
                
            }
            catch(Exception)
            {
                return CommonLibrary.OperationResult.Error("Error starting process "+mProcess.StartInfo.FileName);
            }


            int tryCounter = 30;
            CommonLibrary.OperationResult result = CommonLibrary.OperationResult.Error("Error starting process " + mProcess.StartInfo.FileName);
            while (tryCounter>0)
            {
                try
                {
                    mProcess.Refresh();
                    var process = Process.GetProcessById(mProcess.Id);
                    mOldCpuTime = mProcess.TotalProcessorTime;
                    if ((process!=null)&&(mOldCpuTime.Ticks>0))
                     { tryCounter = 0; result = CommonLibrary.OperationResult.OK; }
                }
                catch(Exception)
                {
                    tryCounter++;
                    Thread.Sleep(1000);
                }
                
            }

            if (result != CommonLibrary.OperationResult.OK) return result;

            mStartTime.Start();

            return CommonLibrary.OperationResult.OK;
        }

        public CommonLibrary.OperationResult StopProcess()
        {
            if (mProcess!=null)
            {
                try
                {
                    if (!mProcess.HasExited)
                    {
                        mProcess.Kill();
                    }
                }
                catch(Exception e)
                {
                    return CommonLibrary.OperationResult.Error(e.Message);
                }
            }
            return CommonLibrary.OperationResult.OK;
        }

        public CommonLibrary.OperationResult TryGetDebugInfo(out ProcessDebugInfo info)
        {
            info = new ProcessDebugInfo();

            try
                {
                lock (locker)
                {
                    mProcess.Refresh();
                    TimeSpan processTime = mProcess.TotalProcessorTime;
                    TimeSpan newCpuTime = processTime - mOldCpuTime;
                    TimeSpan totalCpuTime = mStartTime.Elapsed;
                    double cpuUsage = 100.0 * newCpuTime.TotalMilliseconds / (Environment.ProcessorCount * totalCpuTime.TotalMilliseconds);
                    mStartTime.Restart();
                    mOldCpuTime = processTime;
                    info.CpuUsage = (float)cpuUsage;
                    

                }
                    
                    info.HandleCount = mProcess.HandleCount;
                    info.WorkingSet = mProcess.WorkingSet64;
                    info.PrivateBytes = mProcess.PrivateMemorySize64;
                    info.Time = DateTime.Now;
                    if (info.CpuUsage > 100.0f)
                    {
                        return CommonLibrary.OperationResult.Error("Too short observer sample time");
                    }
            }
            catch (Exception)
            {

                    if (mProcess.HasExited)
                    {
                       
                        return CommonLibrary.OperationResult.Error("Observed process is finished");
                    }
                    return CommonLibrary.OperationResult.Error("Can't get data observed process data");
            }
            
            return CommonLibrary.OperationResult.OK;
        }


        private Process mProcess;
        private TimeSpan mOldCpuTime= new TimeSpan();
        Stopwatch mStartTime = new Stopwatch();

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (mProcess!=null)
                    {
                        mProcess.Dispose();
                        
                    }
                }

                mStartTime.Stop();
                mProcess = null;

                disposedValue = true;
            }
        }


        // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
        public void Dispose()
        {
            Dispose(true);

        }
        #endregion

    }
}
