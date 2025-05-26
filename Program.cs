namespace ProgAssi
{
    using System;
    using System.Media;
    using System.Threading;
    using System.Text;
    using System.Collections.Generic;
    using System.Linq;

    // QUESTION 1: Keyword Recognition Implementation
    // Delegate for handling sentiment detection events
    public delegate void SentimentEventHandler(string sentiment, string userInput, string userName);


    // Interface defining the contract for all response handlers
    public interface IResponseHandler
    {
        bool CanHandle(string input);
        void Handle(string input, string userName);
        event SentimentEventHandler SentimentDetected;
    }

    // Base class providing common functionality for all response handlers
    // Implements keyword recognition, sentiment detection, memory, and random responses
    public abstract class ResponseHandlerBase : IResponseHandler
    {
        protected List<string> Keywords { get; set; } = new List<string>();
        protected ConsoleColor ResponseColor { get; set; } = ConsoleColor.White;
        protected Random Random { get; } = new Random();
        protected Dictionary<string, List<string>> ResponseVariations { get; } = new Dictionary<string, List<string>>();

        public event SentimentEventHandler SentimentDetected = delegate { };
        protected Dictionary<string, string> UserMemory { get; private set; } = new Dictionary<string, string>();

        public virtual bool CanHandle(string input)
        {
            return Keywords.Any(keyword => input.Contains(keyword, StringComparison.OrdinalIgnoreCase));
        }

        public abstract void Handle(string input, string userName);

        protected void TypeLine(string text)
        {
            Console.ForegroundColor = ResponseColor;
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(20);
            }
            Console.WriteLine();
            Console.ResetColor();
        }

        // QUESTION 5: Sentiment Detection Implementation
        // Analyzes input for positive/negative sentiment words and triggers event
        protected void CheckForSentiment(string input, string userName)
        {
            var negativeWords = new List<string> { "worried", "scared", "afraid", "nervous", "frustrated" };
            var positiveWords = new List<string> { "happy", "excited", "great", "good", "interested" };

            if (negativeWords.Any(w => input.Contains(w, StringComparison.OrdinalIgnoreCase)))
                SentimentDetected?.Invoke("negative", input, userName);
            else if (positiveWords.Any(w => input.Contains(w, StringComparison.OrdinalIgnoreCase)))
                SentimentDetected?.Invoke("positive", input, userName);
        }

        // QUESTION 4: Memory and Recall Implementation

        protected void Remember(string key, string value)
        {
            if (UserMemory.ContainsKey(key))
                UserMemory[key] = value;
            else
                UserMemory.Add(key, value);
        }

        protected string Recall(string key)
        {
            return UserMemory.ContainsKey(key) ? UserMemory[key] : null;
        }

        public string RecallMemory(string key) => Recall(key);

        // QUESTION 2: Random Responses Implementation

        protected string GetRandomResponse(string variationKey)
        {
            if (ResponseVariations.ContainsKey(variationKey) && ResponseVariations[variationKey].Count > 0)
            {
                var responses = ResponseVariations[variationKey];
                string memoryKey = $"last_response_{variationKey}";
                int lastIndex = Recall(memoryKey) != null ? int.Parse(Recall(memoryKey)) : -1;

                // Filter out the last used response if possible
                var availableResponses = responses
                    .Select((r, i) => new { Response = r, Index = i })
                    .Where(x => x.Index != lastIndex)
                    .ToList();

                // If all responses have been used, reset
                if (availableResponses.Count == 0)
                    availableResponses = responses.Select((r, i) => new { Response = r, Index = i }).ToList();

                var selected = availableResponses[Random.Next(availableResponses.Count)];
                Remember(memoryKey, selected.Index.ToString());

                return selected.Response;
            }
            return string.Empty;
        }
    }

    // Handler implementations with conversation flow
    public class GreetingHandler : ResponseHandlerBase
    {
        public GreetingHandler()
        {
            Keywords = new List<string> { "hello", "hey", "how are you" };
            ResponseColor = ConsoleColor.Green;
            ResponseVariations.Add("greeting", new List<string> {
                $"\nI'm doing well, {{0}}! Ready to help with your cybersecurity questions.",
                $"\nAll systems functioning optimally, {{0}}! How can I assist you today?",
                $"\nHello {{0}}! I'm here and ready to discuss cybersecurity with you.",
                $"\nGreetings {{0}}! Your digital safety is my top priority. What can I help with?",
                $"\nHi there {{0}}! Let's talk about keeping you secure online."

            });
        }

        public override void Handle(string input, string userName)
        {
            CheckForSentiment(input, userName);
            string response = string.Format(GetRandomResponse("greeting"), userName);
            TypeLine(response);
        }
    }

    public class PurposeHandler : ResponseHandlerBase
    {
        public PurposeHandler()
        {
            Keywords = new List<string> { "purpose", "what are you", "why are you here", "function", "Goal" };
            ResponseColor = ConsoleColor.Cyan;
            ResponseVariations.Add("purpose", new List<string> {
                $"\n{{0}}, my purpose is to:\n- Educate about cyber threats\n- Provide safety tips\n- Help recognize scams\n- Promote digital safety",
                $"\n{{0}}, I'm here to:\n• Teach cybersecurity basics\n• Warn about online dangers\n• Suggest protection methods\n• Answer your questions",
                $"\n{{0}}, I was created to:\n1. Increase security awareness\n2. Prevent cyber crimes\n3. Share best practices\n4. Make the web safer",
                $"\n{{0}}, my mission includes:\n~ Cybersecurity education\n~ Threat prevention\n~ Safe browsing guidance\n~ Password protection",
                $"\n{{0}}, I exist to:\n> Identify digital risks\n> Recommend security measures\n> Explain security concepts\n> Help you stay safe online"
            });
        }

        public override void Handle(string input, string userName)
        {
            CheckForSentiment(input, userName);
            string response = string.Format(GetRandomResponse("purpose"), userName);
            TypeLine(response);
        }
    }

    public class PasswordSafetyHandler : ResponseHandlerBase
    {
        public PasswordSafetyHandler()
        {
            Keywords = new List<string> { "password", "passwords", "secure password" };
            ResponseColor = ConsoleColor.Magenta;
            ResponseVariations.Add("main", new List<string> {
                "\nStrong Password Guidelines:\n- Use at least 12 characters\n- Combine uppercase, lowercase, numbers & symbols\n- Avoid personal information\n- Use unique passwords for each account\n\nPro Tip: Use passphrases like 'Coffee@RainyCapeTown2023!'",
                "\nPassword Safety Tips:\n1. Never share your passwords\n2. Change passwords every 3-6 months\n3. Use a password manager\n4. Enable two-factor authentication\n\nRemember, a strong password is your first defense!",
                "\nCreating Secure Passwords:\n• Length is more important than complexity\n• Avoid dictionary words\n• Consider using the first letters of a sentence\n• Example: 'I love hiking in Table Mountain every Sunday!' becomes 'IlhiTMeS!'",
            "\nPassword Security Facts:\n- 80% of hacking breaches involve weak passwords\n- Adding just one special character makes a password 10x harder to crack\n- The most common password is still '123456' - don't be that person!",
            "\nAdvanced Password Tips:\n1. Use a different password for every account\n2. Consider using passwordless authentication where available\n3. Regularly change your password to ensure ultimate safety"
                });
        }

        public override void Handle(string input, string userName)
        {
            CheckForSentiment(input, userName);
            Remember("last_topic", "password safety");
            string response;
            if (Recall("interest") == "password safety")
            {
                response = $"Since you're interested in password safety, {userName}, here's more:\n" +
                          GetRandomResponse("main");
            }
            else
            {
                response = GetRandomResponse("main");
            }

            TypeLine(response);

            if (input.Contains("remember", StringComparison.OrdinalIgnoreCase))
            {
                Remember("interest", "password safety");
                TypeLine($"\nI'll remember you're interested in password safety, {userName}!");
            }
        }
    }

    public class PhishingHandler : ResponseHandlerBase
    {
        public PhishingHandler()
        {
            Keywords = new List<string> { "phishing", "scam", "email scam" };
            ResponseColor = ConsoleColor.Magenta;
            ResponseVariations.Add("main", new List<string> {
                "\nHow to Spot Phishing Attempts:\n- Check sender email addresses carefully\n- Hover over links to see actual URLs\n- Look for poor grammar/spelling\n- Be wary of urgent requests\n- Verify unexpected attachments\n\nREMEMBER banks will NEVER ask for your PIN via email/SMS",
                "\nPhishing Red Flags:\n1. Generic greetings like 'Dear Customer'\n2. Threats of account closure\n3. Requests for immediate action\n4. Suspicious attachments\n\nWhen in doubt, contact the organization directly!",
                "\nAnti-Phishing Tips:\n• Don't click links in unsolicited emails\n• Bookmark important sites instead of clicking links\n• Check for HTTPS in URLs\n• Keep your browser updated",
                "\nAdvanced Phishing Defense:\n~ Enable email filtering\n~ Use email authentication \n~ Verify sender phone numbers independently\n~ Report phishing attempts to your IT department",
            "\nPhishing Statistics:\n- 90% of data breaches start with phishing\n- Employees receive 14 malicious emails per year on average\n- Spear phishing accounts for 65% of targeted attacks"
            });
        }

        public override void Handle(string input, string userName)
        {
            CheckForSentiment(input, userName);
            Remember("last_topic", "phishing");
            string response = Recall("important_topic") == "phishing"
        ? $"{userName}, since phishing is important to you:\n{GetRandomResponse("main")}"
        : GetRandomResponse("main");

            TypeLine(response);

            if (input.Contains("remember", StringComparison.OrdinalIgnoreCase))
            {
                Remember("important_topic", "phishing");
                TypeLine($"\nI'll remember that phishing is an important topic for you, {userName}!");
            }
        }
    }

    public class BrowsingHandler : ResponseHandlerBase
    {
        public BrowsingHandler()
        {
            Keywords = new List<string> { "browsing", "safe internet", "https" };
            ResponseColor = ConsoleColor.Magenta;
            ResponseVariations.Add("main", new List<string> {
                "\nSafe Browsing Practices:\n- Always look for HTTPS in website URLs\n- Keep your browser and plugins updated\n- Use a reputable antivirus program\n- Avoid public Wi-Fi for sensitive transactions\n- Clear browser cache and cookies regularly",
                "\nInternet Safety Tips:\n1. Verify website security before entering credentials\n2. Use ad-blockers to avoid malicious ads\n3. Be cautious with downloads\n4. Check privacy policies of websites",
                "\nSecure Web Browsing:\n• Use privacy-focused browsers when possible\n• Enable phishing protection in your browser\n• Review extension permissions regularly\n• Consider using a separate browser for financial transactions",
            "\nBrowser Security Enhancements:\n~ Enable automatic updates\n~ Use secure DNS \n~ Disable Flash and Java plugins\n~ Regularly clear browsing data",
            "\nDangerous Online Behaviors to Avoid:\n- Using the same password across sites\n- Ignoring browser security warnings\n- Downloading software from untrusted sources\n- Clicking 'Remember me' on public computers"
                });
        }

        public override void Handle(string input, string userName)
        {
            CheckForSentiment(input, userName);
            Remember("last_topic", "safe browsing");
            TypeLine(GetRandomResponse("main"));
        }
    }

    public class SocialEngineeringHandler : ResponseHandlerBase
    {
        public SocialEngineeringHandler()
        {
            Keywords = new List<string> { "social engineering", "phone scam", "impersonation" };
            ResponseColor = ConsoleColor.Magenta;
            ResponseVariations.Add("main", new List<string> {
                "\nSocial Engineering Awareness:\n- Never share sensitive information over the phone\n- Verify identities before granting access\n- Be cautious of 'too good to be true' offers\n- Don't plug in unknown USB devices\n- Report suspicious requests to your IT department",
                "\nAvoiding Social Engineering:\n1. Question unexpected requests for information\n2. Verify unusual requests through another channel\n3. Be wary of urgent or threatening language\n4. Educate family members about common scams",
                "\nCommon Social Engineering Tactics:\n• Pretexting (creating fake scenarios)\n• Baiting (offering something enticing)\n• Quid pro quo (offering something in exchange)\n• Tailgating (following into secure areas)",
           "\nReal-World Social Engineering Examples:\n- Tech support scams claiming your computer is infected\n- Fake IT staff asking for your password\n- 'CEO fraud' emails requesting urgent wire transfers\n- Fake job offers requesting personal information",
            "\nPsychological Triggers Exploited:\n~ Authority (pretending to be someone important)\n~ Urgency (creating time pressure)\n~ Familiarity (pretending to know you)\n~ Social proof (claiming others have complied)"
                });
        }

        public override void Handle(string input, string userName)
        {
            CheckForSentiment(input, userName);
            Remember("last_topic", "social engineering");
            string response = Recall("last_social_engineering_response") != null
        ? $"{userName}, building on our previous talk about social engineering:\n{GetRandomResponse("main")}"
        : GetRandomResponse("main");

            TypeLine(response);
            Remember("last_social_engineering_response", "true");
        }
    }

    public class TwoFactorAuthHandler : ResponseHandlerBase
    {
        public TwoFactorAuthHandler()
        {
            Keywords = new List<string> { "2fa", "two factor", "authentication" };
            ResponseColor = ConsoleColor.Magenta;
            ResponseVariations.Add("main", new List<string> {
                "\nTwo-Factor Authentication (2FA):\n- Always enable 2FA where available\n- Use authenticator apps instead of SMS when possible\n- Keep backup codes in a secure place\n- Consider hardware security keys for high-value accounts\n- Review authorized devices regularly",
                "\nBenefits of 2FA:\n1. Adds an extra layer of security beyond passwords\n2. Protects against credential stuffing attacks\n3. Can prevent unauthorized access even if password is compromised\n4. Available on most major platforms",
                "\nImplementing 2FA:\n• Use apps like Google Authenticator \n• Set up backup methods in case you lose your primary\n• Be cautious of 2FA code requests (could be phishing)\n• Consider using biometric 2FA where available",
            "\n2FA Statistics:\n- Reduces account takeover risk by 99.9%\n- Only 28% of users enable it when available\n- SMS 2FA is better than nothing but vulnerable to SIM swapping\n- Push notification 2FA has the highest user adoption",
            "\nAdvanced 2FA Tips:\n~ Use different 2FA methods for different accounts\n~ Store backup codes in a password manager\n~ Register multiple devices for critical accounts\n~ Periodically review active 2FA sessions"
                });
        }

        public override void Handle(string input, string userName)
        {
            CheckForSentiment(input, userName);
            Remember("last_topic", "two-factor authentication");
            string interest = Recall("2fa_interest");
            string response = interest == "yes"
                ? $"{userName}, since you asked about 2FA before:\n{GetRandomResponse("main")}"
                : GetRandomResponse("main");

            TypeLine(response);

            if (input.Contains("remember", StringComparison.OrdinalIgnoreCase))
            {
                Remember("2fa_interest", "yes");
                TypeLine($"\nI'll remember your interest in two-factor authentication, {userName}!");
            }
        }
    }

    // QUESTION 6: Error Handling Implementation

    public class DefaultHandler : ResponseHandlerBase
    {
        public DefaultHandler()
        {
            Keywords = new List<string>();
            ResponseVariations.Add("default", new List<string> {
                "\nI didn't quite understand that. Could you rephrase?",
                "\nI'm not sure I follow. Can you try asking differently?",
                "\nThat's not something I'm programmed to handle. Try asking about cybersecurity topics."
            });
        }

        public override bool CanHandle(string input) => true;

        public override void Handle(string input, string userName)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            TypeLine(GetRandomResponse("default"));
            TypeLine("Try asking about: passwords, phishing, safe browsing, social engineering, or 2FA.");
            Console.ResetColor();
        }
    }

    // QUESTION 5: Enhanced Sentiment Response Handler

    public class SentimentResponseHandler
    {
        private readonly Dictionary<string, Action<string>> _sentimentResponses;

        public SentimentResponseHandler()
        {
            _sentimentResponses = new Dictionary<string, Action<string>>
            {
                ["negative"] = (name) => {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine($"\nI understand this might feel overwhelming, {name}. Let's take it one step at a time.");
                    Console.ResetColor();
                },
                ["positive"] = (name) => {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nGreat to see your enthusiasm about cybersecurity, {name}!");
                    Console.ResetColor();
                }
            };
        }

        public void HandleSentiment(string sentiment, string userInput, string userName)
        {
            _sentimentResponses[sentiment]?.Invoke(userName);
        }
    }

    // QUESTION 7: Code Optimization Implementation

    class Program
    {
        private static List<IResponseHandler> responseHandlers;
        private static SentimentResponseHandler sentimentHandler;

        static void Main()
        {
            PlayWelcomeMessage();
            InitializeResponseHandlers();
            DisplayAnimatedAsciiArt();
            string userName = GetUserName();

            bool continueChat = true;
            while (continueChat)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"\n{userName}, what would you like to know about cybersecurity? (or type 'exit' to quit): ");
                Console.ResetColor();

                string input = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(input))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nPlease enter a question or type 'exit' to quit.");
                    Console.ResetColor();
                    continue;
                }

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    continueChat = false;
                    continue;
                }

                // Handle follow-up questions (Question 3: Conversation Flow)
                if (input.Contains("more") && responseHandlers.FirstOrDefault() is ResponseHandlerBase baseHandler)
                {
                    string lastTopic = baseHandler.RecallMemory("last_topic");
                    if (!string.IsNullOrEmpty(lastTopic))
                    {
                        Console.WriteLine($"\nContinuing our discussion about {lastTopic}...");
                        input = lastTopic;

                        string interest = baseHandler.RecallMemory("interest");
                        if (!string.IsNullOrEmpty(interest))
                        {
                            Console.WriteLine($"I remember you were particularly interested in {interest}...");
                        }
                    }
                }

                var handler = responseHandlers.FirstOrDefault(h => h.CanHandle(input)) ?? responseHandlers.Last();
                handler.Handle(input, userName);
            }

            DisplayGoodbyeMessage(userName);
        }

        // Initializes all response handlers and connects sentiment detection
        private static void InitializeResponseHandlers()
        {
            sentimentHandler = new SentimentResponseHandler();
            responseHandlers = new List<IResponseHandler>
            {
                new GreetingHandler(),
                new PurposeHandler(),
                new PasswordSafetyHandler(),
                new PhishingHandler(),
                new BrowsingHandler(),
                new SocialEngineeringHandler(),
                new TwoFactorAuthHandler(),
                new DefaultHandler()
            };


            foreach (var handler in responseHandlers.OfType<ResponseHandlerBase>())
            {
                handler.SentimentDetected += sentimentHandler.HandleSentiment;
            }
        }

        static void PlayWelcomeMessage()
        {
            try
            {
                Console.WriteLine("\nInitializing cybersecurity assistant...\n");
                using (SoundPlayer player = new SoundPlayer("C:\\Users\\lab_services_student\\source\\repos\\ProgAssi\\Audio\\Welcome.wav"))
                {
                    player.Play();
                    Thread.Sleep(3000); // Allow audio to play
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Audio note: " + ex.Message);
            }
        }

        static void DisplayAnimatedAsciiArt()
        {
            string[] colors = { "Red", "Green", "Blue", "Yellow", "Cyan", "Magenta" };
            int colorIndex = 0;

            Console.WriteLine("\nLoading cybersecurity protocols...\n");

            for (int i = 0; i < 5; i++)
            {
                Console.ForegroundColor = (ConsoleColor)Enum.Parse(
                    typeof(ConsoleColor), colors[colorIndex % colors.Length]);

                switch (i % 4)
                {
                    case 0:
                        Console.WriteLine(@"
  ██████╗ ██╗   ██╗██████╗ ███████╗██████╗ 
 ██╔════╝ ╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗
 ██║       ╚████╔╝ ██████╔╝█████╗  ██████╔╝
 ██║        ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗
 ╚██████╗    ██║   ██████ ║███████╗██║  ██║
  ╚═════╝    ╚═╝   ╚══════╝╚══════╝╚═╝  ╚═╝
            ");
                        break;
                    case 1:
                        Console.WriteLine(@"
  ██████╗ ██╗   ██╗██████╗ ███████╗██████╗ 
 ██╔════╝ ╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗
 ██║       ╚████╔╝ ██████╔╝█████╗  ██████╔╝
 ██║        ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗
 ╚██████╗    ██║   ██████ ║███████╗██║  ██║
  ╚═════╝    ╚═╝   ╚══════╝╚══════╝╚═╝  ╚═╝
 ███████╗███████╗ ██████╗██╗   ██╗██████╗ ██╗████████╗██╗   ██╗
 ██╔════╝██╔════╝██╔════╝██║   ██║██╔══██╗██║╚══██╔══╝╚██╗ ██╔╝
 ███████╗█████╗  ██║     ██║   ██║██████╔╝██║   ██║    ╚████╔╝ 
 ╚════██║██╔══╝  ██║     ██║   ██║██╔══██╗██║   ██║     ╚██╔╝  
 ███████║███████╗╚██████╗╚██████╔╝██║  ██║██║   ██║      ██║   
 ╚══════╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝  ╚═╝╚═╝   ╚═╝      ╚═╝   
            ");
                        break;
                    case 2:
                        Console.WriteLine(@"
  ██████╗ ██╗   ██╗██████╗ ███████╗██████╗ 
 ██╔════╝ ╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗
 ██║       ╚████╔╝ ██████╔╝█████╗  ██████╔╝
 ██║        ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗
 ╚██████╗    ██║   ██████ ║███████╗██║  ██║
  ╚═════╝    ╚═╝   ╚══════╝╚══════╝╚═╝  ╚═╝
 ███████╗███████╗ ██████╗██╗   ██╗██████╗ ██╗████████╗██╗   ██╗
 ██╔════╝██╔════╝██╔════╝██║   ██║██╔══██╗██║╚══██╔══╝╚██╗ ██╔╝
 ███████╗█████╗  ██║     ██║   ██║██████╔╝██║   ██║    ╚████╔╝ 
 ╚════██║██╔══╝  ██║     ██║   ██║██╔══██╗██║   ██║     ╚██╔╝  
 ███████║███████╗╚██████╗╚██████╔╝██║  ██║██║   ██║      ██║   
 ╚══════╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝  ╚═╝╚═╝   ╚═╝      ╚═╝   
  ██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗
  ╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝
            ");
                        break;
                    case 3:
                        Console.WriteLine(@"
  ██████╗ ██╗   ██╗██████╗ ███████╗██████╗ 
 ██╔════╝ ╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗
 ██║       ╚████╔╝ ██████╔╝█████╗  ██████╔╝
 ██║        ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗
 ╚██████╗    ██║   ██████ ║███████╗██║  ██║
  ╚═════╝    ╚═╝   ╚══════╝╚══════╝╚═╝  ╚═╝
 ███████╗███████╗ ██████╗██╗   ██╗██████╗ ██╗████████╗██╗   ██╗
 ██╔════╝██╔════╝██╔════╝██║   ██║██╔══██╗██║╚══██╔══╝╚██╗ ██╔╝
 ███████╗█████╗  ██║     ██║   ██║██████╔╝██║   ██║    ╚████╔╝ 
 ╚════██║██╔══╝  ██║     ██║   ██║██╔══██╗██║   ██║     ╚██╔╝  
 ███████║███████╗╚██████╗╚██████╔╝██║  ██║██║   ██║      ██║   
 ╚══════╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝  ╚═╝╚═╝   ╚═╝      ╚═╝   
 ██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗
 ╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝
 !!!CYBERSECURITY!!!CYBERSECURITY!!!CYBERSECURITY!!!
            ");
                        break;
                }

                Thread.Sleep(300);
                Console.Clear();
                colorIndex++;
            }

            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(@"
   ██████╗ ██╗   ██╗██████╗ ███████╗██████╗ 
 ██╔════╝ ╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗
 ██║       ╚████╔╝ ██████╔╝█████╗  ██████╔╝
 ██║        ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗
 ╚██████╗    ██║   ██████ ║███████╗██║  ██║
  ╚═════╝    ╚═╝   ╚══════╝╚══════╝╚═╝  ╚═╝
 ███████╗███████╗ ██████╗██╗   ██╗██████╗ ██╗████████╗██╗   ██╗
 ██╔════╝██╔════╝██╔════╝██║   ██║██╔══██╗██║╚══██╔══╝╚██╗ ██╔╝
 ███████╗█████╗  ██║     ██║   ██║██████╔╝██║   ██║    ╚████╔╝ 
 ╚════██║██╔══╝  ██║     ██║   ██║██╔══██╗██║   ██║     ╚██╔╝  
 ███████║███████╗╚██████╗╚██████╔╝██║  ██║██║   ██║      ██║   
 ╚══════╝╚══════╝ ╚═════╝ ╚═════╝ ╚═╝  ╚═╝╚═╝   ╚═╝      ╚═╝   
 ██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗██╗
 ╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝╚═╝
 !!!CYBERSECURITY!!!CYBERSECURITY!!!CYBERSECURITY!!!
");
        }

        static string GetUserName()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║                                        ║");
            Console.WriteLine("║  Welcome to Ashley's Cybersecurity Bot ║");
            Console.WriteLine("║                                        ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.ResetColor();

            Console.Write("\nTo personalize your experience, please tell me your name: ");
            string name = Console.ReadLine();

            while (string.IsNullOrWhiteSpace(name))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nThe name cannot be empty. Let's try that again. :)");
                Console.ResetColor();
                Console.WriteLine("Please enter your name: ");
                name = Console.ReadLine();
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nNice to meet you, {name}! I'm here to help you stay safe online.");
            Console.ResetColor();

            return name;
        }

        static void DisplayGoodbyeMessage(string userName)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (char c in $"Thank you for using Ashley's Cybersecurity Bot, {userName}!")
            {
                Console.Write(c);
                Thread.Sleep(20);
            }
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.DarkRed;
            foreach (char c in "Remember to stay vigilant online and protect your digital life.")
            {
                Console.Write(c);
                Thread.Sleep(20);
            }
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("GoodBye!!");
            Console.ResetColor();
        }
    }
}

