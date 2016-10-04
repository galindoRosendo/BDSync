using System;
using System.IO;

namespace syncBD
{
	class MainClass
	{
		public static void Main (string[] args)
		{
            foreach (var item in args)
            {
                if (item.Equals("--test"))
                {
                    Console.WriteLine("testing mode");
                    
                    string path = Directory.GetCurrentDirectory() + @"\gnditemPJldo1\gnditem20160930.dbf";
                    String[] allLines = File.ReadAllLines(path);
                    for (int i = 0; i < 19; i++)
                    {
                        Console.WriteLine(allLines[i]);
                    }
                    //foreach (var line in allLines)
                    //{
                    //    Console.WriteLine(line);
                    //}
                    Console.ReadKey();
                }
                else
                {
                    syncronizer synker = new syncronizer();
                    synker.run();
                }
            }

		}
	}
}
