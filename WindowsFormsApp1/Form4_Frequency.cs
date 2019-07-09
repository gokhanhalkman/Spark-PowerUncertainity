using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;

namespace WindowsFormsApp1
{
    public partial class Form4_Frequency : Form
    {
        
        double frequency;

        double Fo, correctedFrequency; //Average, reference, measured frequency
        double standardDev, standardUncert, N; //standart deviation, arithmetic average

        bool isSinus; //sinus 1, square 0
        double triggerLevel; // V
        double gateTime; // sec
        double fallRiseTime; // nanosec
        double amplitude; // Vpp
        double noiseOfCounter, noiseOfSignGenerator; // V
        double slewRate; // V/s
        double tJitter, tAcc; // s, when f < 225MHz
        double tRes, tTrigger; //s

        double resolutionUncert, timeBaseAccuracyUncert, timeBaseEffectUncert;
        double systematicUncert;
        double extendedUncert;

        double heatEffect, aging, lineVoltage;

        double trustabilityRatio = 1;

        string warning = "";

        bool isHighFrequency = false;

        // // // // // // // // // // // // // // // // // // // // // // 
        private void button1_Click(object sender, EventArgs e) //Start
        {
            N = 0; Fo = 0; standardDev = 0;
            measuredFrequencies.Clear();
            foreach (string freq in listBox1.Items)
            {
                measuredFrequencies.Add(Double.Parse(freq, System.Globalization.NumberStyles.Float));
            }
            N = measuredFrequencies.Count;
            foreach (double frequency in measuredFrequencies) { Fo += frequency / N; }
            double sumOfDiffrences = 0;
            foreach (double frequency in measuredFrequencies) { sumOfDiffrences += Math.Pow((frequency - Fo), 2); }
            standardDev = Math.Sqrt(sumOfDiffrences / (N - 1));
            standardUncert = standardDev / Math.Sqrt(N);

            if (measuredFrequencies.Count < 3)
            {
               warning = "Not enough frequency samples";           
            }
            else
            {
                if (textBox20.Text.Equals(""))
                {
                    warning = "Inputs cannot be left empty";
                }
                else
                {
                    label36.Text = "";

                    if (comboBox1.Text.Equals("kHz")) frequency = Convert.ToDouble(textBox20.Text) * Math.Pow(10, 3);
                    else if (comboBox1.Text.Equals("MHz")) frequency = Convert.ToDouble(textBox20.Text) * Math.Pow(10, 6);
                    else if (comboBox1.Text.Equals("GHz")) frequency = Convert.ToDouble(textBox20.Text) * Math.Pow(10, 9);
                    else frequency = 0;

                    if (comboBox2.Text.Equals("68,27%")) trustabilityRatio = 1;
                    else if (comboBox2.Text.Equals("90%")) trustabilityRatio = 1.645;
                    else if (comboBox2.Text.Equals("95%")) trustabilityRatio = 1.96;
                    else if (comboBox2.Text.Equals("95,45% (suggested)")) trustabilityRatio = 2;
                    else if (comboBox2.Text.Equals("99%")) trustabilityRatio = 2.576;
                    else if (comboBox2.Text.Equals("99,73%")) trustabilityRatio = 3;
                    else
                    {
                        warning = "Scope interval is not valid";
                        trustabilityRatio = 1;
                    }

                    if (comboBox3.Text.Equals("Sinusodial"))
                        isSinus = true;
                    else if (comboBox3.Text.Equals("Square"))
                        isSinus = false;
                    else
                    {
                        isSinus = false;
                        warning = "Wave type is not valid";
                    }

                    gateTime = Convert.ToDouble(textBox3.Text);
                    fallRiseTime = Double.Parse(textBox5.Text, System.Globalization.NumberStyles.Float);

                    tRes = Double.Parse(textBox12.Text, System.Globalization.NumberStyles.Float);
                    timeBaseAccuracyUncert = Double.Parse(textBox11.Text, System.Globalization.NumberStyles.Float);

                    heatEffect = Double.Parse(textBox9.Text, System.Globalization.NumberStyles.Float);
                    aging = Double.Parse(textBox24.Text, System.Globalization.NumberStyles.Float);
                    lineVoltage = Double.Parse(textBox23.Text, System.Globalization.NumberStyles.Float);

                    triggerLevel = Double.Parse(textBox1.Text, System.Globalization.NumberStyles.Float);
                    amplitude = Double.Parse(textBox4.Text, System.Globalization.NumberStyles.Float);

                    noiseOfCounter = Double.Parse(textBox8.Text, System.Globalization.NumberStyles.Float);
                    noiseOfSignGenerator = Double.Parse(textBox7.Text, System.Globalization.NumberStyles.Float);

                    if (!isHighFrequency)
                    {
                        tJitter = Double.Parse(textBox15.Text, System.Globalization.NumberStyles.Float);
                        tAcc = Double.Parse(textBox22.Text, System.Globalization.NumberStyles.Float);
                    }
                    else
                    {
                        tJitter = 0;
                        tAcc = 0;
                    }

                    slewRate = isSinus ? Math.PI * Fo * amplitude * Math.Cos(Math.Asin(2 * triggerLevel / amplitude)) : amplitude * 0.8 * Math.Pow(10, 9) / fallRiseTime;
                    label19.Text = slewRate.ToString();

                    tTrigger = Math.Sqrt(Math.Pow(noiseOfCounter, 2) + Math.Pow(noiseOfSignGenerator, 2)) / slewRate;

                    resolutionUncert = Fo < 255000000 ? Math.Sqrt((24 * Math.Pow(tRes, 2) + 32 * Math.Pow(tTrigger, 2)) / N + 2 * Math.Pow(tJitter, 2)) / gateTime : Math.Sqrt(Math.Pow(1 / Fo, 2) + Math.Pow(1.4 * tTrigger / gateTime, 2));
                    label18.Text = resolutionUncert.ToString();

                    timeBaseEffectUncert = Math.Sqrt(Math.Pow(heatEffect, 2) + Math.Pow(aging, 2) + Math.Pow(lineVoltage, 2));
                    label15.Text = timeBaseEffectUncert.ToString();

                    systematicUncert = tAcc / gateTime;
                    extendedUncert = trustabilityRatio * Math.Sqrt(Math.Pow(standardUncert, 2) + Math.Pow(resolutionUncert * Fo, 2) / 3 + Math.Pow(systematicUncert * Fo, 2) / 3 + Math.Pow(timeBaseAccuracyUncert * Fo, 2) + Math.Pow(timeBaseEffectUncert * Fo, 2) / 3);


                    textBox13.Text = Fo.ToString("0.00E00");
                    textBox14.Text = standardUncert.ToString("0.00E00");
                    textBox10.Text = extendedUncert.ToString("0.00E00");

                    correctedFrequency = Fo * (1 + 0.000000000002);
                    textBox6.Text = correctedFrequency.ToString("0.00E00");
                }
            }

            label36.Text = warning;

            textBox17.Text = Fo.ToString("0.00E00");
            textBox19.Text = extendedUncert.ToString("0.00E00");
        }

