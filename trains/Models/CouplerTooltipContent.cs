using System.Collections.Generic;
using System.Text;

namespace Ca.Jwsm.Railroader.Api.Trains.Models
{
    public sealed class CouplerTooltipContent
    {
        private readonly List<string> _lines = new List<string>();

        public CouplerTooltipContent(string title, string text)
        {
            Title = title ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(text))
            {
                _lines.Add(text);
            }
        }

        public string Title { get; set; }

        public IReadOnlyList<string> Lines
        {
            get { return _lines; }
        }

        public void AppendLine(string line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                _lines.Add(line);
            }
        }

        public string BuildText()
        {
            if (_lines.Count == 0)
            {
                return string.Empty;
            }

            var builder = new StringBuilder();
            for (int i = 0; i < _lines.Count; i++)
            {
                if (i > 0)
                {
                    builder.AppendLine();
                }

                builder.Append(_lines[i]);
            }

            return builder.ToString();
        }
    }
}
