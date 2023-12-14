#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

#endregion

namespace NebulaModel.Utils;
// Example usage:
//
//  using UnityEngine;
//  using System.Collections;
//  using System.Collections.Generic;
//  using MiniJSON;
//
//  public class MiniJSONTest : MonoBehaviour {
//      void Start () {
//          var jsonString = "{ \"array\": [1.44,2,3], " +
//                          "\"object\": {\"key1\":\"value1\", \"key2\":256}, " +
//                          "\"string\": \"The quick brown fox \\\"jumps\\\" over the lazy dog \", " +
//                          "\"unicode\": \"\\u3041 Men\u00fa sesi\u00f3n\", " +
//                          "\"int\": 65536, " +
//                          "\"float\": 3.1415926, " +
//                          "\"bool\": true, " +
//                          "\"null\": null }";
//
//          var dict = Json.Deserialize(jsonString) as Dictionary<string,object>;
//
//          Debug.Log("deserialized: " + dict.GetType());
//          Debug.Log("dict['array'][0]: " + ((List<object>) dict["array"])[0]);
//          Debug.Log("dict['string']: " + (string) dict["string"]);
//          Debug.Log("dict['float']: " + (double) dict["float"]); // floats come out as doubles
//          Debug.Log("dict['int']: " + (long) dict["int"]); // ints come out as longs
//          Debug.Log("dict['unicode']: " + (string) dict["unicode"]);
//
//          var str = Json.Serialize(dict);
//
//          Debug.Log("serialized: " + str);
//      }
//  }

/// <summary>
///     This class encodes and decodes JSON strings.
///     Spec. details, see http://www.json.org/
///     JSON uses Arrays and Objects. These correspond here to the datatypes IList and IDictionary.
///     All numbers are parsed to doubles.
/// </summary>
public static class MiniJson
{
    /// <summary>
    ///     Parses the string json into a value
    /// </summary>
    /// <param name="json">A JSON string.</param>
    /// <returns>An List&lt;object&gt;, a Dictionary&lt;string, object&gt;, a double, an integer,a string, null, true, or false</returns>
    public static object Deserialize(string json)
    {
        // save the string for debug information
        return json == null ? null : Parser.Parse(json);
    }

    /// <summary>
    ///     Converts a IDictionary / IList object or a simple type (string, int, etc.) into a JSON string
    /// </summary>
    /// <param name="obj">A Dictionary&lt;string, object&gt; / List&lt;object&gt;</param>
    /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
    public static string Serialize(object obj)
    {
        return Serializer.Serialize(obj);
    }

    private sealed class Parser : IDisposable
    {
        private const string WORD_BREAK = "{}[],:\"";

        private StringReader json;

        private Parser(string jsonString)
        {
            json = new StringReader(jsonString);
        }

        private char PeekChar => Convert.ToChar(json.Peek());

        private char NextChar => Convert.ToChar(json.Read());

        private string NextWord
        {
            get
            {
                var word = new StringBuilder();

                while (!IsWordBreak(PeekChar))
                {
                    word.Append(NextChar);

                    if (json.Peek() == -1)
                    {
                        break;
                    }
                }

                return word.ToString();
            }
        }

        private TOKEN NextToken
        {
            get
            {
                EatWhitespace();

                if (json.Peek() == -1)
                {
                    return TOKEN.NONE;
                }

                switch (PeekChar)
                {
                    case '{':
                        return TOKEN.CURLY_OPEN;
                    case '}':
                        json.Read();
                        return TOKEN.CURLY_CLOSE;
                    case '[':
                        return TOKEN.SQUARED_OPEN;
                    case ']':
                        json.Read();
                        return TOKEN.SQUARED_CLOSE;
                    case ',':
                        json.Read();
                        return TOKEN.COMMA;
                    case '"':
                        return TOKEN.STRING;
                    case ':':
                        return TOKEN.COLON;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                        return TOKEN.NUMBER;
                }

                return NextWord switch
                {
                    "false" => TOKEN.FALSE,
                    "true" => TOKEN.TRUE,
                    "null" => TOKEN.NULL,
                    _ => TOKEN.NONE
                };
            }
        }

