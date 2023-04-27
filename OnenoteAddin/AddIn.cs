using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Extensibility;
using Microsoft.Office.Core;
using Microsoft.Office.Interop.OneNote;
using Microsoft.Win32;
using Application = Microsoft.Office.Interop.OneNote.Application;  // Conflicts with System.Windows.Forms

namespace RemarkableSync.OnenoteAddin
{
	[ComVisible(true)]
	[Guid("B5DBC585-DBEE-4572-8760-C8119FAAA522")]
	[ProgId("RemarkableSync.OnenoteAddin")]

	public class AddIn : IDTExtensibility2, IRibbonExtensibility
	{
		[DllImport("USER32.DLL")]
		private static extern bool SetForegroundWindow(IntPtr hWnd);

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected Application OneNoteApplication
		{ get; set; }

		private const string _settingsRegPath = @"Software\Microsoft\Office\OneNote\AddInsData\RemarkableSync.OnenoteAddin";
		private const string _useLoggingRegKey = @"LogFile";

		private RmDownloadForm _downloadForm;
		private SettingsForm _settingForm;
		private Thread _downloadFormThread;
		private Thread _settingFormThread;
		private ReferenceCountedObjectBase _refCountObj;
		private FileStream _filestream;
		private StreamWriter _streamwriter;

		internal class CWin32WindowWrapper : IWin32Window
		{
			private readonly IntPtr _windowHandle;

			public CWin32WindowWrapper(IntPtr windowHandle)
			{
				_windowHandle = windowHandle;
			}

			public IntPtr Handle
			{
				get { return _windowHandle; }
			}
		}

		public AddIn()
		{
			_refCountObj = new ReferenceCountedObjectBase();
			_downloadFormThread = null;
			_settingFormThread = null;
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
			Logger.Debug("called");
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

			_settingForm?.Invoke(new Action(() =>
			{
				// close the form on the forms thread
				_settingForm?.Close();
				_settingForm = null;
			}));

			_streamwriter?.Close();
			_filestream?.Close();
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
			CheckConsoleRedirect();
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
			if (_downloadFormThread == null)
			{
				Window context = control.Context as Window;
				CWin32WindowWrapper owner = new CWin32WindowWrapper((IntPtr)context.WindowHandle);

				_downloadFormThread = new Thread(ShowDownloadForm);
				_downloadFormThread.SetApartmentState(ApartmentState.STA);
				_downloadFormThread.Start(owner);
			}
			else
			{
				// shouldn't happen as the download form is modal
				_downloadForm?.Invoke(new Action(() => {
					SetForegroundWindow(_downloadForm.Handle);
				}));
			}
			return;
		}

		public async Task onSettingsClicked(IRibbonControl control)
        {
			if (_settingFormThread == null)
			{
				Window context = control.Context as Window;
				CWin32WindowWrapper owner = new CWin32WindowWrapper((IntPtr)context.WindowHandle);

				_settingFormThread = new Thread(ShowSettingsForm);
				_settingFormThread.SetApartmentState(ApartmentState.STA);
				_settingFormThread.Start(owner);
			}
			else
			{
				// shouldn't happen as the setting form is modal
				_settingForm?.Invoke(new Action(() => {
					SetForegroundWindow(_downloadForm.Handle);
				}));
			}
			return;
		}

		private void ShowDownloadForm(dynamic owner)
		{
			System.Windows.Forms.Application.EnableVisualStyles();
			_downloadForm = new RmDownloadForm(OneNoteApplication, _settingsRegPath);
			_downloadForm.Visible = false;
			_downloadForm.ShowDialog(owner);
			_downloadForm = null;
			_downloadFormThread = null;
		}

		private void ShowSettingsForm(dynamic owner)
		{
			System.Windows.Forms.Application.EnableVisualStyles();
			_settingForm = new SettingsForm(_settingsRegPath);
			_settingForm.Visible = false;
			_settingForm.ShowDialog(owner);
			_settingForm = null;
			_settingFormThread = null;
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
					Properties.Resources.DownloadDoc.Save(imageStream, ImageFormat.Png);
					break;
            }
			return new CCOMStreamWrapper(imageStream);
		}

		private void CheckConsoleRedirect()
		{
			string regValue = null; ;
			try
			{
				var settingsKey = Registry.CurrentUser.OpenSubKey(_settingsRegPath);
				regValue = (string)settingsKey.GetValue(_useLoggingRegKey, null);
			}
			catch (Exception err)
			{
				Logger.Error($"Unable to get \"{_settingsRegPath}\" regkey. Error: {err.Message}");
				return;
			}

			if (regValue == null || regValue.Length == 0)
			{
				return;
			}

			_filestream = new FileStream(regValue, FileMode.Create);
			_streamwriter = new StreamWriter(_filestream);
			_streamwriter.AutoFlush = true;
			Console.SetOut(_streamwriter);
			Console.SetError(_streamwriter);
		}
	}

	class AddInClassFactory : ClassFactoryBase
	{
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public override void virtual_CreateInstance(IntPtr pUnkOuter, ref Guid riid, out IntPtr ppvObject)
		{
			Logger.Debug("AddInClassFactory.CreateInstance().");
			Logger.Debug("Requesting Interface : " + riid.ToString());

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
