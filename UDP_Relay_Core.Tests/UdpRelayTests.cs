using System.Net;
using System.Net.Sockets;
using System.Text;
using UDP_Relay_Core;
using Xunit;

namespace UDP_Relay_Core.Tests;

/// <summary>
/// Integration test for the relay engine. Runs entirely over the loopback interface:
/// a datagram sent to the relay's listening port must be forwarded to the relay endpoint.
/// </summary>
[Trait("Category", "Integration")]
public class UdpRelayTests
{
    private static int GetFreeUdpPort()
    {
        using var probe = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        return ((IPEndPoint)probe.Client.LocalEndPoint!).Port;
    }

    [Fact]
    public async Task StartRelaying_forwards_datagram_to_relay_endpoint()
    {
        // The endpoint the relay forwards to — bound up front on an ephemeral port we keep open.
        using var destination = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        int destinationPort = ((IPEndPoint)destination.Client.LocalEndPoint!).Port;

        int relayListenPort = GetFreeUdpPort();

        using var relay = new UDP_Relay();
        relay.StartRelaying(
            new IPEndPoint(IPAddress.Loopback, relayListenPort),
            new IPEndPoint(IPAddress.Loopback, destinationPort));

        byte[] payload = Encoding.UTF8.GetBytes("relay-roundtrip-42");
        var receiveTask = destination.ReceiveAsync();

        // UDP is lossy and the relay binds its socket asynchronously, so resend until the
        // forwarded datagram arrives or we give up. On loopback this succeeds on the first
        // or second attempt; the retry loop just removes a startup race.
        UdpReceiveResult? received = null;
        for (int attempt = 0; attempt < 10 && received is null; attempt++)
        {
            using (var sender = new UdpClient())
            {
                await sender.SendAsync(payload, payload.Length,
                    new IPEndPoint(IPAddress.Loopback, relayListenPort));
            }

            if (await Task.WhenAny(receiveTask, Task.Delay(500)) == receiveTask)
            {
                received = await receiveTask;
            }
        }

        relay.StopRelaying();

        Assert.True(received is not null, "Relay did not forward the datagram within the timeout.");
        Assert.Equal("relay-roundtrip-42", Encoding.UTF8.GetString(received.Value.Buffer));
    }

    [Fact]
    public async Task Relay_survives_send_to_unreachable_destination()
    {
        int relayListenPort = GetFreeUdpPort();
        int destinationPort = GetFreeUdpPort(); // deliberately has no listener at first

        using var relay = new UDP_Relay();
        relay.StartRelaying(
            new IPEndPoint(IPAddress.Loopback, relayListenPort),
            new IPEndPoint(IPAddress.Loopback, destinationPort));

        using var sender = new UdpClient();
        byte[] toTheVoid = Encoding.UTF8.GetBytes("to-the-void");

        // Forward several datagrams while nothing is listening on the destination. On Windows this
        // makes the relay's socket observe WSAECONNRESET (10054) on its next receive — the relay
        // must NOT die from it.
        for (int i = 0; i < 3; i++)
        {
            await sender.SendAsync(toTheVoid, toTheVoid.Length,
                new IPEndPoint(IPAddress.Loopback, relayListenPort));
            await Task.Delay(150);
        }

        // Bring the destination online; the relay must still be forwarding. A late
        // "to-the-void" datagram forwarded just before the destination bound may still
        // be in flight, so we drain any stale datagram and keep prompting until a fresh
        // "still-alive" arrives. That the relay eventually delivers "still-alive" — not
        // that the very first datagram seen equals it — is what proves it survived the
        // WSAECONNRESET; asserting on the first datagram made this test flaky in CI.
        using var destination = new UdpClient(new IPEndPoint(IPAddress.Loopback, destinationPort));
        byte[] stillAlive = Encoding.UTF8.GetBytes("still-alive");

        bool receivedStillAlive = false;
        var receiveTask = destination.ReceiveAsync();
        for (int attempt = 0; attempt < 10 && !receivedStillAlive; attempt++)
        {
            await sender.SendAsync(stillAlive, stillAlive.Length,
                new IPEndPoint(IPAddress.Loopback, relayListenPort));

            while (await Task.WhenAny(receiveTask, Task.Delay(500)) == receiveTask)
            {
                UdpReceiveResult result = await receiveTask; // already completed — no blocking
                receiveTask = destination.ReceiveAsync();    // queue the next read before inspecting
                if (Encoding.UTF8.GetString(result.Buffer) == "still-alive")
                {
                    receivedStillAlive = true;
                    break;
                }
            }
        }

        relay.StopRelaying();

        Assert.True(receivedStillAlive,
            "Relay stopped forwarding after a send to an unreachable port (a 10054 likely killed the task).");
    }
}
