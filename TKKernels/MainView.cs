using System.Text;

namespace TKKernels
{
	public partial class MainView : Form
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Repopath;

		public ImageHandling ImgH;
		public OpenClContextHandling ContextH;
		public GuiBuilder GuiB;


		private int oldZoom;
		private Point mouseDownLocation;
		private bool isDragging = false;

		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		public ImageObject? Img => this.ImgH.CurrentImage;


		public OpenClMemoryHandling? MemH => this.ContextH.MemH;
		public OpenClKernelHandling? KernelH => this.ContextH.KernelH;



		// ----- ----- ----- CONSTRUCTOR ----- ----- ----- \\
		public MainView()
		{
			this.InitializeComponent();

			// Set repopath & oldzoom
			this.Repopath = this.GetRepopath(true);
			this.oldZoom = (int) this.numericUpDown_zoom.Value;

			// Window position
			this.StartPosition = FormStartPosition.Manual;
			this.Location = new Point(0, 0);

			// Init. classes
			this.ImgH = new ImageHandling(this.Repopath, this.listBox_images);
			this.ContextH = new OpenClContextHandling(this.Repopath, this.listBox_log, this.comboBox_devices);
			this.GuiB = new GuiBuilder(this, this.listBox_log);

			// Register events
			this.comboBox_devices.SelectedIndexChanged += (s, e) => this.ToggleUI();
			this.pictureBox_view.DoubleClick += (s, e) => this.ImportImage();
			this.panel_view.DoubleClick += (s, e) => this.ImportImage();
			this.listBox_images.DoubleClick += (s, e) => this.MoveImage();
			this.listBox_images.SelectedIndexChanged += (s, e) => this.ToggleUI();
			this.pictureBox_view.MouseWheel += this.pictureBox_view_MouseWheel;
			this.pictureBox_view.MouseDown += this.pictureBox_view_MouseDown;
			this.pictureBox_view.MouseMove += this.pictureBox_view_MouseMove;
			this.pictureBox_view.MouseUp += this.pictureBox_view_MouseUp;
			this.listBox_log.DoubleClick += (s, e) => this.ExportLogLineToClipboard(this.listBox_log.SelectedIndex);
			this.listBox_log.Click += (s, e) => this.ExportLogFullToFile();
			this.listBox_kernels.DoubleClick += (s, e) => this.KernelH?.LoadKernel(listBox_kernels.SelectedItem?.ToString() ?? "");


			// Start UI
			this.ToggleUI();

			// Select device
			this.SelectDeviceLike("Intel");

			// Load resources
			this.LoadResources();
		}






		// ----- ----- ----- METHODS ----- ----- ----- \\
		// Meta
		public string GetRepopath(bool root = false)
		{
			string repo = AppDomain.CurrentDomain.BaseDirectory;

			if (root)
			{
				repo += @"..\..\..\";
			}

			repo = Path.GetFullPath(repo);

			return repo;
		}

		public void ToggleUI()
		{
			// PictureBox
			this.pictureBox_view.Image = this.Img?.Img;
			this.pictureBox_view.SizeMode = PictureBoxSizeMode.Zoom;

			// Zoom (change size of PictureBox))
			this.oldZoom = (int) this.numericUpDown_zoom.Value;
			this.pictureBox_view.Width = (int) (this.Img?.Img?.Width * this.oldZoom / 100.0 ?? 1);
			this.pictureBox_view.Height = (int) (this.Img?.Img?.Height * this.oldZoom / 100.0 ?? 1);

			// Memory label
			long total = (this.MemH?.GetMemoryTotal() / 1024 / 1024) ?? 0;
			long used = (this.MemH?.GetMemoryAllocated<byte>() / 1024 / 1024) ?? 0;
			this.label_memory.Text = "Memory: " + used + " / " + total + " MB";

			// Size label
			this.label_size.Text = "Size: " + (this.Img?.Width ?? 0) + " x " + (this.Img?.Height ?? 0) + " px, " + (this.Img?.Size ?? 0) / 1024 + " KB";

			// Fill kernels
			this.KernelH?.FillKernelsList(listBox_kernels);

			// Fill kernel params
			this.GuiB.FillParamElements(this.KernelH?.GetParamsString() ?? []);
		}

		private void LoadResources()
		{
			// List all images in Repopath/Resources
			string path = Path.Combine(this.Repopath, "Resources");
			string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).Where(x => x.EndsWith(".jpg") || x.EndsWith(".jpeg") || x.EndsWith(".png") || x.EndsWith(".bmp") || x.EndsWith(".tif") || x.EndsWith(".tiff")).ToArray();

			// Add images
			foreach (string file in files)
			{
				this.ImgH.AddImage(file);
			}

