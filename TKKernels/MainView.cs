namespace TKKernels
{
	public partial class MainView : Form
	{
		// ----- ----- ----- ATTRIBUTES ----- ----- ----- \\
		public string Repopath;

		public ImageHandling ImgH;
		public OpenClContextHandling ContextH;


		private int oldZoom;
		private Point mouseDownLocation;
		private bool isDragging = false;

		// ----- ----- ----- LAMBDA ----- ----- ----- \\
		public ImageObject? Img => ImgH.CurrentImage;


		public OpenClMemoryHandling? MemH => ContextH.MemH;



		// ----- ----- ----- CONSTRUCTOR ----- ----- ----- \\
		public MainView()
		{
			InitializeComponent();

			// Set repopath & oldzoom
			Repopath = GetRepopath(true);
			oldZoom = (int) numericUpDown_zoom.Value;

			// Window position
			StartPosition = FormStartPosition.Manual;
			Location = new Point(0, 0);

			// Init. classes
			ImgH = new ImageHandling(Repopath, listBox_images);
			ContextH = new OpenClContextHandling(Repopath, listBox_log, comboBox_devices);

			// Register events
			pictureBox_view.DoubleClick += (s, e) => ImportImage();
			panel_view.DoubleClick += (s, e) => ImportImage();
			listBox_images.DoubleClick += (s, e) => MoveImage();
			listBox_images.SelectedIndexChanged += (s, e) => ToggleUI();
			pictureBox_view.MouseWheel += pictureBox_view_MouseWheel;
			pictureBox_view.MouseDown += pictureBox_view_MouseDown;
			pictureBox_view.MouseMove += pictureBox_view_MouseMove;
			pictureBox_view.MouseUp += pictureBox_view_MouseUp;


			// Start UI
			ToggleUI();

			// Select device
			SelectDeviceLike("Intel");
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
			pictureBox_view.Image = Img?.Img;
			pictureBox_view.SizeMode = PictureBoxSizeMode.Zoom;

			// Zoom (change size of PictureBox))
			oldZoom = (int) numericUpDown_zoom.Value;
			pictureBox_view.Width = (int) (Img?.Img?.Width * oldZoom / 100.0 ?? 1);
			pictureBox_view.Height = (int) (Img?.Img?.Height * oldZoom / 100.0 ?? 1);

			// Memory label
			long total = (MemH?.GetMemoryTotal() / 1024 / 1024) ?? 0;
			long used = (MemH?.GetMemoryAllocated<byte>() / 1024 / 1024) ?? 0;
			label_memory.Text = "Memory: " + used + " / " + total + " MB";

			// Size label
			label_size.Text = "Size: " +( Img?.Width ?? 0) + " x " + (Img?.Height ?? 0) + " px, " + (Img?.Size ?? 0) / 1024 + " KB";

		}

		private void SelectDeviceLike(string nameLike)
		{
			// Get device names
			List<string> names = ContextH.GetDeviceNames();

			// Find index where name contains nameLike
			int index = names.FindIndex(x => x.Contains(nameLike));

			// Set index
			comboBox_devices.SelectedIndex = index;
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
				ImgH.AddImage(ofd.FileName);
			}

			ToggleUI();
		}


		// Transfer (move) image
		public void MoveImage()
		{
			// Abort if no Img or no MemH
			if (Img == null || MemH == null)
			{
				return;
			}

			// Move image data Host <-> Device
			if (Img.OnHost)
			{
				// Make rows from image (byte[])
				List<byte[]> rows = Img.GetPixelRowsAsBytes();

				// Push rows to device (-> long Ptr)
				Img.Ptr = MemH.PushChunks<byte>(rows);

				// Image null
				Img.ClearImage();

				// Log with MemH
				MemH.Log("Moved image to device", "<" + Img.Ptr.ToString() + ">, " + MemH.GetBuffersSize(Img.Ptr) + " B");
			}
			else if (Img.OnDevice)
			{
				// Device -> Host
				List<byte[]> rows = MemH.PullChunks<byte>(Img.Ptr);

				// Aggregate rows to image
				Img.SetImageFromChunks(rows);

				// Null Ptr
				Img.Ptr = 0;

				// Log with MemH
				MemH.Log("Moved image to host", Img.Size + " B");
			}

			ToggleUI();
		}






		// ----- ----- ----- EVENTS ----- ----- ----- \\
		// Toggles
		private void numericUpDown_zoom_ValueChanged(object sender, EventArgs e)
		{
			ToggleUI();
		}


		// Dragging & zoom
		private void pictureBox_view_MouseDown(object? sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				isDragging = true;
				mouseDownLocation = e.Location;
				pictureBox_view.Cursor = Cursors.Hand;
			}
		}

		private void pictureBox_view_MouseMove(object? sender, MouseEventArgs e)
		{
			if (isDragging)
			{
				// Neue Position berechnen
				int dx = e.X - mouseDownLocation.X;
				int dy = e.Y - mouseDownLocation.Y;
				pictureBox_view.Left += dx;
				pictureBox_view.Top += dy;
			}
		}

		private void pictureBox_view_MouseUp(object? sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				isDragging = false;
				pictureBox_view.Cursor = Cursors.Default;
			}
		}

		private void pictureBox_view_MouseWheel(object? sender, MouseEventArgs e)
		{
			int zoomChange = e.Delta > 0 ? 5 : -5;
			int newZoom = Math.Max((int) numericUpDown_zoom.Minimum, Math.Min((int) numericUpDown_zoom.Maximum, oldZoom + zoomChange));

			numericUpDown_zoom.Value = newZoom;
		}



		

	}
}
