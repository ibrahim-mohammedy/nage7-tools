using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace UIBuildExecutionList
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                MethodInfo[] methods = GetTestMethods(args[0]);

                Dictionary<string, MethodInfo> hashedMethods = new Dictionary<string, MethodInfo>();
                foreach (MethodInfo mi in methods)
                {
                    string hashedName = HashString(mi.Name);
                    if (hashedMethods.ContainsKey(hashedName))
                    {
                        throw new Exception(hashedName);
                    }

                    hashedMethods[hashedName] = mi;
                }

                XmlDocument xd = new XmlDocument();
                xd.Load(args[1]);

                XmlNode el = xd.SelectSingleNode("//ExecutionList");

                el.RemoveAll();

                Dictionary<string, string> synchronize = new Dictionary<string, string>();

                Dictionary<string, List<XmlElement>> dependentScripts = new Dictionary<string, List<XmlElement>>();

                foreach (MethodInfo mi in GetRandomizedMethods(methods))
                {
                    XmlElement xm = xd.CreateElement(mi.Name);
                    xm.SetAttribute("persona", "admin");

                    bool hasDependency = false;
                    foreach (Attribute a in mi.GetCustomAttributes(typeof(PropertyAttribute), false))
                    {
                        if (a is PropertyAttribute pa)
                        {
                            if (pa.Properties["Synchronize"].Count > 0)
                            {
                                string key = pa.Properties["Synchronize"][0].ToString();

                                synchronize[key] = mi.Name;

                                xm.SetAttribute("fqn", $"{mi.DeclaringType.FullName}.{mi.Name}");

                                if (!dependentScripts.ContainsKey(key)) dependentScripts[key] = new List<XmlElement>();

                                dependentScripts[key].Add(xm);

                                hasDependency = true;
                            }
                        }
                    }

                    if (!hasDependency) el.AppendChild(xm);
                }

                foreach (string key in dependentScripts.Keys)
                {
                    List<XmlElement> dependents = dependentScripts[key];

                    dependents.Sort((x, y) => String.Compare(x.GetAttribute("fqn"), y.GetAttribute("fqn")));

                    for (int index = 1; index < dependents.Count; index++)
                    {
                        dependents[index].SetAttribute("dependentScripts", dependents[index - 1].Name);
                    }

                    dependents.ForEach(d => el.AppendChild(d));
                }

                xd.Save(args[1]);
            }
            else if (args.Length == 3)
            {
                ExportCSV(args[1], args[2]);
            }
        }

        static List<MethodInfo> GetRandomizedMethods(MethodInfo[] methods)
        {
            Random r = new Random();

            List<MethodInfo> items = new List<MethodInfo>(methods);
            List<MethodInfo> randomized = new List<MethodInfo>();

            while (items.Count > 0)
            {
                int index = r.Next(0, items.Count);

                randomized.Add(items[index]);
                items.RemoveAt(index);
            }

            return randomized;
        }

        static string HashString(string text)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);

            char[] hash2 = new char[16];

            // Note that here we are wasting bits of hash!
            // But it isn't really important, because hash.Length == 32
            for (int i = 0; i < hash2.Length; i++)
            {
                hash2[i] = chars[hash[i] % chars.Length];
            }

            return new string(hash2);
        }

        static MethodInfo[] GetTestMethods(string assembly)
        {
            Assembly ta = Assembly.LoadFile(assembly);

            try
            {
                return ta.GetTypes()
                          .SelectMany(t => t.GetMethods())
                          .Where(m => m.GetCustomAttributes(typeof(TestAttribute), false).Length > 0)
                          .ToArray();
            }
            catch (Exception)
            {
            }

            return null;
        }

        static void ExportCSV(string dll, string csvFile)
        {
            using (StreamWriter sw = File.CreateText(csvFile))
            {
                Assembly ta = Assembly.LoadFile(dll);

                try
                {
                    var methods = ta.GetTypes()
                              .SelectMany(t => t.GetMethods())
                              .Where(m => m.GetCustomAttributes(typeof(TestAttribute), false).Length > 0)
                              .ToArray();
                    List<string> methodNames = new List<string>();

                    foreach (MethodInfo mi in methods)
                    {
                        TestOfAttribute testOf = (TestOfAttribute)mi.GetCustomAttributes(typeof(TestOfAttribute), true)[0];
                        if (!string.IsNullOrWhiteSpace(testOf.Properties["TestOf"][0].ToString())) continue;

                        sw.Write(mi.Name);
                        sw.Write(",");

                        DescriptionAttribute description = (DescriptionAttribute)mi.GetCustomAttributes(typeof(DescriptionAttribute), true)[0];

                        sw.Write($"\"{description.Properties["Description"][0]}\"");
                        sw.Write(",");

                        sw.Write($"\"{testOf.Properties["TestOf"][0]}\"");
                        sw.Write(",");

                        sw.WriteLine();
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}