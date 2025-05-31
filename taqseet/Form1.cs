using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using iTextSharp.text;
using iTextSharp.text.pdf;
using ArabicSupport;
using PdfiumViewer;

namespace taqseet
{
    public partial class Form1 : Form
    {
        int a = 1;
        int t = 0;
        int d = 0;
        int target = 208;
        bool slidingIn;
        int monthlyincome;
        List<string> allSubscribers = new List<string>();
        PdfViewer pdfViewer;
        public Form1()
        {
            InitializeComponent();

            pdfViewer = new PdfViewer
            {
                Dock = DockStyle.Fill
            };
            panelPdfViewer.Controls.Add(pdfViewer);
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            settingspanel.Height = Screen.PrimaryScreen.WorkingArea.Height;
            CountSubscribers();
            monthlyincome = GetTotalMonthlyIncome();
            label14.Text = monthlyincome.ToString("N0") + " د.ع";
            LoadAllSubscribers();
            CheckMonthlyUpdateAlerts();
            loadstorename();
            updatealertpanel();
            richTextBoxSubInfo.Text = "يرجى تحديد شخص";
            richTextBoxDebts.Text = "يرجى تحديد شخص";
            LoadSubscribersWithDebt();
            txtStoreName.Text = textBox1.Text;
            LoadTotalDebt();
            CalculateDebtCount();
            //debt alert red
            updatedebtredpanel();
            LoadRecentUpdates();
            
            
           
           

        }
       public void updatedebtredpanel()
        {
            if (d > 0)
            {
                panel10.Visible = true;
                label34.Visible = true;
                label34.Text = d.ToString();
            }
            else
            {
                 panel10.Visible = false;
                label34.Visible = false;
                label34.Text = d.ToString();
            }
        }
       public void updatealertpanel()
       {
           if (t > 0)
           {
               panel6.Visible = true;
               label23.Visible = true;
               label23.Text = t.ToString();
           }
           else
           {
               panel6.Visible = false;
               label23.Visible = false;
               label23.Text = t.ToString();
           }
       }

        private void CalculateDebtCount()
        {
            int debtCount = 0;

            // Check how many people are listed in listBoxDebts
            if (listBoxDebts.Items.Count > 0)
            {
                debtCount = listBoxDebts.Items.Count;
                
            }

            // Display the debt count on a label (label30)
            label32.Text = debtCount.ToString();
            d = listBoxDebts.Items.Count;
        }


        private void LoadTotalDebt()
        {
            string generalFilePath = Application.StartupPath + "\\general.txt";

            if (!File.Exists(generalFilePath))
            {
                label30.Text = "0 د.ع";
                return;
            }

            string[] lines = File.ReadAllLines(generalFilePath);
            foreach (string line in lines)
            {
                if (line.StartsWith("الديون الكلية:"))
                {
                    string debtStr = line.Substring("الديون الكلية:".Length).Trim().Replace(",", "");
                    int totalDebt = 0;
                    if (int.TryParse(debtStr, out totalDebt))
                    {
                        label30.Text = totalDebt.ToString("N0") + " د.ع";
                    }
                    else
                    {
                        label30.Text = "0 د.ع";
                    }
                    return;
                }
            }

            // If the "الديون الكلية:" line is not found
            label30.Text = "0 د.ع";
        }



