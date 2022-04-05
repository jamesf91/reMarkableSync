using System;
using System.Threading;

namespace RemarkableSync.OnenoteAddin
{
	/// <summary>
	/// Summary description for GarbageCollection.
	/// </summary>
	class GarbageCollection
	{
		protected bool				m_bContinueThread;
		protected bool				m_GCWatchStopped;
		protected int				m_iInterval;
		protected ManualResetEvent	m_EventThreadEnded;

		public GarbageCollection(int iInterval)
		{
			m_bContinueThread = true;
			m_GCWatchStopped = false;
			m_iInterval = iInterval;
			m_EventThreadEnded = new ManualResetEvent(false);
		}

		public void GCWatch()
		{
			Logger.LogMessage("GarbageCollection.GCWatch() is now running on another thread.");
			// Pause for a moment to provide a delay to make threads more apparent.
			while (ContinueThread())
			{
				GC.Collect();
				Thread.Sleep(m_iInterval);
			}

			Logger.LogMessage("Goind to call m_EventThreadEnded.Set().");
			m_EventThreadEnded.Set();
		}

		protected bool ContinueThread()
		{
			lock(this)
			{
				return m_bContinueThread;
			}
		}

		public void StopThread()
		{
			lock(this)
			{
				Logger.LogMessage("Stopping thread.");
				m_bContinueThread = false;
			}
		}

		public void WaitForThreadToStop()
		{
			Logger.LogMessage("Called");
			m_EventThreadEnded.WaitOne();
			m_EventThreadEnded.Reset();
			Logger.LogMessage("Exiting.");
		}
	}
}
