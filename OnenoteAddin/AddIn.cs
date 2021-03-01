using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Extensibility;
using Microsoft.Office.Core;
using Application = Microsoft.Office.Interop.OneNote.Application;  // Conflicts with System.Windows.Forms

namespace RemarkableSync.OnenoteAddin
{
	[ComVisible(true)]
	[Guid("B5DBC585-DBEE-4572-8760-C8119FAAA522")]
	[ProgId("RemarkableSync.OnenoteAddin")]

	public class AddIn : IDTExtensibility2, IRibbonExtensibility
	{
		protected Application OneNoteApplication
		{ get; set; }

		private RmDownloadForm _downloadForm;
		private ReferenceCountedObjectBase _refCountObj;

		public AddIn()
		{
			_refCountObj = new ReferenceCountedObjectBase();
		}

		~AddIn()
        {
			_refCountObj = null;

		}

		/// <summary>
		/// Returns the XML in Ribbon.xml so OneNote knows how to render our ribbon
		/// </summary>
		/// <param name="RibbonID"></param>
		/// <returns></returns>
		public string GetCustomUI(string RibbonID)
		{
			Console.WriteLine("AddIn.GetCustomUI(). called");
			return Properties.Resources.ribbon;
		}

		public void OnAddInsUpdate(ref Array custom)
		{
		}

		/// <summary>
		/// Cleanup
		/// </summary>
		/// <param name="custom"></param>
		public void OnBeginShutdown(ref Array custom)
		{
			_downloadForm?.Invoke(new Action(() =>
			{
				// close the form on the forms thread
				_downloadForm?.Close();
				_downloadForm = null;
			}));
		}

		/// <summary>
		/// Called upon startup.
		/// Keeps a reference to the current OneNote application object.
		/// </summary>
		/// <param name="application"></param>
		/// <param name="connectMode"></param>
		/// <param name="addInInst"></param>
		/// <param name="custom"></param>
		public void OnConnection(object Application, ext_ConnectMode ConnectMode, object AddInInst, ref Array custom)
		{
			SetOneNoteApplication((Application)Application);
		}

		public void SetOneNoteApplication(Application application)
		{
			OneNoteApplication = application;
		}

		/// <summary>
		/// Cleanup
		/// </summary>
		/// <param name="RemoveMode"></param>
		/// <param name="custom"></param>
		[SuppressMessage("Microsoft.Reliability", "CA2001:AvoidCallingProblematicMethods", MessageId = "System.GC.Collect")]
		public void OnDisconnection(ext_DisconnectMode RemoveMode, ref Array custom)
		{
			OneNoteApplication = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		public void OnStartupComplete(ref Array custom)
		{
		}

		public async Task onDownloadButtonClicked(IRibbonControl control)
		{
			ShowDownloadForm();
			return;
		}

		public async Task onSettingsClicked(IRibbonControl control)
        {
			return;
        }

		private void ShowDownloadForm()
		{
			if (_downloadForm == null)
			{
				_downloadForm = new RmDownloadForm(OneNoteApplication);
				System.Windows.Forms.Application.Run(_downloadForm);
				_downloadForm = null;
			}
			else
            {
				_downloadForm.Focus();
            }
		}

		/// <summary>
		/// Specified in Ribbon.xml, this method returns the image to display on the ribbon button
		/// </summary>
		/// <param name="imageName"></param>
		/// <returns></returns>
		public IStream GetImage(string imageName)
		{
			MemoryStream imageStream = new MemoryStream();
			switch (imageName)
            {
				case "DownloadDoc":
					Properties.Resources.DownloadDoc.Save(imageStream, ImageFormat.Png);
					break;
				case "Settings":
					Properties.Resources.Settings.Save(imageStream, ImageFormat.Png);
					break;
				case "Logo":
				default:
					Properties.Resources.Logo.Save(imageStream, ImageFormat.Png);
					break;
            }
			return new CCOMStreamWrapper(imageStream);
		}
	}

	class AddInClassFactory : ClassFactoryBase
	{
		public override void virtual_CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
		{
			Console.WriteLine("AddInClassFactory.CreateInstance().");
			Console.WriteLine("Requesting Interface : " + riid.ToString());

			if (riid == Marshal.GenerateGuidForType(typeof(IDTExtensibility2)) ||
				riid == ManagedCOMLocalServer.IID_IDispatch ||
				riid == ManagedCOMLocalServer.IID_IUnknown)
			{
				AddIn addInObj = new AddIn();

				ppvObject = Marshal.GetComInterfaceForObject(addInObj, typeof(IDTExtensibility2));
			}
			else if (riid == Marshal.GenerateGuidForType(typeof(IRibbonExtensibility)))
            {
				AddIn addInObj = new AddIn();

				ppvObject = Marshal.GetComInterfaceForObject(addInObj, typeof(IRibbonExtensibility));
			}
			else
			{
				throw new COMException("No interface", unchecked((int)0x80004002));
			}
		}
	}
}