        private void loadstorename()
        {
            string generalFilePath = Application.StartupPath + "\\general.txt";

            if (File.Exists(generalFilePath))
            {
                string[] lines = File.ReadAllLines(generalFilePath);

                if (lines.Length >= 2)
                {
                    // Expected format: اسم المتجر: [اسم المتجر]
                    string storeLine = lines[1];

                    if (storeLine.StartsWith("اسم المتجر:"))
                    {
                        textBox1.Text = storeLine.Substring("اسم المتجر:".Length).Trim();
                    }
                }
            }
        }
        private void CountSubscribers()
        {
             string folderPath = Application.StartupPath + "\\مشتركين";

            if (Directory.Exists(folderPath))
            {
                int fileCount = Directory.GetFiles(folderPath, "*.txt").Length;
                label12.Text = fileCount.ToString();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            settingspanel.BringToFront();
            // If panel is currently hidden (off-screen), we slide it in
            if (!slidingIn)
            {
                a = 0; // Reset animation step
                slidingIn = true;
            }
            else
            {
                a = 208; // Start from the max slide position
                slidingIn = false;
            }

            timer1.Start(); // Start the animation

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int speed = 10; // Change to control how fast it slides

            if (slidingIn)
            {
                if (a < target)
                {
                    settingspanel.Left += speed;
                    pictureBox1.Left += speed;
                    a += speed;
                }
                else
                {
                    timer1.Stop();
                }
            }
            else
            {
                if (a > 0)
                {
                    settingspanel.Left -= speed;
                    pictureBox1.Left -= speed;
                    a -= speed;
                }
                else
                {
                    timer1.Stop();
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            panel1.Visible = true;
            panel1.BringToFront();
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            panel1.Visible = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CultureInfo arCulture = new CultureInfo("ar-SA");
            arCulture.DateTimeFormat.Calendar = new GregorianCalendar();

            string name = txtName.Text.Trim();
            string phone = txtPhone.Text.Trim();
            string amountText = txtAmount.Text.Trim();
            string monthlyPayText = txtmonthlypay.Text.Trim();
            string monthText = txtMonth.Text.Trim();
            string device = txtDevice.Text.Trim();

            int totalamount = 0;
            int monthlypay = 0;
            int monthCount = 0;

            // التحقق من إدخال الأرقام بشكل صحيح
            if (!int.TryParse(amountText, out totalamount))
            {
                MessageBox.Show("يرجى إدخال رقم صحيح في خانة مبلغ القسط الكامل.");
                return;
            }

            if (!int.TryParse(monthlyPayText, out monthlypay))
            {
                MessageBox.Show("يرجى إدخال رقم صحيح في خانة الدفع الشهري.");
                return;
            }

            if (!int.TryParse(monthText, out monthCount))
            {
                MessageBox.Show("يرجى إدخال رقم صحيح في خانة عدد الأشهر.");
                return;
            }

            if (!Regex.IsMatch(phone, @"^\d{11}$"))
            {
                MessageBox.Show("يرجى إدخال رقم هاتف صحيح مكون من 11 رقمًا.");
                return;
            }

            string folderPath = Path.Combine(Application.StartupPath, "مشتركين");
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string fileName = name + ".txt";
            string filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath))
            {
                MessageBox.Show("يوجد مشترك بهذا الاسم بالفعل.");
                return;
            }

            int debt = 0;
            int currentamount = 0;
            int remainingamount = totalamount - currentamount;
            string date = DateTime.Now.ToString("yyyy/M/d         الوقت: h:mm tt", arCulture);

            using (StreamWriter writer = new StreamWriter(filePath, false))
            {
                writer.WriteLine("                  ======== بيانات المشترك ========");
                writer.WriteLine("الاسم:".PadRight(20) + name);
                writer.WriteLine("رقم الهاتف:".PadRight(20) + phone);
                writer.WriteLine("اسم الجهاز:".PadRight(20) + device);
                writer.WriteLine("تاريخ التسجيل:".PadRight(20) + date);
                writer.WriteLine("-----------------------------------------------------------------------------");

                writer.WriteLine("                  ======== معلومات الدفع ========");
                writer.WriteLine("مبلغ القسط الكامل:".PadRight(20) + totalamount.ToString("N0"));
                writer.WriteLine("الدفع الشهري:".PadRight(20) + monthlypay.ToString("N0"));
                writer.WriteLine("عدد الأشهر:".PadRight(20) + monthCount);
                writer.WriteLine("المبلغ المستلم:".PadRight(20) + currentamount.ToString("N0"));
                writer.WriteLine("المبلغ المتبقي:".PadRight(20) + remainingamount.ToString("N0"));
                writer.WriteLine("الديون:".PadRight(20) + debt);
                writer.WriteLine("اخر دفع:".PadRight(20) + date);
                writer.WriteLine("-----------------------------------------------------------------------------");

                writer.WriteLine("                  ======== حالة الدفع الشهرية ========");
                for (int i = 1; i <= monthCount; i++)
                {
                    writer.WriteLine(("شهر " + i + ":").PadRight(15) + "لم يتم الدفع");
                }
            }

            // Generate PDF version
            string pdfFolder = Path.Combine(folderPath, "subscribers pdf");
            if (!Directory.Exists(pdfFolder))
                Directory.CreateDirectory(pdfFolder);

            string pdfPath = Path.Combine(pdfFolder, Path.GetFileNameWithoutExtension(filePath) + ".pdf");
            ConvertTxtToPdfArabic(filePath, pdfPath);

            LogUpdate("تمت إضافة مشترك جديد باسم " + name);

            // تحديث العائد الشهري في general.txt بدون حذف بقية المعلومات
            string generalFilePath = Path.Combine(Application.StartupPath, "general.txt");
            List<string> generalContent = new List<string>();

            if (File.Exists(generalFilePath))
            {
                generalContent = File.ReadAllLines(generalFilePath).ToList();
                for (int i = 0; i < generalContent.Count; i++)
                {
                    if (generalContent[i].StartsWith("العائد الشهري:"))
                    {
                        int oldIncome = 0;
                        int.TryParse(generalContent[i].Split(':')[1].Trim().Replace(",", ""), out oldIncome);
                        int newIncome = oldIncome + monthlypay;
                        generalContent[i] = "العائد الشهري: " + newIncome.ToString("N0");
                        break;
                    }
                }
            }

            // If the line doesn't exist, add it
            if (!generalContent.Any(line => line.StartsWith("العائد الشهري:")))
            {
                generalContent.Insert(0, "العائد الشهري: " + monthlypay.ToString("N0"));
            }

            File.WriteAllLines(generalFilePath, generalContent);

            MessageBox.Show("تم تسجيل المشترك وحفظ الملف");

            CountSubscribers();

            monthlyincome = GetTotalMonthlyIncome();
            label14.Text = monthlyincome.ToString("N0") + " د.ع";

            txtName.Text = "";
            txtPhone.Text = "";
            txtAmount.Text = "";
            txtmonthlypay.Text = "";
            txtDevice.Text = "";
            txtMonth.Text = "";

            LoadAllSubscribers();
        }
        public void ConvertTxtToPdfArabic(string txtPath, string pdfPath)
        {
            var arFontPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "arial.ttf");
            BaseFont bf = BaseFont.CreateFont(arFontPath, BaseFont.IDENTITY_H, BaseFont.EMBEDDED);
            iTextSharp.text.Font font = new iTextSharp.text.Font(bf, 12, iTextSharp.text.Font.NORMAL, iTextSharp.text.BaseColor.BLACK);


            using (FileStream fs = new FileStream(pdfPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                Document document = new Document(PageSize.A4);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                string[] lines = File.ReadAllLines(txtPath);

                foreach (var line in lines)
                {
                    // Fix Arabic shaping and direction
                    string fixedLine = ArabicFixer.Fix(line, false, false);
                    Paragraph p = new Paragraph(fixedLine, font)
                    {
                        Alignment = Element.ALIGN_RIGHT // RTL alignment
                    };
                    document.Add(p);
                }

                document.Close();
                writer.Close();
            }
        }

        

        private void button4_Click(object sender, EventArgs e)
        {
            infopanel.Visible = true;
            infopanel.BringToFront();
            panel7.Visible = false;
            alertpanel.Visible = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            infopanel.Visible = false;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            homepanel.Visible = true;
            pictureBox1_Click(pictureBox1, EventArgs.Empty);
            homepanel.BringToFront();
            
        }

        //read monthly income from general.txt
        private int GetTotalMonthlyIncome()
        {
            string generalFilePath = Application.StartupPath + "\\general.txt";

            if (File.Exists(generalFilePath))
            {
                string[] lines = File.ReadAllLines(generalFilePath);

                if (lines.Length > 0)
                {
                    string firstLine = lines[0]; // Should be: "العائد الشهري: [رقم]"

                    string[] parts = firstLine.Split(':');
                    if (parts.Length == 2)
                    {
                        int income;
                        string numberPart = parts[1].Trim().Replace(",", ""); // Remove commas
                        if (int.TryParse(numberPart, out income))
                        {
                            return income;
                        }
                    }
                }
            }

            return 0;
        }

        private void button8_Click(object sender, EventArgs e)
        {

            if (listBoxSubscribers.SelectedItem == null || listBoxSubscribers.SelectedItem.ToString() == "لا توجد نتائج مطابقة")
            {
                MessageBox.Show("لم يتم تحديد مشترك.");
                return;
            }

            string name = listBoxSubscribers.SelectedItem.ToString();
            string folderPath = Application.StartupPath + "\\مشتركين";
            string filePath = Path.Combine(folderPath, name + ".txt");

            if (!File.Exists(filePath))
            {
                MessageBox.Show("الملف غير موجود.");
                return;
            }

            // Step 1: Confirm deletion
            DialogResult result = MessageBox.Show(
                "هل أنت متأكد من حذف هذا المشترك؟",
                "تأكيد الحذف",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (result == DialogResult.No)
            {
                return;
            }

            // Step 2: Read monthly pay and debt from subscriber file
            int monthlyPay = 0;
            int debtAmount = 0;

            foreach (string line in File.ReadAllLines(filePath))
            {
                if (line.StartsWith("الدفع الشهري:"))
                    int.TryParse(line.Split(':')[1].Trim().Replace(",", ""), out monthlyPay);
                else if (line.StartsWith("الديون:"))
                    int.TryParse(line.Split(':')[1].Trim().Replace(",", ""), out debtAmount);
            }

            // Step 3: Read general.txt and update income/debt values
            string generalFilePath = Application.StartupPath + "\\general.txt";
            if (File.Exists(generalFilePath))
            {
                List<string> generalContent = new List<string>(File.ReadAllLines(generalFilePath));
                for (int i = 0; i < generalContent.Count; i++)
                {
                    if (generalContent[i].StartsWith("العائد الشهري:"))
                    {
                        int oldIncome;
                        int.TryParse(generalContent[i].Split(':')[1].Trim().Replace(",", ""), out oldIncome);
                        int newIncome = Math.Max(0, oldIncome - monthlyPay);
                        generalContent[i] = "العائد الشهري: " + newIncome.ToString("N0");
                    }
                    else if (generalContent[i].StartsWith("الديون الكلية:"))
                    {
                        int oldDebt;
                        int.TryParse(generalContent[i].Split(':')[1].Trim().Replace(",", ""), out oldDebt);
                        int newDebt = Math.Max(0, oldDebt - debtAmount);
                        generalContent[i] = "الديون الكلية: " + newDebt.ToString("N0");
                    }
                }
                File.WriteAllLines(generalFilePath, generalContent);
            }

            // Step 4: Delete the subscriber file
            File.Delete(filePath);

            // Step 5: Refresh UI
            LoadAllSubscribers();
            
            txtSearch.Clear();
            MessageBox.Show("تم حذف المشترك بنجاح.");
            LogUpdate("تم حذف المشترك " + name);

            // Step 6: Update UI values
            monthlyincome = GetTotalMonthlyIncome();
            label14.Text = monthlyincome.ToString("N0") + " د.ع";
            CountSubscribers();
            LoadAllSubscribers();
            CheckMonthlyUpdateAlerts();
            LoadSubscribersWithDebt();
            LoadTotalDebt();
            CalculateDebtCount();
            //debt alert red
            updatedebtredpanel();
            richTextBoxDebts.Text = "";
            updatealertpanel();
        }
        //load all subs
        private void LoadAllSubscribers()
        {
            string folderPath = Application.StartupPath + "\\مشتركين";

            listBoxSubscribers.Items.Clear();
            allSubscribers.Clear();

            if (Directory.Exists(folderPath))
            {
                string[] files = Directory.GetFiles(folderPath, "*.txt");

                foreach (string file in files)
                {
                    string subName = Path.GetFileNameWithoutExtension(file);
                    allSubscribers.Add(subName);
                    listBoxSubscribers.Items.Add(subName);
                }
            }
            else
            {
                MessageBox.Show("مجلد المشتركين غير موجود.");
            }
        }

        private void listBoxSubscribers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxSubscribers.SelectedItem == null)
                return;

            string selectedName = listBoxSubscribers.SelectedItem.ToString();
            string pdfFolder = Path.Combine(Application.StartupPath, "مشتركين", "subscribers pdf");
            string pdfPath = Path.Combine(pdfFolder, selectedName + ".pdf");

            if (File.Exists(pdfPath))
            {
                if (pdfViewer.Document != null)
                {
                    pdfViewer.Document.Dispose();
                }
                var pdfDocument = PdfiumViewer.PdfDocument.Load(pdfPath);
                pdfViewer.Document = pdfDocument;
                pdfViewer.Renderer.Zoom = 2.5f;
                
            }
            else
            {
                MessageBox.Show("ملف PDF للمشترك غير موجود.");
            }
        }
        

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string query = txtSearch.Text.Trim().ToLower();
            listBoxSubscribers.Items.Clear();

