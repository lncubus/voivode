using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace voivode
{
    public class MapInfo
    {
        public readonly Dictionary<string, CityInfo> cities = new Dictionary<string, CityInfo>();

		public MapInfo(string filename)
        {
            string[] ini = File.ReadAllLines(filename);
            CityInfo city = null;
            foreach (string line in ini)
            {
                if (string.IsNullOrEmpty(line))
                    continue;
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    // begin new section
                    string name = line.Substring(1, line.Length - 2);
                    Image image = Image.FromFile(name);
                    name = Path.GetFileNameWithoutExtension(name);
                    city = new CityInfo { image = image };
                    cities.Add(name, city);
                    continue;
                }
                if (!line.Contains("=") || city == null)
                    continue;
                string[] parts = line.Split(new[] {'=', ','}, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 5)
                    continue;
                for (int i = 0; i < parts.Length; i++)
                    parts[i] = parts[i].Trim();
                int x = int.Parse(parts[1]);
                int y = int.Parse(parts[2]);
				int w = int.Parse(parts[3]);
				if (!parts[3].StartsWith("+"))
					w -= x;
				int h = int.Parse(parts[4]);
				if (!parts[4].StartsWith("+"))
					h -= y;
                Rectangle rect = new Rectangle(x, y, w, h);
                city.regions.Add(parts[0], rect);
            }
        }
    }

    public class CityInfo
    {
        public Dictionary<string, Rectangle> regions = new Dictionary<string, Rectangle>();
        public Image image;
    }
}
