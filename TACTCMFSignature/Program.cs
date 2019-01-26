using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using TACTLib.Core.Product.Tank;

namespace TACTCMFSignature {
    internal class Program {
        public static void Main(string[] args) {
            foreach (var file in args) {
                Console.WriteLine(file);
                if (Directory.Exists(file)) {
                    Main(Directory.GetFiles(file));
                    continue;
                }

                if (!File.Exists(file)) continue;
                var cs = File.ReadAllText(file).Trim();
                if (cs.StartsWith("// <TACT ")) continue;
                var model = new CMFCryptHandler.CompiledTACTHeader {
                                                                       HASH = BitConverter.ToString(CMFCryptHandler.CreateDigest(cs))
                                                                                          .Replace("-", string.Empty),
                                                                       NAME = $"TACTLib.{Path.GetFileNameWithoutExtension(file)}.dll"
                                                                   };
                var writerSettings = new XmlWriterSettings {
                                                               OmitXmlDeclaration = true,
                                                               ConformanceLevel   = ConformanceLevel.Document ,
                                                               Encoding           = Encoding.UTF8
                                                           };
                var serializer = new XmlSerializer(typeof(CMFCryptHandler.CompiledTACTHeader));
                using (var fs = File.Open(file, FileMode.OpenOrCreate)) {
                    fs.SetLength(0);
                    using (var writer = new StreamWriter(fs))
                    using (var xmlWriter = XmlTextWriter.Create(writer, writerSettings)) {
                        writer.Write("// ");
                        serializer.Serialize(xmlWriter, model);
                        writer.Flush();
                        writer.WriteLine();
                        writer.WriteLine(cs);
                    }
                }
            }
        }
    }
}