            bool found = false;

            foreach (string name in allSubscribers)
            {
                if (name.ToLower().Contains(query))
                {
                    listBoxSubscribers.Items.Add(name);
                    found = true;
                }
            }

            if (!found)
            {
                listBoxSubscribers.Items.Add("لا توجد نتائج مطابقة");
                
            }
        }
        private void CheckMonthlyUpdateAlerts()
        {
            string folderPath = Application.StartupPath + "\\مشتركين";
            string[] files = Directory.GetFiles(folderPath, "*.txt");

            listBoxAlerts.Items.Clear();

            foreach (string file in files)
            {
                string[] lines = File.ReadAllLines(file);
                string lastUpdateLine = null;

                foreach (string line in lines)
                {
                    if (line.StartsWith("اخر دفع:"))
                    {
                        lastUpdateLine = line;
                        break;
                    }
                }

                if (lastUpdateLine != null)
                {
                    try
                    {
                        // Extract the date portion: skip "اخر تحديث:" and trim
                        string rawDate = lastUpdateLine.Replace("اخر دفع:", "").Trim();

                        // Get only the date part before any spaces
                        string[] rawParts = rawDate.Split(' ');
                        string datePart = rawParts[0].Trim();

                        // Try parsing the date only (ignore Arabic time part)
                        DateTime lastUpdateDate;
                        if (DateTime.TryParseExact(datePart, "yyyy/M/d", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastUpdateDate))
                        {
                            if ((DateTime.Now - lastUpdateDate).TotalDays >= 30)
                            {
                                string fileName = Path.GetFileNameWithoutExtension(file);
                                listBoxAlerts.Items.Add(fileName + " عليه تنبيه");
                                t++;
                            }
                        }
                    }
                    catch
                    {
                        // Ignore bad format errors silently
                    }
                }
            }

            if (listBoxAlerts.Items.Count == 0)
            {
                listBoxAlerts.Items.Add("لا توجد تنبيهات حالياً");
                t = 0;
            }
            label18.Text = t.ToString() ;
        }

