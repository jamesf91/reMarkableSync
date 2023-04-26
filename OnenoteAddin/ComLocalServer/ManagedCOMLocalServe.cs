using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Text;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Collections;
using System.Configuration;

namespace RemarkableSync.OnenoteAddin
{
	[Flags]
	enum COINIT : uint
	{
		/// Initializes the thread for multi-threaded object concurrency.
		COINIT_MULTITHREADED = 0x0,
		/// Initializes the thread for apartment-threaded object concurrency. 
		COINIT_APARTMENTTHREADED = 0x2,
		/// Disables DDE for Ole1 support.
		COINIT_DISABLE_OLE1DDE = 0x4,
		/// Trades memory for speed.
		COINIT_SPEED_OVER_MEMORY = 0x8
	}

	[Flags]
	enum CLSCTX : uint
	{
		CLSCTX_INPROC_SERVER    = 0x1, 
		CLSCTX_INPROC_HANDLER   = 0x2, 
		CLSCTX_LOCAL_SERVER     = 0x4, 
		CLSCTX_INPROC_SERVER16  = 0x8,
		CLSCTX_REMOTE_SERVER    = 0x10,
		CLSCTX_INPROC_HANDLER16 = 0x20,
		CLSCTX_RESERVED1        = 0x40,
		CLSCTX_RESERVED2        = 0x80,
		CLSCTX_RESERVED3        = 0x100,
		CLSCTX_RESERVED4        = 0x200,
		CLSCTX_NO_CODE_DOWNLOAD = 0x400,
		CLSCTX_RESERVED5        = 0x800,
		CLSCTX_NO_CUSTOM_MARSHAL= 0x1000,
		CLSCTX_ENABLE_CODE_DOWNLOAD = 0x2000,
		CLSCTX_NO_FAILURE_LOG   = 0x4000,
		CLSCTX_DISABLE_AAA      = 0x8000,
		CLSCTX_ENABLE_AAA       = 0x10000,
		CLSCTX_FROM_DEFAULT_CONTEXT = 0x20000,
		CLSCTX_INPROC           = CLSCTX_INPROC_SERVER|CLSCTX_INPROC_HANDLER,
		CLSCTX_SERVER           = CLSCTX_INPROC_SERVER|CLSCTX_LOCAL_SERVER|CLSCTX_REMOTE_SERVER,
		CLSCTX_ALL				= CLSCTX_SERVER|CLSCTX_INPROC_HANDLER
	}

	[Flags]
	enum REGCLS : uint
	{ 
		REGCLS_SINGLEUSE         = 0, 
		REGCLS_MULTIPLEUSE       = 1, 
		REGCLS_MULTI_SEPARATE    = 2, 
		REGCLS_SUSPENDED         = 4, 
		REGCLS_SURROGATE         = 8
	}

	// We import the POINT structure because it is referenced
	// by the MSG structure.
	[ComVisible(false)]
	[StructLayout( LayoutKind.Sequential )]
	public struct POINT 
	{
		public int X;
		public int Y;

		public POINT( int x, int y ) 
		{
			this.X = x;
			this.Y = y;
		}

		public static implicit operator Point( POINT p ) 
		{
			return new Point( p.X,  p.Y );
		}

		public static implicit operator POINT( Point p ) 
		{
			return new POINT( p.X, p.Y );
		}
	}

	// We import the MSG structure because it is referenced 
	// by the GetMessage(), TranslateMessage() and DispatchMessage()
	// Win32 APIs.
	[ComVisible(false)]
	[StructLayout(LayoutKind.Sequential)]
	public struct MSG
	{
		public IntPtr hwnd;
		public uint message;
		public IntPtr wParam;
		public IntPtr lParam;
		public uint time;
		public POINT pt;
	}

	// Note that ManagedCOMLocalServer is NOT declared as public.
    // This is so that it will not be exposed to COM when we call regasm
	// or tlbexp.
	class ManagedCOMLocalServer
	{
		// CoInitializeEx() can be used to set the apartment model
		// of individual threads.
		[DllImport("ole32.dll")]
		static extern int CoInitializeEx(IntPtr pvReserved, uint dwCoInit);

