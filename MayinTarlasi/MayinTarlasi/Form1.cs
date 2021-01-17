using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Media;

namespace MayinTarlasi
{
    public partial class Form1 : Form
    {
        private const int boyut = 400;//mayın tarlası boyutu
        Image[] mayinResim = new Image[1];//mayın resmini resim dizisine atmak için yaptım
        private int mayinSayisi = 0;//mayin sayisinin ön ataması sonra kullanıcı tarafından değiştirilecek
        private string seciliLabel;//hangi labeldeyiz ?
        private int seciliLabelinNoSu = 0;//labelin numarası ne?
        private DateTime ilkTarih;//oyuna başlama zamanı
        private DateTime suanKi;//oyun sonu zaman
        bool zamanAlindiMi = false;//ilk işlem yapıldıgında süre başlasın sonraki işlemlerde bidaha baştan başlamasın
        private bool enSagaGeldiMi = false, enSolaGeldiMi = false;//oyunun bitiş koşulları en ust sol ve sağ köşe
        List<Label> sonBas = new List<Label>();//0 ve 19 numaralı labeller
        SoundPlayer myPlayer = new SoundPlayer();//müzik çalma nesnesi

        public Form1()
        {
            InitializeComponent();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton zorlukSeviye = (RadioButton)sender;//senderdan gelen bilgi radiobutton dur
            switch (zorlukSeviye.Text)
            {
                case "Kolay":
                    mayinSayisi = 0;
                    mayinOlustur();
                    kapat();
                    break;
                case "Orta":
                    mayinSayisi = 50;   //butonun textine göre zorluk seviyesi seçimi
                    mayinOlustur();
                    kapat();
                    break;
                case "Zor":
                    mayinSayisi = 80;
                    mayinOlustur();
                    kapat();
                    break;
            }
        }
        private void kapat()//mayınlar dizildikten sonra mayın sayısında bir değişiklik yapılmasını önlüyorum
        {
            radioButton1.Enabled = false;
            radioButton2.Enabled = false;
            radioButton3.Enabled = false;
        }
        private void mayinOlustur()//mayınları oluşturan fonksiyon
        {
            mayinResim[0] = ımageList1.Images[10];//mayın resmini resim dizisine attım
            int[] mayinlar = new int[mayinSayisi];//mayın sayısına göre bir mayın dizisi oluşturdum
            Random rnd0 = new Random();

            for (int i = 0; i < mayinSayisi; i++)
            {
            x:
                int secilenMayin = rnd0.Next(0, 400);//0 dan 400 e kadar rastgele bir int değer oluşturdum
                if (mayinlar.Contains(secilenMayin))//herbir mayının labellara ayrı ayrı dağılması için
                {                                   //int değeri farklı indexlere dağıtması gerekiyor
                    goto x;                         //bu yüzden aynı değerler olmamalı
                }

                mayinlar[i] = secilenMayin;//koşullar sağlanırsa değeri diziye at
            }

            for (int i = 0; i < boyut; i++)//0 dan 400 e kadar label olusturuyorum
            {
                Label lbl = new Label();
                lbl.Name = i.ToString();
                lbl.Width = 15;
                lbl.Height = 15;
                lbl.Margin = new Padding(0, -1, -1, -1);
                lbl.Tag = mayinlar.Contains(i);//i değeri mayınlar dizisinin içerisinde var mı?(true or false)
                if (i == 0 || i == 19)
                {
                    sonBas.Add(lbl); //en sağ ve sol üst koseleri listeye atarak kaydettim çünkü
                }                      //oyun bitiş koşulunda en üst sağ ve sol köşelerde mayın varsa
                if (i == 390)          //oyuncu üstteki herhangi bir yere varırsa kazanacaktır
                {//oyun başlangıç konumu
                    lbl.Image = ımageList1.Images[6];//resmini belirtiyorum
                    seciliLabel = lbl.Name;//hangi konumda oldugunu belirtiyorum 
                    seciliLabelinNoSu = int.Parse(seciliLabel);//konum no sunu alıyorum

                    if ((bool)lbl.Tag == true)//label mayın içeriyor mu?
                    {//ilk başlangıç konumuna da işlemi uyguladım çünkü burada da mayın olabilir
                        foreach (Label lbl1 in flowLayoutPanel1.Controls)//labelleri gez
                        {
                            if ((bool)lbl1.Tag == true)//label mayın içeriyorsa
                            {
                                lbl1.Image = mayinResim[0];//mayın resmini goster
                            }
                            else//label mayın içermiyorsa
                            {
                                lbl1.Image = null;//herhangi bir resim gosterme
                            }
                        }
                        oyunBasarisiz();//başarızlık durumu fonksiyonunu çağır
                    }
                    cevredekiMayin();//çevredeki mayınları göster
                }
                lbl.Click += label_click;// her label aynı eventa gidecek
                flowLayoutPanel1.Controls.Add(lbl);//labelleri flowlayoutpanele ekle
            }
        }
        private void label_click(object sender, EventArgs e)//labela basıldıgında(yön tuşlarıyla bastırıyorum)
        {
            timer1.Start();//süreyi başlat
            if (zamanAlindiMi == false)//bir sonraki basışta süreyi baştan başlatmamak için yaptım
            {
                ilkTarih = DateTime.Now;//başlama zamanını alıyorum
                myPlayer.Stream = Properties.Resources.x;//kaynak dosyasına attıgım müziği buluyorum
                myPlayer.Play();//müziği açıyorum
                zamanAlindiMi = true;//bidaha başlatma
            }
            Label lbl = (Label)sender;//sender dan gelen bilgi labeldir
            bool basilipBasilmamaDurumu = (bool)lbl.Tag;//labelda mayın var mı ?

            if (basilipBasilmamaDurumu == true)//eğer labelde mayın varsa
            {
                foreach (Label lbl1 in flowLayoutPanel1.Controls)//labelleri gez
                {
                    if ((bool)lbl1.Tag == true)//eğer labelde mayın varsa
                    {
                        lbl1.Image = mayinResim[0];//mayın resmini goster
                    }
                    else
                    {
                        lbl1.Image = null;//yoksa herhangi bir resim gosterme
                    }

                }
                timer1.Stop();//zamanı durdur
                oyunBasarisiz();//başarısızlık durumunu goster
            }
            else
            {
                lbl.Image = ımageList1.Images[6]; // yoksa ilerlemenin en başına konulan resmi goster
            }
        }
        private void cevredekiMayin()//cevredeki mayınları gosteren fonksiyon
        {
            int mayinSayac = 0;//kaç tane mayın var ? başlangıç değeri sıfır
            foreach (Label lbl2 in flowLayoutPanel1.Controls)//labelleri gez
            {
                if ((int.Parse(lbl2.Name) == (int.Parse(seciliLabel) - 1)))//solundaki labelde
                {
                    if ((bool)lbl2.Tag == true)//mayın varsa
                    {
                        mayinSayac++;
                        int durum = solSinir();
                        //eğer label sol sınırda ise bir önceki eleman bir üst satırın en sonuna denk geldiğinden orada mayın varsa dikkate alamayacağız
                        if (durum == 1)
                        {
                            mayinSayac--;
                        }
                    }
                }
                else if ((int.Parse(lbl2.Name) == (int.Parse(seciliLabel) + 1)))//sağındaki labellde
                {
                    if ((bool)lbl2.Tag == true)//mayın varsa
                    {
                        mayinSayac++;
                        int durum = sagSinir();//aynı koşul sağ sınır için de
                        if (durum == 1)
                        {
                            mayinSayac--;
                        }
                    }
                }
                else if ((int.Parse(lbl2.Name) == (int.Parse(seciliLabel) + 20)))//altındaki labelde
                {
                    if ((bool)lbl2.Tag == true)//mayın varsa
                    {
                        mayinSayac++;
                    }
                }
                else if ((int.Parse(lbl2.Name) == (int.Parse(seciliLabel) - 20)))//üstündeki labelde
                {
                    if ((bool)lbl2.Tag == true)//mayın varsa
                    {
                        mayinSayac++;
                    }
                }

            }

            label3.Text = mayinSayac.ToString();
        }

        
        private int solSinir()
        {
            int[] sinir = new int[20];
            int sinirSayac = 0;
            for (int i = 0; i <= 380; i += 20) //sol sınırı diziye yaz
            {
                sinir[sinirSayac] = i;
                sinirSayac++;
            }

            foreach (var ara in sinir)
            {
                if (seciliLabel == ara.ToString()) //eğer seçili label sol sınırdaysa
                {
                    return 1;
                }
            }
            return 0;
        }
        private int sagSinir()
        {
            int[] sinir = new int[20];
            int sinirSayac = 0;
            for (int i = 19; i <= 399; i += 20) //sağ sınırı diziye yazdır
            {
                sinir[sinirSayac] = i;
                sinirSayac++;
            }
            foreach (var ara in sinir) //eğer seçili label sağ sınırdaysa
            {
                if (seciliLabel == ara.ToString())
                {
                    return 1;
                }
            }

            return 0;
        }