        public void Dispose()
        {
            json.Dispose();
            json = null;
        }

        private static bool IsWordBreak(char c)
        {
            return char.IsWhiteSpace(c) || WORD_BREAK.IndexOf(c) != -1;
        }

        public static object Parse(string jsonString)
        {
            using var instance = new Parser(jsonString);
            return instance.ParseValue();
        }

        private Dictionary<string, object> ParseObject()
        {
            var table = new Dictionary<string, object>();

            // ditch opening brace
            json.Read();

            // {
            while (true)
            {
                switch (NextToken)
                {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.CURLY_CLOSE:
                        return table;
                    case TOKEN.CURLY_OPEN:
                    case TOKEN.SQUARED_OPEN:
                    case TOKEN.SQUARED_CLOSE:
                    case TOKEN.COLON:
                    case TOKEN.STRING:
                    case TOKEN.NUMBER:
                    case TOKEN.TRUE:
                    case TOKEN.FALSE:
                    case TOKEN.NULL:
                    default:
                        // name
                        var name = ParseString();
                        if (name == null)
                        {
                            return null;
                        }

                        // :
                        if (NextToken != TOKEN.COLON)
                        {
                            return null;
                        }
                        // ditch the colon
                        json.Read();

                        // value
                        table[name] = ParseValue();
                        break;
                }
            }
        }

        private List<object> ParseArray()
        {
            var array = new List<object>();

            // ditch opening bracket
            json.Read();

            // [
            var parsing = true;
            while (parsing)
            {
                var nextToken = NextToken;

                switch (nextToken)
                {
                    case TOKEN.NONE:
                        return null;
                    case TOKEN.COMMA:
                        continue;
                    case TOKEN.SQUARED_CLOSE:
                        parsing = false;
                        break;
                    case TOKEN.CURLY_OPEN:
                    case TOKEN.CURLY_CLOSE:
                    case TOKEN.SQUARED_OPEN:
                    case TOKEN.COLON:
                    case TOKEN.STRING:
                    case TOKEN.NUMBER:
                    case TOKEN.TRUE:
                    case TOKEN.FALSE:
                    case TOKEN.NULL:
                    default:
                        var value = ParseByToken(nextToken);

                        array.Add(value);
                        break;
                }
            }

            return array;
        }

        private object ParseValue()
        {
            var nextToken = NextToken;
            return ParseByToken(nextToken);
        }

        private object ParseByToken(TOKEN token)
        {
            return token switch
            {
                TOKEN.STRING => ParseString(),
                TOKEN.NUMBER => ParseNumber(),
                TOKEN.CURLY_OPEN => ParseObject(),
                TOKEN.SQUARED_OPEN => ParseArray(),
                TOKEN.TRUE => true,
                TOKEN.FALSE => false,
                TOKEN.NULL => null,
                _ => null
            };
        }

        private string ParseString()
        {
            var s = new StringBuilder();

            // ditch opening quote
            json.Read();

            var parsing = true;
            while (parsing)
            {
                if (json.Peek() == -1)
                {
                    break;
                }

                var c = NextChar;
                switch (c)
                {
                    case '"':
                        parsing = false;
                        break;
                    case '\\':
                        if (json.Peek() == -1)
                        {
                            parsing = false;
                            break;
                        }

                        c = NextChar;
                        switch (c)
                        {
                            case '"':
                            case '\\':
                            case '/':
                                s.Append(c);
                                break;
                            case 'b':
                                s.Append('\b');
                                break;
                            case 'f':
                                s.Append('\f');
                                break;
                            case 'n':
                                s.Append('\n');
                                break;
                            case 'r':
                                s.Append('\r');
                                break;
                            case 't':
                                s.Append('\t');
                                break;
                            case 'u':
                                var hex = new char[4];

                                for (var i = 0; i < 4; i++)
                                {
                                    hex[i] = NextChar;
                                }

                                s.Append((char)Convert.ToInt32(new string(hex), 16));
                                break;
                        }
                        break;
                    default:
                        s.Append(c);
                        break;
                }
            }

            return s.ToString();
        }