		// CoUninitialize() is used to uninitialize a COM thread.
		[DllImport("ole32.dll")]
		static extern void CoUninitialize();

		// PostThreadMessage() allows us to post a Windows Message to
		// a specific thread (identified by its thread id).
		// We will need this API to post a WM_QUIT message to the main 
		// thread in order to terminate this application.
		[DllImport("user32.dll")]
		static extern bool PostThreadMessage(uint idThread, uint Msg, UIntPtr wParam,
			IntPtr lParam);

		// GetCurrentThreadId() allows us to obtain the thread id of the
		// calling thread. This allows us to post the WM_QUIT message to
		// the main thread.
		[DllImport("kernel32.dll")]
		static extern uint GetCurrentThreadId();

		// We will be manually performing a Message Loop within the main thread
		// of this application. Hence we will need to import GetMessage(), 
		// TranslateMessage() and DispatchMessage().
		[DllImport("user32.dll")]
		static extern bool GetMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin,
			uint wMsgFilterMax);

		[DllImport("user32.dll")]
		static extern bool TranslateMessage([In] ref MSG lpMsg);

		[DllImport("user32.dll")]
		static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

		// Define two common GUID objects for public usage.
		public static Guid IID_IUnknown  = new Guid("{00000000-0000-0000-C000-000000000046}");
		public static Guid IID_IDispatch = new Guid("{00020400-0000-0000-C000-000000000046}");

		protected static uint	m_uiMainThreadId;  // Stores the main thread's thread id.
		protected static int	m_iObjsInUse;  // Keeps a count on the total number of objects alive.
		protected static int	m_iServerLocks;// Keeps a lock count on this application.

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        // This property returns the main thread's id.
        public static uint MainThreadId
		{
			get
			{
				return m_uiMainThreadId;
			}
		}

		// This method performs a thread-safe incrementation of the objects count.
		public static int InterlockedIncrementObjectsCount()
		{
			Logger.Debug("Called");
			// Increment the global count of objects.
			return Interlocked.Increment(ref m_iObjsInUse);
		}

		// This method performs a thread-safe decrementation the objects count.
		public static int InterlockedDecrementObjectsCount()
		{
			Logger.Debug("Called");
			// Decrement the global count of objects.
			return Interlocked.Decrement(ref m_iObjsInUse);
		}

		// Returns the total number of objects alive currently.
		public static int ObjectsCount
		{
			get
			{
				lock(typeof(ManagedCOMLocalServer))
				{
					return m_iObjsInUse;
				}
			}
		}

		// This method performs a thread-safe incrementation the 
		// server lock count.
		public static int InterlockedIncrementServerLockCount()
		{
			Logger.Debug("Called");
			// Increment the global lock count of this server.
			return Interlocked.Increment(ref m_iServerLocks);
		}

		// This method performs a thread-safe decrementation the 
		// server lock count.
		public static int InterlockedDecrementServerLockCount()
		{
			Logger.Debug("Called");
			// Decrement the global lock count of this server.
			return Interlocked.Decrement(ref m_iServerLocks);
		}

		// Returns the current server lock count.
		public static int ServerLockCount
		{
			get
			{
				lock(typeof(ManagedCOMLocalServer))
				{
					return m_iServerLocks;
				}
			}
		}

		// AttemptToTerminateServer() will check to see if 
		// the objects count and the server lock count has
		// both dropped to zero.
		// If so, we post a WM_QUIT message to the main thread's
		// message loop. This will cause the message loop to
		// exit and hence the termination of this application.
		public static void AttemptToTerminateServer()
		{
			lock(typeof(ManagedCOMLocalServer))
			{
				Logger.Debug("Called");

				// Get the most up-to-date values of these critical data.
				int iObjsInUse = ObjectsCount;
				int iServerLocks = ServerLockCount;

				// Print out these info for debug purposes.
				StringBuilder sb = new StringBuilder("");		  
				sb.AppendFormat("m_iObjsInUse : {0}. m_iServerLocks : {1}", iObjsInUse, iServerLocks);
				Logger.Debug(sb.ToString());

				if ((iObjsInUse > 0) || (iServerLocks > 0))
				{
					Logger.Debug("There are still referenced objects or the server lock count is non-zero.");
				}
				else
				{
					UIntPtr wParam = new UIntPtr(0);
					IntPtr lParam = new IntPtr(0);
					Logger.Debug("PostThreadMessage(WM_QUIT)");
					PostThreadMessage(MainThreadId, 0x0012, wParam, lParam);
				}
			}
		}

