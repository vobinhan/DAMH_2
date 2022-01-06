using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;

using AForge.Video;
using AForge.Video.DirectShow;
using System.IO.Ports;

namespace Doan2
{
    public partial class Form1 : Form
    {

        private FilterInfoCollection camera;
        private VideoCaptureDevice cam;
        string InputData = String.Empty;
        delegate void SetTextCallback(string text);
        //--------------------------------
        DataTable dt = new DataTable();

        IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "8xn9oKbUmANOZQJRuTfHRgdQ5Ng1zuq5aH9U7y9a",
            BasePath = "https://do-an-2-35c4c-default-rtdb.firebaseio.com/"
        };

        IFirebaseClient client;
        private object obj;

        //private void Form1_Load(object sender, EventArgs e)
        //{
        //    client = new FireSharp.FirebaseClient(config);

        //    if (client != null)
        //    {
        //        MessageBox.Show("ket noi");
        //    }
        //}

            public Form1()
        {
            InitializeComponent();
            //----------------------            
            camera = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo info in camera)
            {
                comboBox1.Items.Add(info.Name);
            }
            comboBox1.SelectedIndex = 0;
            serialPort1.DataReceived += new SerialDataReceivedEventHandler(DataReceive);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            client = new FireSharp.FirebaseClient(config);

            //if (client != null)
            //{
            //    MessageBox.Show("ket noi");
            //}

            dt.Columns.Add("Image", typeof(Image));

            dataGridView1.DataSource = dt;



            //---------------------------------

            string[] ports = SerialPort.GetPortNames();
            comboBox2.Items.AddRange(ports);
            comboBox2.SelectedIndex = 0;

            if (cam != null && cam.IsRunning)
            {
                cam.Stop();
            }
            cam = new VideoCaptureDevice(camera[comboBox1.SelectedIndex].MonikerString);
            cam.NewFrame += Cam_NewFrame;
            cam.Start();
        }
        private void DataReceive(object obj, SerialDataReceivedEventArgs e)
        {
            InputData = serialPort1.ReadExisting();
            if (InputData != String.Empty)
            {
                SetText(InputData);
                CapSave();
            }
        }
        private void SetText(string text)
        {
            if (this.textBox1.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else this.textBox1.Text += text;
        }
        //chụp và lưu 

        private void CapSave()
        {
            if (InputData == "0") ///nhận 0 
            {
                pictureBox2.Image = pictureBox1.Image;
                Invoke((MethodInvoker)(delegate ()
                {
                    var image = pictureBox2.Image;
                    SaveImageCapture(image);
                }));

            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            export();
        }
        private async void export()
        {
            dt.Rows.Clear();
            int i = 0;
            FirebaseResponse resp1 = await client.GetTaskAsync("Counter/node");
            Counter_class obj1 = resp1.ResultAs<Counter_class>();
            int cnt = Convert.ToInt32(obj1.cnt);

            while (true)
            {

                if (i == cnt)
                {
                    break;
                }
                i++;
                try
                {
                    FirebaseResponse resp2 = await client.GetTaskAsync("Information/" + i);
                    Data obj2 = resp2.ResultAs<Data>();

                    DataRow row = dt.NewRow();

                    //---------------------------- image 

                    byte[] b = Convert.FromBase64String(obj2.Img);

                    MemoryStream ms = new MemoryStream();
                    ms.Write(b, 0, Convert.ToInt32(b.Length));

                    Bitmap bm = new Bitmap(ms, false);

                    row["Image"] = bm;

                    dt.Rows.Add(row);
                }
                catch
                {

                }
            }
            MessageBox.Show("Hoàn tất !");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                serialPort1.PortName = comboBox2.Text;
                serialPort1.Open();
                progressBar1.Value = 100;
            }
            catch
            {
                MessageBox.Show("Không thể mở " + comboBox2.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close(); // đóng COM
                progressBar1.Value = 0; // giá trị progress = 0
            }
        }
        private void Cam_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Bitmap bitmap = (Bitmap)eventArgs.Frame.Clone();
            pictureBox1.Image = bitmap;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            if (cam != null && cam.IsRunning)
            {
                cam.Stop();
            }
        }
        private void Form1_Closing(object sender, FormClosingEventArgs e)
        {
            if (serialPort1.IsOpen)
            {
                serialPort1.Close();
            }
        }

        public async void SaveImageCapture(Image image)
        {
            string filename = DateTime.Now.ToString("dd_MM_yyyy");
            FileStream fileStream = new FileStream(@"D:\ĐỒ ÁN 2\anh\" + filename + ".png", FileMode.CreateNew);
            image.Save(fileStream, ImageFormat.Png);

            FirebaseResponse resp = await client.GetTaskAsync("Counter/node");
            Counter_class get = resp.ResultAs<Counter_class>();S


            MemoryStream ms = new MemoryStream();
            pictureBox2.Image.Save(ms, ImageFormat.Png);

            byte[] a = ms.GetBuffer();

            string output = Convert.ToBase64String(a);


            var data = new Data
            {
                Id = (Convert.ToInt32(get.cnt) + 1).ToString(),
                Img = output

            };

            SetResponse response = await client.SetTaskAsync("Information/" + data.Id, data);
            Data result = response.ResultAs<Data>();

            //MessageBox.Show("Data Insert " + result.Id); // hien thong bao anh da tai len FB


            var obj = new Counter_class
            {
                cnt = data.Id
            };

            SetResponse response1 = await client.SetTaskAsync("Counter/node", obj);
        }
    }
}
