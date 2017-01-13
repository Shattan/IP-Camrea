#region Biblioteki Systemu
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Drawing.Drawing2D;
#endregion

#region Biblioteki Emgu
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;

using DirectShowLib;
#endregion

namespace KamerowaGra
{
    public partial class EkranGlowny : Form
    {
        #region Zmienne globalne

        #region Tworzenie strumienia kamery
        //Znalezienie kamer w systemie przy użyciu DirectShow.Net dll 
        DsDevice[] Kamery = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
        Capture strumien = new Capture();
        int FPS = 0;
        #endregion

        #region Przełączniki
        bool przechwytywanieAktywne = false;
        bool resetuj = false;
        bool punktDodany = false;
        #endregion

        #region Tworzenie graczy, piłki i długopisów
        Bitmap obrazekPilki = null;
        Bitmap obrazekGracza1 = null;
        Bitmap obrazekGracza2 = null;
        Gracz gracz1 = new Gracz();
        Gracz gracz2 = new Gracz();
        Gracz graczAutomatyczny = new Gracz();
        Pilka pilka = new Pilka();
        Dlugopis dlugopis = new Dlugopis();
        #endregion

        #endregion

        #region Klasy

        #region Utworzenie klasy Piłka i jej parametrów
        public class Pilka
        {
            public int wielkosc;
            public int odbicia;
            public float obrot;
            public float silaGrawitacji;
            public PointF pozycja;
            public PointF predkoscPrzemieszczania;
            public PointF predkoscPrzyspieszania;

            public Pilka()
            {
                wielkosc = 0;
                obrot = 0.0f;
                odbicia = 0;
                pozycja = new PointF(0.0f,0.0f);
                predkoscPrzemieszczania = new PointF(0.0f, 0.0f);
                predkoscPrzyspieszania = new PointF(0.0f, 0.0f);
                silaGrawitacji = 0.0f;
            }
        }
        #endregion

        #region Utworzenie klasy Gracz, w której będą zapisane ustawienia wykrywania dla poszczególnych graczy
        public class Gracz
        {
            //Dane gracza
            public string nazwa;
            public int punkty;
            public int wielkosc;
            public PointF polozenie;
            public PointF poprzedniePolozenie;
            public Rectangle obszarOdbijania;
            //Parametry filtra
            public Gray hmin;    //Hue - Minimalna barwa/odcień
            public Gray hmax;    //Hue - Maksymalna barwa/odcień
            public Gray smin;    //Saturation - Minimalne nasycenie
            public Gray smax;    //Saturation - Maksymalne nasycenie
            public Gray vmin;    //Value - Minimalna luminacja/jasność/jaskrawość
            public Gray vmax;    //Value - Maksymalna luminacja/jasność/jaskrawość
            public int minimalnaTolerancja;
            public int maksymalnaTolerancja;

            public Gracz()
            {
                nazwa = "";
                punkty = 0;
                wielkosc = 0;
                polozenie = new PointF(0.0f, 0.0f);
                poprzedniePolozenie = new PointF(0.0f, 0.0f);
                obszarOdbijania = new Rectangle(0, 0, 0, 0);
                hmin = new Gray(0);
                hmax = new Gray(256);
                smin = new Gray(0);
                smax = new Gray(256);
                vmin = new Gray(0);
                vmax = new Gray(256);
                minimalnaTolerancja = 1;
                maksymalnaTolerancja = 1;
            }

            public void KopiujKolory(Gracz gracz)
            {
                hmin = gracz.hmin;
                hmax = gracz.hmax;
                smin = gracz.smin;
                smax = gracz.smax;
                vmin = gracz.vmin;
                vmax = gracz.vmax;
                minimalnaTolerancja = gracz.minimalnaTolerancja;
                maksymalnaTolerancja = gracz.maksymalnaTolerancja;
            }

            public void UstawKolor(Gray H, Gray S, Gray V)
            {
                hmin.Intensity = H.Intensity - minimalnaTolerancja;
                if (hmin.Intensity < 0) hmin.Intensity = 0;
                if (hmin.Intensity > 255) hmin.Intensity = 255;
                hmax.Intensity = H.Intensity + maksymalnaTolerancja;
                if (hmax.Intensity < 0) hmax.Intensity = 0;
                if (hmax.Intensity > 256) hmax.Intensity = 256;

                smin.Intensity = S.Intensity - minimalnaTolerancja;
                if (smin.Intensity < 0) smin.Intensity = 0;
                if (smin.Intensity > 255) smin.Intensity = 255;
                smax.Intensity = S.Intensity + maksymalnaTolerancja;
                if (smax.Intensity < 0) smax.Intensity = 0;
                if (smax.Intensity > 256) smax.Intensity = 256;

                vmin.Intensity = V.Intensity - minimalnaTolerancja;
                if (vmin.Intensity < 0) vmin.Intensity = 0;
                if (vmin.Intensity > 255) vmin.Intensity = 255;
                vmax.Intensity = V.Intensity + maksymalnaTolerancja;
                if (vmax.Intensity < 0) vmax.Intensity = 0;
                if (vmax.Intensity > 256) vmax.Intensity = 256;
            }

            public Rectangle ObszarOdbijania()
            {
                int wysokosc = wielkosc * 4 / 5;
                int szerokosc = wielkosc * 3 / 5;
                obszarOdbijania = new Rectangle((int)polozenie.X - szerokosc / 2, (int)polozenie.Y - wysokosc/2, szerokosc, wysokosc);
                return obszarOdbijania;
            }
        }
        #endregion

        #region Utworzenie klasy Dlugopis zawierajacej rozne kolory narzedzia do rysowania lini

        public class Dlugopis
        {
            public List<Pen> czerwony;
            public List<Pen> niebieski;
            public List<Pen> zielony;
            public List<Pen> zolty;
            public List<Pen> czarny;
            public List<Pen> bialy;
            public List<Pen> szary;
            public List<Pen> pomaranczowy;
            public List<Pen> brazowy;
            public List<Pen> rozowy;
            public List<Pen> fioletowy;
            public List<Pen> dodany;

