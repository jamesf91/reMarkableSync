using System;
using System.Runtime.InteropServices;

namespace RemarkableSync.OnenoteAddin
{
	class ClassFactoryBase : IClassFactory
	{
		// CoRegisterClassObject() is used to register a Class Factory
		// into COM's internal table of Class Factories.
		[DllImport("ole32.dll")]
		static extern int CoRegisterClassObject([In] ref Guid rclsid,
			[MarshalAs(UnmanagedType.IUnknown)] object pUnk, uint dwClsContext,
			uint flags, out uint lpdwRegister);

		// Called by an COM EXE Server that can register multiple class objects 
		// to inform COM about all registered classes, and permits activation 
		// requests for those class objects. 
		// This function causes OLE to inform the SCM about all the registered 
		// classes, and begins letting activation requests into the server process.
		[DllImport("ole32.dll")]
		static extern int CoResumeClassObjects();

		// CoRevokeClassObject() is used to unregister a Class Factory
		// from COM's internal table of Class Factories.
		[DllImport("ole32.dll")]
		static extern int CoRevokeClassObject(uint dwRegister);

		public ClassFactoryBase()
		{
		}

		protected UInt32	m_locked = 0;
		protected uint		m_ClassContext = (uint)CLSCTX.CLSCTX_LOCAL_SERVER;
		protected Guid		m_ClassId;
		protected uint		m_Flags;
		protected uint		m_Cookie;

		public virtual void virtual_CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
		{
			IntPtr nullPtr = new IntPtr(0);
			ppvObject = nullPtr;
		}

		public uint ClassContext
		{
			get
			{
				return m_ClassContext;
			}
			set
			{
				m_ClassContext = value;
			}
		}

		public Guid ClassId
		{
			get
			{
				return m_ClassId;
			}
			set
			{
				m_ClassId = value;
			}
		}

		public uint Flags
		{
			get
			{
				return m_Flags;
			}
			set
			{
				m_Flags = value;
			}
		}

		public bool RegisterClassObject()
		{
			// Register the class factory
			int i = CoRegisterClassObject
				(
				ref m_ClassId, 
				this, 
				ClassContext, 
				Flags,
				out m_Cookie
				);

			if (i == 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public bool RevokeClassObject()
		{
			int i = CoRevokeClassObject(m_Cookie);

			if (i == 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool ResumeClassObjects()
		{
			int i = CoResumeClassObjects();

			if (i == 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		#region IClassFactory Implementations
		public void CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
		{
			virtual_CreateInstance(pUnkOuter, ref riid, out ppvObject);
		}

		public void LockServer(bool bLock)
		{
			if (bLock)
			{
				ManagedCOMLocalServer.InterlockedIncrementServerLockCount();
			}
			else
			{
				ManagedCOMLocalServer.InterlockedDecrementServerLockCount();
			}

			// Always attempt to see if we need to shutdown this server application.
			ManagedCOMLocalServer.AttemptToTerminateServer();
		}
		#endregion
	}
}