        private void timer1_Tick_1(object sender, EventArgs e)
        {
            suanKi = DateTime.Now;//bitiş zamanını al
            TimeSpan span = suanKi.Subtract(ilkTarih);//farkı bul
            label6.Text = span.Minutes.ToString("00") + ":" + span.Seconds.ToString("00");//zaman lbl a yazdır
        }
        private void gerideKalaniBoya()
        {
            foreach (Label lbl3 in flowLayoutPanel1.Controls)
            {
                if (lbl3.Image != null && (lbl3.Name != seciliLabel))//eğer geçitğimiz labelse ve bu buton şuanki
                {                                                      //label değilse
                    lbl3.Image = ımageList1.Images[7];//beyaz yap
                }

            }

        }
        private void oyunBasarisiz()//oyun başarızlık bilgilendirme fonksiyonu
        {
            if (MessageBox.Show("Oyunu Kaybettiniz.Yeni Oyuna Başlamak İstiyormusunuz ?", "Bilgilendirme", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information)
                == DialogResult.Yes)
            {
                Application.Restart();

            }
            else//olumsuzssa çık
            {
                Application.Exit();
            }
        }
        private void koseKontrol()//oyun galibiyet kontrolü
        {
            if (seciliLabelinNoSu == 0)//en üst sol köşeye geldiyse
            {
                enSolaGeldiMi = true;//geldi
                if (enSolaGeldiMi && enSagaGeldiMi)//eğer iki köşeye de geldiyse
                {
                    bilgilendirme();//galibiyet bilgilendir
                }
            }
            else if (seciliLabelinNoSu == 19)//en üst sağ köşeye geldiyse
            {
                enSagaGeldiMi = true;//geldi
                if (enSolaGeldiMi && enSagaGeldiMi)//eğer iki köşeye de geldiyse
                {
                    bilgilendirme();//galibiyet bilgilendir
                }
            }
            else if (((bool)sonBas[0].Tag == true) || ((bool)sonBas[1].Tag == true))//eğer iki köşede de mayın varsa
            {
                int[] sinir = new int[18];
                int sinirsayac = 0;
                for (int i = 1; i <= 18; i++)//en üst satırı köşeler hariç komple diziye yaz
                {
                    sinir[sinirsayac] = i;
                    sinirsayac++;
                }

                for (int i = 0; i < sinir.Length; i++)
                {
                    if (sinir[i] == int.Parse(seciliLabel)) //en üst satırdan herhangi bir labele geldiyse
                    {
                        bilgilendirme();//galibiyet bilgilendir
                    }
                }
            }


        }

       
        private void bilgilendirme()
        {

            timer1.Stop();//süreyi durdur
            if (MessageBox.Show("Oyunu Kazandınız.Yeni Oyuna Başlamak İstiyormusunuz ?", "Bilgilendirme", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information)
                == DialogResult.Yes)
            {
                Application.Restart();
            }
            else
            {
                Application.Exit();//oyundan çık
            }

        }

