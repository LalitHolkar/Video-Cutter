using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoCutter
{
    public partial class VideoSplitter : Form
    {
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Label lblPath;
        public VideoSplitter()
        {
            InitializeComponent();
            this.Load += new EventHandler(Form1_Load);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            BindddlSplitTime();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Video Files|*.mp4;*.avi;*.mov;*.mkv";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                label6.Visible = false;
                label6.Text = ofd.FileName;
                button1.Text = "Uploaded";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Select folder to save split video files";

            if (fbd.ShowDialog() == DialogResult.OK)
            {
                label7.Visible = false;
                label7.Text = fbd.SelectedPath;
                button2.Text = "Selected";
            }
        }

        private async void split_Click(object sender, EventArgs e)
        {

            string videoPath = label6.Text;
            string outputDir = label7.Text;

            if (!File.Exists(videoPath))
            {
                MessageBox.Show("Please select a valid video file.");
                return;
            }

            ComboBoxItem selectedItem = ddlSplitTime.SelectedItem as ComboBoxItem;
            int selectedTime = selectedItem?.Value ?? 0;
            if (!int.TryParse(selectedTime.ToString(), out int segmentTime) || segmentTime <= 0)
            {
                MessageBox.Show("Enter a valid number of seconds.");
                return;
            }

            if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
            {
                MessageBox.Show("Please select an output folder.");
                return;
            }
            EnableDisableControls(false);
            string rawName = txtVideoName.Text?.Trim();
            string videoName = string.IsNullOrWhiteSpace(rawName) ? "part_%03d.mp4" : $"{rawName}_%03d.mp4";
            string outputPattern = Path.Combine(outputDir, videoName);
            string arguments = $"-i \"{videoPath}\" -map 0 -c:v libx264 -preset fast -crf 23 -c:a aac -movflags +faststart -f segment -segment_time {segmentTime} -reset_timestamps 1 \"{outputPattern}\"";



            try
            {
                await Task.Run(() =>
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        Arguments = arguments,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (Process proc = new Process { StartInfo = psi })
                    {
                        proc.Start();
                        string errorOutput = proc.StandardError.ReadToEnd();
                        proc.WaitForExit();
                    }
                });

                button3.Text = "Split";
                lblstatus.Text = "✅ Split completed!";
                lblstatus.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
            finally
            {
                EnableDisableControls(true);
            }
        }

        private void BindddlSplitTime()
        {
            ddlSplitTime.Items.Clear();
            ddlSplitTime.Items.Add(new ComboBoxItem("30 seconds", 30));
            ddlSplitTime.Items.Add(new ComboBoxItem("1 minute", 60));
            ddlSplitTime.Items.Add(new ComboBoxItem("2 minutes", 120));
            ddlSplitTime.Items.Add(new ComboBoxItem("3 minutes", 180));
            ddlSplitTime.Items.Add(new ComboBoxItem("4 minutes", 240));
            ddlSplitTime.Items.Add(new ComboBoxItem("5 minutes", 300));
            ddlSplitTime.Items.Add(new ComboBoxItem("10 minutes", 600));
            ddlSplitTime.SelectedIndex = 0;
        }
        private void EnableDisableControls(bool status)
        {
            if (status == false)
            {
                progressBar1.Location = new System.Drawing.Point(
    (this.ClientSize.Width - progressBar1.Width) / 2,
    (this.ClientSize.Height - progressBar1.Height) / 2
);
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.MarqueeAnimationSpeed = 30;
                progressBar1.BringToFront();
            }
            progressBar1.Visible = status == false ? true : false;
            button1.Enabled = status;
            button2.Enabled = status;
            button3.Enabled = status;
            ddlSplitTime.Enabled = status;
            txtVideoName.Enabled = status;
            btnClear.Enabled = status;
        }
        private void btnClear_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }
    }


    public class ComboBoxItem
    {
        public string Text { get; set; }
        public int Value { get; set; }
        public ComboBoxItem(string text, int value)
        {
            Text = text;
            Value = value;
        }
        public override string ToString()
        {
            return Text; // Displayed in dropdown
        }
    }
}
