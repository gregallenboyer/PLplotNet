using System;
using System.Text;

namespace PLplot
{

    /// <summary>A PLplot stream.</summary>
    /// <remarks>
    /// A new PLplot stream is created when this class is instantiated.
    /// The PLplot stream is ended when the instance is disposed.
    /// An instance is thread-safe, however all calls from all
    /// instances are serialized.
    /// </remarks>
    public partial class PLStream : IDisposable
    {
        static object libLock = new object();
        int streamId = -1;
        bool disposed = false; 

        /// <summary>Creates a new PLplot stream.</summary>
        public PLStream()
        {
            Native.mkstrm(out streamId);
            if (streamId < 0)
                throw new SystemException("cannot create PLplot stream");
        }

        /// <summary>Ends the PLplot stream.</summary>
        ~PLStream()
        {
            if (!disposed)
                (this as IDisposable).Dispose();
        }        

        /// <summary>Ends the PLplot stream.</summary>
        void IDisposable.Dispose()
        {
            EndStream();
            disposed = true;
        }

        /// <summary>The stream id of this stream as returned by plgstrm().</summary>    
        public int Id
        {
            get 
            { 
                if (disposed)
                    throw new ObjectDisposedException("PLplot stream was disposed");
                if (streamId < 0)
                    throw new SystemException("PLplot stream has ended");
                return streamId;                 
            }
        }

        public override string ToString() 
        {
            return String.Format("PLplot stream {0}", streamId);
        }

        private void ActivateStream()
        {
            Native.sstrm(Id);
        }

        private void EndStream()
        {
            lock (libLock)
            {
                if (streamId >= 0)
                {
                    ActivateStream();
                    Native.end1();
                    streamId = -1;
                }
            }
        }

        /// <summary>plcpstrm: Copy state parameters from the reference stream to the current stream</summary>
        /// <param name="iplsr">Reference stream.</param>
        /// <param name="flags">If flags is set to true the device coordinates are not copied from the reference to current stream.</param>
        /// <remarks>Copies state parameters from the reference stream to the current stream. 
        /// Tell driver interface to map device coordinates unless flags == 1.
        ///
        /// This function is used for making save files of selected plots (e.g. from the TK driver). 
        /// After initializing, you can get a copy of the current plot to the specified device by switching to 
        /// this stream and issuing a plcpstrm and a plreplot, with calls to plbop and pleop as appropriate. 
        /// The plot buffer must have previously been enabled (done automatically by some display drivers, such as X). 
        /// </remarks>
        public void cpstrm(PLStream iplsr, bool flags)
        {
            lock (libLock)
            {
                ActivateStream();
                Native.cpstrm(iplsr.Id, flags);
            }
        }

    }

}