        private void listBoxAlerts_SelectedIndexChanged(object sender, EventArgs e)
        {
             if (listBoxAlerts.SelectedItem == null)
        return;

    string selectedAlert = listBoxAlerts.SelectedItem.ToString();

    // Format: [name] عليه تنبيه
    string suffix = " عليه تنبيه";

    if (selectedAlert.EndsWith(suffix))
    {
        string name = selectedAlert.Substring(0, selectedAlert.Length - suffix.Length);

        string filePath = Path.Combine(Application.StartupPath + "\\مشتركين", name + ".txt");

        if (File.Exists(filePath))
        {
            string content = File.ReadAllText(filePath);
            richTextBoxSubInfo.Text = content; // show in your RichTextBox
        }
        else
        {
            MessageBox.Show("لم يتم العثور على ملف المشترك المحدد.");
        }
    }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            alertpanel.BringToFront();
            alertpanel.Visible = true;
            panel7.Visible = false;
            infopanel.Visible = false;
        }

        private void richTextBoxSubInfo_TextChanged(object sender, EventArgs e)
        {

        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            alertpanel.Visible = false;
        }

        private void button9_Click(object sender, EventArgs e)
        {
            string generalFilePath = Application.StartupPath + "\\general.txt";
            string storeNameLine = "اسم المتجر: " + textBox1.Text;

            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("يرجى إدخال اسم المتجر أولاً.");
                return;
            }

            
                string[] existingLines = File.Exists(generalFilePath) ? File.ReadAllLines(generalFilePath) : new string[0];

