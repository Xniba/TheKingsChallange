namespace TheKings_Challange
{
    using Newtonsoft.Json;
    using System;

    internal class Program
    {
        static void Main(string[] args)
        {
            string urlClient = "https://gist.githubusercontent.com/christianpanton/10d65ccef9f29de3acd49d97ed423736/raw/b09563bc0c4b318132c7a738e679d4f984ef0048/kings";

            List<Monarch> monarchs = new List<Monarch>();
            DownloadDatabaseFromClientHttps(urlClient, monarchs).GetAwaiter().GetResult();

            if (monarchs.Count == 0)
            {
                Console.WriteLine("Can't read data form Url or is empty");
                Console.WriteLine("Trying to read data from a file and start program...\n");
                ReadFormFile(monarchs);
            }

            AllInOne(monarchs);
        }

        public class Monarch
        {
            public int Id { get; set; }     // Id
            public string Nm { get; set; }  // Name
            public string Cty { get; set; } // Country
            public string Hse { get; set; } // House
            public string Yrs { get; set; } // Years of reign
        }
        static async Task DownloadDatabaseFromClientHttps(string urlClient, List<Monarch> monarchs)
        {
            try
            {
                monarchs.AddRange(JsonConvert.DeserializeObject<List<Monarch>>(await new HttpClient().GetStringAsync(urlClient)));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
                Console.WriteLine("Can't download Database from url");
            }


            if (monarchs == null)
            {
                Console.WriteLine("Database from url is empty");
            }
        }
        static void ReadFormFile(List<Monarch> monarchs)
        {
            string directoryPath = GetDirectoryPath() + @"\Files";
            string filePath = directoryPath + @"\DataBase.txt";

            monarchs.AddRange(JsonConvert.DeserializeObject<List<Monarch>>(File.ReadAllText(filePath)));
        }
        static void CloseApp()
        {
            Console.WriteLine("Press Enter, to close the window");
            Console.ReadLine();
            Environment.Exit(0);
        }
        static string GetDirectoryPath()
        {
            string path = "";
            try
            {
                path = new DirectoryInfo(".").FullName;
                int ile = path.IndexOf("bin") - 1;

                if (ile < 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("!!!Error in\n" +
                        "GetDirectoryPath - IndexOf less then 0");
                    Console.ResetColor();
                    CloseApp();
                }

                path = path.Substring(0, ile);
            }
            catch
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("!!!Error in\n" +
                    "GetDirectoryPath - Catch");
                Console.ResetColor();
                CloseApp();
            }

            return path;
        }
        static int CalculateReignDuration(string date)
        {
            int duration = 1;
            var years = date.Split('-');
            if (years.Length > 1)
            {
                if (years[1] != "")
                {
                    duration = 1 + Math.Abs(int.Parse(years[0]) - int.Parse(years[1]));
                }
                else
                {
                    duration = 1 + 2022 - int.Parse(years[0]);
                }
            }

            return duration;
        }

        static void AllInOne(List<Monarch> monarchs)
        {
            // I. How many monarchs are there in the list?
            AmountOfKings(monarchs);

            // II. Which monarch ruled the longest (and for how long)?
            YearsOfReginKing(monarchs);

            // III.Which house ruled the longest(and for how long)?
            YearsOfReginHouse(monarchs);

            // IV.What was the most common first name ?
            MostCommonName(monarchs);

        }
        static void AmountOfKings(List<Monarch> monarchs)
        {
            int amountOfKings = monarchs
            .Where(monarchs => !string.IsNullOrEmpty(monarchs.Nm))
            .Count();


            Console.WriteLine("I. Q:How many monarchs are there in the list?");
            Console.WriteLine("I. A:There are {0} monarchs in the list", amountOfKings);
            Console.WriteLine();
        }
        static void YearsOfReginKing(List<Monarch> monarchs)
        {
            var kingWithLongestReign = monarchs
                .Where(monarchs => !string.IsNullOrEmpty(monarchs.Nm))
                .Select(monarch =>
                {
                    return new
                    {
                        Id = monarch.Id,
                        Name = monarch.Nm,
                        Duration = CalculateReignDuration(monarch.Yrs)
                    };
                })
            .OrderByDescending(monarch => monarch.Duration)
            .FirstOrDefault();


            Console.WriteLine("II. Q:Which monarch ruled the longest (and for how long)?");
            Console.WriteLine("II. A:The longest ruled monarch was {0} and reigned for {1} years", kingWithLongestReign.Name, kingWithLongestReign.Duration);
            Console.WriteLine();
        }
        static void YearsOfReginHouse(List<Monarch> monarchs)
        {
            var houseWithLongestReign = monarchs
                .GroupBy(monarch => monarch.Hse)
                .Select(group => new
                {
                    House = group.Key,
                    TotalYears = group.Sum(monarch =>
                    {
                        return CalculateReignDuration(monarch.Yrs);
                    })
                })
                .OrderByDescending(house => house.TotalYears)
                .FirstOrDefault();

            Console.WriteLine("III. Q:Which house ruled the longest(and for how long)?");
            Console.WriteLine("III. A:The longest ruled monarch was {0} and reigned for {1} years", houseWithLongestReign.House, houseWithLongestReign.TotalYears);
            Console.WriteLine();
        }
        static void MostCommonName(List<Monarch> monarchs)
        {
            var mostCommonName = monarchs
           .Where(name => !string.IsNullOrEmpty(name.Nm))
           .Select(name => name.Nm.Split(' ')[0])
           .GroupBy(name => name)
           .OrderByDescending(group => group.Count())
           .FirstOrDefault();

            Console.WriteLine("IV. Q:What was the most common first name ?");
            Console.WriteLine("IV. A:The most common name was {0}", mostCommonName.Key);
            Console.WriteLine();
        }

    }
}