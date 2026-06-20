using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace AGRA_EASY_MOBILE.Services;

internal sealed class EasySoapManualClient : IDisposable
{
    private const string SoapNamespace = "http://groupe-agra/";
    private readonly CookieContainer _cookies = new();
    private readonly HttpClientHandler _handler;
    private readonly HttpClient _http;
    private readonly string _url;

    public EasySoapManualClient(string url)
    {
        _url = url;
        _handler = new HttpClientHandler
        {
            CookieContainer = _cookies,
            UseCookies = true
        };
        _http = new HttpClient(_handler)
        {
            Timeout = TimeSpan.FromSeconds(60)
        };
    }

    public Task<T?> InvokeAsync<T>(string operation, params SoapParameter[] parameters)
        => InvokeAsync<T>(operation, operation + "Result", parameters);

    public async Task<T?> InvokeAsync<T>(string operation, string resultName, params SoapParameter[] parameters)
    {
        var document = await PostAsync(operation, parameters);
        var resultElement = document.Descendants(XName.Get(resultName, SoapNamespace)).FirstOrDefault();
        if (resultElement == null || resultElement.IsEmpty)
            return default;

        return DeserializeResult<T>(resultElement);
    }

    public async Task InvokeVoidAsync(string operation, params SoapParameter[] parameters)
    {
        await PostAsync(operation, parameters);
    }

    public void Dispose()
    {
        _http.Dispose();
        _handler.Dispose();
    }

    private async Task<XDocument> PostAsync(string operation, params SoapParameter[] parameters)
    {
        var body = BuildSoapEnvelope(operation, parameters);
        using var request = new HttpRequestMessage(HttpMethod.Post, _url);
        request.Content = new StringContent(body, Encoding.UTF8, "text/xml");
        request.Headers.TryAddWithoutValidation("SOAPAction", "\"" + SoapNamespace + operation + "\"");

        using var response = await _http.SendAsync(request);
        var responseText = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"SOAP {operation} a retourne HTTP {(int)response.StatusCode} {response.ReasonPhrase} : {responseText}");

