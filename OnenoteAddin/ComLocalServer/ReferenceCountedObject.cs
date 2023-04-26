using System;
using System.Runtime.InteropServices;

namespace RemarkableSync.OnenoteAddin
{
	/// <summary>
	/// Summary description for ReferenceCountedObjectBase.
	/// </summary>
	[ComVisible(false)]  // This ComVisibleAttribute is set to false so that TLBEXP and REGASM will not expose it nor COM-register it.
	public class ReferenceCountedObjectBase
	{
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public ReferenceCountedObjectBase()
		{
			Logger.Debug("constructor called");
			// We increment the global count of objects.
			ManagedCOMLocalServer.InterlockedIncrementObjectsCount();
		}

		~ReferenceCountedObjectBase()
		{
			Logger.Debug("destructor called");
			// We decrement the global count of objects.
			ManagedCOMLocalServer.InterlockedDecrementObjectsCount();
			// We then immediately test to see if we the conditions
			// are right to attempt to terminate this server application.
			ManagedCOMLocalServer.AttemptToTerminateServer();
		}
	}
}
