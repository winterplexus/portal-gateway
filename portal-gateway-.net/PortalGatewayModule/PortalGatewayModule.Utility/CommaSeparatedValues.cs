//
//  CommaSeparatedValues.cs
//
//  Wiregrass Code Technology 2020-2022
//
using System;
using System.Collections;
using System.Text;

namespace PortalGatewayModule.Utility
{
    public class CommaSeparatedValues
    {
        private const char separator = DefaultSeparator;
        private readonly ArrayList list = new ArrayList();

        public const char DefaultSeparator = ',';

        public char Separator { get; set; }

        public CommaSeparatedValues()
        {
            list.Clear();
        }

        public IEnumerator Parse(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }

            list.Clear();

            if (line.Length == 0)
            {
                list.Add(line);
                return list.GetEnumerator();
            }

            var buffer = new StringBuilder();
            var i = 0;

            do
            {
                buffer.Length = 0;
                if ((i < line.Length) && (line[i] == '"'))
                {
                    i = GetIndexNextQuoted(line, buffer, ++i);
                }
                else
                {
                    i = GetIndexNextUnquoted(line, buffer, i);
                }

                list.Add(buffer.ToString());
                i++;
            } while (i < line.Length);

            return list.GetEnumerator();
        }

        protected static int GetIndexNextQuoted(string text, StringBuilder buffer, int index)
        {
            int i;

            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            var len = text.Length;

            for (i = index; i < len; i++)
            {
                if ((text[i] == '"') && (i + 1 < len))
                {
                    if (text[i + 1] == '"')
                    {
                        i++;
                    }
                    else if (text[i + 1] == separator)
                    {
                        i++;
                        break;
                    }
                }
                else if ((text[i] == '"') && (i + 1 == len))
                {
                    break;
                }

                buffer.Append(text[i]);
            }

            return i;
        }

        protected static int GetIndexNextUnquoted(string text, StringBuilder buffer, int index)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            var i = text.IndexOf(separator, index);
            if (i == -1)
            {
                buffer.Append(text.Substring(index));
                return text.Length;
            }

            buffer.Append(text.Substring(index, (i - index)));
            return i;
        }
    }
}