        private void yonVer(object sender, EventArgs e)
        {
            Button yonButon = (Button)sender; // gelen bilgi butondur
            string butonAdi = yonButon.Name;//butonun adını alarak hangisi oldugunu buluyorum

            switch (butonAdi)//yon tusunun adına gore islem yap
            {
                case "yukariButon"://yukarıya çıkma durumu için

                    foreach (Label lbl3 in flowLayoutPanel1.Controls)//labelleri gez
                    {
                        if (seciliLabelinNoSu < 20)//eğer label en ust sırada ise bu sıradan yukarı cıkma durumunda
                        {
                            MessageBox.Show("ÜST SINIRI GEÇEMESSİN !");
                            break;//foreachten çık
                        }
                        if ((int.Parse(lbl3.Name) == (int.Parse(seciliLabel) - 20)))//boyutumuz  20*20 oldugundan
                        {                                              //hemen ustu 20 eleman eksiğine denk geliyor
                            label_click(lbl3,EventArgs.Empty);
                            seciliLabel = lbl3.Name;//secili label artık yeni labelimiz
                            seciliLabelinNoSu = int.Parse(seciliLabel);//yeni tıklanan labelin no su
                            gerideKalaniBoya();//geride kalan "geçtiğimiz" yerleri boya
                            koseKontrol();//galibiyet durumu kontrol et
                            break;//foreachten çık
                        }
                    }
                    break;//yukarı durumundan çık
                case "asagiButon"://asagi durumu
                    foreach (Label lbl3 in flowLayoutPanel1.Controls)//labelleri gez
                    {
                        if (seciliLabelinNoSu >= 380)//eğer label en alt sıradaysa
                        {
                            MessageBox.Show("ALT SINIRI GEÇEMESSİN!");
                            break;//foreachten çık
                        }
                        if ((int.Parse(lbl3.Name) == (int.Parse(seciliLabel) + 20)))//boyut 20*20 oldugundan alt tarafı
                        {                                                   //20 eleman sonrasına denk geliyor

                            label_click(lbl3, EventArgs.Empty);//labele tıkla(basarısızlık durumu butona tıklama fonksiyonunda kontrol ediliyor)
                            seciliLabel = lbl3.Name;//secili label artık yeni butonumuz
                            seciliLabelinNoSu = int.Parse(seciliLabel);//yeni butonun buton no su
                            gerideKalaniBoya();//geride kalan "geçtiğimiz" yerleri boya
                            koseKontrol();//galibiyet kontrolü
                            break;//foreachten çık
                        }
                    }
                    break;//asagi durumundan çık
                case "sagaButon"://sağa gitme durumu
                    foreach (Label lbl3 in flowLayoutPanel1.Controls)//labelleri gez
                    {
                        if (seciliLabelinNoSu == 399)//en sondaki labelse
                        {
                            MessageBox.Show("SAĞ SINIRI GEÇEMESSİN!");
                            break;//foreachten çık
                        }
                        if ((int.Parse(lbl3.Name) == (int.Parse(seciliLabel) + 1)))//sağ tarafı bir sonraki elemandır
                        {
                            int durum = sagSinir();//bir önceki elemanın nerede oldğunu kontrol et                           
                            if (durum == 1)//eğer bir önceki eleman sağ sınırdaysa
                            {
                                MessageBox.Show("SAĞ SINIRI GEÇEMESSİN!");
                                break;//foreachten çık
                            }
                            label_click(lbl3, EventArgs.Empty);//sıkıntı yoksa labele bas
                            seciliLabel = lbl3.Name;//secili label yeni labelimiz
                            seciliLabelinNoSu = int.Parse(seciliLabel);// '' '' '' no su dur
                            gerideKalaniBoya();//geride kalan "geçtiğimiz" yerleri boya
                            koseKontrol();//galibiyet kontrolü
                            break;//foreachten çık
                        }
                    }
                    break;//saga durumundan cık
                case "solaButon"://sola gitme durumu
                    foreach (Label lbl3 in flowLayoutPanel1.Controls)//labelleri gez
                    {
                        if (seciliLabelinNoSu == 0)//eger label en baştaki label ise
                        {
                            MessageBox.Show("SOL SINIRI GEÇEMESSİN!");
                            break;//foreachten çık
                        }
                        if ((int.Parse(lbl3.Name) == (int.Parse(seciliLabel) - 1)))//solundaki eleman 1 eksiğidir
                        {
                            int durum = solSinir();//bir önceki elemanın nerede olduğunu kontrol et
                            if (durum == 1)//eğer bir önceki eleman sol sınırındaysa
                            {
                                MessageBox.Show("SOL SINIRI GEÇEMESSİN!");
                                break;//foreachten çık
                            }
                            label_click(lbl3, EventArgs.Empty);//yeni labele tıkla
                            seciliLabel = lbl3.Name;//seçili label artık yeni butonumuz
                            seciliLabelinNoSu = int.Parse(seciliLabel); // '''''''' no su
                            gerideKalaniBoya();//geride kalan "geçtiğimiz" yerleri boya
                            koseKontrol();//galibiyet kontrolü
                            break;//foreachten çık
                        }
                    }
                    break;//sola gitme durumundan çık
            }

            cevredekiMayin();//yeni labelimizin çevresindeki mayınları goster

        }
     
    }
}
