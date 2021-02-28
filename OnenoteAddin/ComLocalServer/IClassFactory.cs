using System;
using System.Runtime.InteropServices;

namespace RemarkableSync.OnenoteAddin
{
	// Interface IClassFactory is here to provide a C# definition of the
	// COM IClassFactory interface.
	[
	  ComImport, // This interface originated from COM.
	  ComVisible(false), // It is not hard to imagine that this interface must not be exposed to COM.
	  InterfaceType(ComInterfaceType.InterfaceIsIUnknown), // Indicate that this interface is not IDispatch-based.
	  Guid("00000001-0000-0000-C000-000000000046")  // This GUID is the actual GUID of IClassFactory.
	]
	public interface IClassFactory
	{
		void CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject);
		void LockServer(bool fLock);
	}    
}
