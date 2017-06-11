using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections;
using Transfer.Properties;
using System.Data.OleDb;

namespace Transfer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        int icon;
        string dosya, yol, yeniyol, masaustu, KullaniciAdi, Sifre, Ftp;
        FtpWebRequest FTP;
        sorgu sg = new sorgu();
        WebClient wc = new WebClient();

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // kullanıcının masaüstü yolunu bulan kod
                masaustu = Environment.GetFolderPath(System.Environment.SpecialFolder.DesktopDirectory);
                // kullanıcının masaüstünde Transfer2 adında dosya olup olmadığını kontrol ediyoruz.
                if (!Directory.Exists(masaustu + "/Transfer2"))
                {
                    // klasör yoksa biz CreateDirectory diyerek masaüstünde bir tane klasör oluşturuyoruz.
                    Directory.CreateDirectory(masaustu + "/Transfer2");
                }
                //DataTable oluşturmamızın sebebi veri tabanında bulunan ftp bilgilerini alacak olmamız 
                //veri tabanı bağlantılarımızı sorgu adında başka bir sınıf (class) içinde tanımlandı
                //ve ihtiyacımız oldukça oradan çağırıyoruz.
                DataTable dt = sg.baglan();
                comboBox3.DataSource = dt;
                comboBox3.ValueMember = "Ad";
                //FileSystemWatcher ile belirlediğimiz herhangi bir dosyayı dinleyebiliriz.
                //biz kişinin masaüstündeki Transfer2 klasörünü dinleyecez buraya sürüklenip atılan dosyaları ftp’ ye aktarıcaz.
                FileSystemWatcher fsw = new FileSystemWatcher();
                //Filter dinlenecek olan dosyanın türünü belirlememizi sağlar *.* tüm dosyaları dinle anlamına gelir
                fsw.Filter = "*.*";
                // IncludeSubdirectories klasörün altındaki alt klasörleri de dinle demek. 
                fsw.IncludeSubdirectories = true;
                //NotifyFilter aktarılacak dosyanın hangi özelliklerine ulaşmak istediğimizi belirler.
                fsw.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime |
                                                    NotifyFilters.FileName | NotifyFilters.Size;
                // Path dosya yolunu belirler
                fsw.Path = masaustu + "/Transfer2";
                //  Created ve Deleted kullanacağımız event’lerdir (event’ler belirli koşullarda tetiklenecek olan olaylardır).
                fsw.Created += new FileSystemEventHandler(fsw_Created);
                fsw.Deleted += new FileSystemEventHandler(fsw_Deleted);
                // EnableRaisingEvents ile aktif hale getiriyoruz.
                //Veri tabanımızdan çektiğimiz bilgileri gerekli alanlara atıyoruz bunlar KullaniciAdi , Sifre , Ftp  
                fsw.EnableRaisingEvents = true;
                KullaniciAdi = dt.Rows[0]["KullaniciAdi"].ToString();
                Sifre = dt.Rows[0]["Sifre"].ToString();
                Ftp = dt.Rows[0]["FtpAdresi"].ToString();
                yol = Ftp;
                label2.Text = yol;
                listView1.Items.Clear();
                Listele(KullaniciAdi, Sifre, Ftp);
                this.Icon = Resources.paylas2;
            }
            catch (Exception)
            {

            }
        }

        void fsw_Deleted(object sender, FileSystemEventArgs e)
        {

        }

        // FTP YE DOSYA GÖNDEREN METOT  UPLOAD

        void fsw_Created(object sender, FileSystemEventArgs e)
        {
            // Burada ilk etapda Transfer2 klasör’üne bırakılan dosyanın tam olarak yolu’nu alıyoruz
            // bunuda e.Name özelliği sayesinde elde ediyoruz. 
            FileInfo FI = new FileInfo(masaustu + "/Transfer2/" + e.Name);
            // uri değişkenimize label2 de o an bulunan yolu alıyoruz ve dosyamızın adını ekliyoruz bu bizim yükleyeceğimiz dosyanı yolu.
            string uri = label2.Text + FI.Name;
            // FtpWebRequest yaratıyoruz ve bu uri değişkenimizi veriyoruz.
            FTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
            // Credentials kısmına KullaniciAdi ve Sifre bilgilerimi giriyoruz dosyayı Upload yapacağımızı belirtiyoruz.
            // UseBinary özelliğini true olarak ayarlıyoruz daha sonra byte[] dizisi oluşturuyoruz 
            FTP.Credentials = new NetworkCredential(KullaniciAdi, Sifre);
            FTP.KeepAlive = false;
            FTP.Method = WebRequestMethods.Ftp.UploadFile;
            FTP.UseBinary = true;
            FTP.ContentLength = FI.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            // FileStream oluşturuyoruz ve FS değişkenimize  FI değişkenimizi okuyup aktarıyoruz.
            FileStream FS = FI.OpenRead();
            // Daha sonra try-catch içinde while döngüsü yardımı ile dosyamızı yazdırıyoruz.
            // Bu kısımda oluşabilecek herhangi bir hatayı catch kısmında yakalıyoruz ve ekrana anlaşılır bir biçimde hatayı verdiriyoruz.
            try
            {
                Application.DoEvents();
                Stream strm = FTP.GetRequestStream();
                contentLen = FS.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = FS.Read(buff, 0, buffLength);
                }
                strm.Close();
                FS.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata");
            }
        }
        // Burada Listele method’u 3 tane parametre almıştır bunlar hangi ftp’ye bağlanacağı ve bu ftp’nin kullanıcı bilgileridir.
        //Kendisine gönderilen ftp bilgilerine göre o ftp’bağlanıp oradaki bütün dosyaları listView’e atmaktadır
        //ama bunu yaparken dosyanın uzantısına göre imageList’in içinden doğru icon’u bulup onu ekranda göstermektedir. 
        //imageList’in içine icon dosyalarımızı sırası ile atıyoruz ki sonradan Listele method’u oradaki icon’ları id lerine göre alıyor ve ekranda gösteriyor.

        public void Listele(string KullaniciAdi, string Sifre, string Url)
        {
            string[] DosyaListesi;
            StringBuilder result = new StringBuilder();
            FtpWebRequest FTP;
            try
            {
                FTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(Url));
                FTP.UseBinary = true;
                FTP.Credentials = new NetworkCredential(KullaniciAdi, Sifre);
                FTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = FTP.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string line = reader.ReadLine();
                while (line != null)
                {
                    result.Append(line);
                    result.Append("\n");
                    line = reader.ReadLine();
                }
                result.Remove(result.ToString().LastIndexOf('\n'), 1);
                reader.Close();
                response.Close();
                DosyaListesi = result.ToString().Split('\n');
                for (int x = 0; x < DosyaListesi.Length; x++)
                {
                    int nokta = DosyaListesi[x].LastIndexOf('.') + 1;

                    string dosyauznatisi = DosyaListesi[x].ToString().Substring(nokta, DosyaListesi[x].ToString().Length - nokta);

                    #region İcon'lar
                    if (dosyauznatisi != "xls" && dosyauznatisi != "exe" && dosyauznatisi != "pdf" && dosyauznatisi != "htm" && dosyauznatisi != "html" && dosyauznatisi != "doc" && dosyauznatisi != "mp3" && dosyauznatisi != "mp4" && dosyauznatisi != "wav" && dosyauznatisi != "xlsx" && dosyauznatisi != "txt" && dosyauznatisi != "rar" && dosyauznatisi != "dll" && dosyauznatisi != "png" && dosyauznatisi != "jpg" && dosyauznatisi != "psd" && dosyauznatisi != "avi" && dosyauznatisi != "ico" && dosyauznatisi != "bmp" && dosyauznatisi != "gif" && dosyauznatisi != "css" && dosyauznatisi != "aspx" && dosyauznatisi != "php" && dosyauznatisi != "js" && dosyauznatisi != "swf" && dosyauznatisi != "cs")
                    {

                        if (nokta == 0)
                        {
                            icon = 6;
                        }
                        else
                        {
                            icon = 23;
                        }
                    }
                    else
                        if (dosyauznatisi == "xls" || dosyauznatisi == "xlsx")
                        {
                            icon = 0;
                        }
                        else if (dosyauznatisi == "exe")
                        {
                            icon = 1;
                        }
                        else if (dosyauznatisi == "pdf")
                        {
                            icon = 2;
                        }
                        else if (dosyauznatisi == "html" || dosyauznatisi == "htm")
                        {
                            icon = 3;
                        }
                        else if (dosyauznatisi == "doc")
                        {
                            icon = 4;
                        }
                        else if (dosyauznatisi == "mp3" || dosyauznatisi == "mp4" || dosyauznatisi == "wav")
                        {
                            icon = 5;
                        }
                        else if (dosyauznatisi == "txt")
                        {
                            icon = 7;
                        }
                        else if (dosyauznatisi == "rar" || dosyauznatisi == "zip")
                        {
                            icon = 8;
                        }
                        else if (dosyauznatisi == "dll")
                        {
                            icon = 9;
                        }
                        else if (dosyauznatisi == "png")
                        {
                            icon = 10;
                        }
                        else if (dosyauznatisi == "jpg")
                        {
                            icon = 11;
                        }
                        else if (dosyauznatisi == "psd")
                        {
                            icon = 12;
                        }
                        else if (dosyauznatisi == "avi")
                        {
                            icon = 13;
                        }
                        else if (dosyauznatisi == "ico")
                        {
                            icon = 14;
                        }
                        else if (dosyauznatisi == "bmp")
                        {
                            icon = 15;
                        }
                        else if (dosyauznatisi == "gif")
                        {
                            icon = 16;
                        }
                        else if (dosyauznatisi == "css")
                        {
                            icon = 17;
                        }
                        else if (dosyauznatisi == "aspx")
                        {
                            icon = 18;
                        }
                        else if (dosyauznatisi == "php")
                        {
                            icon = 19;
                        }
                        else if (dosyauznatisi == "js")
                        {
                            icon = 20;
                        }
                        else if (dosyauznatisi == "swf")
                        {
                            icon = 21;
                        }
                        else if (dosyauznatisi == "cs")
                        {
                            icon = 22;
                        }
                        else if (dosyauznatisi == null)
                        {
                            icon = 23;
                        }
                     #endregion
                   
                    DirectoryInfo di = new DirectoryInfo(DosyaListesi[x].ToString());
                    ListViewItem item = new ListViewItem();
                    item.Text = di.Name;
                    item.ImageIndex = icon;
                    item.Tag = di;
                    String info = String.Format(
                   "Name: {0}\nCreation Time: {1}\nParent: {2}\nRoot: {3}",
                   di.Name,
                   di.CreationTime,
                   di.Parent,
                   di.Root
                   );
                    item.ToolTipText = info;
                    listView1.Items.Add(item);
                   
                }
            }
            catch (Exception)
            {
                yol = label2.Text;
            }
        }

        private void yenileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            Listele(KullaniciAdi, Sifre, label2.Text);
        }

        // FTP DEN DOSYA ALAN METOT DOWNLOAD

        private void kaydetToolStripMenuItem_Click(object sender, EventArgs e)
        {
             // Ftp’deki dosyayı kendi bilgisayarımıza indirmek için gerekli kodlama.
            // NetworkCredential ile kimlik doğrulanması gibi işlemlerde kullanılır. 
            wc.Credentials = new NetworkCredential(KullaniciAdi, Sifre);
            // oluşturduğumuz bu byte[] dizisi ile  listView ‘ de seçili olan dosyanın tam yolunu buluyoruz.
            byte[] dosya = wc.DownloadData(label2.Text + listView1.SelectedItems[0].Text);
            // Daha sonra bu dosyayı FileStream’e yazdırıyoruz ve stream değişkenimizi close() diyerek sonlandırıyoruz.
            // aynı şekilde kaydettiğimiz nesneleri masaüstündeki transfer2 içine atıyoruz.
            FileStream stream = File.Create(masaustu + "/Transfer2/" + listView1.SelectedItems[0].Text);
            stream.Write(dosya, 0, dosya.Length);
            stream.Close();
        }
            
        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            // eğer listwiew de nesne haricinde olan boş bir alana sağ tıklayıp kaydet dersek hata alırız.
            // bu hatadan kurtulmak için bu kodu yazıyoruz.
            if (listView1.SelectedItems.Count == 0)
            {
                kaydetToolStripMenuItem.Enabled = false;
                silToolStripMenuItem.Enabled = false;
                yenileToolStripMenuItem.Enabled = true;
            }
            else
            {
                kaydetToolStripMenuItem.Enabled = true;
                silToolStripMenuItem.Enabled = true;
                yenileToolStripMenuItem.Enabled = false;
            }
        }

        // FTP DEKİ BİLGİYİ SİLMWK İÇİN DELETE

        private void silToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Ftp ‘ de işlem yapacağım için ilk olarak FtpWebRequest nesnesini yaratıyorum.
            //daha sonra işlem sırasında hata verirse hatayı yakalayabilmek için 
            //try-catch ‘e sokuyorum ve geri kalan kodlarımızı orada yazıyoruz.

            FtpWebRequest FTP;
            try
            {
                //Sileceğimiz dosyanın yolunu label2 + seçilen dosya adı olarak buluyoruz 

                FTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(label2.Text + listView1.SelectedItems[0].Text));
                FTP.UseBinary = true;
                //Credentials özelliğine kullanıcı adı ve şifre bilgilerimizi giriyoruz 
                //daha sonra ftp’den dosya silme işlemi yapacağımız için WebRequestMethods.Ftp ‘ yi DeleteFile  olarak ayarlıyoruz.
                FTP.Credentials = new NetworkCredential(KullaniciAdi, Sifre);
                FTP.Method = WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse response = (FtpWebResponse)FTP.GetResponse();
                //Silme işlemini yaptıktan sonra listView nesnemizi Clear() koutu ile silip tekrardan güncel bilgi ile yeniliyoruz.
                //Listele method’unu daha önce yazmıştık tekrar aynı method’u çağırarak listemizin yenilenmesini sağlıyoruz.
                //daha sonra kullanıcıya dosyanın silindiğini MessageBox.Show komutu ile bildiriyoruz.
                //eğer işlem sırasında bir hata çıkarsa hatayı catch bolg’unda yakalıyorum ve kullanıcıya mantıklı anlaşılır bir mesaj gösteriyoruz
                listView1.Items.Clear();
                Listele(KullaniciAdi, Sifre, label2.Text);
                MessageBox.Show("Dosya Silindi.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Hata");
            }
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            // Kullanılan bilgisayarın ekran genişlik ve yükseklik değerlerini ilk 2 satır kod yardımı ile alıyoruz.
            int genişlik = Screen.PrimaryScreen.Bounds.Width;
            int yukseklik = Screen.PrimaryScreen.Bounds.Height;
            // daha sonra formun tam ekran olma olayını WindowsState == FormWindowState.Maximized satırı ile anlıyoruz .
            if (WindowState == FormWindowState.Maximized)
            {
                listView1.Size = new Size(525, 1005);
            }
            else
            {
                listView1.Size = new Size(525, 425);
            }
        }
         
        // ÇİFT TIKLAMADA DOSYANIN İÇİNİ AÇMAMIZI SAĞLAYAN METHOT

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                //bakmamız gereken kısım çitt tıklanan bir klasörmü yada dosyamı dosya ise bir resimmi olması lazım 
                //biz de bunu kontrol için çift tıklama anında seçili olan dosyanın tam adını alıyoruz içinde   .   varmı diye bakıyoruz
                //eğer nokta yoksa bu bir klasördür ve biz bunu kullanamayız ama    .   varsa resim olmama ihtimaline karşılık 
                string dosya = listView1.SelectedItems[0].Text;
                if (!dosya.Contains('.'))
                {
                    listView1.Items.Clear();
                    yol = label2.Text + dosya + "/";
                    Listele(KullaniciAdi, Sifre, yol);
                    label2.Text = yol;
                }
                //şeçilen dosyanın içinde .jpg , .png ve son olarakta .bmp  karakterleri geçiyormu bunu kontrol ediyoruz
                //eğer seçilen dosya resimse her koşul sağlanıyor demektir ve diğer formumuzu resim ön izleme yapmak için açabiliriz ,
                //resimonizle formumuzu new’leyerek  ve ShowDialog() diyerek açıyoruz.
               /* else if (dosya.Contains(".jpg") || dosya.Contains(".png") || dosya.Contains(".bmp"))
                {
                    label3.Text = label2.Text + "/" + dosya;
                    resimonizle ro = new resimonizle();
                    ro.ShowDialog();
                }
                */
            }
            catch (Exception)
            {

            }
        }
         

        private void toolStripMenuItem4_Click(object sender, EventArgs e)
        {
            // İleri buton’una tıklandığı zaman kesinlikle bir tane klasör seçili olması lazım 
            // onun için ilk olarak seçili dosya sayısını kontrol ediyoruz eğer seçili dosya sayısı 0 deyilse dosya seçili demektir ve bizde devam ediyoruz 
            if (listView1.SelectedItems.Count != 0)
            {
                dosya = listView1.SelectedItems[0].Text;
                if (!dosya.Contains('.'))
                {
                    listView1.Items.Clear();
                    yol = label2.Text + dosya + "/";
                    Listele(KullaniciAdi, Sifre, yol);
                    label2.Text = yol;
                }
                else
                {
                    MessageBox.Show("Klasör seçiniz.", "Bilgi mesajı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                MessageBox.Show("Klasör seçiniz.", "Bilgi mesajı", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            // Geri buton’umuzu kodlarken ise label2 ‘nin boş olup olmadığını kontrol etmemiz lazım 
            // bununda sebebi eğer label2 boş ise geri gitmek istediğimiz zaman program hata verecektir 
            // gerekli string parçalamalarını yaptıktan sonra tekrar Listele method’umuzu çalıştırıyoruz 
            // tabiki bu sefer bir arkada bulunan dosyanın yolunu veriyoruz Listele method’umuza böylece bir üst dizine çıkmış oluyoruz
            if (label2.Text != "")
            {
                int geri = label2.Text.Substring(0, label2.Text.Length - 1).LastIndexOf('/');
                int fark = label2.Text.Length - geri;
                yeniyol = label2.Text.Substring(0, label2.Text.Length - fark + 1);
                listView1.Items.Clear();
                label2.Text = yeniyol;
                Listele(KullaniciAdi, Sifre, yeniyol);
            }

        }


        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
               // DataRowView dv = (DataRowView)comboBox3.SelectedValue;
                DataTable dt = sg.Sorgula(comboBox3.Text);
                KullaniciAdi = dt.Rows[0]["KullaniciAdi"].ToString();
                Sifre = dt.Rows[0]["Sifre"].ToString();
                Ftp = dt.Rows[0]["FtpAdresi"].ToString();
                yol = Ftp;
                label2.Text = yol;
                listView1.Items.Clear();
                Listele(KullaniciAdi, Sifre, Ftp);
            }
            catch (Exception)
            {

            }
        }

        private void fTPBilgileriToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // sadece kayıt formuna yönlendirme işlevini yapar.
            ftpbilgi ftp = new ftpbilgi();
            ftp.ShowDialog();
        }

        private void label1_TextChanged(object sender, EventArgs e)
        {
            // Burada sadece veri tabanından çektiğimiz ftp bilgilerini comboBox ‘ a basıyoruz 
            // comboBox’ın DataSource özelliğine DataTable ‘ mizi aktarıyoruz ValueMember özelliğine ise veri tabanındaki hangi alanın ekranda görünmesini istiyorsak o alanı atıyoruz
            DataTable dt = sg.baglan();
            comboBox3.DataSource = dt;
            comboBox3.ValueMember = "Ad";
        }

        private void çıkışToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