        // // // // // // // // // // // // // // // // // // // // // //

        
        private void textBox20_TextChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text.Equals("GHz"))
            {
                textBox15.Enabled = false;
                textBox22.Enabled = false;
                isHighFrequency = true;
            }
            else if (textBox20.Text.Equals(""))
            {
                textBox15.Enabled = true;
                textBox22.Enabled = true;
            }
            else if (Convert.ToDouble(textBox20.Text) > 255 && comboBox1.Text.Equals("MHz"))
            {
                textBox15.Enabled = false;
                textBox22.Enabled = false;
                isHighFrequency = true;
            }
            else
            {
                textBox15.Enabled = true;
                textBox22.Enabled = true;
                isHighFrequency = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.Text.Equals("GHz"))
            {
                textBox15.Enabled = false;
                textBox22.Enabled = false;
                isHighFrequency = true;
            }
            else if (textBox20.Text.Equals(""))
            {
                textBox15.Enabled = true;
                textBox22.Enabled = true;
            }
            else if (Convert.ToDouble(textBox20.Text) > 255 && comboBox1.Text.Equals("MHz"))
            {
                textBox15.Enabled = false;
                textBox22.Enabled = false;
                isHighFrequency = true;
            }
            else
            {
                textBox15.Enabled = true;
                textBox22.Enabled = true;
                isHighFrequency = false;
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
            textBox7.Text = "";
            textBox8.Text = "";
            textBox9.Text = "";
            textBox10.Text = "";
            textBox11.Text = "";
            textBox12.Text = "";
            textBox13.Text = "";
            textBox14.Text = "";
            textBox15.Text = "";

            textBox17.Text = "";

            textBox19.Text = "";
            textBox20.Text = "";

            textBox22.Text = "";
            textBox23.Text = "";
            textBox24.Text = "";

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

        List<long> knownFrequencies = new List<long>();
        List<double> calFactors = new List<double>();
        List<double> calFacUncerts = new List<double>();        

        public Form4_Frequency()
        {
            InitializeComponent();
        }

    }
}
