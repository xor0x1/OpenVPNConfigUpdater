using System;
using System.IO;
using System.Diagnostics;
using System.Linq;

namespace OpenVPNConfigUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] configPaths = { @"C:\Program Files (x86)\OpenVPN\config", @"C:\Program Files\OpenVPN\config", Path.Combine(@"C:\Users", Environment.UserName, @"OpenVPN\config") };
            string targetExtension = ".ovpn";
            string searchString = "remote";

            string[] configFiles = FindConfigFiles(configPaths, targetExtension);
            if (configFiles.Length == 0)
            {
                Console.WriteLine("Конфигурационные файлы .ovpn не найдены.");
                WaitForKeyPress();
                return;
            }

            Console.WriteLine("Найденные конфигурационные файлы:");
            foreach (var configFilePath in configFiles)
            {
                Console.WriteLine(configFilePath);
            }

            Console.WriteLine("\nВведите начало имени файла VPN (например, MyVPN):");
            string fileNameStartsWith = Console.ReadLine().Trim();

            Console.WriteLine("Введите новый адрес подключения (например, 123.123.123.123 или vpn.example.com):");
            string newDomain = Console.ReadLine().Trim();

            Console.WriteLine("\nВведите порт подключения (по умолчанию 1194):");
            string portInput = Console.ReadLine().Trim();
            int port = string.IsNullOrEmpty(portInput) ? 1194 : int.Parse(portInput);

            string newRemote = "remote " + newDomain + " " + port + " udp";

            foreach (var configFilePath in configFiles)
            {
                if (configFilePath.Contains(fileNameStartsWith))
                {
                    Console.WriteLine("\nНайден конфигурационный файл: " + configFilePath);
                    if (CheckConfigFileForString(configFilePath, searchString))
                    {
                        UpdateConfigFile(configFilePath, searchString, newRemote);
                        Console.WriteLine("Файл успешно обновлен.");
                    }
                    else
                    {
                        Console.WriteLine("В файле нет строки \"remote\". Файл не был изменен.");
                    }
                }
            }

            Console.WriteLine("\nВсе файлы обработаны. Файлы сохранены.");

            WaitForKeyPress();
        }

        static bool CheckConfigFileForString(string filePath, string searchString)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                return lines.Any(line => line.TrimStart().StartsWith(searchString));
            }
            catch (Exception e)
            {
                Console.WriteLine("Произошла ошибка при проверке файла: " + e.Message);
                return false;
            }
        }

        static string[] FindConfigFiles(string[] paths, string targetExtension)
        {
            var configFileList = new System.Collections.Generic.List<string>();
            foreach (var path in paths)
            {
                if (Directory.Exists(path))
                {
                    configFileList.AddRange(Directory.GetFiles(path, "*" + targetExtension, SearchOption.AllDirectories));
                }
            }

            return configFileList.ToArray();
        }

        static void UpdateConfigFile(string filePath, string searchString, string newRemote)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].TrimStart().StartsWith(searchString))
                    {
                        lines[i] = newRemote;
                        break;
                    }
                }
                File.WriteAllLines(filePath, lines);
            }
            catch (Exception e)
            {
                Console.WriteLine("Произошла ошибка: " + e.Message);
            }
        }

        static void WaitForKeyPress()
        {
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}
