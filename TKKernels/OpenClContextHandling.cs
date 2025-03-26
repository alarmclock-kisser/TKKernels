using OpenTK;
using OpenTK.Compute.OpenCL;
using System.Text;

namespace TKKernels
{
	public class OpenClContextHandling
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Repopath;

		public ListBox LogBox;
		public ComboBox DevicesCombo;


		public Dictionary<CLPlatform, CLDevice[]> Devices = [];

		public CLContext? Ctx = null;
		public CLDevice? Dev = null;

		public OpenClMemoryHandling? MemH;
		public OpenClKernelHandling? KernelH;



		// ----- ----- ----- LAMBDA ----- ----- ----- \\







		// ----- ----- ----- CONSTRUCTOR ----- ----- ----- \\
		public OpenClContextHandling(string repopath, ListBox? logBox = null, ComboBox? devicesCombo = null)
		{
			// Set attributes
			this.Repopath = repopath;
			this.LogBox = logBox ?? new ListBox();
			this.DevicesCombo = devicesCombo ?? new ComboBox();

			// Register events
			DevicesCombo.SelectedIndexChanged += (s, e) => InitializeContext(DevicesCombo.SelectedIndex);

			// Get devices
			Devices = GetDevices();
			Log("OpenCL Devices loaded", Devices.Count + " platforms", 1);

			// Fill combo and set device
			FillDevicesCombo(-1);
		}






		// ----- ----- ----- METHODS ----- ----- ----- \\
		// Log
		public void Log(string message, string inner = "", int layer = 1, bool update = false)
		{
			string msg = "[" + DateTime.Now.ToString("HH:mm:ss.fff") + "] ";
			msg += "<CTX>";

			for (int i = 0; i <= layer; i++)
			{
				msg += " - ";
			}

			msg += message;

			if (inner != "")
			{
				msg += "  (" + inner + ")";
			}

			if (update)
			{
				LogBox.Items[^1] = msg;
			}
			else
			{
				LogBox.Items.Add(msg);
				LogBox.SelectedIndex = LogBox.Items.Count - 1;
			}
		}


		// Get devices
		public Dictionary<CLPlatform, CLDevice[]> GetDevices(DeviceType? type = null)
		{
			// Get platforms
			var err = CL.GetPlatformIds(out CLPlatform[] platforms);
			if (err != CLResultCode.Success)
			{
				Log("Error getting platforms", err.ToString());
				return [];
			}

			// Get devices for each platform
			Dictionary<CLPlatform, CLDevice[]> devices = [];
			foreach (var platform in platforms)
			{
				err = CL.GetDeviceIds(platform, type ?? DeviceType.All, out CLDevice[] devs);
				if (err != CLResultCode.Success)
				{
					Log("Error getting devices", err.ToString());
					return [];
				}
				devices[platform] = devs;
			}

			return devices;
		}

		public List<CLDevice> GetDevicesList(DeviceType? type = null)
		{
			List<CLDevice> devices = [];
			foreach (var devs in Devices.Values)
			{
				devices.AddRange(devs);
			}

			return devices;
		}

		public void FillDevicesCombo(int set = -1)
		{
			// Clear
			DevicesCombo.Items.Clear();
			
			// Fill
			foreach (var platform in Devices.Keys)
			{
				foreach (var device in Devices[platform])
				{
					DevicesCombo.Items.Add(GetPlatformName(platform) + " - " + GetDeviceName(device));
				}
			}

			// Set
			if (set >= 0 && set < DevicesCombo.Items.Count)
			{
				DevicesCombo.SelectedIndex = set;
			}
		}


		// Info
		public string GetDeviceName(CLDevice device)
		{
			// Get name
			var err = CL.GetDeviceInfo(device, DeviceInfo.Name, out byte[] name);
			if (err != CLResultCode.Success)
			{
				Log("Error getting device name", err.ToString());
				return "";
			}

			// Convert to string
			return Encoding.ASCII.GetString(name).Trim();

		}

		public string GetDeviceType(CLDevice device)
		{
			// Get type
			var err = CL.GetDeviceInfo(device, DeviceInfo.Type, out byte[] type);
			if (err != CLResultCode.Success)
			{
				Log("Error getting device type", err.ToString());
				return "";
			}

			// Convert to string
			return Encoding.ASCII.GetString(type).Trim();
		}

		public string GetPlatformName(CLPlatform platform)
		{
			// Get name
			var err = CL.GetPlatformInfo(platform, PlatformInfo.Name, out byte[] name);
			if (err != CLResultCode.Success)
			{
				Log("Error getting platform name", err.ToString());
				return "";
			}

			// Convert to string
			return Encoding.ASCII.GetString(name).Trim();
		}

		public string GetPlatformVersion(CLPlatform platform)
		{
			// Get vendor
			var err = CL.GetPlatformInfo(platform, PlatformInfo.Version, out byte[] version);
			if (err != CLResultCode.Success)
			{
				Log("Error getting platform vendor", err.ToString());
				return "";
			}

			// Convert to string
			return Encoding.ASCII.GetString(version).Trim();
		}

		public List<string> GetDeviceNames()
		{
			// Get devices
			List<CLDevice> devices = GetDevicesList();

			// Get names
			List<string> names = [];
			foreach (var device in devices)
			{
				names.Add(GetDeviceName(device));
			}

			return names;
		}


		// Context
		public void InitializeContext(int deviceIndex)
		{
			// Void previous
			Dispose();

			// Get devices
			List<CLDevice> devices = GetDevicesList();

			// Check index
			if (deviceIndex < 0 || deviceIndex >= devices.Count)
			{
				Log("No OpenCL Context initialized", "Invalid device index", 1);
				return;
			}

			// Get device
			Dev = devices[deviceIndex];

			// Create context -> Ctx
			Ctx = CL.CreateContext(0, [Dev.Value], 0, 0, out var err);
			if (err != CLResultCode.Success)
			{
				Log("Error creating context", err.ToString());
				Ctx = null;
				return;
			}

			// Create objects
			MemH = new OpenClMemoryHandling(this);
			KernelH = new OpenClKernelHandling(this);

			// Log
			Log("OpenCL Context initialized", GetDeviceName(Dev.Value), 1);
		}

		public void Dispose()
		{
			// Void context
			if (Ctx != null)
			{
				CL.ReleaseContext(Ctx.Value);
				Ctx = null;
			}

			// Void device
			if (Dev != null)
			{
				CL.ReleaseDevice(Dev.Value);
				Dev = null;
			}

			// Dispose objects
			MemH?.Dispose();
			MemH = null;
			KernelH?.Dispose();
			KernelH = null;
		}

	}
}
