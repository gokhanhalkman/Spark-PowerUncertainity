using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form4_Frequency : Form
    {
        
        double frequency;

        double Fo, correctedFrequency; //Average easured frequency
        double sigma, N; //standart deviation, arithmetic average

        bool isSinus; //sinus 1, square 0
        double triggerLevel; // V
        double gateTime; // sec
        double fallRiseTime; // nanosec
        double amplitude; // Vpp
        double noiseOfCounter, noiseOfPulse; // V
        double slewRate; // V/s
        double tJitter, tAcc; // s, when f < 225MHz
        double tRes, tTrigger; //s

        double resolutionUncert, timeBaseAccuracyUncert, timeBaseEffectUncert;
        double systematicUncert;
        double generalUncert;
        double decleration, certificateValue;

        double heatEffect, aging, lineVoltage;
        double slope, constant;

        double trustabilityRatio = 1;

        string warning = "";

        // // // // // // // // // // // // // // // // // // // // // // 
        private void button1_Click(object sender, EventArgs e) //Start
        {
            AddMeasuredFrequencies(); // average, standerd deviation
            if(measuredFrequencies.Count < 3)
            {
               warning = "Not enough frequency samples";           
            }
            else
            {
                if (textBox5.Text.Equals("")|| textBox8.Text.Equals("") || textBox18.Text.Equals(""))
                {
                    warning = "Inputs cannot be left empty";
                }
                else
                {
                    label36.Text = "";

                    frequency = ReadFrequency();
                    trustabilityRatio = ReadTrustabilityRatio();  
                    ReadParameters();

                    textBox13.Text = Fo.ToString("0.00000E00");
                    textBox14.Text = sigma.ToString("0.00000E00");

                }

            }

            label36.Text = warning;
        }

        
        List<double> extuncerts = new List<double>();


        // // // // // // // // // // // // // // // // // // // // // //


        void WriteReferenceFrequency()
        {
            correctedFrequency = Fo * (1 + 0.000000000002); 
            textBox6.Text = correctedFrequency.ToString("0.00000E00");
        }
        

        void ReadParameters()
        {
            if (comboBox3.Text.Equals("Sinusodial"))
                isSinus = true;
            else if (comboBox3.Text.Equals("Square"))
                isSinus = false;
            else
            {
                isSinus = false;
                warning = "Wave type is not valid";
            }

            if (!textBox1.Text.Equals(""))
                if (Fo < 255000000)
                    triggerLevel = 0.5;
                else
                    triggerLevel = 0;
            else
                triggerLevel = Convert.ToDouble(textBox1.Text);

            if (!textBox3.Text.Equals(""))
                if (Fo < 255000000)
                    gateTime = 10;
                else if (Fo < 5000000000)
                    gateTime = 1;
                else if (Fo < 5700000000)
                    gateTime = 0.2;
                else if (Fo < 11300000000)
                    gateTime = 0.4;
                else if (Fo < 16900000000)
                    gateTime = 0.6;
                else if (Fo < 22500000000)
                    gateTime = 0.8;
                else
                    gateTime = 1;
            else
                gateTime = Convert.ToDouble(textBox3.Text);

            fallRiseTime = 2; //ns
             
            amplitude = Fo <= 255000000 ? 10*Math.Sqrt(2) : 11 * Math.Sqrt(2);

            if (!textBox4.Text.Equals(""))
                noiseOfCounter = Fo <= 255000000 ? 0.00035 : 0.0001;
            else
                noiseOfCounter = Convert.ToDouble(textBox4.Text);

            if (!textBox11.Text.Equals(""))
                noiseOfPulse = Fo <= 255000000 ? 0.00035 : 0.0001;
            else
                noiseOfPulse = Convert.ToDouble(textBox11.Text);

            slewRate = isSinus ? Math.PI * Fo * amplitude * Math.Cos(Math.Asin(2 * triggerLevel / amplitude)) : amplitude * 0.8 * Math.Pow(10, 9) / fallRiseTime;

            if(Fo < 255000000)
            {
                textBox7.Enabled = false;
            }
                
            
        }


        private void button2_Click(object sender, EventArgs e) //Clear
        {
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            textBox6.Text = "";
            textBox8.Text = "";
            textBox9.Text = "";
            textBox10.Text = "";
            textBox11.Text = "";
            textBox15.Text = "";
            textBox16.Text = "";
            listBox1.Items.Clear();
            measuredFrequencies.Clear();

        }


        private void button4_Click(object sender, EventArgs e) //clear list
        {
            listBox1.Items.Clear();
            measuredFrequencies.Clear();
        }


        List<double> measuredFrequencies = new List<double>();

        private void button5_Click(object sender, EventArgs e)
        {
            //An instance of Form 3 is already created on Form 2 and hid (not closed) when test is selected
            Form2_Login.f3.Show();
            //When new test is clicked this instance is disposed
            this.Close();            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (!textBox2.Text.Equals(""))
            {
                listBox1.Items.Add(textBox2.Text);
                textBox2.Text = "";
            }
        }

        private void AddMeasuredFrequencies()
        {
            N = 0; Fo = 0; sigma = 0;
            measuredFrequencies.Clear();
            foreach(string freq in listBox1.Items)
            {
                measuredFrequencies.Add(Double.Parse(freq, System.Globalization.NumberStyles.Float));
            }
            N = measuredFrequencies.Count;
            foreach (double frequency in measuredFrequencies) { Fo += frequency / N; }
            double sumOfDiffrences = 0;
            foreach (double frequency in measuredFrequencies) { sumOfDiffrences += Math.Pow((frequency - Fo), 2); }
            sigma = Math.Sqrt(sumOfDiffrences / (N - 1));
        }

        List<long> knownFrequencies = new List<long>();
        List<double> calFactors = new List<double>();
        List<double> calFacUncerts = new List<double>();        

        public Form4_Frequency()
        {
            InitializeComponent();
            MakeKnownList(knownFrequencies, calFactors, calFacUncerts);

        }

        private void MakeKnownList(List<long> kf, List<double> cf, List<double> cfu)
        {
            kf.Add(9000); cf.Add(0.987); cfu.Add(0.0076);//9k
            kf.Add(30000); cf.Add(0.984); cfu.Add(0.0076);
            kf.Add(50000); cf.Add(0.986); cfu.Add(0.0076);
            kf.Add(100000); cf.Add(0.985); cfu.Add(0.0076);
            kf.Add(300000); cf.Add(0.985); cfu.Add(0.0076);
            kf.Add(500000); cf.Add(0.985); cfu.Add(0.0076);//500k

            kf.Add(1000000); cf.Add(0.987); cfu.Add(0.0076);//1M
            kf.Add(3000000); cf.Add(0.968); cfu.Add(0.0076);
            kf.Add(5000000); cf.Add(0.983); cfu.Add(0.0076);

            kf.Add(10000000); cf.Add(0.971); cfu.Add(0.0082);//10M
            kf.Add(30000000); cf.Add(0.992); cfu.Add(0.0082);
            kf.Add(50000000); cf.Add(1); cfu.Add(0.01);

            kf.Add(100000000); cf.Add(0.995); cfu.Add(0.0072);//100M
            kf.Add(300000000); cf.Add(0.992); cfu.Add(0.0073);
            kf.Add(500000000); cf.Add(0.99); cfu.Add(0.0073);
            kf.Add(800000000); cf.Add(0.998); cfu.Add(0.0074);

            kf.Add(1000000000); cf.Add(1.004); cfu.Add(0.0075);//1G
            kf.Add(1200000000); cf.Add(1.013); cfu.Add(0.0076);
            kf.Add(1500000000); cf.Add(1.014); cfu.Add(0.0081);
            kf.Add(2000000000); cf.Add(1.01); cfu.Add(0.0083);
            kf.Add(3000000000); cf.Add(1.005); cfu.Add(0.0086);
            kf.Add(4000000000); cf.Add(0.997); cfu.Add(0.0089);
            kf.Add(5000000000); cf.Add(0.994); cfu.Add(0.0092);
            kf.Add(6000000000); cf.Add(1.001); cfu.Add(0.0096);
            kf.Add(7000000000); cf.Add(0.999); cfu.Add(0.01);
            kf.Add(8000000000); cf.Add(0.994); cfu.Add(0.01);
            kf.Add(9000000000); cf.Add(0.996); cfu.Add(0.011);//9G 
        }

        double ReadFrequency()
        {
            if (comboBox1.Text.Equals("kHz")) return Convert.ToDouble(textBox2.Text) * Math.Pow(10, 3);
            else if (comboBox1.Text.Equals("MHz")) return Convert.ToDouble(textBox2.Text) * Math.Pow(10, 6);
            else if (comboBox1.Text.Equals("GHz")) return Convert.ToDouble(textBox2.Text) * Math.Pow(10, 9);
            else return 0;
        }

        double ReadTrustabilityRatio()
        {
            if (comboBox2.Text.Equals("68,27%")) return 1;
            else if (comboBox2.Text.Equals("90%")) return 1.645;
            else if (comboBox2.Text.Equals("95%")) return 1.96;
            else if (comboBox2.Text.Equals("95,45% (suggested)")) return 2;
            else if (comboBox2.Text.Equals("99%")) return 2.576;
            else if (comboBox2.Text.Equals("99,73%")) return 3;
            else
            {
                warning = "Scope interval is not valid";
                return 1;
            }
        }

    }
}
