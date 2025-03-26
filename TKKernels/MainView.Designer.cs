namespace TKKernels
{
    partial class MainView
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

		#region Windows Form Designer generated code

		/// <summary>
		///  Required method for Designer support - do not modify
		///  the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.comboBox_devices = new ComboBox();
			this.listBox_log = new ListBox();
			this.pictureBox_view = new PictureBox();
			this.panel_view = new Panel();
			this.listBox_images = new ListBox();
			this.numericUpDown_zoom = new NumericUpDown();
			this.label_memory = new Label();
			this.label_size = new Label();
			this.button_compileAll = new Button();
			this.groupBox_kernel = new GroupBox();
			this.listBox_kernels = new ListBox();
			this.panel_params = new Panel();
			((System.ComponentModel.ISupportInitialize) this.pictureBox_view).BeginInit();
			this.panel_view.SuspendLayout();
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_zoom).BeginInit();
			this.groupBox_kernel.SuspendLayout();
			this.SuspendLayout();
			// 
			// comboBox_devices
			// 
			this.comboBox_devices.FormattingEnabled = true;
			this.comboBox_devices.Location = new Point(12, 12);
			this.comboBox_devices.Name = "comboBox_devices";
			this.comboBox_devices.Size = new Size(292, 23);
			this.comboBox_devices.TabIndex = 0;
			// 
			// listBox_log
			// 
			this.listBox_log.FormattingEnabled = true;
			this.listBox_log.ItemHeight = 15;
			this.listBox_log.Location = new Point(12, 795);
			this.listBox_log.Name = "listBox_log";
			this.listBox_log.Size = new Size(810, 154);
			this.listBox_log.TabIndex = 1;
			// 
			// pictureBox_view
			// 
			this.pictureBox_view.BackColor = Color.White;
			this.pictureBox_view.Location = new Point(0, 0);
			this.pictureBox_view.Name = "pictureBox_view";
			this.pictureBox_view.Size = new Size(512, 512);
			this.pictureBox_view.TabIndex = 2;
			this.pictureBox_view.TabStop = false;
			// 
			// panel_view
			// 
			this.panel_view.BackColor = Color.Black;
			this.panel_view.Controls.Add(this.pictureBox_view);
			this.panel_view.Location = new Point(310, 12);
			this.panel_view.Name = "panel_view";
			this.panel_view.Size = new Size(512, 512);
			this.panel_view.TabIndex = 3;
			// 
			// listBox_images
			// 
			this.listBox_images.FormattingEnabled = true;
			this.listBox_images.ItemHeight = 15;
			this.listBox_images.Location = new Point(628, 590);
			this.listBox_images.Name = "listBox_images";
			this.listBox_images.Size = new Size(194, 199);
			this.listBox_images.TabIndex = 4;
			// 
			// numericUpDown_zoom
			// 
			this.numericUpDown_zoom.Increment = new decimal(new int[] { 5, 0, 0, 0 });
			this.numericUpDown_zoom.Location = new Point(747, 530);
			this.numericUpDown_zoom.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
			this.numericUpDown_zoom.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
			this.numericUpDown_zoom.Name = "numericUpDown_zoom";
			this.numericUpDown_zoom.Size = new Size(75, 23);
			this.numericUpDown_zoom.TabIndex = 5;
			this.numericUpDown_zoom.Value = new decimal(new int[] { 100, 0, 0, 0 });
			this.numericUpDown_zoom.ValueChanged += this.numericUpDown_zoom_ValueChanged;
			// 
			// label_memory
			// 
			this.label_memory.AutoSize = true;
			this.label_memory.Location = new Point(12, 38);
			this.label_memory.Name = "label_memory";
			this.label_memory.Size = new Size(58, 15);
			this.label_memory.TabIndex = 6;
			this.label_memory.Text = "Memory: ";
			// 
			// label_size
			// 
			this.label_size.AutoSize = true;
			this.label_size.Location = new Point(310, 527);
			this.label_size.Name = "label_size";
			this.label_size.Size = new Size(33, 15);
			this.label_size.TabIndex = 7;
			this.label_size.Text = "Size: ";
			// 
			// button_compileAll
			// 
			this.button_compileAll.Location = new Point(6, 170);
			this.button_compileAll.Name = "button_compileAll";
			this.button_compileAll.Size = new Size(75, 23);
			this.button_compileAll.TabIndex = 8;
			this.button_compileAll.Text = "Compile all";
			this.button_compileAll.UseVisualStyleBackColor = true;
			this.button_compileAll.Click += this.button_compileAll_Click;
			// 
			// groupBox_kernel
			// 
			this.groupBox_kernel.Controls.Add(this.button_compileAll);
			this.groupBox_kernel.Location = new Point(212, 590);
			this.groupBox_kernel.Name = "groupBox_kernel";
			this.groupBox_kernel.Size = new Size(92, 199);
			this.groupBox_kernel.TabIndex = 9;
			this.groupBox_kernel.TabStop = false;
			this.groupBox_kernel.Text = "Kernels";
			// 
			// listBox_kernels
			// 
			this.listBox_kernels.FormattingEnabled = true;
			this.listBox_kernels.ItemHeight = 15;
			this.listBox_kernels.Location = new Point(12, 590);
			this.listBox_kernels.Name = "listBox_kernels";
			this.listBox_kernels.Size = new Size(194, 199);
			this.listBox_kernels.TabIndex = 10;
			// 
			// panel_params
			// 
			this.panel_params.BackColor = Color.White;
			this.panel_params.Location = new Point(310, 590);
			this.panel_params.Name = "panel_params";
			this.panel_params.Size = new Size(312, 199);
			this.panel_params.TabIndex = 11;
			// 
			// MainView
			// 
			this.AutoScaleDimensions = new SizeF(7F, 15F);
			this.AutoScaleMode = AutoScaleMode.Font;
			this.ClientSize = new Size(834, 961);
			this.Controls.Add(this.panel_params);
			this.Controls.Add(this.listBox_kernels);
			this.Controls.Add(this.groupBox_kernel);
			this.Controls.Add(this.label_size);
			this.Controls.Add(this.label_memory);
			this.Controls.Add(this.numericUpDown_zoom);
			this.Controls.Add(this.listBox_images);
			this.Controls.Add(this.panel_view);
			this.Controls.Add(this.listBox_log);
			this.Controls.Add(this.comboBox_devices);
			this.Name = "MainView";
			this.Text = "OpenCL Kernels with Precompiler (WinForms)";
			((System.ComponentModel.ISupportInitialize) this.pictureBox_view).EndInit();
			this.panel_view.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize) this.numericUpDown_zoom).EndInit();
			this.groupBox_kernel.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ComboBox comboBox_devices;
		private ListBox listBox_log;
		private PictureBox pictureBox_view;
		private Panel panel_view;
		private ListBox listBox_images;
		private NumericUpDown numericUpDown_zoom;
		private Label label_memory;
		private Label label_size;
		private Button button_compileAll;
		private GroupBox groupBox_kernel;
		private ListBox listBox_kernels;
		private Panel panel_params;
	}
}
