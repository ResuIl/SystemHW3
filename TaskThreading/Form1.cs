using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Button;

namespace TaskThreading
{
    public partial class Form1 : Form
    {
        CancellationTokenSource cancellation;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                textBox1.Text = openFileDialog1.FileName;
        }

        private void UpdateProgressBar(int value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<int>(UpdateProgressBar), value);
                return;
            }

            progressBar1.Value = value;
        }

        private void Task(CancellationToken token)
        {
            if (textBox1.Text.Length > 0 && textBox2.Text.Length > 0)
            {

                ThreadPool.QueueUserWorkItem(o =>
                {
                    var text = File.ReadAllText(textBox1.Text);
                    byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(text);
                    byte[] keyBytes = BitConverter.GetBytes(Convert.ToInt32(textBox2.Text));

                    for (int i = 0; i < inputBytes.Length; i++)
                    {
                        if (token.IsCancellationRequested)
                        {
                            UpdateProgressBar(Convert.ToInt32(0));
                            return;
                        }
                        inputBytes[i] = (byte)(inputBytes[i] ^ keyBytes[i % 4]);
                        UpdateProgressBar(Convert.ToInt32(i * 1));
                        Thread.Sleep(1000);  
                    }
                    string output = System.Text.Encoding.UTF8.GetString(inputBytes);
                    File.WriteAllText(textBox1.Text, output);
                    UpdateProgressBar(Convert.ToInt32(100));
                });           
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            cancellation = new CancellationTokenSource();

            if (radioButton1.Checked)
            {
                radioButton1.Checked = false;
                radioButton1.Enabled = false;
                radioButton2.Checked = true;
            } 
            else
            {
                radioButton1.Checked = true;
                radioButton1.Enabled = true;
                radioButton2.Checked = false;
            }
            Task(cancellation.Token);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            cancellation.Cancel();
            if (radioButton1.Checked)
            {
                radioButton1.Checked = false;
                radioButton1.Enabled = false;
                radioButton2.Checked = true;
            }
            else
            {
                radioButton1.Checked = true;
                radioButton1.Enabled = true;
                radioButton2.Checked = false;
            }
        }
    }
}