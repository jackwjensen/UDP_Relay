using System.Net;
using UDP_Relay_Core;
using Xunit;

namespace UDP_Relay_Core.Tests;

/// <summary>
/// Tests for <see cref="XML_Wrapper"/> — the XPath-based settings reader.
/// Each test instance writes its own throwaway XML file and deletes it afterwards.
/// </summary>
public class XmlWrapperTests : IDisposable
{
    private readonly string _path;

    public XmlWrapperTests()
    {
        _path = Path.Combine(Path.GetTempPath(), $"xmlwrapper_test_{Guid.NewGuid():N}.xml");
        File.WriteAllText(_path,
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\n" +
            "<Settings>\n" +
            "  <Name>relay</Name>\n" +
            "  <Port>55530</Port>\n" +
            "  <IP>192.168.1.255</IP>\n" +
            "</Settings>\n");
    }

    public void Dispose()
    {
        if (File.Exists(_path))
        {
            File.Delete(_path);
        }
    }

    [Fact]
    public void GetNodeValue_returns_inner_text()
    {
        var settings = new XML_Wrapper(_path);
        Assert.Equal("relay", settings.GetNodeValue("/Settings/Name"));
    }

    [Fact]
    public void GetInt_parses_integer_node()
    {
        var settings = new XML_Wrapper(_path);
        Assert.Equal(55530, settings.GetInt("/Settings/Port"));
    }

    [Fact]
    public void GetIPAddress_parses_address_node()
    {
        var settings = new XML_Wrapper(_path);
        Assert.Equal(IPAddress.Parse("192.168.1.255"), settings.GetIPAddress("/Settings/IP"));
    }

    [Fact]
    public void GetIPEndPoint_combines_address_and_port()
    {
        var settings = new XML_Wrapper(_path);
        var endPoint = settings.GetIPEndPoint("/Settings/IP", "/Settings/Port");
        Assert.Equal(new IPEndPoint(IPAddress.Parse("192.168.1.255"), 55530), endPoint);
    }

    [Fact]
    public void SetInt_updates_value_read_back_from_same_instance()
    {
        var settings = new XML_Wrapper(_path);
        settings.SetInt("/Settings/Port", 60000);
        Assert.Equal(60000, settings.GetInt("/Settings/Port"));
    }

    [Fact]
    public void GetNode_returns_null_for_missing_path()
    {
        var settings = new XML_Wrapper(_path);
        Assert.Null(settings.GetNode("/Settings/DoesNotExist"));
    }
}