                // Create a new list to store the updated lines
                List<string> updatedLines = new List<string>();

                if (existingLines.Length > 0)
                {
                    updatedLines.Add(existingLines[0]); // Keep the original first line
                }

                updatedLines.Add(storeNameLine); // Insert the new line as the second line

                // Add remaining original lines (if any)
                for (int i = 1; i < existingLines.Length; i++)
                {
                    updatedLines.Add(existingLines[i]);
                }

                // Write all lines back to the file
                File.WriteAllLines(generalFilePath, updatedLines.ToArray());


                if (listBoxAlerts.SelectedItem == null)
                {
                    MessageBox.Show("عليك تحديد شخص اولاً");
                    return;
                }

                string content = richTextBoxSubInfo.Text;
                string[] lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

                string phone = "";
                string device = "";
                string monthlyPay = "";
                string storeName = textBox1.Text.Trim();
                string name = "";

                foreach (string line in lines)
                {
                    if (line.StartsWith("رقم الهاتف:"))
                    {
                        phone = line.Substring("رقم الهاتف:".Length).Trim();
                    }
                    else if (line.StartsWith("اسم الجهاز:"))
                    {
                        device = line.Substring("اسم الجهاز:".Length).Trim();
                    }
                    else if (line.StartsWith("الدفع الشهري:"))
                    {
                        monthlyPay = line.Substring("الدفع الشهري:".Length).Trim();
                    }
                     else if (line.StartsWith("الاسم:"))
                    {
                        name = line.Substring("الاسم:".Length).Trim();
                    }
                }