		// ProcessArguments() will process the command-line arguments
		// of this application. 
		// If the return value is true, we carry
		// on and start this application.
		// If the return value is false, we terminate
		// this application immediately.
		protected static bool ProcessArguments(string[] args)
		{
			bool bRet = true;

			if (args.Length > 0)
			{
				RegistryKey key = null;
				RegistryKey key2 = null;

				switch (args[0].ToLower())
				{
					case "-embedding":
						Logger.Debug("Request to start as out-of-process COM server.");
						break;

					case "-register":
					case "/register":
						try 
						{
							key = Registry.ClassesRoot.CreateSubKey("CLSID\\" + Marshal.GenerateGuidForType(typeof(AddIn)).ToString("B"));
							key2 = key.CreateSubKey("LocalServer32");
							key2.SetValue(null, Application.ExecutablePath);
						} 
						catch (Exception ex)
						{
							MessageBox.Show("Error while registering the server:\n"+ex.ToString());
							Logger.Error("Error while registering the server:\n" + ex.ToString());
						}
						finally
						{
							if (key != null)
								key.Close();
							if (key2 != null)
								key2.Close();
						}
						bRet = false;
						break;

					case "-unregister":
					case "/unregister":
						try 
						{
							key = Registry.ClassesRoot.OpenSubKey("CLSID\\" + Marshal.GenerateGuidForType(typeof(AddIn)).ToString("B"), true);
							key.DeleteSubKey("LocalServer32");
						} 
						catch (Exception ex)
						{
							MessageBox.Show("Error while unregistering the server:\n"+ex.ToString());
						}
						finally
						{
							if (key != null)
								key.Close();
							if (key2 != null)
								key2.Close();
						}
						bRet = false;
						break;

					default:
						Logger.Info("Unknown argument: " + args[0] + "\nValid are : -register, -unregister and -embedding");
						break;
				}
			}

			return bRet;
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			if (!ProcessArguments(args))
			{
				return;
			}

			// Initialize critical member variables.
			m_iObjsInUse = 0;
			m_iServerLocks = 0;
			m_uiMainThreadId = GetCurrentThreadId();

			// Register the SimpleCOMObjectClassFactory.
			AddInClassFactory factory = new AddInClassFactory();
			factory.ClassContext = (uint)CLSCTX.CLSCTX_LOCAL_SERVER;
			factory.ClassId = Marshal.GenerateGuidForType(typeof(AddIn));
			factory.Flags = (uint)REGCLS.REGCLS_MULTIPLEUSE | (uint)REGCLS.REGCLS_SUSPENDED;
			factory.RegisterClassObject();
			ClassFactoryBase.ResumeClassObjects();

			// Start up the garbage collection thread.
			GarbageCollection	GarbageCollector = new GarbageCollection(1000);
			Thread				GarbageCollectionThread = new Thread(new ThreadStart(GarbageCollector.GCWatch));
			
			// Set the name of the thread object.
			GarbageCollectionThread.Name = "Garbage Collection Thread";
			// Start the thread.
			GarbageCollectionThread.Start();

			// Start the message loop.
			MSG			msg;
			IntPtr		null_hwnd = new IntPtr(0);
			while (GetMessage(out msg, null_hwnd, 0, 0) != false) 
			{
				TranslateMessage(ref msg);
				DispatchMessage(ref msg);
			}
			Logger.Debug("Out of message loop.");

			// Revoke the class factory immediately.
			// Don't wait until the thread has stopped before
			// we perform revokation.
			factory.RevokeClassObject();
			Logger.Debug("SimpleCOMObjectClassFactory Revoked.");

			// Now stop the Garbage Collector thread.
			GarbageCollector.StopThread();
			GarbageCollector.WaitForThreadToStop();
			Logger.Debug("GarbageCollector thread stopped.");
		}
	}
}
