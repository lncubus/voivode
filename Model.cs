using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace voivode
{
    class Model
    {
        public void Load(string source)
        {
            Title = source.Between("<h3>", "</h3>").RemoveTags();
            string table = source.Between("<table class=\"table\">", "</table>").Replace("<tr ", "<tr");
            var lines = table.Enumerate("<tr>", "</tr>");
            Regions = new SortedDictionary<string, List<Figure>>();
            foreach (string line in lines)
            {
                var fields = line.Enumerate("<td>", "</td>").ToArray();
                if (fields.Length != 3)
                    throw new IndexOutOfRangeException();
                string region = fields[0];
                string figure = fields[1];
                string good = fields[2];

                List<Figure> list;
                if (!Regions.TryGetValue(region, out list))
                {
                    list = new List<Figure>();
                    Regions.Add(region, list);
                }
                Figure f = new Figure
                {
                    Piece = figure,
                    Thing = good,
                };
                list.Add(f);
            }
        }

        public string Title;
        public SortedDictionary<string, List<Figure>> Regions;
    }

    public class Figure
    {
        public int Number;
        public string Piece;
        public bool IsDown;
        public string Thing;
        public int Count;
        public string Description;
    }

    public static class Parser
    {
        public static string Between(this string body, string start, string end)
        {
            return Between(body, 0, false, start, end);
        }

        public static string Between(this string body, int position, bool stepBack, string start, string end)
        {
            int startPosition = stepBack ? body.LastIndexOf(start, position, StringComparison.Ordinal) : body.IndexOf(start, position, StringComparison.Ordinal);
            if (startPosition < 0)
                return null;
            startPosition += start.Length;
            int endPosition = body.IndexOf(end, startPosition, StringComparison.Ordinal);
            if (endPosition < 0)
                return null;
            return body.Substring(startPosition, endPosition - startPosition);
        }

        public static IEnumerable<string> Enumerate(this string body, string start, string end)
        {
            int position = 0;
            while (position + start.Length + end.Length <= body.Length)
            {
                int startPosition = body.IndexOf(start, position, StringComparison.Ordinal);
                if (startPosition < 0)
                    yield break;
                startPosition += start.Length;
                int endPosition = body.IndexOf(end, startPosition, StringComparison.Ordinal);
                if (endPosition < 0)
                    yield break;
                position = endPosition + end.Length;
                yield return body.Substring(startPosition, endPosition - startPosition);
            }
        }

        public static string RemoveTags(this string body)
        {
            if (string.IsNullOrEmpty(body) || body.IndexOf('<') < 0)
                return body;
            StringBuilder result = new StringBuilder();
            int bracket = 0;
            foreach (char c in body)
            {
                if (c == '<')
                    bracket++;
                else if (c == '>')
                    bracket--;
                else if (bracket == 0)
                    result.Append(c);
            }
            return result.ToString().Trim();
        }
    }
}