        return XDocument.Parse(responseText);
    }

    private static string BuildSoapEnvelope(string operation, IReadOnlyCollection<SoapParameter> parameters)
    {
        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = false,
            Encoding = Encoding.UTF8,
            Indent = false
        };

        var builder = new StringBuilder();
        using var textWriter = new Utf8StringWriter(builder);
        using var writer = XmlWriter.Create(textWriter, settings);
        writer.WriteStartDocument();
        writer.WriteStartElement("soap", "Envelope", "http://schemas.xmlsoap.org/soap/envelope/");
        writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
        writer.WriteAttributeString("xmlns", "xsd", null, "http://www.w3.org/2001/XMLSchema");
        writer.WriteStartElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/");
        writer.WriteStartElement(operation, SoapNamespace);

        foreach (var parameter in parameters)
            WriteParameter(writer, parameter);

        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndElement();
        writer.WriteEndDocument();
        return builder.ToString();
    }

    private sealed class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter(StringBuilder builder)
            : base(builder, CultureInfo.InvariantCulture)
        {
        }

        public override Encoding Encoding => Encoding.UTF8;
    }

    private static void WriteParameter(XmlWriter writer, SoapParameter parameter)
    {
        writer.WriteStartElement(parameter.Name, SoapNamespace);

        if (parameter.Value == null)
        {
            writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
            writer.WriteEndElement();
            return;
        }

        var value = parameter.Value;
        var valueType = value.GetType();
        if (value is string text)
        {
            writer.WriteString(text);
        }
        else if (value is bool boolean)
        {
            writer.WriteString(boolean ? "true" : "false");
        }
        else if (value is DateTime date)
        {
            writer.WriteString(date.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
        }
        else if (value is byte[] bytes)
        {
            writer.WriteBase64(bytes, 0, bytes.Length);
        }
        else if (valueType.IsPrimitive || value is decimal)
        {
            writer.WriteString(Convert.ToString(value, CultureInfo.InvariantCulture));
        }
        else if (value is System.Collections.IEnumerable items && value is not string)
        {
            foreach (var item in items)
                WriteComplexValue(writer, item?.GetType().Name ?? "Item", item);
        }
        else
        {
            WriteObjectContent(writer, value);
        }

        writer.WriteEndElement();
    }

    private static void WriteComplexValue(XmlWriter writer, string elementName, object? value)
    {
        writer.WriteStartElement(elementName, SoapNamespace);
        if (value == null)
            writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
        else
            WriteAnyValue(writer, value);
        writer.WriteEndElement();
    }

    private static void WriteObjectContent(XmlWriter writer, object value)
    {
        foreach (var property in GetSerializableProperties(value.GetType()))
        {
            var elementName = GetXmlElementName(property);
            writer.WriteStartElement(elementName, SoapNamespace);
            WriteAnyValue(writer, property.GetValue(value));
            writer.WriteEndElement();
        }
    }

    private static void WriteAnyValue(XmlWriter writer, object? value)
    {
        if (value == null)
        {
            writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
            return;
        }

        var valueType = value.GetType();
        if (value is string text)
            writer.WriteString(text);
        else if (value is bool boolean)
            writer.WriteString(boolean ? "true" : "false");
        else if (value is DateTime date)
            writer.WriteString(date.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture));
        else if (value is byte[] bytes)
            writer.WriteBase64(bytes, 0, bytes.Length);
        else if (IsSimpleType(valueType))
            writer.WriteString(Convert.ToString(value, CultureInfo.InvariantCulture));
        else if (value is System.Collections.IEnumerable items && value is not string)
        {
            foreach (var item in items)
                WriteComplexValue(writer, item?.GetType().Name ?? "Item", item);
        }
        else
        {
            WriteObjectContent(writer, value);
        }
    }

    private static T? DeserializeResult<T>(XElement resultElement)
    {
        var targetType = typeof(T);
        if (targetType == typeof(string))
            return (T?)(object?)resultElement.Value;

        if (targetType == typeof(bool))
            return (T)(object)XmlConvert.ToBoolean(resultElement.Value);

        if (targetType == typeof(int))
            return (T)(object)XmlConvert.ToInt32(resultElement.Value);

        if (targetType == typeof(decimal))
            return (T)(object)XmlConvert.ToDecimal(resultElement.Value);

        if (targetType == typeof(byte[]))
            return (T)(object)Convert.FromBase64String(resultElement.Value);

        if (targetType.IsArray)
        {
            var elementType = targetType.GetElementType()!;
            var values = resultElement.Elements().Select(x => DeserializeElement(x, elementType)).ToArray();
            var array = Array.CreateInstance(elementType, values.Length);
            for (var i = 0; i < values.Length; i++)
                array.SetValue(values[i], i);
            return (T)(object)array;
        }

        return (T?)DeserializeElement(resultElement, targetType);
    }

    private static object? DeserializeElement(XElement element, Type targetType)
    {
        if (targetType == typeof(string))
            return element.Value;

        if (targetType == typeof(bool))
            return XmlConvert.ToBoolean(element.Value);

        if (targetType == typeof(int))
            return XmlConvert.ToInt32(element.Value);

        if (targetType == typeof(decimal))
            return XmlConvert.ToDecimal(element.Value);

        if (targetType == typeof(DateTime))
            return XmlConvert.ToDateTime(element.Value, XmlDateTimeSerializationMode.RoundtripKind);

        return DeserializeObject(element, targetType);
    }

    private static object? DeserializeObject(XElement element, Type targetType)
    {
        if (IsNil(element))
            return null;

        var instance = Activator.CreateInstance(targetType);
        if (instance == null)
            return null;

        foreach (var property in GetSerializableProperties(targetType))
        {
            if (!property.CanWrite)
                continue;

            var propertyType = property.PropertyType;
            var elementName = GetXmlElementName(property);
            var child = FindChild(element, elementName);
            if (child == null)
                continue;

            property.SetValue(instance, DeserializeValue(child, propertyType));
        }

        return instance;
    }

    private static object? DeserializeValue(XElement element, Type targetType)
    {
        if (IsNil(element))
            return null;

        var nullableType = Nullable.GetUnderlyingType(targetType);
        if (nullableType != null)
            return DeserializeValue(element, nullableType);

        if (targetType == typeof(string))
            return element.Value;

        if (targetType == typeof(bool))
            return XmlConvert.ToBoolean(element.Value);

        if (targetType == typeof(int))
            return XmlConvert.ToInt32(element.Value);

        if (targetType == typeof(decimal))
            return XmlConvert.ToDecimal(element.Value);

        if (targetType == typeof(double))
            return XmlConvert.ToDouble(element.Value);

        if (targetType == typeof(float))
            return XmlConvert.ToSingle(element.Value);

        if (targetType == typeof(long))
            return XmlConvert.ToInt64(element.Value);

        if (targetType == typeof(DateTime))
            return XmlConvert.ToDateTime(element.Value, XmlDateTimeSerializationMode.RoundtripKind);

        if (targetType == typeof(byte[]))
            return Convert.FromBase64String(element.Value);

        if (targetType.IsEnum)
            return Enum.Parse(targetType, element.Value);

        if (targetType.IsArray)
        {
            var elementType = targetType.GetElementType()!;
            var values = element.Elements().Select(x => DeserializeValue(x, elementType)).ToArray();
            var array = Array.CreateInstance(elementType, values.Length);
            for (var i = 0; i < values.Length; i++)
                array.SetValue(values[i], i);
            return array;
        }

        return DeserializeObject(element, targetType);
    }

    private static IEnumerable<PropertyInfo> GetSerializableProperties(Type type)
    {
        return type.GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Where(x => x.CanRead && x.GetIndexParameters().Length == 0)
            .OrderBy(x => x.GetCustomAttribute<XmlElementAttribute>()?.Order ?? int.MaxValue)
            .ThenBy(x => x.MetadataToken);
    }

    private static string GetXmlElementName(PropertyInfo property)
    {
        var xmlName = property.GetCustomAttribute<XmlElementAttribute>()?.ElementName;
        return string.IsNullOrWhiteSpace(xmlName) ? property.Name : xmlName;
    }

    private static XElement? FindChild(XElement parent, string localName)
    {
        return parent.Elements(XName.Get(localName, SoapNamespace)).FirstOrDefault()
            ?? parent.Elements().FirstOrDefault(x => x.Name.LocalName == localName);
    }

    private static bool IsNil(XElement element)
    {
        return string.Equals(
            element.Attribute(XName.Get("nil", "http://www.w3.org/2001/XMLSchema-instance"))?.Value,
            "true",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsSimpleType(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        return type.IsPrimitive
            || type.IsEnum
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(Guid);
    }
}

internal readonly record struct SoapParameter(string Name, object? Value);
