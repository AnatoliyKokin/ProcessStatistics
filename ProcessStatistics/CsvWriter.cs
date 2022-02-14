using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace ProcessStatistics
{
    
    class CsvWriter : IDisposable
    {
        private StreamWriter mFileWriter;
        private bool mIsOpen = false;
        private char mSeparator = ';';
        public CsvWriter(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; private set; } 

        public char Separator 
        { 
            get 
            { return mSeparator; } 

            set
            {
                if ((value == ';') || (value == ','))
                {
                    mSeparator = value;
                }
            }
        }

        public CommonLibrary.OperationResult Open()
        {
            if (mIsOpen) return CommonLibrary.OperationResult.OK;
            try
            {
                mFileWriter = new StreamWriter(FileName);
                
                mIsOpen = true;
            }
            catch(UnauthorizedAccessException)
            {
                mIsOpen = false;
                return CommonLibrary.OperationResult.Error("No root to create file " + FileName);
            }
            catch (ArgumentException)
            {
                mIsOpen = false;
                return CommonLibrary.OperationResult.Error("Wrong file name " + FileName);
            }
            catch (PathTooLongException)
            {
                mIsOpen = false;
                return CommonLibrary.OperationResult.Error("Wrong file name " + FileName);
            }
            catch (DirectoryNotFoundException)
            {
                mIsOpen = false;
                return CommonLibrary.OperationResult.Error("Path not found " + FileName);
            }
            catch(Exception)
            {
                mIsOpen = false;
                return CommonLibrary.OperationResult.Error("Error open file " + FileName);
            }

            disposedValue = false;

            if (mIsOpen)
            {
                try
                {
                    mFileWriter.WriteLine("sep=" + mSeparator.ToString());
                }
                catch(ObjectDisposedException)
                {
                    mIsOpen = false;
                    return CommonLibrary.OperationResult.Error("Object StreamWriter is disposed");
                }
            }

            return CommonLibrary.OperationResult.OK;
        }

        public CommonLibrary.OperationResult WriteLine(IList<string> args)
        {
            if (!mIsOpen) return CommonLibrary.OperationResult.Error("Файл "+FileName+ " не открыт для записи");

            string writeStr = "";
            for(int i=0;i<args.Count-1;i++)
            {
                writeStr += args[i];
                writeStr += mSeparator.ToString();
            }
            if (args.Count>0) writeStr += args[args.Count - 1];

            try
            {
                mFileWriter.WriteLine(writeStr);
            }
            catch (ObjectDisposedException)
            {
                return CommonLibrary.OperationResult.Error("Object StreamWriter is disposed");
            }
            return CommonLibrary.OperationResult.OK;
        }
        public CommonLibrary.OperationResult Close()
        {
            if (!mIsOpen) return CommonLibrary.OperationResult.Error("File " + FileName+ " already closed");
            try
            {
                if (mFileWriter != null)
                {
                    mFileWriter.Close();
                }
                mIsOpen = false;
            }
            catch (EncoderFallbackException)
            {
                return CommonLibrary.OperationResult.Error("Error close file "+FileName);
            }

            return CommonLibrary.OperationResult.OK;
        }

        #region IDisposable Support
        private bool disposedValue = true; // Для определения избыточных вызовов

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    mFileWriter.Dispose();
                }

                mFileWriter = null;
                FileName = null;
                mIsOpen = false;
                disposedValue = true;
            }
        }

        ~CsvWriter()
        {
            if (mIsOpen) mFileWriter.Close();
            Dispose(false);
        }

        // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

    }
}