        private object ParseNumber()
        {
            var number = NextWord;

            if (number.IndexOf('.') == -1)
            {
                if (long.TryParse(number, out var parsedInt))
                {
                    return parsedInt;
                }
            }

            if (double.TryParse(number, out var parsedDouble))
            {
                return parsedDouble;
            }

            return null;
        }

        private void EatWhitespace()
        {
            while (char.IsWhiteSpace(PeekChar))
            {
                json.Read();

                if (json.Peek() == -1)
                {
                    break;
                }
            }
        }

        private enum TOKEN
        {
            NONE,
            CURLY_OPEN,
            CURLY_CLOSE,
            SQUARED_OPEN,
            SQUARED_CLOSE,
            COLON,
            COMMA,
            STRING,
            NUMBER,
            TRUE,
            FALSE,
            NULL
        }
    }

    private sealed class Serializer
    {
        private readonly StringBuilder builder;

        private Serializer()
        {
            builder = new StringBuilder();
        }

        public static string Serialize(object obj)
        {
            var instance = new Serializer();

            instance.SerializeValue(obj);

            return instance.builder.ToString();
        }

        private void SerializeValue(object value)
        {
            IList asList;
            IDictionary asDict;
            string asStr;

            if (value == null)
            {
                builder.Append("null");
            }
            else if ((asStr = value as string) != null)
            {
                SerializeString(asStr);
            }
            else if (value is bool b)
            {
                builder.Append(b ? "true" : "false");
            }
            else if ((asList = value as IList) != null)
            {
                SerializeArray(asList);
            }
            else if ((asDict = value as IDictionary) != null)
            {
                SerializeObject(asDict);
            }
            else if (value is char c)
            {
                SerializeString(new string(c, 1));
            }
            else
            {
                SerializeOther(value);
            }
        }

        private void SerializeObject(IDictionary obj)
        {
            var first = true;

            builder.Append('{');

            foreach (var e in obj.Keys)
            {
                if (!first)
                {
                    builder.Append(',');
                }

                SerializeString(e.ToString());
                builder.Append(':');

                SerializeValue(obj[e]);

                first = false;
            }

            builder.Append('}');
        }

        private void SerializeArray(IEnumerable anArray)
        {
            builder.Append('[');

            var first = true;

            foreach (var obj in anArray)
            {
                if (!first)
                {
                    builder.Append(',');
                }

                SerializeValue(obj);

                first = false;
            }

            builder.Append(']');
        }

        private void SerializeString(string str)
        {
            builder.Append('\"');

            var charArray = str.ToCharArray();
            foreach (var c in charArray)
            {
                switch (c)
                {
                    case '"':
                        builder.Append("\\\"");
                        break;
                    case '\\':
                        builder.Append(@"\\");
                        break;
                    case '\b':
                        builder.Append("\\b");
                        break;
                    case '\f':
                        builder.Append("\\f");
                        break;
                    case '\n':
                        builder.Append("\\n");
                        break;
                    case '\r':
                        builder.Append("\\r");
                        break;
                    case '\t':
                        builder.Append("\\t");
                        break;
                    default:
                        var codepoint = Convert.ToInt32(c);
                        if (codepoint is >= 32 and <= 126)
                        {
                            builder.Append(c);
                        }
                        else
                        {
                            builder.Append("\\u");
                            builder.Append(codepoint.ToString("x4"));
                        }
                        break;
                }
            }

            builder.Append('\"');
        }

        private void SerializeOther(object value)
        {
            switch (value)
            {
                // NOTE: decimals lose precision during serialization.
                // They always have, I'm just letting you know.
                // Previously floats and doubles lost precision too.
                case float f:
                    builder.Append(f.ToString("R"));
                    break;
                case int:
                case uint:
                case long:
                case sbyte:
                case byte:
                case short:
                case ushort:
                case ulong:
                    builder.Append(value);
                    break;
                case double:
                case decimal:
                    builder.Append(Convert.ToDouble(value).ToString("R"));
                    break;
                default:
                    SerializeString(value.ToString());
                    break;
            }
        }
    }
}
