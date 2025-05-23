using System;
using System.Net.NetworkInformation;
using System.Text;
using System.IO;
using System.Linq;


//namespace for the ping application
namespace Ping_pong_program
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== C# Ping ===");
            //main program loop
            while (true)
            {
                //display menu options to the user
                Console.WriteLine("\n1. Start ping");
                Console.WriteLine("2. Indlæs tidligere resultater");
                Console.WriteLine("3. Afslut");
                Console.Write("Vælg en funktion: ");
                string valg = Console.ReadLine();
                //process user selection
                switch (valg)
                {
                    case "1":
                        StartPing();
                        break;
                    case "2":
                        IndLæsResultater();
                        break;
                    case "3":
                        return; //exit the application
                    default:
                        Console.WriteLine("Ugyldigt valg!");
                        break;
                }
            }
        }

        static void StartPing()
        {
            string host;
            //loop until user enters a valid host
            while (true)
            {
                Console.Write("Indtast IP-adresse eller domænenavn: ");
                host = Console.ReadLine();
                //check for empty input or spaces
                if (string.IsNullOrWhiteSpace(host) || host.Contains(" "))
                {
                    Console.WriteLine("Ugyldig IP-adresse eller domænenavn. Prøv igen.");
                    continue;
                }

                //validate IP adress format (x.x.x.x)
                string[] parts = host.Split('.');
                if (parts.Length == 4)
                {
                    bool validIp = true;
                    foreach (string part in parts)
                    {
                        //check each octet is a number between 0-255
                        if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                        {
                            validIp = false;
                            break;
                        }
                    }

                    if (validIp)
                        break;
                }

                //simple domain name validation (contains at least one period not at start/end
                if (host.Contains('.') && host.IndexOf('.') != 0 && host.IndexOf('.') != host.Length - 1)
                {
                    break;
                }

                Console.WriteLine("Ugyldig IP-adresse eller domænenavn. Prøv igen.");
            }




            //get number of ping attempts from user
            int attempts;
            while (true)
            {
                Console.Write("Antal ping-forsøg (default 4): ");
                string attemptsInput = Console.ReadLine();
                //use default value if input is empty
                if (string.IsNullOrEmpty(attemptsInput))
                {
                    attempts = 4;
                    break;
                }
                //validate input is a positive number
                if (int.TryParse(attemptsInput, out attempts) && attempts > 0)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Ugyldigt input. Indtast et tal.");
                }
            }
            //get packet size from user
            int bufferSize;
            while (true)
            {
                Console.Write("Pakkestørrelse i bytes (default 32): ");
                string sizeInput = Console.ReadLine();
                //use default value if input is empty
                if (string.IsNullOrEmpty(sizeInput))
                {
                    bufferSize = 32;
                    break;
                }
                //validate input is a positive number
                if (int.TryParse(sizeInput, out bufferSize) && bufferSize > 0)
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Ugyldigt input. Indtast en gyldig pakke størrelse.");
                }
            }


            //create random data buffer for ping packet
            byte[] buffer = new byte[bufferSize];
            new Random().NextBytes(buffer);
            //initialize ping sender and result storage
            Ping pingSender = new Ping();
            StringBuilder resultBuilder = new StringBuilder();

            Console.WriteLine($"\nPinger {host} med {bufferSize} bytes data:\n");
            //execute ping attempts
            for (int i = 0; i < attempts; i++)
            {
                try
                {
                    //send ping with 1000ms timeout
                    PingReply reply = pingSender.Send(host, 1000, buffer);
                    //process successful ping
                    if (reply.Status == IPStatus.Success)
                    {
                        string output = $"Svar fra {reply.Address}: bytes={reply.Buffer.Length} tid={reply.RoundtripTime}ms TTL={reply.Options?.Ttl}";
                        Console.WriteLine(output);
                        resultBuilder.AppendLine(output);
                    }
                    //handle ping faliure
                    else
                    {
                        string output = $"Fejl: {reply.Status}";
                        Console.WriteLine(output);
                        resultBuilder.AppendLine(output);
                    }
                }
                //handle exeptions (host not found, network issues, etc.)
                catch (Exception ex)
                {
                    string output = $"Undtagelse: {ex.Message}";
                    Console.WriteLine(output);
                    resultBuilder.AppendLine(output);
                }
            }
            //ask user if they want to save the result
            while (true)
            {
                Console.Write("\nVil du gemme resultaterne? (j/n): ");
                string svar = Console.ReadLine().ToLower();

                if (svar == "j")
                {
                    //save results to user specified file
                    string filename;
                    while (true)
                    {
                        Console.Write("Indtast filnavn (fx pingresultater.txt): ");
                        filename = Console.ReadLine();

                        // Tjek for tomt input eller ugyldige filnavne
                        if (string.IsNullOrWhiteSpace(filename) ||
                            filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                        {
                            Console.WriteLine("Ugyldigt filnavn. Prøv igen.");
                            continue;
                        }

                        try
                        {
                            File.WriteAllText(filename, resultBuilder.ToString());
                            Console.WriteLine("Resultater gemt.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Fejl ved gemning: {ex.Message}. Prøv et andet filnavn.");
                        }
                    }

                    Console.WriteLine("Resultater gemt.");
                    break;
                }
                else if (svar == "n")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Brug venligst 'j' eller 'n'.");
                }
            }

        }

        static void IndLæsResultater()
        {
            //get filename from user
            Console.Write("Indtast filnavn for at indlæse resultater: ");
            string filename = Console.ReadLine();
            //check if file exists and display its contents
            if (File.Exists(filename))
            {
                string content = File.ReadAllText(filename);
                Console.WriteLine("\n=== Gemte Ping-resultater ===");
                Console.WriteLine(content);
            }
            else
            {
                Console.WriteLine("Filen findes ikke.");
            }

        }
    }
}
