namespace WiiMotionController
{
	partial class MainForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
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
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.accelLabel = new System.Windows.Forms.Label();
            this.motionLabel = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // accelLabel
            // 
            this.accelLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.accelLabel.Location = new System.Drawing.Point(1, 9);
            this.accelLabel.Name = "accelLabel";
            this.accelLabel.Size = new System.Drawing.Size(299, 13);
            this.accelLabel.TabIndex = 0;
            this.accelLabel.Text = "Accelerometer Data";
            this.accelLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // motionLabel
            // 
            this.motionLabel.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.motionLabel.Location = new System.Drawing.Point(1, 39);
            this.motionLabel.Name = "motionLabel";
            this.motionLabel.Size = new System.Drawing.Size(299, 13);
            this.motionLabel.TabIndex = 1;
            this.motionLabel.Text = "Motion:";
            this.motionLabel.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(111, 20);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(75, 23);
            this.startButton.TabIndex = 2;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(300, 61);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.motionLabel);
            this.Controls.Add(this.accelLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "WiiMotion Controller";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.Label accelLabel;
        private System.Windows.Forms.Label motionLabel;
        private System.Windows.Forms.Button startButton;


    }
}