            public Dlugopis()
            {
                czerwony = new List<Pen> 
                { 
                    new Pen(Color.Red, 0), 
                    new Pen(Color.Red, 1), 
                    new Pen(Color.Red, 2), 
                    new Pen(Color.Red, 3), 
                    new Pen(Color.Red, 4),
                    new Pen(Color.Red, 5), 
                    new Pen(Color.Red, 6), 
                    new Pen(Color.Red, 7), 
                    new Pen(Color.Red, 8), 
                    new Pen(Color.Red, 9), 
                    new Pen(Color.Red, 10), 
                    new Pen(Color.Red, 11), 
                    new Pen(Color.Red, 12), 
                    new Pen(Color.Red, 13), 
                    new Pen(Color.Red, 14) 
                };
                niebieski = new List<Pen> 
                { 
                    new Pen(Color.Blue, 0), 
                    new Pen(Color.Blue, 1), 
                    new Pen(Color.Blue, 2), 
                    new Pen(Color.Blue, 3), 
                    new Pen(Color.Blue, 4),
                    new Pen(Color.Blue, 5), 
                    new Pen(Color.Blue, 6), 
                    new Pen(Color.Blue, 7), 
                    new Pen(Color.Blue, 8), 
                    new Pen(Color.Blue, 9), 
                    new Pen(Color.Blue, 10), 
                    new Pen(Color.Blue, 11), 
                    new Pen(Color.Blue, 12), 
                    new Pen(Color.Blue, 13), 
                    new Pen(Color.Blue, 14) 
                };
                zielony = new List<Pen> 
                { 
                    new Pen(Color.Green, 0), 
                    new Pen(Color.Green, 1), 
                    new Pen(Color.Green, 2), 
                    new Pen(Color.Green, 3), 
                    new Pen(Color.Green, 4),
                    new Pen(Color.Green, 5), 
                    new Pen(Color.Green, 6), 
                    new Pen(Color.Green, 7), 
                    new Pen(Color.Green, 8), 
                    new Pen(Color.Green, 9), 
                    new Pen(Color.Green, 10), 
                    new Pen(Color.Green, 11), 
                    new Pen(Color.Green, 12), 
                    new Pen(Color.Green, 13), 
                    new Pen(Color.Green, 14) 
                };
                zolty = new List<Pen> 
                { 
                    new Pen(Color.Yellow, 0), 
                    new Pen(Color.Yellow, 1), 
                    new Pen(Color.Yellow, 2), 
                    new Pen(Color.Yellow, 3), 
                    new Pen(Color.Yellow, 4),
                    new Pen(Color.Yellow, 5), 
                    new Pen(Color.Yellow, 6), 
                    new Pen(Color.Yellow, 7), 
                    new Pen(Color.Yellow, 8), 
                    new Pen(Color.Yellow, 9), 
                    new Pen(Color.Yellow, 10), 
                    new Pen(Color.Yellow, 11), 
                    new Pen(Color.Yellow, 12), 
                    new Pen(Color.Yellow, 13), 
                    new Pen(Color.Yellow, 14) 
                };
                czarny = new List<Pen> 
                { 
                    new Pen(Color.Black, 0), 
                    new Pen(Color.Black, 1), 
                    new Pen(Color.Black, 2), 
                    new Pen(Color.Black, 3), 
                    new Pen(Color.Black, 4),
                    new Pen(Color.Black, 5), 
                    new Pen(Color.Black, 6), 
                    new Pen(Color.Black, 7), 
                    new Pen(Color.Black, 8), 
                    new Pen(Color.Black, 9), 
                    new Pen(Color.Black, 10), 
                    new Pen(Color.Black, 11), 
                    new Pen(Color.Black, 12), 
                    new Pen(Color.Black, 13), 
                    new Pen(Color.Black, 14) 
                };
                bialy = new List<Pen> 
                { 
                    new Pen(Color.White, 0), 
                    new Pen(Color.White, 1), 
                    new Pen(Color.White, 2), 
                    new Pen(Color.White, 3), 
                    new Pen(Color.White, 4),
                    new Pen(Color.White, 5), 
                    new Pen(Color.White, 6), 
                    new Pen(Color.White, 7), 
                    new Pen(Color.White, 8), 
                    new Pen(Color.White, 9), 
                    new Pen(Color.White, 10), 
                    new Pen(Color.White, 11), 
                    new Pen(Color.White, 12), 
                    new Pen(Color.White, 13), 
                    new Pen(Color.White, 14) 
                };
                szary = new List<Pen> 
                { 
                    new Pen(Color.Gray, 0), 
                    new Pen(Color.Gray, 1), 
                    new Pen(Color.Gray, 2), 
                    new Pen(Color.Gray, 3), 
                    new Pen(Color.Gray, 4),
                    new Pen(Color.Gray, 5), 
                    new Pen(Color.Gray, 6), 
                    new Pen(Color.Gray, 7), 
                    new Pen(Color.Gray, 8), 
                    new Pen(Color.Gray, 9), 
                    new Pen(Color.Gray, 10), 
                    new Pen(Color.Gray, 11), 
                    new Pen(Color.Gray, 12), 
                    new Pen(Color.Gray, 13), 
                    new Pen(Color.Gray, 14) 
                };
                pomaranczowy = new List<Pen> 
                { 
                    new Pen(Color.Orange, 0), 
                    new Pen(Color.Orange, 1), 
                    new Pen(Color.Orange, 2), 
                    new Pen(Color.Orange, 3), 
                    new Pen(Color.Orange, 4),
                    new Pen(Color.Orange, 5), 
                    new Pen(Color.Orange, 6), 
                    new Pen(Color.Orange, 7), 
                    new Pen(Color.Orange, 8), 
                    new Pen(Color.Orange, 9), 
                    new Pen(Color.Orange, 10), 
                    new Pen(Color.Orange, 11), 
                    new Pen(Color.Orange, 12), 
                    new Pen(Color.Orange, 13), 
                    new Pen(Color.Orange, 14) 
                };
                brazowy = new List<Pen> 
                { 
                    new Pen(Color.Brown, 0), 
                    new Pen(Color.Brown, 1), 
                    new Pen(Color.Brown, 2), 
                    new Pen(Color.Brown, 3), 
                    new Pen(Color.Brown, 4),
                    new Pen(Color.Brown, 5), 
                    new Pen(Color.Brown, 6), 
                    new Pen(Color.Brown, 7), 
                    new Pen(Color.Brown, 8), 
                    new Pen(Color.Brown, 9), 
                    new Pen(Color.Brown, 10), 
                    new Pen(Color.Brown, 11), 
                    new Pen(Color.Brown, 12), 
                    new Pen(Color.Brown, 13), 
                    new Pen(Color.Brown, 14) 
                };
                rozowy = new List<Pen> 
                { 
                    new Pen(Color.Pink, 0), 
                    new Pen(Color.Pink, 1), 
                    new Pen(Color.Pink, 2), 
                    new Pen(Color.Pink, 3), 
                    new Pen(Color.Pink, 4),
                    new Pen(Color.Pink, 5), 
                    new Pen(Color.Pink, 6), 
                    new Pen(Color.Pink, 7), 
                    new Pen(Color.Pink, 8), 
                    new Pen(Color.Pink, 9), 
                    new Pen(Color.Pink, 10), 
                    new Pen(Color.Pink, 11), 
                    new Pen(Color.Pink, 12), 
                    new Pen(Color.Pink, 13), 
                    new Pen(Color.Pink, 14) 
                };
                fioletowy = new List<Pen> 
                { 
                    new Pen(Color.Violet, 0), 
                    new Pen(Color.Violet, 1), 
                    new Pen(Color.Violet, 2), 
                    new Pen(Color.Violet, 3), 
                    new Pen(Color.Violet, 4),
                    new Pen(Color.Violet, 5), 
                    new Pen(Color.Violet, 6), 
                    new Pen(Color.Violet, 7), 
                    new Pen(Color.Violet, 8), 
                    new Pen(Color.Violet, 9), 
                    new Pen(Color.Violet, 10), 
                    new Pen(Color.Violet, 11), 
                    new Pen(Color.Violet, 12), 
                    new Pen(Color.Violet, 13), 
                    new Pen(Color.Violet, 14) 
                };
                dodany = new List<Pen>();
            }

            public void dodajDlugopis(Color kolor, int grubosc)
            {
                dodany.Add(new Pen(kolor, grubosc));
            }
        }
        #endregion

        #endregion

        #region Funkcje Przetwarzające