                if (phone == "" || device == "" || monthlyPay == "" || storeName == "")
                {
                    MessageBox.Show("تأكد من ملء كافة المعلومات.");
                    return;
                }
                LogUpdate("تم تذكير " + name+" بقسطه");

                string message = "هذه رسالة تذكيرية من \"" + storeName + "\"\nيرجى دفع قسط جهازك \"" + device + "\" البالغ \"" + monthlyPay + " د.ع\"\nشكراً لك";

                string url = "https://wa.me/964" + phone.Substring(1) + "?text=" + Uri.EscapeDataString(message);
                System.Diagnostics.Process.Start(url);
        }

        private void button11_Click(object sender, EventArgs e)
        {
            string subscriberName = textBoxSubName.Text.Trim();
            string newDebtText = textBoxDebtAmount.Text.Trim();

            if (string.IsNullOrEmpty(subscriberName) || string.IsNullOrEmpty(newDebtText))
            {
                MessageBox.Show("يرجى ملء اسم المشترك ومبلغ الدين.");
                return;
            }
            LogUpdate("تم إضافة دين بقيمة " + newDebtText + " د.ع للمشترك " + subscriberName);
            int newDebt;
            if (!int.TryParse(newDebtText.Replace(",", ""), out newDebt))
            {
                MessageBox.Show("يرجى إدخال رقم صحيح.");
                return;
            }

            string filePath = Path.Combine(Application.StartupPath + "\\مشتركين", subscriberName + ".txt");

            if (!File.Exists(filePath))
            {
                MessageBox.Show("لم يتم العثور على ملف هذا المشترك.");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("الديون:"))
                {
                    string oldDebtStr = lines[i].Substring("الديون:".Length).Trim().Replace(",", "");
                    int oldDebt = 0;
                    int.TryParse(oldDebtStr, out oldDebt);

                    int updatedDebt = oldDebt + newDebt;
                    lines[i] = "الديون: " + updatedDebt.ToString("N0");
                    break;
                }
            }

            File.WriteAllLines(filePath, lines);

            MessageBox.Show("تمت إضافة الدين بنجاح.");
            string generalFilePath = Application.StartupPath + "\\general.txt";
            int totalDebts = 0;
            bool debtLineFound = false;

            // Read all lines from general.txt
            List<string> generalLines = new List<string>();
            if (File.Exists(generalFilePath))
            {
                generalLines = File.ReadAllLines(generalFilePath).ToList();
                for (int i = 0; i < generalLines.Count; i++)
                {
                    if (generalLines[i].StartsWith("الديون الكلية:"))
                    {
                        string oldDebtStr = generalLines[i].Substring("الديون الكلية:".Length).Trim().Replace(",", "");
                        int.TryParse(oldDebtStr, out totalDebts);

                        totalDebts += newDebt;
                        generalLines[i] = "الديون الكلية: " + totalDebts.ToString("N0");
                        debtLineFound = true;
                        break;
                    }
                }
            }

            // If the line doesn't exist, add it
            if (!debtLineFound)
            {
                totalDebts = newDebt;
                generalLines.Add("الديون الكلية: " + totalDebts.ToString("N0"));
            }

            // Write updated content
            File.WriteAllLines(generalFilePath, generalLines);
          
            LoadSubscribersWithDebt();
            LoadTotalDebt();
            CalculateDebtCount();
            updatedebtredpanel();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            panel5.Visible = false;
        }

        private void button10_Click(object sender, EventArgs e)
        {
            panel7.BringToFront();
            panel7.Visible = true;
            alertpanel.Visible = false;
            infopanel.Visible = false;
            LoadSubscribersWithDebt();
        }
        private void LoadSubscribersWithDebt()
        {
            listBoxDebts.Items.Clear();

            string folderPath = Application.StartupPath + "\\مشتركين";
            if (!Directory.Exists(folderPath)) return;

            string[] files = Directory.GetFiles(folderPath, "*.txt");

            foreach (string file in files)
            {
                string[] lines = File.ReadAllLines(file);
                foreach (string line in lines)
                {
                    if (line.StartsWith("الديون:"))
                    {
                        string debtStr = line.Split(':')[1].Trim().Replace(",", "");

                        int debtAmount;
                        if (int.TryParse(debtStr, out debtAmount) && debtAmount > 0)
                        {
                            string subName = Path.GetFileNameWithoutExtension(file);
                            listBoxDebts.Items.Add(subName);
                        }
                        break;
                    }
                }
            }

        }
        //listbox activity log
        private void LogUpdate(string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy/MM/dd - hh:mm tt");
            string fullMessage = "[" + timestamp + "] " + message;
            listBoxUpdates.Items.Insert(0, fullMessage);

            // Limit to 20 messages
            if (listBoxUpdates.Items.Count > 20)
            {
                listBoxUpdates.Items.RemoveAt(20);
            }

            // Save all items to file
            string path = Application.StartupPath + "\\recent_updates.txt";
            File.WriteAllLines(path, listBoxUpdates.Items.Cast<string>());
        }
        private void LoadRecentUpdates()
        {
            string path = Application.StartupPath + "\\recent_updates.txt";
            if (File.Exists(path))
            {
                string[] updates = File.ReadAllLines(path);
                listBoxUpdates.Items.Clear();
                foreach (string update in updates)
                {
                    listBoxUpdates.Items.Add(update);
                }
            }
        }

        private void listBoxDebts_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (listBoxDebts.SelectedItem == null) return;

            string name = listBoxDebts.SelectedItem.ToString();
            string filePath = Path.Combine(Application.StartupPath + "\\مشتركين", name + ".txt");

            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                richTextBoxDebts.Text = content;
            }
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (listBoxDebts.SelectedItem == null)
            {
                MessageBox.Show("الرجاء اختيار مشترك من القائمة.");
                return;
            }

            string subName = listBoxDebts.SelectedItem.ToString();
            string folderPath = Application.StartupPath + "\\مشتركين";
            string filePath = Path.Combine(folderPath, subName + ".txt");

            if (!File.Exists(filePath))
            {
                MessageBox.Show("ملف المشترك غير موجود.");
                return;
            }

            string phone = "";
            string device = "";
            string debt = "";
            string name = "";

            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (line.StartsWith("رقم الهاتف:"))
                {
                    phone = line.Split(':')[1].Trim();
                }
                else if (line.StartsWith("الديون:"))
                {
                    debt = line.Split(':')[1].Trim();
                }
                else if (line.StartsWith("اسم الجهاز:"))
                {
                    device = line.Split(':')[1].Trim();
                }
                else if (line.StartsWith("الاسم:"))
                {
                    name = line.Substring("الاسم:".Length).Trim();
                }
            }
            
                

                LogUpdate("تم تذكير " + name +" بدينه ");
            if (phone == "" || debt == "" || device == "")
            {
                MessageBox.Show("تعذر استخراج معلومات المشترك بشكل صحيح.");
                return;
            }

            string storeName = txtStoreName.Text.Trim();
            if (storeName == "")
            {
                MessageBox.Show("يرجى إدخال اسم المتجر.");
                return;
            }

            // Construct the message
            string message = "هذه رسالة تذكيرية من \"" + storeName + "\"\nيرجى تسديد مبلغ الدين البالغ \"" + debt + "\"\nلجهازك \"" + device + "\"\nشكراً لك";

            // Encode and send
            string encodedMessage = Uri.EscapeDataString(message);
            string whatsappUrl = "https://wa.me/964" + phone + "?text=" + encodedMessage;

            System.Diagnostics.Process.Start(whatsappUrl);


        }

        private void button14_Click(object sender, EventArgs e)
        {
            panel7.Visible = false;
        }

        private void button15_Click(object sender, EventArgs e)
        {
            panel5.Visible = true;
            panel5.BringToFront();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            string subscriberName = textBoxSubName.Text.Trim();
            string removeDebtText = textBoxDebtAmount.Text.Trim();

            if (string.IsNullOrEmpty(subscriberName) || string.IsNullOrEmpty(removeDebtText))
            {
                MessageBox.Show("يرجى ملء اسم المشترك ومبلغ الخصم.");
                return;
            }

            LogUpdate("تم خصم مبلغ " + removeDebtText + " د.ع من ديون المشترك " + subscriberName);

            int removeDebt = 0;
            if (!int.TryParse(removeDebtText.Replace(",", ""), out removeDebt) || removeDebt <= 0)
            {
                MessageBox.Show("يرجى إدخال رقم صحيح.");
                return;
            }

            string filePath = Path.Combine(Application.StartupPath + "\\مشتركين", subscriberName + ".txt");

            if (!File.Exists(filePath))
            {
                MessageBox.Show("لم يتم العثور على ملف هذا المشترك.");
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("الديون:"))
                {
                    string oldDebtStr = lines[i].Substring("الديون:".Length).Trim().Replace(",", "");
                    int oldDebt = 0;
                    int.TryParse(oldDebtStr, out oldDebt);

                    int updatedDebt = oldDebt - removeDebt;
                    if (updatedDebt < 0) updatedDebt = 0;

                    lines[i] = "الديون: " + updatedDebt.ToString("N0");
                    break;
                }
            }

            File.WriteAllLines(filePath, lines);

            string generalFilePath = Application.StartupPath + "\\general.txt";
            int totalDebts = 0;

            if (File.Exists(generalFilePath))
            {
                List<string> generalLines = File.ReadAllLines(generalFilePath).ToList();
                for (int i = 0; i < generalLines.Count; i++)
                {
                    if (generalLines[i].StartsWith("الديون الكلية:"))
                    {
                        string totalDebtStr = generalLines[i].Substring("الديون الكلية:".Length).Trim().Replace(",", "");
                        int.TryParse(totalDebtStr, out totalDebts);

                        totalDebts -= removeDebt;
                        if (totalDebts < 0) totalDebts = 0;

                        generalLines[i] = "الديون الكلية: " + totalDebts.ToString("N0");
                        break;
                    }
                }
                File.WriteAllLines(generalFilePath, generalLines.ToArray());
            }

            MessageBox.Show("تم خصم الدين بنجاح.");
           
            LoadSubscribersWithDebt();
            LoadTotalDebt();
            CalculateDebtCount();
            updatedebtredpanel();
        }

       
        

        







    }

}


