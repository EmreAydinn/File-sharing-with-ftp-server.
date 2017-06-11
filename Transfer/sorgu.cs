using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.OleDb;
using System.Data;

namespace Transfer
{
    class sorgu
    {
        public OleDbConnection baglanti()
        {
            OleDbConnection bag = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;data source =Transfer.mdb");
            bag.Open();
            return bag;
        }

        public DataTable baglan()
        {
            // baglan method’u ile veri tabanındaki tüm verileri çekip DataTable cinsinden geri döndürüyoruz.
            OleDbConnection bag = baglanti();
            OleDbCommand cmd = new OleDbCommand("select * from Bilgiler", bag);
            DataTable dt = new DataTable();
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            da.Fill(dt);
            bag.Close();
            bag.Dispose();
            cmd.Dispose();
            return dt;
        }

        public DataTable Sorgula(string Ad)
        {
            // Sorgula method’ u ile parametre olarak gönderilen Ad ile veri tabanımızda bu ada göre arama yapıp geriye DataTable döndürüyoruz.
            OleDbConnection bag = baglanti();
            OleDbCommand cmd = new OleDbCommand("select * from Bilgiler where Ad = @Ad", bag);
            cmd.Parameters.Add("@Ad",Ad);
            DataTable dt = new DataTable();
            OleDbDataAdapter da = new OleDbDataAdapter(cmd);
            da.Fill(dt);
            bag.Close();
            bag.Dispose();
            cmd.Dispose();
            return dt;
        }

        public void FtpBilgiKayit(string GorunecekAd, string Ftp, string KullaniciAdi, string Sifre)
        {
            // FtpBilgiKayit method’u ile kullanıcın parametre olarak gönderdiği bilgileri veri tabanına kaydediyoruz.
            // Burada sadece kayıt yapduğımız için geriye birşey döndürmemize gerek yok.
            OleDbConnection bag = baglanti();
            OleDbCommand cmd = new OleDbCommand("insert into Bilgiler (Ad,FtpAdresi,KullaniciAdi,Sifre) " +
                                                            "values (@GorunecekAd,@Ftp,@KullaniciAdi,@Sifre)", bag);
            cmd.Parameters.Add("@GorunecekAd", GorunecekAd);
            cmd.Parameters.Add("@Ftp",Ftp);
            cmd.Parameters.Add("@KullaniciAdi", KullaniciAdi);
            cmd.Parameters.Add("@Sifre", Sifre);
            cmd.ExecuteNonQuery();
            bag.Close();
            bag.Dispose();
            cmd.Dispose();
        }

    }
}