        #region Funkcja ustawiania początkowych wartości okna
        void UstawOkienka()
        {
            Point punkt = new Point(0,0); //Punkt do ustawiania lokacji elementów

            punkt.X = Screen.PrimaryScreen.Bounds.Width - PrzyciskZakonczProgram.Width;
            punkt.Y = 0;
            PrzyciskZakonczProgram.Location = punkt;

            punkt.X = punkt.X - PrzyciskUstawienia.Width - 5;
            punkt.Y = 0;
            PrzyciskUstawienia.Location = punkt;

            punkt.X = punkt.X - PrzyciskRozpocznij.Width - 5;
            punkt.Y = 0;
            PrzyciskRozpocznij.Location = punkt;

            punkt.X = punkt.X;
            punkt.Y = punkt.Y + ZaznaczanieAutomatyczneRozpoczecie.Height + 5;
            ZaznaczanieAutomatyczneRozpoczecie.Location = punkt;

            punkt.X = 0;
            punkt.Y = 0;
            PanelUstawien.Location = punkt;
            PanelUstawien.Visible = false;

            punkt.X = Screen.PrimaryScreen.Bounds.Width/2 - EtykietkaOdliczaniaDoRozpoczecia.Width/2;
            punkt.Y = 0;
            EtykietkaOdliczaniaDoRozpoczecia.Location = punkt;
            EtykietkaOdliczaniaDoRozpoczecia.Visible = false;

            punkt.X = 0;
            punkt.Y = Screen.PrimaryScreen.Bounds.Height - EtykietkaPunktowLewegoGracza.Height;
            EtykietkaPunktowLewegoGracza.Location = punkt;

            punkt.X = Screen.PrimaryScreen.Bounds.Width - EtykietkaPunktowPrawegoGracza.Width;
            punkt.Y = Screen.PrimaryScreen.Bounds.Height - EtykietkaPunktowPrawegoGracza.Height;
            EtykietkaPunktowPrawegoGracza.Location = punkt;

            punkt.X = Screen.PrimaryScreen.Bounds.Width/2 - EtykietkaPunktowPrawegoGracza.Width/2;
            punkt.Y = 0;
            EtykietkaLiczbyOdbicPilki.Location = punkt;

            float proporcja = (float)Screen.PrimaryScreen.Bounds.Height / (float)strumien.QueryFrame().Height;
            EtykietkaLewegoKomunikatuOBledzie.Width = (Screen.PrimaryScreen.Bounds.Width - (int)((float)strumien.QueryFrame().Width * proporcja)) / 2;
            EtykietkaLewegoKomunikatuOBledzie.Height = Screen.PrimaryScreen.Bounds.Height/2;
            punkt.X = 0;
            punkt.Y = EtykietkaPunktowLewegoGracza.Location.Y - EtykietkaLewegoKomunikatuOBledzie.Height;
            EtykietkaLewegoKomunikatuOBledzie.Location = punkt;
            EtykietkaLewegoKomunikatuOBledzie.Visible = false;

            EtykietkaPrawegoKomunikatuOBledzie.Width = (Screen.PrimaryScreen.Bounds.Width - (int)((float)strumien.QueryFrame().Width * proporcja)) / 2;
            EtykietkaPrawegoKomunikatuOBledzie.Height = Screen.PrimaryScreen.Bounds.Height / 2;
            punkt.X = Screen.PrimaryScreen.Bounds.Width - EtykietkaPrawegoKomunikatuOBledzie.Width;
            punkt.Y = EtykietkaPunktowPrawegoGracza.Location.Y - EtykietkaPrawegoKomunikatuOBledzie.Height;
            EtykietkaPrawegoKomunikatuOBledzie.Location = punkt;
            EtykietkaPrawegoKomunikatuOBledzie.Visible = false;

            EtykietkaFPS.Location = new Point(Location.X, Location.Y);
            SzukajKamer();
        }
        #endregion

        #region Funkcja Wyszukująca zainstalowanych kamer
        void SzukajKamer()
        {
            //Utworzenie listy do przechowania kamer dla ComboBox'a
            List<KeyValuePair<int, string>> ListaWlasciwosciKamer = new List<KeyValuePair<int, string>>();


            if (Kamery != null)
            {
                int IndeksWybranejKamery = 0;
                foreach (DirectShowLib.DsDevice Kamera in Kamery)
                {
                    ListaWlasciwosciKamer.Add(new KeyValuePair<int, string>(IndeksWybranejKamery, Kamera.Name));
                    IndeksWybranejKamery++;
                }

                //Wyczyszczenie listy kamer
                MenuRozwijalneKamer.DataSource = null;
                MenuRozwijalneKamer.Items.Clear();

                //Przypisanie nowych wartości do listy
                MenuRozwijalneKamer.DataSource = new BindingSource(ListaWlasciwosciKamer, null);
                MenuRozwijalneKamer.DisplayMember = "Value";
                MenuRozwijalneKamer.ValueMember = "Key";
                strumien = new Capture(MenuRozwijalneKamer.SelectedIndex);
            }
            else
            {
                MenuRozwijalneKamer.Text = "Nie wykryto zainstalowanych kamer";
            }
        }
        #endregion

        #region Funkcja ustawiania początkowych wartości dla piłek i graczy
        void UstawParametry(Bitmap obraz)
        {
            obrazekPilki = new Bitmap("Obrazki/Pilka1.png");
            pilka.odbicia = 0;
            pilka.wielkosc = obraz.Height / 15;
            pilka.obrot = 0.0f;
            pilka.pozycja = new PointF((float)obraz.Width / 2.0f, (float)obraz.Height / 2.0f);
            pilka.predkoscPrzemieszczania = new PointF(20.0f, -10.0f);
            pilka.predkoscPrzyspieszania = new PointF(1.0f + (5.0f / (float)obraz.Width), 1.0f + (0.0f / (float)obraz.Height));
            pilka.silaGrawitacji = 4.0f;

            obrazekGracza1 = new Bitmap("Obrazki/Gracz1.png");
            obrazekGracza2 = new Bitmap("Obrazki/Gracz2.png");
            gracz1.punkty = gracz2.punkty = 0;
            gracz1.wielkosc = gracz2.wielkosc = obraz.Height / 4;
            gracz1.polozenie = gracz2.polozenie = new PointF(-300.0f, -300.0f);
        }
        #endregion

        #region Funkcje aktualizująca pozycje suwaków
        void OdswiezSuwakiHSV(Gracz gracz)
        {
            EtykietkaWartosciHue.Text = ((int)gracz.hmin.Intensity).ToString() + " - " + ((int)gracz.hmax.Intensity).ToString();
            SuwakHueMinimum.Value = (int)gracz.hmin.Intensity;
            SuwakHueMaksimum.Value = (int)gracz.hmax.Intensity;
            EtykietkaWartosciSaturation.Text = ((int)gracz.smin.Intensity).ToString() + " - " + ((int)gracz.smax.Intensity).ToString();
            SuwakSaturationMinimum.Value = (int)gracz.smin.Intensity;
            SuwakSaturationMaksimum.Value = (int)gracz.smax.Intensity;
            EtykietkaWartosciValue.Text = ((int)gracz.vmin.Intensity).ToString() + " - " + ((int)gracz.vmax.Intensity).ToString();
            SuwakValueMinimum.Value = (int)gracz.vmin.Intensity;
            SuwakValueMaksimum.Value = (int)gracz.vmax.Intensity;
        }

        void OdswiezSuwakiTolerancji(Gracz gracz)
        {
            EtykietkaWartosciTolerancji.Text = gracz.minimalnaTolerancja.ToString() + " - " + gracz.maksymalnaTolerancja.ToString();
            SuwakTolerancjiMinimum.Value = gracz.minimalnaTolerancja;
            SuwakTolerancjiMaksimum.Value = gracz.maksymalnaTolerancja;
        }
        #endregion