			// Log with GuiB;
		}

		private void SelectDeviceLike(string nameLike)
		{
			// Get device names
			List<string> names = this.ContextH.GetDeviceNames();

			// Find index where name contains nameLike
			int index = names.FindIndex(x => x.Contains(nameLike));

			// Set index
			this.comboBox_devices.SelectedIndex = index;
		}

		private void ExportLogLineToClipboard(int index)
		{
			// Check index
			if (index < 0 || index >= this.listBox_log.Items.Count)
			{
				return;
			}

			// Get line
			string line = this.listBox_log.Items[index].ToString() ?? ("Error exporting log line #" + (index + 1) + " / " + this.listBox_log.Items.Count);

			// Copy to clipboard
			Clipboard.SetText(line);

			// MsgBox
			MessageBox.Show("Log line #" + (index + 1) + " / " + this.listBox_log.Items.Count, "Copied to clipboard", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

		private void ExportLogFullToFile(bool clipboard = false)
		{
			// Check CTRL down
			if (ModifierKeys != Keys.Control)
			{
				return;
			}

			// Get log .TXT path
			string path = Path.Combine(this.Repopath, "Logs", ("log_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt"));

			// Get all log lines
			string[] lines = new string[this.listBox_log.Items.Count];
			for (int i = 0; i < this.listBox_log.Items.Count; i++)
			{
				lines[i] = this.listBox_log.Items[i].ToString() ?? "\n";
			}
			string linesString = string.Join("\n", lines);

			// Copy to clipboard
			if (clipboard)
			{
				Clipboard.SetText(linesString);
			}

			// Write to file
			File.WriteAllText(path, linesString);

			// MsgBox
			MessageBox.Show("Exported to " + path, "Log exported to file", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}


		// Import image
		private void ImportImage()
		{
			// OFD at MyPictures, single image file
			OpenFileDialog ofd = new OpenFileDialog
			{
				Title = "Import an image file",
				InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
				Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
				Multiselect = false
			};

			// OFD show -> AddImage
			if (ofd.ShowDialog() == DialogResult.OK)
			{
				// Add image
				this.ImgH.AddImage(ofd.FileName);
			}

			this.ToggleUI();
		}


		// Transfer (move) image
		public void MoveImage()
		{
			// Abort if no Img or no MemH
			if (this.Img == null || this.MemH == null)
			{
				return;
			}

			// Move image data Host <-> Device
			if (this.Img.OnHost)
			{
				// Make rows from image (byte[])
				List<byte[]> rows = this.Img.GetPixelRowsAsBytes();

				// Push rows to device (-> long Ptr)
				this.Img.Ptr = this.MemH.PushChunks<byte>(rows);

				// Image null
				this.Img.ClearImage();

				// Log with MemH
				this.MemH.Log("Moved image to device", "<" + this.Img.Ptr.ToString() + ">, " + this.MemH.GetBuffersSize(this.Img.Ptr) + " B");
			}
			else if (this.Img.OnDevice)
			{
				// Device -> Host
				List<byte[]> rows = this.MemH.PullChunks<byte>(this.Img.Ptr);

				// Aggregate rows to image
				this.Img.SetImageFromChunks(rows);

				// Null Ptr
				this.Img.Ptr = 0;

				// Log with MemH
				this.MemH.Log("Moved image to host", this.Img.Size + " B");
			}

			this.ToggleUI();
		}






		// ----- ----- ----- EVENTS ----- ----- ----- \\
		// Toggles
		private void numericUpDown_zoom_ValueChanged(object sender, EventArgs e)
		{
			this.ToggleUI();
		}


		// Dragging & zoom
		private void pictureBox_view_MouseDown(object? sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.isDragging = true;
				this.mouseDownLocation = e.Location;
				this.pictureBox_view.Cursor = Cursors.Hand;
			}
		}

		private void pictureBox_view_MouseMove(object? sender, MouseEventArgs e)
		{
			if (this.isDragging)
			{
				// Neue Position berechnen
				int dx = e.X - this.mouseDownLocation.X;
				int dy = e.Y - this.mouseDownLocation.Y;
				this.pictureBox_view.Left += dx;
				this.pictureBox_view.Top += dy;
			}
		}

		private void pictureBox_view_MouseUp(object? sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.isDragging = false;
				this.pictureBox_view.Cursor = Cursors.Default;
			}
		}

		private void pictureBox_view_MouseWheel(object? sender, MouseEventArgs e)
		{
			int zoomChange = e.Delta > 0 ? 5 : -5;
			int newZoom = Math.Max((int) this.numericUpDown_zoom.Minimum, Math.Min((int) this.numericUpDown_zoom.Maximum, this.oldZoom + zoomChange));

			this.numericUpDown_zoom.Value = newZoom;
		}

		private void button_compileAll_Click(object sender, EventArgs e)
		{
			// Call KernelH
			this.ContextH.KernelH?.ReCompileAll(true);

			ToggleUI();
		}
	}
}
