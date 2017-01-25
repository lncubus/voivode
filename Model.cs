using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace voivode
{
    class Model
    {
        public const string FigureIsDown = "(повалена)";
        public const string FigureIsCaptured = "(в плену)";
        public const string GreenFigure = "class=\"success\"";
        public const string RedFigure = "class=\"danger\"";

        public const string AlertStart = "<div class=\"alert";
        public const string AlertEnd = "</div>";

        public void Load(string source)
        {
            Title = source.Between("<h3>", "</h3>").RemoveTags();
            var alert = source.Between(AlertStart, AlertEnd);
            if (string.IsNullOrEmpty(alert))
                Alert = null;
            else
                Alert = (AlertStart + alert).RemoveTags();
            Regions.Clear();
            string table = source.Replace(" >", ">").Between("<table ", "</table>");
			if (string.IsNullOrEmpty(table))
				return;
			table = table.Replace("<tr ", "<tr").Replace("<td ", "<td");
            var lines = table.Enumerate("<tr", "</tr>");
            string region = null;
            foreach (string line in lines)
            {
                bool mine = line.Contains(GreenFigure) || line.Contains(RedFigure);
                var fields = line.Enumerate("<td>", "</td>").ToArray();
                if (fields.Length != 3)
                    throw new IndexOutOfRangeException();
                string new_region = fields[0].Trim();
                if (!string.IsNullOrEmpty(new_region))
                    region = new_region;
                string figure = fields[1].Trim();
                string good = fields[2].Trim();

                List<Figure> list;
                if (!Regions.TryGetValue(region, out list))
                {
                    list = new List<Figure>();
                    Regions.Add(region, list);
                }
                string piece = null;
                string number = null;
                bool down = false;
                bool captured = false;
                if (figure.Length > 3)
                {
                    if (figure.Contains(FigureIsCaptured))
                    {
                        captured = true;
                        figure = figure.Replace(FigureIsCaptured, string.Empty).Trim();
                    }
                    if (figure.Contains(FigureIsDown))
                    {
                        down = true;
                        figure = figure.Replace(FigureIsDown, string.Empty).Trim();
                    }
                    string[] parts = figure.Split('-');
                    number = parts[0];
                    piece = parts[1];
                    int brace = piece.IndexOf("(");
                    if (brace >= 0)
                        piece = piece.Remove(brace).Trim();
                }
                string thing = null;
                int count = 0;
                string description = null;
                if (good.Length > 3)
                {
                    thing = good.Between(string.Empty, "(").Trim();
                    description = good.Between("\"", "\"");
                    count = int.Parse(good.Between("(", "шт.)").Trim());
                    if (!string.IsNullOrEmpty(description))
                        description = description.Trim();
                }
                if (string.IsNullOrEmpty(thing) && string.IsNullOrEmpty(figure))
                    continue;
                Figure f = new Figure
                {
                    Mine = mine,
                    Piece = piece,
                    Number = number,
                    IsDown = down,
                    IsCaptured = captured,
                    Thing = thing,
                    Count = count,
                    Description = description,
                };
                list.Add(f);
            }
        }

        public string Title;
        public string Alert;
        public SortedDictionary<string, List<Figure>> Regions = new SortedDictionary<string, List<Figure>>();
    }

    public class Figure
    {
        public bool Mine;
        public string Number;
        public string Piece;
        public bool IsDown;
        public bool IsCaptured;
        public string Thing;
        public int Count;
        public string Description;

        public override string ToString()
        {
            List<string> parts = new List<string>();
            if (!string.IsNullOrEmpty(Number))
            {
                parts.Add(Number + "-" + Piece);
                if (IsDown)
                    parts.Add(Model.FigureIsDown);
                if (IsCaptured)
                    parts.Add(Model.FigureIsCaptured);
            }
            if (!string.IsNullOrEmpty(Thing))
            {
                parts.Add(Thing);
                if (Count > 0)
                    parts.Add("(" + Count + " шт.)");
                if (!string.IsNullOrEmpty(Description))
                    parts.Add("\"" + Description + "\"");
            }
            if (!parts.Any())
                return string.Empty;
            return string.Join(" ", parts.ToArray());
        }
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
