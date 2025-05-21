using System.Net.NetworkInformation;
using System.Text;

namespace YourCompany.NetworkTools.PingUtility
{
    // Main program class with UI handling
    public class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Ping Pong - Netværks Værktøj";
            var application = new PingApplication();
            application.Run();
        }
    }

    // Application class that handles the main program flow
    public class PingApplication
    {
        private readonly IPingService _pingService;
        private readonly IUserInterface _userInterface;
        private readonly IFileService _fileService;
        private readonly ThemeManager _themeManager;

        public PingApplication()
        {
            _pingService = new PingService();
            _userInterface = new EnhancedConsoleInterface();
            _fileService = new FileService();
            _themeManager = new ThemeManager();
        }

        public void Run()
        {
            // Apply random theme on startup
            _themeManager.ApplyRandomTheme();

            // Display welcome animation
            _userInterface.DisplayAnimation();

            bool isRunning = true;
            while (isRunning)
            {
                // Display menu and get user choice
                _userInterface.DisplayHeader("PING PONG");
                _userInterface.DisplayMenu(new[]
                {
                    "Start ping",
                    "Indlæs tidligere resultater",
                    "Skift tema",
                    "Afslut"
                });

                string userChoice = _userInterface.GetInput("Vælg en funktion");

                switch (userChoice)
                {
                    case "1":
                        ExecutePingOperation();
                        break;
                    case "2":
                        LoadResults();
                        break;
                    case "3":
                        ChangeTheme();
                        break;
                    case "4":
                        _userInterface.DisplayMessage("Afslutter programmet...");
                        Thread.Sleep(1000);
                        isRunning = false;
                        break;
                    default:
                        _userInterface.DisplayError("Ugyldigt valg!");
                        break;
                }
            }
        }

        private void ChangeTheme()
        {
            _userInterface.DisplayHeader("Tema Vælger");

            string[] availableThemes = _themeManager.GetAvailableThemes();
            for (int i = 0; i < availableThemes.Length; i++)
            {
                _userInterface.DisplayMessage($"{i + 1}. {availableThemes[i]}");
            }

            int themeIndex = _userInterface.GetValidNumberInput(
                "Vælg et tema",
                1, availableThemes.Length, 1) - 1;

            _themeManager.ApplyTheme(availableThemes[themeIndex]);
            _userInterface.DisplaySuccess($"Tema ændret til {availableThemes[themeIndex]}!");
        }

        private void ExecutePingOperation()
        {
            _userInterface.DisplayHeader("PING OPERATION");

            // Get host with validation
            string hostAddress = _userInterface.GetValidInput(
                "Indtast IP-adresse eller domænenavn",
                input => InputValidator.IsValidHostOrIp(input),
                "Ugyldig IP-adresse eller domænenavn. Prøv igen."
            );

            // Get attempts with validation
            int pingAttempts = _userInterface.GetValidNumberInput(
                "Antal ping-forsøg (default 4)",
                1, int.MaxValue, 4
            );

            // Get buffer size with validation
            int pingBufferSize = _userInterface.GetValidNumberInput(
                "Pakkestørrelse i bytes (default 32)",
                1, 65500, 32
            );

            // Display ping animation
            _userInterface.DisplayPingAnimation();

            // Execute ping and get results
            _userInterface.DisplayBanner($"Pinger {hostAddress} med {pingBufferSize} bytes data");

            var pingResults = _pingService.ExecutePing(hostAddress, pingAttempts, pingBufferSize);
            int successCount = pingResults.Count(r => r.IsSuccess);

            // Display results with progress bar
            _userInterface.DisplayProgressBar(successCount, pingAttempts);

            foreach (var result in pingResults)
            {
                if (result.IsSuccess)
                    _userInterface.DisplaySuccess(result.Message);
                else
                    _userInterface.DisplayError(result.Message);
            }

            // Display statistics
            _userInterface.DisplayStatistics(pingResults);

            // Ask if user wants to save results
            if (_userInterface.GetYesNoInput("\nVil du gemme resultaterne?", "j", "n"))
            {
                SaveResults(pingResults);
            }
        }

        private void SaveResults(List<PingResult> results)
        {
            string filename = _userInterface.GetValidInput(
                "Indtast filnavn (fx pingresultater.txt)",
                input => InputValidator.IsValidFilename(input),
                "Ugyldigt filnavn. Prøv igen."
            );

            try
            {
                // Check if file already exists
                if (File.Exists(filename))
                {
                    // Ask for confirmation before overwriting
                    bool shouldOverwrite = _userInterface.GetYesNoInput(
                        "En fil med dette navn findes allerede, skal den overskrives?",
                        "j", "n");

                    if (!shouldOverwrite)
                    {
                        _userInterface.DisplayMessage("Gemning annulleret.");
                        return; // Exit without saving
                    }
                }

                _fileService.SaveResults(filename, results);
                _userInterface.DisplaySuccess("Resultater gemt.");
            }
            catch (Exception ex)
            {
                _userInterface.DisplayError($"Fejl ved gemning: {ex.Message}");
            }
        }

        private void LoadResults()
        {
            _userInterface.DisplayHeader("INDLÆS RESULTATER");

            string filename = _userInterface.GetInput("Indtast filnavn for at indlæse resultater");

            try
            {
                string fileContent = _fileService.LoadFile(filename);
                _userInterface.DisplayBanner("Gemte Ping-resultater");
                _userInterface.DisplayMessage(fileContent);
            }
            catch (FileNotFoundException)
            {
                _userInterface.DisplayError("Filen findes ikke.");
            }
            catch (Exception ex)
            {
                _userInterface.DisplayError($"Fejl ved indlæsning: {ex.Message}");
            }
        }
    }

    // Class to manage console themes
    public class ThemeManager
    {
        private readonly Dictionary<string, ConsoleTheme> _themes;

        public ThemeManager()
        {
            _themes = new Dictionary<string, ConsoleTheme>
            {
                { "Standard", new ConsoleTheme(ConsoleColor.White, ConsoleColor.Black, ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.Cyan) },
                { "Hacker", new ConsoleTheme(ConsoleColor.Green, ConsoleColor.Black, ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.Yellow) },
                { "Ocean", new ConsoleTheme(ConsoleColor.Cyan, ConsoleColor.DarkBlue, ConsoleColor.White, ConsoleColor.Red, ConsoleColor.Blue) },
                { "Vintage", new ConsoleTheme(ConsoleColor.Yellow, ConsoleColor.DarkMagenta, ConsoleColor.Green, ConsoleColor.Red, ConsoleColor.White) },
                { "Nord", new ConsoleTheme(ConsoleColor.Gray, ConsoleColor.DarkBlue, ConsoleColor.Cyan, ConsoleColor.Red, ConsoleColor.White) }
            };
        }

        public string[] GetAvailableThemes()
        {
            return _themes.Keys.ToArray();
        }

        public void ApplyTheme(string themeName)
        {
            if (_themes.TryGetValue(themeName, out ConsoleTheme theme))
            {
                Console.ForegroundColor = theme.TextColor;
                Console.BackgroundColor = theme.BackgroundColor;
                Console.Clear();
            }
        }

        public void ApplyRandomTheme()
        {
            string[] themeNames = GetAvailableThemes();
            var random = new Random();
            ApplyTheme(themeNames[random.Next(themeNames.Length)]);
        }

        public ConsoleTheme GetCurrentTheme(string themeName)
        {
            return _themes.TryGetValue(themeName, out ConsoleTheme theme) ? theme : _themes["Standard"];
        }
    }

    // Class to store theme colors
    public class ConsoleTheme
    {
        public ConsoleColor TextColor { get; }
        public ConsoleColor BackgroundColor { get; }
        public ConsoleColor SuccessColor { get; }
        public ConsoleColor ErrorColor { get; }
        public ConsoleColor HighlightColor { get; }

        public ConsoleTheme(
            ConsoleColor textColor,
            ConsoleColor backgroundColor,
            ConsoleColor successColor,
            ConsoleColor errorColor,
            ConsoleColor highlightColor)
        {
            TextColor = textColor;
            BackgroundColor = backgroundColor;
            SuccessColor = successColor;
            ErrorColor = errorColor;
            HighlightColor = highlightColor;
        }
    }

    // Service for handling ping operations
    public class PingService : IPingService
    {
        public List<PingResult> ExecutePing(string host, int attempts, int bufferSize)
        {
            var results = new List<PingResult>();
            var buffer = new byte[bufferSize];
            var random = new Random();
            random.NextBytes(buffer);

            using (var pingSender = new Ping())
            {
                for (int i = 0; i < attempts; i++)
                {
                    try
                    {
                        // Add a small delay between pings to make it visually interesting
                        if (i > 0) Thread.Sleep(500);

                        PingReply reply = pingSender.Send(host, 1000, buffer);

                        if (reply.Status == IPStatus.Success)
                        {
                            results.Add(new PingResult
                            {
                                IsSuccess = true,
                                Message = $"Svar fra {reply.Address}: bytes={reply.Buffer.Length} tid={reply.RoundtripTime}ms TTL={reply.Options?.Ttl ?? 0}",
                                Time = reply.RoundtripTime
                            });
                        }
                        else
                        {
                            results.Add(new PingResult
                            {
                                IsSuccess = false,
                                Message = $"Fejl: {reply.Status}",
                                Time = 0
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        results.Add(new PingResult
                        {
                            IsSuccess = false,
                            Message = $"Undtagelse: {ex.Message}",
                            Time = 0
                        });
                    }
                }
            }

            return results;
        }
    }

    // File operations service
    public class FileService : IFileService
    {
        public void SaveResults(string filename, List<PingResult> results)
        {
            var builder = new StringBuilder();

            // Add header with date and time
            builder.AppendLine($"=== Ping Resultater - {DateTime.Now} ===");
            builder.AppendLine();

            // Add statistics
            int successCount = results.Count(r => r.IsSuccess);
            builder.AppendLine($"Succesfulde ping: {successCount}/{results.Count} ({(double)successCount / results.Count * 100:F1}%)");

            if (successCount > 0)
            {
                double averageTime = results.Where(r => r.IsSuccess).Average(r => r.Time);
                builder.AppendLine($"Gennemsnitlig svartid: {averageTime:F2}ms");
            }

            builder.AppendLine();

            // Add individual results
            foreach (var result in results)
            {
                builder.AppendLine(result.Message);
            }

            File.WriteAllText(filename, builder.ToString());
        }

        public string LoadFile(string filename)
        {
            if (!File.Exists(filename))
            {
                throw new FileNotFoundException();
            }

            return File.ReadAllText(filename);
        }
    }

    // Enhanced console interface with animations and visual elements
    public class EnhancedConsoleInterface : IUserInterface
    {
        public void DisplayHeader(string header)
        {
            int width = Console.WindowWidth;
            string line = new string('=', width - 1);

            Console.WriteLine();

            // Save the current console color
            ConsoleColor originalColor = Console.ForegroundColor;

            // Change to highlight color for header
            Console.ForegroundColor = ConsoleColor.Cyan;

            Console.WriteLine(line);

            // Center the header text
            string centeredHeader = header.PadLeft(((width - 1) - header.Length) / 2 + header.Length);
            Console.WriteLine(centeredHeader);

            Console.WriteLine(line);

            // Reset back to original color
            Console.ForegroundColor = originalColor;

            Console.WriteLine();
        }

        public void DisplayBanner(string text)
        {
            int width = Console.WindowWidth;
            string topLine = "╔" + new string('═', width - 3) + "╗";
            string bottomLine = "╚" + new string('═', width - 3) + "╝";

            // Save the current console color
            ConsoleColor originalColor = Console.ForegroundColor;

            // Change to highlight color for banner
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine();
            Console.WriteLine(topLine);

            // Center the banner text
            string centeredText = text.PadLeft(((width - 3) - text.Length) / 2 + text.Length);
            Console.WriteLine("║" + centeredText + new string(' ', width - 3 - centeredText.Length) + "║");

            Console.WriteLine(bottomLine);

            // Reset back to original color
            Console.ForegroundColor = originalColor;

            Console.WriteLine();
        }

        public void DisplayMenu(string[] options)
        {
            // Display options with fancy formatting
            ConsoleColor originalColor = Console.ForegroundColor;

            for (int i = 0; i < options.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"  [{i + 1}] ");
                Console.ForegroundColor = originalColor;
                Console.WriteLine(options[i]);
            }

            Console.WriteLine();
        }

        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void DisplayError(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ {message}");
            Console.ForegroundColor = originalColor;
        }

        public void DisplaySuccess(string message)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ {message}");
            Console.ForegroundColor = originalColor;
        }

        public void DisplayAnimation()
        {
            string[] frames = {
                @"
    ____  _                ____                 
   / __ \(_)___  ____ _   / __ \____  ____  ____ 
  / /_/ / / __ \/ __ `/  / /_/ / __ \/ __ \/ __ \
 / ____/ / / / / /_/ /  / ____/ /_/ / / / / / / /
/_/   /_/_/ /_/\__, /  /_/    \____/_/ /_/_/ /_/ 
              /____/                             
",
                @"
    ____  _                ____                 
   / __ \(_)___  ____ _   / __ \____  ____  ____ 
  / /_/ / / __ \/ __ `/  / /_/ / __ \/ __ \/ __ \
 / ____/ / / / / /_/ /  / ____/ /_/ / / / / / / /
/_/   /_/_/ /_/\__, /  /_/    \____/_/ /_/_/ /_/ 
              /____/                             
"
            };

            Console.Clear();

            // Animate the logo
            for (int i = 0; i < 5; i++)
            {
                Console.Clear();
                ConsoleColor originalColor = Console.ForegroundColor;

                Console.ForegroundColor = i % 2 == 0 ? ConsoleColor.Cyan : ConsoleColor.Green;
                Console.WriteLine(frames[i % 2]);

                Console.ForegroundColor = originalColor;
                Thread.Sleep(300);
            }
        }

        public void DisplayPingAnimation()
        {
            string[] frames = {
                "  ◐  Sender ping...",
                "  ◓  Sender ping...",
                "  ◑  Sender ping...",
                "  ◒  Sender ping..."
            };

            // Animate the ping sending
            for (int i = 0; i < 8; i++)
            {
                Console.Write("\r" + frames[i % 4] + new string(' ', Console.WindowWidth - frames[i % 4].Length));
                Thread.Sleep(100);
            }

            Console.WriteLine("\r" + new string(' ', Console.WindowWidth));
        }

        public void DisplayProgressBar(int success, int total)
        {
            int width = 40;
            int filled = (int)Math.Round((double)success / total * width);

            ConsoleColor originalColor = Console.ForegroundColor;

            Console.Write("[");

            // Fill successful part
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(new string('█', filled));

            // Fill failed part
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write(new string('█', width - filled));

            Console.ForegroundColor = originalColor;
            Console.WriteLine($"] {success}/{total} ({(double)success / total * 100:F1}%)");
        }

        public void DisplayStatistics(List<PingResult> results)
        {
            int successCount = results.Count(r => r.IsSuccess);

            Console.WriteLine("\n=== Statistik ===");
            Console.WriteLine($"Sendte pakker: {results.Count}");
            Console.WriteLine($"Modtagne pakker: {successCount}");
            Console.WriteLine($"Tabte pakker: {results.Count - successCount} ({(double)(results.Count - successCount) / results.Count * 100:F1}%)");

            if (successCount > 0)
            {
                double averageTime = results.Where(r => r.IsSuccess).Average(r => r.Time);
                double minimumTime = results.Where(r => r.IsSuccess).Min(r => r.Time);
                double maximumTime = results.Where(r => r.IsSuccess).Max(r => r.Time);

                Console.WriteLine($"Minimums svartid: {minimumTime}ms");
                Console.WriteLine($"Maksimums svartid: {maximumTime}ms");
                Console.WriteLine($"Gennemsnitlig svartid: {averageTime:F2}ms");
            }
        }

        public string GetInput(string prompt)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"{prompt}: ");
            Console.ForegroundColor = originalColor;
            return Console.ReadLine();
        }

        public string GetValidInput(string prompt, Func<string, bool> validator, string errorMessage)
        {
            while (true)
            {
                string input = GetInput(prompt);
                if (validator(input))
                {
                    return input;
                }
                DisplayError(errorMessage);
            }
        }

        public int GetValidNumberInput(string prompt, int min, int max, int defaultValue)
        {
            while (true)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{prompt}: ");
                Console.ForegroundColor = ConsoleColor.White;
                string input = Console.ReadLine();

                if (string.IsNullOrEmpty(input))
                {
                    return defaultValue;
                }

                if (int.TryParse(input, out int result) && result >= min && result <= max)
                {
                    return result;
                }

                DisplayError($"Ugyldigt input. Indtast et tal mellem {min} og {max}.");
            }
        }

        public bool GetYesNoInput(string prompt, string yesOption, string noOption)
        {
            while (true)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{prompt} ({yesOption}/{noOption}): ");
                Console.ForegroundColor = originalColor;
                string input = Console.ReadLine().ToLower();

                if (input == yesOption.ToLower())
                    return true;
                if (input == noOption.ToLower())
                    return false;

                DisplayError($"Brug venligst '{yesOption}' eller '{noOption}'.");
            }
        }
    }

    // Input validation helper
    public static class InputValidator
    {
        public static bool IsValidHostOrIp(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Contains(" "))
            {
                return false;
            }

            // Check if it's a valid IP address
            string[] parts = input.Split('.');
            if (parts.Length == 4)
            {
                bool isValidIp = true;
                foreach (string part in parts)
                {
                    if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                    {
                        isValidIp = false;
                        break;
                    }
                }

                if (isValidIp)
                    return true;
            }

            // Simple domain validation
            return input.Contains('.') &&
                   input.IndexOf('.') != 0 &&
                   input.IndexOf('.') != input.Length - 1;
        }

        public static bool IsValidFilename(string filename)
        {
            return !string.IsNullOrWhiteSpace(filename) &&
                   filename.IndexOfAny(Path.GetInvalidFileNameChars()) < 0;
        }
    }

    // Data model for ping results
    public class PingResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public long Time { get; set; } // Store time for statistics
    }

    // Interfaces for dependency injection and testability

    public interface IPingService
    {
        List<PingResult> ExecutePing(string host, int attempts, int bufferSize);
    }

    public interface IFileService
    {
        void SaveResults(string filename, List<PingResult> results);
        string LoadFile(string filename);
    }

    public interface IUserInterface
    {
        void DisplayHeader(string header);
        void DisplayMenu(string[] options);
        void DisplayMessage(string message);
        void DisplayError(string message);
        void DisplaySuccess(string message);
        void DisplayAnimation();
        void DisplayPingAnimation();
        void DisplayProgressBar(int success, int total);
        void DisplayBanner(string text);
        void DisplayStatistics(List<PingResult> results);
        string GetInput(string prompt);
        string GetValidInput(string prompt, Func<string, bool> validator, string errorMessage);
        int GetValidNumberInput(string prompt, int min, int max, int defaultValue);
        bool GetYesNoInput(string prompt, string yesOption, string noOption);
    }
}