        #region Funkcje Serializujące i deserializujące
        void Serializuj()
        {
            try
            {
                FileStream fs = new FileStream("./Gracz 1.xml", FileMode.Create);
                XmlSerializer serializer = new XmlSerializer(typeof(Gracz));
                serializer.Serialize(fs, gracz1);
                fs.Close();

                fs = new FileStream("./Gracz 2.xml", FileMode.Create);
                serializer = new XmlSerializer(typeof(Gracz));
                serializer.Serialize(fs, gracz2);
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd: " + ex.Message, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        void Deserializuj()
        {
            try
            {
                FileStream fs = new FileStream("./Gracz 1.xml", FileMode.Open);
                XmlSerializer serializer = new XmlSerializer(typeof(Gracz));
                gracz1 = (Gracz)serializer.Deserialize(fs);
                fs.Close();

                fs = new FileStream("./Gracz 2.xml", FileMode.Open);
                serializer = new XmlSerializer(typeof(Gracz));
                gracz2 = (Gracz)serializer.Deserialize(fs);
                fs.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Błąd: " + ex.Message, "Błąd!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Funkcja poszukująca średniej koloru na obszarze
        /*
        private void sredniKolor(ref Gracz gracz, Bitmap obrazek, Rectangle obszar)
        {
            Point lewygora = new Point(obszar.X, obszar.Y);
            Point prawydol = new Point(obszar.X + obszar.Width, obszar.Y + obszar.Height);
            Bitmap kadrowanyObrazek = kadruj(obrazek, lewygora, prawydol);
            Bitmap usrednionaMiniatura = new Bitmap(1, 1);
            using (Graphics nowaGrafika = Graphics.FromImage(usrednionaMiniatura))
            {
                nowaGrafika.InterpolationMode = InterpolationMode.HighQualityBicubic;
                nowaGrafika.DrawImage(kadrowanyObrazek, new Rectangle(0, 0, 1, 1));
            }
            Color pixel = usrednionaMiniatura.GetPixel(0, 0);

            Image <Bgr,byte>img = new Image<Bgr,byte>(usrednionaMiniatura);
            Image<Hsv, Byte> hsvimg = img.Convert<Hsv, Byte>();           
            Image<Gray, Byte>[] channels = hsvimg.Split();  
            gracz.UstawKolor(channels);
        }*/

        private void SredniKolor(ref Gracz gracz, Image<Bgr,byte> obrazek, Rectangle obszar)
        {
            obrazek = Kadruj(obrazek, obszar);
            Image<Gray, Byte>[] channels = obrazek.Split();
            Gray H = channels[0].GetAverage();
            Gray S = channels[1].GetAverage();
            Gray V = channels[2].GetAverage();
            gracz.UstawKolor(H,S,V);
        }
        #endregion

        #region Funkcje kadrujące
        private Bitmap Kadruj(Bitmap obrazek, Point LewyGora, Point PrawyDol)
        {
            Bitmap obrazekKadrowany = new Bitmap(obrazek);
            obrazekKadrowany = obrazekKadrowany.Clone(new Rectangle(LewyGora.X, LewyGora.Y, PrawyDol.X - LewyGora.X, PrawyDol.Y - LewyGora.Y), System.Drawing.Imaging.PixelFormat.DontCare);
            return obrazekKadrowany;
        }


        private Image<Bgr, byte> Kadruj(Image<Bgr, byte> obrazek, Rectangle obszar)
        {
            Point LewyGora = new Point(obszar.X, obszar.Y);
            Point PrawyDol = new Point(obszar.X + obszar.Width, obszar.Y + obszar.Height);
            Bitmap obrazekKadrowany = new Bitmap(obrazek.ToBitmap());
            obrazekKadrowany = obrazekKadrowany.Clone(new Rectangle(LewyGora.X, LewyGora.Y, PrawyDol.X - LewyGora.X, PrawyDol.Y - LewyGora.Y), System.Drawing.Imaging.PixelFormat.DontCare);
            obrazek = new Image<Bgr, byte>(obrazekKadrowany);
            return obrazek;
        }

        private void Gra()
        {
            #region Utworzenie i ustawienia przechwytywanego obrazu
            strumien.FlipHorizontal = true;
            Image<Bgr, byte> przechwyconaKlatka = strumien.QueryFrame();
            #endregion

            if (przechwyconaKlatka != null && przechwytywanieAktywne == true)
            {
                try
                {

                    #region Czynności wykonywane podczas otwartego okienka ustawień
                    if (PanelUstawien.Visible == true)
                    {
                        #region Autowykrywanie
                        if (ZaznaczenieAutomatycznegoPobieraniaWartosci.Checked == true)
                        {
                            OdswiezSuwakiHSV(graczAutomatyczny);
                            int szerokoscObszaruWykrywania = 10 + (10 * SuwakWielkosciPaletek.Value);
                            int wysokoscObszaruWykrywania = 10 + (10 * SuwakWielkosciPaletek.Value);
                            Rectangle obszarWykrywania = new Rectangle((przechwyconaKlatka.Width / 2) - (szerokoscObszaruWykrywania / 2), (przechwyconaKlatka.Height / 2) - (wysokoscObszaruWykrywania / 2), szerokoscObszaruWykrywania, wysokoscObszaruWykrywania);

                            if (graczAutomatyczny != null)
                            {
                                if (obszarWykrywania != null)
                                {
                                    SredniKolor(ref graczAutomatyczny, przechwyconaKlatka, obszarWykrywania);

                                    Image<Gray, byte> obrazFiltrowany = null;
                                    obrazFiltrowany = FiltrujHSV(przechwyconaKlatka, graczAutomatyczny);

                                    if (obrazFiltrowany != null)
                                    {
                                        Contour<Point> plamka = null;
                                        plamka = ZnajdzNajwiekszyObiekt(obrazFiltrowany);

                                        if (plamka != null)
                                        {
                                            PointF srodekObiektu = new PointF();
                                            ZnajdzSrodekObiektu(plamka, ref srodekObiektu);

                                            if (!srodekObiektu.IsEmpty)
                                            {
                                                if (WyborRzeczywistegoEkranu.Checked == true)
                                                {
                                                    //Wypisanie napisu
                                                    //Utworzenie czcionki do napisów na ekranie
                                                    //MCvFont czcionka = new MCvFont(Emgu.CV.CvEnum.FONT.CV_FONT_HERSHEY_COMPLEX, 0.5, 0.5);
                                                    //przechwyconaKlatka.Draw("TU!", ref czcionka, srodekObiektu, new Bgr(0, 255, 0));

                                                    EtykietkaPrawegoKomunikatuOBledzie.Visible = false;
                                                    przechwyconaKlatka.Draw(obszarWykrywania, new Bgr(255, 255, 255), 1);
                                                    Wyswietlacz.Image = przechwyconaKlatka.ToBitmap();
                                                }
                                                else
                                                {
                                                    EtykietkaPrawegoKomunikatuOBledzie.Visible = false;
                                                    Image<Bgr, byte> obrazBGR = obrazFiltrowany.Convert<Bgr, byte>();
                                                    obrazBGR.Draw(plamka, new Bgr(0.0, 255.0, 0.0), -1); //-1 dla zamalowania największego obiektu
                                                    obrazBGR.Draw(obszarWykrywania, new Bgr(255, 255, 0), 1);
                                                    Wyswietlacz.Image = obrazBGR.ToBitmap();
                                                }
                                            }
                                            else
                                            {
                                                EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                                EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie można było określić środka obiektu!";
                                                if (WyborRzeczywistegoEkranu.Checked == true)
                                                {
                                                    przechwyconaKlatka.Draw(obszarWykrywania, new Bgr(255, 255, 255), 1);
                                                    Wyswietlacz.Image = przechwyconaKlatka.ToBitmap();
                                                }
                                                else
                                                {
                                                    Image<Bgr, byte> obrazBGR = obrazFiltrowany.Convert<Bgr, byte>();
                                                    obrazBGR.Draw(obszarWykrywania, new Bgr(255, 255, 0), 1);
                                                    Wyswietlacz.Image = obrazBGR.ToBitmap();
                                                }
                                            }
                                        }
                                        else
                                        {
                                            EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                            EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie znaleziono obiektu!";
                                            if (WyborRzeczywistegoEkranu.Checked == true)
                                            {
                                                przechwyconaKlatka.Draw(obszarWykrywania, new Bgr(255, 255, 255), 1);
                                                Wyswietlacz.Image = przechwyconaKlatka.ToBitmap();
                                            }
                                            else
                                            {
                                                Image<Bgr, byte> obrazBGR = obrazFiltrowany.Convert<Bgr, byte>();
                                                obrazBGR.Draw(obszarWykrywania, new Bgr(255, 255, 0), 1);
                                                Wyswietlacz.Image = obrazBGR.ToBitmap();
                                            }
                                        }
                                    }
                                    else
                                    {
                                        EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                        EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie udało się filtrowanie obrazu!";
                                    }
                                }
                                else
                                {
                                    EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                    EtykietkaPrawegoKomunikatuOBledzie.Text = "Błąd obszaru wykrywania!";
                                }
                            }
                            else
                            {
                                EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie udało się utworzyć automatycznego gracza!";
                            }
                        }
                        #endregion

                        #region Brak autowykrywania
                        else
                        {
                            Image<Gray, byte> obrazFiltrowany = null;
                            if (WyborGracza1.Checked == true)
                            {
                                if (gracz1 != null)
                                {
                                    obrazFiltrowany = FiltrujHSV(przechwyconaKlatka, gracz1);
                                }
                                else
                                {
                                    EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                    EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie udało się utworzyć pierwszego gracza!";
                                }
                            }
                            else
                            {
                                if (gracz2 != null)
                                {
                                    obrazFiltrowany = FiltrujHSV(przechwyconaKlatka, gracz2);
                                }
                                else
                                {
                                    EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                    EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie udało się utworzyć drugiego gracza!";
                                }
                            }
                            if (obrazFiltrowany != null)
                            {
                                Contour<Point> plamka = null;
                                plamka = ZnajdzNajwiekszyObiekt(obrazFiltrowany);

                                if (plamka != null)
                                {
                                    PointF srodekObiektu = new PointF();
                                    ZnajdzSrodekObiektu(plamka, ref srodekObiektu);

                                    if (!srodekObiektu.IsEmpty)
                                    {
                                        if (WyborRzeczywistegoEkranu.Checked == true)
                                        {
                                            EtykietkaPrawegoKomunikatuOBledzie.Visible = false;
                                            Wyswietlacz.Image = przechwyconaKlatka.ToBitmap();
                                        }
                                        else
                                        {
                                            EtykietkaPrawegoKomunikatuOBledzie.Visible = false;
                                            Image<Bgr, byte> obrazBGR = obrazFiltrowany.Convert<Bgr, byte>();
                                            obrazBGR.Draw(plamka, new Bgr(0.0, 255.0, 0.0), -1); //-1 dla zamalowania największego obiektu
                                            Wyswietlacz.Image = obrazBGR.ToBitmap();
                                        }
                                    }
                                    else
                                    {
                                        EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                        EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie można było określić środka obiektu!";
                                        if (WyborRzeczywistegoEkranu.Checked == true)
                                        {
                                            Wyswietlacz.Image = przechwyconaKlatka.ToBitmap();
                                        }
                                        else
                                        {
                                            Image<Bgr, byte> obrazBGR = obrazFiltrowany.Convert<Bgr, byte>();
                                            Wyswietlacz.Image = obrazBGR.ToBitmap();
                                        }
                                    }
                                }
                                else
                                {
                                    EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                    EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie znaleziono obiektu!";
                                    if (WyborRzeczywistegoEkranu.Checked == true)
                                    {
                                        Wyswietlacz.Image = przechwyconaKlatka.ToBitmap();
                                    }
                                    else
                                    {
                                        Image<Bgr, byte> obrazBGR = obrazFiltrowany.Convert<Bgr, byte>();
                                        Wyswietlacz.Image = obrazBGR.ToBitmap();
                                    }
                                }
                            }
                            else
                            {
                                EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie udało się filtrowanie obrazu!";
                            }

                        }
                        #endregion

                    }
                    #endregion

                    #region Czynności wykonywane podczas zwykłej gry
                    else
                    {
                        Bitmap koncowyObraz = przechwyconaKlatka.ToBitmap();
                        Graphics grafikaGraczy = Graphics.FromImage(koncowyObraz);
                        Graphics grafikaPilki = Graphics.FromImage(koncowyObraz);
                        Image<Gray, byte> obrazGracza1 = null;
                        obrazGracza1 = FiltrujHSV(przechwyconaKlatka, gracz1);
                        Image<Gray, byte> obrazGracza2 = null;
                        obrazGracza2 = FiltrujHSV(przechwyconaKlatka, gracz2);

                        #region Wyswietlanie pierwszego gracza
                        if (obrazGracza1 != null)
                        {
                            Contour<Point> plamka = null;
                            plamka = ZnajdzNajwiekszyObiekt(obrazGracza1);
                            if (plamka != null)
                            {
                                ZnajdzSrodekObiektu(plamka, ref gracz1.polozenie);

                                if (!gracz1.polozenie.IsEmpty)
                                {
                                    if (obrazekGracza1 != null)
                                    {
                                        EtykietkaLewegoKomunikatuOBledzie.Visible = false;
                                        RysujObrazek(obrazekGracza1, gracz1.polozenie, grafikaGraczy, gracz1.wielkosc, gracz1.wielkosc);
                                    }
                                    else
                                    {
                                        EtykietkaLewegoKomunikatuOBledzie.Visible = true;
                                        EtykietkaLewegoKomunikatuOBledzie.Text = "Nie znaleziono grafiki gracza pierwszego!";
                                    }
                                }
                                else
                                {
                                    EtykietkaLewegoKomunikatuOBledzie.Visible = true;
                                    EtykietkaLewegoKomunikatuOBledzie.Text = "Nie mozna okreslic pozycji pierwszego gracza!";
                                }
                            }
                            else
                            {
                                EtykietkaLewegoKomunikatuOBledzie.Visible = true;
                                RysujObrazek(obrazekGracza1, gracz1.polozenie, grafikaGraczy, gracz1.wielkosc, gracz1.wielkosc);
                                EtykietkaLewegoKomunikatuOBledzie.Text = "Nie znaleziono pierwszego gracza!";
                            }
                        }
                        else
                        {
                            EtykietkaLewegoKomunikatuOBledzie.Visible = true;
                            EtykietkaLewegoKomunikatuOBledzie.Text = "Filtrowanie obrazu pierwszego gracza nie powiodło się!";
                        }
                        #endregion

                        #region Wyswietlanie drugiego gracza
                        if (obrazGracza2 != null)
                        {
                            Contour<Point> plamka = null;
                            plamka = ZnajdzNajwiekszyObiekt(obrazGracza2);
                            if (plamka != null)
                            {
                                ZnajdzSrodekObiektu(plamka, ref gracz2.polozenie);

                                if (!gracz2.polozenie.IsEmpty)
                                {
                                    if (obrazekGracza2 != null)
                                    {
                                        EtykietkaPrawegoKomunikatuOBledzie.Visible = false;
                                        RysujObrazek(obrazekGracza2, gracz2.polozenie, grafikaGraczy, gracz2.wielkosc, gracz2.wielkosc);
                                    }
                                    else
                                    {
                                        EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                        EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie znaleziono grafiki drugiego gracza!";
                                    }
                                }
                                else
                                {
                                    EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                    EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie mozna okreslic pozycji drugiego gracza!";
                                }
                            }
                            else
                            {
                                EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                                RysujObrazek(obrazekGracza2, gracz2.polozenie, grafikaGraczy, gracz2.wielkosc, gracz2.wielkosc);
                                EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie znaleziono drugiego gracza!";
                            }
                        }
                        else
                        {
                            EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                            EtykietkaPrawegoKomunikatuOBledzie.Text = "Filtrowanie obrazu drugiego gracza nie powiodło się!";
                        }
                        #endregion

                        #region Wyswietlanie piłki
                        if (obrazekPilki != null)
                        {
                            ObliczTorLotuPilki(koncowyObraz, ref grafikaPilki);
                        }
                        else
                        {
                            grafikaGraczy.DrawString("Nie mozna wczytac pilki!", new Font(FontFamily.GenericSansSerif, 10.0f), Brushes.Red, new Point(0, przechwyconaKlatka.Height - 60));
                        }
                        #endregion


                        Wyswietlacz.Image = koncowyObraz;
                    }
                    #endregion
                }
                catch (Exception wyjatek)
                {
                    Console.WriteLine("{0} Exception caught.", wyjatek);
                };
            }
            else
            {
                EtykietkaPrawegoKomunikatuOBledzie.Visible = true;
                EtykietkaPrawegoKomunikatuOBledzie.Text = "Nie udało się odczytać strumienia video!";
            }
        }
        #endregion

        #region Funkcja filtrująca obraz filtrem HSV
        private Image<Gray, byte> FiltrujHSV(Image<Bgr, byte> obraz, Gracz gracz)
        {
            //Konwersja obrazu do HSV
            Image<Hsv, byte> przeksztalconaKlatka = obraz.Convert<Hsv, byte>();
            
            //Rozdzielenie obrazu na 3 kanały (hue, saturation and value)
            Image<Gray, byte>[] kanaly = obraz.Split();
            
            //Odfiltrowanie wszystkiego poza wartością, którą chcemy zostawić dla kanału hue
            kanaly[0] = kanaly[0].InRange(gracz.hmin, gracz.hmax);
            //To samo co wyżej, ale dla kanału zawierającego saturation
            kanaly[1] = kanaly[1].InRange(gracz.smin, gracz.smax);
            //To samo co wyżej, ale dla kanału zawierającego value
            kanaly[2] = kanaly[2].InRange(gracz.vmin, gracz.vmax);
            
            //Połączenie przefiltrowanych kanałów w jeden obraz przez operator logiczny AND
            kanaly[0] = kanaly[0].And(kanaly[1]);
            kanaly[0] = kanaly[0].And(kanaly[2]);

            return kanaly[0];
        }
        #endregion

        #region Funkcja znajdująca największy obiekt
        private Contour<Point> ZnajdzNajwiekszyObiekt(Image<Gray, byte> obrazek)
        {
            Contour<Point> najwiekszyObiekt = null;
            double najwiekszyObszar = 0;

            for (Contour<Point> obiekty = obrazek.FindContours(CHAIN_APPROX_METHOD.CV_CHAIN_APPROX_SIMPLE, RETR_TYPE.CV_RETR_EXTERNAL); obiekty != null; obiekty = obiekty.HNext)
            {
                int wielkoscPaletek = (10 + (10 * SuwakWielkosciPaletek.Value)) * (10 + (10 * SuwakWielkosciPaletek.Value));
                if (obiekty.Area > najwiekszyObszar && obiekty.Area > wielkoscPaletek)
                {
                    najwiekszyObszar = obiekty.Area;
                    najwiekszyObiekt = obiekty;
                }
            }
            return najwiekszyObiekt;
        }
        #endregion

        #region Funkcja wyszukująca środka obiektu
        private void ZnajdzSrodekObiektu(Contour<Point> punkty, ref PointF punktSrodkowy)
        {
            int ilosc = punkty.Count();
            float SumaX = 0;
            float SumaY = 0;
            foreach (PointF punkt in punkty)
            {
                SumaX += punkt.X;
                SumaY += punkt.Y;
            }
            punktSrodkowy.X = SumaX / ilosc;
            punktSrodkowy.Y = SumaY / ilosc;
        }
        #endregion

        #region Funkcja rysująca obrazek w danym punkcie
        private void RysujObrazek(Bitmap obrazek, PointF punktSrodkowy, Graphics plaszczyzna, int wysokosc, int szerokosc)
        {
            plaszczyzna.DrawImage(obrazek, punktSrodkowy.X - (szerokosc / 2), punktSrodkowy.Y - (wysokosc / 2), szerokosc, wysokosc);
        }
        #endregion

        #region Funkcje obliczająca tor lotu piłki oraz punkty

        private void ZmienTorLotuPoOdbiciu(Gracz gracz)
        {
            pilka.odbicia++;
            pilka.predkoscPrzemieszczania.X = -pilka.predkoscPrzemieszczania.X;
            EtykietkaLiczbyOdbicPilki.Text = "Odbicia: " + pilka.odbicia;

            if (pilka.pozycja.Y < (gracz.polozenie.Y - (gracz.wielkosc / 5)))
                pilka.predkoscPrzemieszczania.Y += -50.0f;

            else if (pilka.pozycja.Y < (gracz.polozenie.Y - (gracz.wielkosc / 3)))
                pilka.predkoscPrzemieszczania.Y += -30.0f;

            else if (pilka.pozycja.Y < (gracz.polozenie.Y - (gracz.wielkosc / 2)))
                pilka.predkoscPrzemieszczania.Y += -10.0f;

            else if (pilka.pozycja.Y > (gracz.polozenie.Y + (gracz.wielkosc / 2)))
                pilka.predkoscPrzemieszczania.Y += 10.0f;

            else if (pilka.pozycja.Y > (gracz.polozenie.Y + (gracz.wielkosc / 3)))
                pilka.predkoscPrzemieszczania.Y += 30.0f;

            else if (pilka.pozycja.Y > (gracz.polozenie.Y + (gracz.wielkosc / 5)))
                pilka.predkoscPrzemieszczania.Y += 50.0f;
        }

        private void RozpocznijAutomatycznieGre(RectangleF plansza, Bitmap ekran)
        {
            int czasDoAutomatycznegoRestartu = 600 + (int)pilka.predkoscPrzemieszczania.X;
            int odliczanie = 0;
            int odlegloscDoRestartu = 0;

            if (pilka.predkoscPrzemieszczania.X > 0)
                odlegloscDoRestartu = ((int)plansza.Width + czasDoAutomatycznegoRestartu) - (int)pilka.pozycja.X;

            else
                odlegloscDoRestartu = (int)pilka.pozycja.X - ((int)plansza.X - czasDoAutomatycznegoRestartu);

            odliczanie = (odlegloscDoRestartu - (odlegloscDoRestartu % 100)) / 100;

            PrzyciskRozpocznij.Text = "Zatrzymaj";
            EtykietkaOdliczaniaDoRozpoczecia.Visible = true;
            EtykietkaOdliczaniaDoRozpoczecia.Text = "Rozpoczęcie za: " + odliczanie;

            if (pilka.pozycja.X > plansza.Width + czasDoAutomatycznegoRestartu || pilka.pozycja.X < plansza.X - czasDoAutomatycznegoRestartu)
            {
                pilka.pozycja = new PointF(ekran.Width / 2.0f, ekran.Height / 2.0f);
                if (pilka.predkoscPrzemieszczania.X > 0)
                    pilka.predkoscPrzemieszczania = new PointF(-15.0f, -10.0f);

                else
                    pilka.predkoscPrzemieszczania = new PointF(15.0f, -10.0f);

                EtykietkaOdliczaniaDoRozpoczecia.Visible = false;
                punktDodany = false;
                PrzyciskRozpocznij.Text = "Zatrzymaj";
            }
        }

        private void DodajPunkt()
        {
            if (punktDodany == false)
            {
                if (pilka.predkoscPrzemieszczania.X > 0)
                {
                    gracz1.punkty++;
                    EtykietkaPunktowLewegoGracza.Text = "Punkty: " + gracz1.punkty;
                    punktDodany = true;
                    PrzyciskRozpocznij.Text = "Resetuj";
                }
                else
                {
                    gracz2.punkty++;
                    EtykietkaPunktowPrawegoGracza.Text = "Punkty: " + gracz2.punkty;
                    punktDodany = true;
                    PrzyciskRozpocznij.Text = "Resetuj";
                }
            }
        }

        private void ResetujPozycjeIPredkoscPilki(Bitmap ekran)
        {
            pilka.pozycja = new PointF(ekran.Width / 2.0f, ekran.Height / 2.0f);
            pilka.predkoscPrzyspieszania = new PointF(1.0f + (1.0f / (float)ekran.Width), 1.0f + (0.8f / (float)ekran.Height));
            if (pilka.predkoscPrzemieszczania.X > 0)
                pilka.predkoscPrzemieszczania = new PointF(-15.0f, -10.0f);

            else
                pilka.predkoscPrzemieszczania = new PointF(15.0f, -10.0f);

            EtykietkaOdliczaniaDoRozpoczecia.Visible = false;
            punktDodany = false;
            resetuj = false;
        }

        private void ObliczTorLotuPilki(Bitmap ekran, ref Graphics grafika)
        {
            RectangleF plansza = new RectangleF(pilka.wielkosc / 2, pilka.wielkosc / 2, ekran.Width - pilka.wielkosc, ekran.Height - pilka.wielkosc);
            pilka.pozycja.X += pilka.predkoscPrzemieszczania.X;
            pilka.pozycja.Y += pilka.predkoscPrzemieszczania.Y;


            if (gracz1.ObszarOdbijania().Contains((new Point((int)pilka.pozycja.X, (int)pilka.pozycja.Y))) && pilka.predkoscPrzemieszczania.X < 0)
                ZmienTorLotuPoOdbiciu(gracz1);
            
            if (gracz2.ObszarOdbijania().Contains((new Point((int)pilka.pozycja.X, (int)pilka.pozycja.Y))) && pilka.predkoscPrzemieszczania.X > 0)
                ZmienTorLotuPoOdbiciu(gracz2);

            if (pilka.pozycja.Y > plansza.Height && pilka.predkoscPrzemieszczania.Y > 0)
            {
                pilka.pozycja.Y = plansza.Y + plansza.Height;
                pilka.predkoscPrzemieszczania.Y = -pilka.predkoscPrzemieszczania.Y;
            }
            if (pilka.pozycja.Y < plansza.Y && pilka.predkoscPrzemieszczania.Y < 0)
            {
                pilka.pozycja.Y = plansza.Y;
                pilka.predkoscPrzemieszczania.Y = -pilka.predkoscPrzemieszczania.Y;
            }
            

            if (pilka.pozycja.X > plansza.Width || pilka.pozycja.X < plansza.X)
            {
                DodajPunkt();

                if (ZaznaczanieAutomatyczneRozpoczecie.Checked == true)
                    RozpocznijAutomatycznieGre(plansza, ekran);
                
                else if (resetuj == true)
                    ResetujPozycjeIPredkoscPilki(ekran);
            }
            
            pilka.predkoscPrzemieszczania.X *= pilka.predkoscPrzyspieszania.X;
            pilka.predkoscPrzemieszczania.Y *= pilka.predkoscPrzyspieszania.Y;
            pilka.predkoscPrzemieszczania.Y += pilka.silaGrawitacji;

            pilka.obrot =  (pilka.pozycja.X*pilka.wielkosc)%360;
            grafika.TranslateTransform(pilka.pozycja.X, pilka.pozycja.Y);//Ustawienie punktu obrotu na środek piłki
            grafika.RotateTransform(pilka.obrot);
            grafika.TranslateTransform(-pilka.pozycja.X, -pilka.pozycja.Y); //Przywrócenie punktu obrotu na początkowy

            RysujObrazek(obrazekPilki, pilka.pozycja, grafika, pilka.wielkosc, pilka.wielkosc);
        }
        #endregion

        #endregion

        #region Główna Funkcja
        public EkranGlowny()
        {
            InitializeComponent(); //Utworzenie obiektów na ekranie

            UstawOkienka();

            Deserializuj();

            UstawParametry(strumien.QueryFrame().ToBitmap());

            OdswiezSuwakiHSV(gracz1);

            OdswiezSuwakiTolerancji(gracz1);

        }
        #endregion

        #region Funkcje elementów formularza

        private void ZaznaczanieLicznikFPS_CheckedChanged(object sender, EventArgs e)
        {
            if (ZaznaczanieLicznikFPS.Checked == true)
            {
                LicznikFPS.Start();
            }
            else
            {
                LicznikFPS.Stop();
                EtykietkaFPS.Visible = false;
            }
        }
        private void LicznikFPS_Tick(object sender, EventArgs e)
        {
            EtykietkaFPS.Text = FPS.ToString();
            EtykietkaFPS.Visible = true;
            FPS = 0;
        }

        #region Operacje wykonywane po uruchomieniu zegara
        private void ProcessFrame(object sender, EventArgs arg)
        {
            if (WyborIdle.Checked == true)
            {
                Gra();
                FPS++;
            }
        }


        private void Zegar_Tick(object sender, EventArgs e)
        {
            if (WyborTimer.Checked == true)
            {
                Gra();
                FPS++;
            }
        }
        #endregion

        #region Zamykanie formularza
        private void EkranGlowny_FormClosing(object sender, FormClosingEventArgs e)
        {
            strumien.Dispose();
            Zegar.Stop();
            Serializuj();
        }
        #endregion

        #region Akcja przycisku "Rozpocznij"
        private void PrzyciskRozpocznij_Click(object sender, EventArgs e)
        {
            if (przechwytywanieAktywne == false)
            {
                Zegar.Start();
                Application.Idle += ProcessFrame;
                PrzyciskRozpocznij.Text = "Zatrzymaj";
                przechwytywanieAktywne = true;
            }
            else
            {
                Application.Idle -= ProcessFrame;
                if (PrzyciskRozpocznij.Text == "Resetuj")
                {
                    Zegar.Start();
                    PrzyciskRozpocznij.Text = "Zatrzymaj";
                    przechwytywanieAktywne = true;
                    resetuj = true;
                }
                else
                {
                    Zegar.Stop();
                    PrzyciskRozpocznij.Text = "Rozpocznij";
                    przechwytywanieAktywne = false;
                }
            }
        }
        #endregion

        #region Akcja przycisku "Ustawienia"
        private void PrzyciskUstawienia_Click(object sender, EventArgs e)
        {

            if (PanelUstawien.Visible == false)
            {
                EtykietkaLewegoKomunikatuOBledzie.Visible = false;
                PanelUstawien.Visible = true;
                PrzyciskUstawienia.Text = "Ukryj Ustawienia";
                WyborRzeczywistegoEkranu.Checked = true;
                ZaznaczenieAutomatycznegoPobieraniaWartosci.Checked = false;
                WyborRzeczywistegoEkranu.Checked = true;
                WyborFiltrowanegoEkranu.Checked = false;
            }
            else
            {
                PanelUstawien.Visible = false;
                PrzyciskUstawienia.Text = "Pokaż Ustawienia";
                WyborRzeczywistegoEkranu.Checked = true;
                ZaznaczenieAutomatycznegoPobieraniaWartosci.Checked = false;
                WyborRzeczywistegoEkranu.Checked = true;
                WyborFiltrowanegoEkranu.Checked = false;
            }
        }

        #endregion
        
        #region Akcja wyboru gracza 1
        private void WyborGracza1_CheckedChanged(object sender, EventArgs e)
        {
            if (WyborGracza1.Checked == true)
            {
                OdswiezSuwakiHSV(gracz1);
                OdswiezSuwakiTolerancji(gracz1);
            }
        }
        #endregion

        #region Akcja wyboru gracza 2
        private void WyborGracza2_CheckedChanged(object sender, EventArgs e)
        {
            if (WyborGracza2.Checked == true)
            {
                OdswiezSuwakiHSV(gracz2);
                OdswiezSuwakiTolerancji(gracz2);
            }
        }
        #endregion


        #region Akcja zaznaczenia automatycznego pobierania koloru
        private void ZaznaczenieAutomatycznegoPobieraniaWartosci_CheckedChanged(object sender, EventArgs e)
        {
            if (ZaznaczenieAutomatycznegoPobieraniaWartosci.Checked == true)
            {
                EtykietkaTolerancji.Enabled = true;
                EtykietkaWartosciTolerancji.Enabled = true;
                EtykietkaMinimalnejTolerancji.Enabled = true;
                EtykietkaMaksymalnejTolerancji.Enabled = true;

                PrzyciskAkceptujKolor.Enabled = true;
                SuwakTolerancjiMinimum.Enabled = true;
                SuwakTolerancjiMaksimum.Enabled = true;

                SuwakHueMinimum.Enabled = false;
                SuwakHueMaksimum.Enabled = false;
                SuwakSaturationMinimum.Enabled = false;
                SuwakSaturationMaksimum.Enabled = false;
                SuwakValueMinimum.Enabled = false;
                SuwakValueMaksimum.Enabled = false;
            }
            else
            {
                if (WyborGracza1.Checked == true)
                {
                    OdswiezSuwakiHSV(gracz1);
                }
                else
                {
                    OdswiezSuwakiHSV(gracz2);
                }
                EtykietkaTolerancji.Enabled = false;
                EtykietkaWartosciTolerancji.Enabled = false;
                EtykietkaMinimalnejTolerancji.Enabled = false;
                EtykietkaMaksymalnejTolerancji.Enabled = false;

                PrzyciskAkceptujKolor.Enabled = false;
                SuwakTolerancjiMinimum.Enabled = false;
                SuwakTolerancjiMaksimum.Enabled = false;

                SuwakHueMinimum.Enabled = true;
                SuwakHueMaksimum.Enabled = true;
                SuwakSaturationMinimum.Enabled = true;
                SuwakSaturationMaksimum.Enabled = true;
                SuwakValueMinimum.Enabled = true;
                SuwakValueMaksimum.Enabled = true;
            }
        }
        #endregion

        #region Akcja przycisku "Akceptuj kolor"
        private void PrzyciskAkceptujKolor_Click(object sender, EventArgs e)
        {
            if (WyborGracza1.Checked == true)
            {
                gracz1.KopiujKolory(graczAutomatyczny);
            }
            else
            {
                gracz2.KopiujKolory(graczAutomatyczny);
            }
        }
        #endregion
        
        #region Akcja suwaka minimalnego odcienia
        private void SuwakHueMinimum_Scroll(object sender, EventArgs e)
        {
            if (WyborGracza1.Checked == true)
            {
                if (SuwakHueMinimum.Value >= SuwakHueMaksimum.Value)
                {
                    gracz1.hmax.Intensity = SuwakHueMaksimum.Value = SuwakHueMinimum.Value + 1; 
                }
                gracz1.hmin.Intensity = SuwakHueMinimum.Value;
            }
            else
            {
                if (SuwakHueMinimum.Value >= SuwakHueMaksimum.Value)
                {
                    gracz2.hmax.Intensity = SuwakHueMaksimum.Value = SuwakHueMinimum.Value + 1;
                }
                gracz2.hmin.Intensity = SuwakHueMinimum.Value;
            }
            EtykietkaWartosciHue.Text = SuwakHueMinimum.Value.ToString() + " - " + SuwakHueMaksimum.Value.ToString();
        }
        #endregion

        #region Akcja suwaka maksymalnego odcienia
        private void SuwakHueMaksimum_Scroll(object sender, EventArgs e)
        {
            if (WyborGracza1.Checked == true)
            {
                if (SuwakHueMaksimum.Value <= SuwakHueMinimum.Value)
                {
                    gracz1.hmin.Intensity = SuwakHueMinimum.Value = SuwakHueMaksimum.Value - 1;
                }
                gracz1.hmax.Intensity = SuwakHueMaksimum.Value;
                EtykietkaWartosciHue.Text = SuwakHueMinimum.Value.ToString() + " - " + SuwakHueMaksimum.Value.ToString();
            }
            else
            {
                if (SuwakHueMaksimum.Value <= SuwakHueMinimum.Value)
                {
                    gracz2.hmin.Intensity = SuwakHueMinimum.Value = SuwakHueMaksimum.Value - 1;
                }
                gracz2.hmax.Intensity = SuwakHueMaksimum.Value;
                EtykietkaWartosciHue.Text = SuwakHueMinimum.Value.ToString() + " - " + SuwakHueMaksimum.Value.ToString();
            }
        }
        #endregion

        #region Akcja suwaka minimalnego nasycenia
        private void SuwakSaturationMinimum_Scroll(object sender, EventArgs e)
        {
            if (WyborGracza1.Checked == true)
            {
                if (SuwakSaturationMinimum.Value >= SuwakSaturationMaksimum.Value)
                {
                    gracz1.smax.Intensity = SuwakSaturationMaksimum.Value = SuwakSaturationMinimum.Value + 1;
                }
                gracz1.smin.Intensity = SuwakSaturationMinimum.Value;
                EtykietkaWartosciSaturation.Text = SuwakSaturationMinimum.Value.ToString() + " - " + SuwakSaturationMaksimum.Value.ToString();
            }
            else
            {
                if (SuwakSaturationMinimum.Value >= SuwakSaturationMaksimum.Value)
                {
                    gracz2.smax.Intensity = SuwakSaturationMaksimum.Value = SuwakSaturationMinimum.Value + 1;
                }
                gracz2.smin.Intensity = SuwakSaturationMinimum.Value;
                EtykietkaWartosciSaturation.Text = SuwakSaturationMinimum.Value.ToString() + " - " + SuwakSaturationMaksimum.Value.ToString();
            }
        }
        #endregion

        #region Akcja suwaka maksymalnego nasycenia
        private void SuwakSaturationMaksimum_Scroll(object sender, EventArgs e)
        {
            if (WyborGracza1.Checked == true)
            {
                if (SuwakSaturationMaksimum.Value <= SuwakSaturationMinimum.Value)
                {
                    gracz1.smin.Intensity = SuwakSaturationMinimum.Value = SuwakSaturationMaksimum.Value - 1;
                }
                gracz1.smax.Intensity = SuwakSaturationMaksimum.Value;
                EtykietkaWartosciSaturation.Text = SuwakSaturationMinimum.Value.ToString() + " - " + SuwakSaturationMaksimum.Value.ToString();
            }
            else
            {
                if (SuwakSaturationMaksimum.Value <= SuwakSaturationMinimum.Value)
                {
                    gracz2.smin.Intensity = SuwakSaturationMinimum.Value = SuwakSaturationMaksimum.Value - 1;
                }
                gracz2.smax.Intensity = SuwakSaturationMaksimum.Value;
                EtykietkaWartosciSaturation.Text = SuwakSaturationMinimum.Value.ToString() + " - " + SuwakSaturationMaksimum.Value.ToString();
            }
        }
        #endregion

        #region Akcja suwaka minimalnej jasności
        private void SuwakValueMinimum_Scroll(object sender, EventArgs e)
        {
            if (WyborGracza1.Checked == true)
            {
                if (SuwakValueMinimum.Value >= SuwakValueMaksimum.Value)
                {
                    gracz1.vmax.Intensity = SuwakValueMaksimum.Value = SuwakValueMinimum.Value + 1;
                }
                gracz1.vmin.Intensity = SuwakValueMinimum.Value;
                EtykietkaWartosciValue.Text = SuwakValueMinimum.Value.ToString() + " - " + SuwakValueMaksimum.Value.ToString();
            }
            else
            {
                if (SuwakValueMinimum.Value >= SuwakValueMaksimum.Value)
                {
                    gracz2.vmax.Intensity = SuwakValueMaksimum.Value = SuwakValueMinimum.Value + 1;
                }
                gracz2.vmin.Intensity = SuwakValueMinimum.Value;
                EtykietkaWartosciValue.Text = SuwakValueMinimum.Value.ToString() + " - " + SuwakValueMaksimum.Value.ToString();
            }
        }
        #endregion

        #region Akcja suwaka maksymalnej jasnosci
        private void SuwakValueMaksimum_Scroll(object sender, EventArgs e)
        {
            if (WyborGracza1.Checked == true)
            {
                if (SuwakValueMaksimum.Value <= SuwakValueMinimum.Value)
                {
                    gracz1.vmin.Intensity = SuwakValueMinimum.Value = SuwakValueMaksimum.Value - 1;
                }
                gracz1.vmax.Intensity = SuwakValueMaksimum.Value;
                EtykietkaWartosciValue.Text = SuwakValueMinimum.Value.ToString() + " - " + SuwakValueMaksimum.Value.ToString();
            }
            else
            {
                if (SuwakValueMaksimum.Value <= SuwakValueMinimum.Value)
                {
                    gracz2.vmin.Intensity = SuwakValueMinimum.Value = SuwakValueMaksimum.Value - 1;
                }
                gracz2.vmax.Intensity = SuwakValueMaksimum.Value;
                EtykietkaWartosciValue.Text = SuwakValueMinimum.Value.ToString() + " - " + SuwakValueMaksimum.Value.ToString();
            }
        }
        #endregion
        
        #region Akcja suwaka minimalnej tolerancji
        private void SuwakTolerancjiMinimum_Scroll(object sender, EventArgs e)
        {
            graczAutomatyczny.minimalnaTolerancja = SuwakTolerancjiMinimum.Value;
            EtykietkaWartosciTolerancji.Text = "Dolna: " + SuwakTolerancjiMinimum.Value.ToString() + "   Górna: " + SuwakTolerancjiMaksimum.Value.ToString();
        }
        #endregion

        #region Akcja suwaka maksymalnej tolerancji
        private void SuwakTolerancjiMaksimum_Scroll(object sender, EventArgs e)
        {
            graczAutomatyczny.maksymalnaTolerancja = SuwakTolerancjiMaksimum.Value;
            EtykietkaWartosciTolerancji.Text = "Dolna: " + SuwakTolerancjiMinimum.Value.ToString() + "   Górna: " + SuwakTolerancjiMaksimum.Value.ToString();
        }
        #endregion

        #region Akcja suwaka wielkości paletek
        private void SuwakWielkosciPaletek_Scroll(object sender, EventArgs e)
        {
            EtykietkaWielkoscPaletek.Text = SuwakWielkosciPaletek.Value.ToString();
        }
        #endregion

        #region Akcja przycisku "Zakończ"
        private void PrzyciskZakonczProgram_Click(object sender, EventArgs e)
        {
            Close();
        }
        #endregion

        private void MenuRozwijalneKamer_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (Kamery != null)
            {
                strumien = new Capture(MenuRozwijalneKamer.SelectedIndex);
            }
        }

        #endregion
    }
}
