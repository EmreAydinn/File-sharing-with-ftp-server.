using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Transfer.Properties;

namespace Transfer
{
    public partial class ftpbilgi : Form
    {
        sorgu sg = new sorgu();
        public ftpbilgi()
        {
            InitializeComponent();
        }

        private void ftpbilgi_Load(object sender, EventArgs e)
        {
            this.Icon = Resources.paylas2;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text != "" && textBox2.Text != "" && textBox3.Text != "" && textBox4.Text != "")
                {
                    string ftp =  textBox1.Text;
                    sg.FtpBilgiKayit(textBox2.Text, ftp, textBox3.Text, textBox4.Text);
                    foreach (Control item in groupBox1.Controls)
                    {
                        if (item is TextBox)
                        {
                            item.Text = "";
                        }
                    }
                    Random rdm = new Random();
                    Application.OpenForms["Form1"].Controls["label1"].Text = rdm.Next(10000).ToString();
                    MessageBox.Show("Kayıt başarılı.", "Bilgi mesajı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                   // Kayıt başarılı mesajından heman önce random bir sayı üretip Form1 ‘ deki label1 nesnesine atadık
                   // bunu yapmamızdaki amacı şu şekilde anlatabiliriz kullanıcı yeni bir tane ftp bilgisi eklediği zaman 
                   // bunun ana formumuzda gözükmesi için programın kapanıp tekrardan açılması gerekir normal şartlarda 
                   // ama bu şekilde form1 ‘ deki label1 ‘ in text özelliğine random bir sayı atar vede form1′ deki label1 ‘ in 
                  // textChanged event’inde form1 ‘ deki comboBox ‘ a tekrardan verileri veri tabanın’dan çekip basarsak ,
                    //hiç programı kapatıp açmakla uğraşmadan comboBox ‘ taki verilerimizi güncellemiş oluruz
                }
                else
                {
                    MessageBox.Show("Boş alan bırakmayınız !", "Bilgi mesajı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
              }
            catch (Exception)
            {
                MessageBox.Show("Hata oluştu tekrar deneyiniz !", "Bilgi mesajı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }



    }
}
