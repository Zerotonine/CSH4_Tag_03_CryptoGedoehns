using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;
using System.Xml.Serialization;

namespace CSH4_Tag_03_CryptoGedöhns
{
    class Program
    {
        //TODO: error handling
        //TODO: lustiges Menü bauen
        static string projectPath;
        const string DATEI = "plainText.txt";
        static void Main(string[] args)
        {
            projectPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.FullName;

            string plainText = File.ReadAllText(projectPath + Path.DirectorySeparatorChar +DATEI);
            Console.WriteLine("Text zum Verschlüsseln: " + plainText);

            encrypt();

            Console.Write("\nNeu entschlüsselter Text: ");
            Console.WriteLine(decrypt());

            Console.ReadKey();
        }

        static void encrypt()
        {
            string plainText = File.ReadAllText(projectPath + Path.DirectorySeparatorChar + DATEI);
            string path = projectPath + Path.DirectorySeparatorChar + DATEI + "_encrypted.txt";
            using (AesCryptoServiceProvider acsp = new AesCryptoServiceProvider())
            {
                acsp.GenerateKey();
                acsp.GenerateIV();
                ICryptoTransform encryptor = acsp.CreateEncryptor(acsp.Key, acsp.IV);

                #region XML Serialisierung
                KIV kiv = new KIV();
                kiv.Key = acsp.Key;
                kiv.IV = acsp.IV;
                XMLSerialisierung(kiv);
                #endregion

                using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
                {
                    using (CryptoStream cs = new CryptoStream(fs, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                }
            }
        }

        static string decrypt()
        {
            string strDecrypted;
            string path = projectPath + Path.DirectorySeparatorChar + DATEI;
            using (AesCryptoServiceProvider acsp = new AesCryptoServiceProvider())
            {
                KIV kiv = XMLSDeserialisierung();

                ICryptoTransform decryptor = acsp.CreateDecryptor(kiv.Key, kiv.IV);

                using(FileStream fs = new FileStream(path + "_encrypted.txt", FileMode.Open, FileAccess.Read))
                {
                    using(CryptoStream cs = new CryptoStream(fs, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            strDecrypted = sr.ReadToEnd();
                        }
                    }
                }
            }
            File.WriteAllText(path + "_decrypted.txt", strDecrypted);
            return strDecrypted;
        }

        static void XMLSerialisierung(KIV kiv)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(KIV));
            string path = projectPath + Path.DirectorySeparatorChar + DATEI + "_KIV.xml";
            
            using(FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                xmlSerializer.Serialize(fs, kiv);
            }
        }

        static KIV XMLSDeserialisierung()
        {
            string path = projectPath + Path.DirectorySeparatorChar + DATEI + "_KIV.xml";
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(KIV));

            using(StreamReader sr = new StreamReader(path))
            {
                KIV kiv = xmlSerializer.Deserialize(sr) as KIV;
                return kiv;
            }

        }
    }
}
