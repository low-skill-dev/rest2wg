using Microsoft.VisualStudio.TestPlatform.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

#pragma warning disable CS0618

namespace vdb_node_wireguard_manipulator.tests;

public class WgStatusParserTests
{
	private ITestOutputHelper _output;
	public WgStatusParserTests(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void CanParseFullOutput()
	{
		var output = "interface: wg0\r\n  public key: Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=\r\n  private key: (hidden)\r\n  listening port: 51820\r\n\r\npeer: zO2jr59gppRHrPlaVshrm/AH8YUzVGToGDvoeS8j4CI=\r\n  endpoint: 31.173.84.131:21260\r\n  allowed ips: 10.8.0.101/32\r\n  latest handshake: 9 hours, 18 minutes, 25 seconds ago\r\n  transfer: 20.19 MiB received, 683.31 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 3Sg1yyugDmxoYFnEu+1i280QZpgD8tAHHHXUZPWDSk4=\r\n  endpoint: 5.188.98.224:60796\r\n  allowed ips: 10.8.0.115/32\r\n  latest handshake: 1 day, 7 hours, 7 minutes, 11 seconds ago\r\n  transfer: 116.43 MiB received, 2.32 GiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: MTy1q7mVlyuUkCnvz0ZrXPCTSzhbjNMbhzfJU/gSsF8=\r\n  endpoint: 31.173.82.117:25566\r\n  allowed ips: 10.8.0.100/32\r\n  latest handshake: 1 day, 13 hours, 13 minutes, 53 seconds ago\r\n  transfer: 13.19 MiB received, 176.12 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: V1evhjZQhhygsdrtriAb5AzuUuE8SkQNHJ4YAYdxGQs=\r\n  allowed ips: 10.0.0.0/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: HTqjO7TgQ1Mke2PKPtC2XGrAAK1INyH6j9ke7cn8cQU=\r\n  allowed ips: 10.1.1.1/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: LwkXubXSXLOzQPK0a6PQp1DWz08lsfk+Oyp7s1056H8=\r\n  allowed ips: 10.6.6.6/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 9c0dwlfFnTPuVon4au2l3mx94jme2czT4CSkd8ZbODM=\r\n  allowed ips: 10.64.64.64/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: /T3Yzw1oFJYYLDsC2bVG1UE2q2fuUPppSI+O3tr18Ek=\r\n  allowed ips: 10.255.255.255/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: rLb4WX7XDfOIs69hO1CGUrQuUqn42NT7OFAnttnupGA=\r\n  allowed ips: 10.8.0.102/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: hSDPy+JWh+IYRK72AThfvET+N8B8/i7SBWvTq1m8Y1I=\r\n  allowed ips: 10.8.0.110/32\r\n  persistent keepalive: every 25 seconds";
		var result = WgStatusParser.ParsePeersFromWgShow(output, out _);

		Assert.NotNull(result);
		Assert.NotEmpty(result);
		Assert.NotNull(result.First());
		Assert.NotNull(result.First().Interface);

		Assert.Equal("wg0", result.First().Interface!.Name);
		Assert.Equal("Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=", result.First().Interface!.PublicKey);

		var first = result.First();
		Assert.Equal("zO2jr59gppRHrPlaVshrm/AH8YUzVGToGDvoeS8j4CI=", first.PublicKey);
		Assert.Equal("31.173.84.131:21260", first.Endpoint);
		Assert.Equal("10.8.0.101/32", first.AllowedIps);
		Assert.Equal("9 hours, 18 minutes, 25 seconds ago", first.LatestHandshake);
		Assert.Equal("20.19 MiB received, 683.31 MiB sent", first.Transfer);
		Assert.Equal("every 25 seconds", first.PersistentKeepalive);

		var last = result.Last();
		Assert.Equal("hSDPy+JWh+IYRK72AThfvET+N8B8/i7SBWvTq1m8Y1I=", last.PublicKey);
		Assert.Null(last.Endpoint);
		Assert.Equal("10.8.0.110/32", last.AllowedIps);
		Assert.Null(last.LatestHandshake);
		Assert.Null(last.Transfer);
		Assert.Equal("every 25 seconds", last.PersistentKeepalive);
	}

	[Fact]
	public void CanParseOneMillionPeers()
	{
		var prepStopwatch = new Stopwatch();
		prepStopwatch.Start();
		var interfaceOutput = "interface: wg0\r\n  public key: Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=\r\n  private key: (hidden)\r\n  listening port: 51820\r\n\r\n";
		var peersOutput = "peer: zO2jr59gppRHrPlaVshrm/AH8YUzVGToGDvoeS8j4CI=\r\n  endpoint: 31.173.84.131:21260\r\n  allowed ips: 10.8.0.101/32\r\n  latest handshake: 9 hours, 18 minutes, 25 seconds ago\r\n  transfer: 20.19 MiB received, 683.31 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 3Sg1yyugDmxoYFnEu+1i280QZpgD8tAHHHXUZPWDSk4=\r\n  endpoint: 5.188.98.224:60796\r\n  allowed ips: 10.8.0.115/32\r\n  latest handshake: 1 day, 7 hours, 7 minutes, 11 seconds ago\r\n  transfer: 116.43 MiB received, 2.32 GiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: MTy1q7mVlyuUkCnvz0ZrXPCTSzhbjNMbhzfJU/gSsF8=\r\n  endpoint: 31.173.82.117:25566\r\n  allowed ips: 10.8.0.100/32\r\n  latest handshake: 1 day, 13 hours, 13 minutes, 53 seconds ago\r\n  transfer: 13.19 MiB received, 176.12 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: V1evhjZQhhygsdrtriAb5AzuUuE8SkQNHJ4YAYdxGQs=\r\n  allowed ips: 10.0.0.0/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: HTqjO7TgQ1Mke2PKPtC2XGrAAK1INyH6j9ke7cn8cQU=\r\n  allowed ips: 10.1.1.1/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: LwkXubXSXLOzQPK0a6PQp1DWz08lsfk+Oyp7s1056H8=\r\n  allowed ips: 10.6.6.6/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 9c0dwlfFnTPuVon4au2l3mx94jme2czT4CSkd8ZbODM=\r\n  allowed ips: 10.64.64.64/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: /T3Yzw1oFJYYLDsC2bVG1UE2q2fuUPppSI+O3tr18Ek=\r\n  allowed ips: 10.255.255.255/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: rLb4WX7XDfOIs69hO1CGUrQuUqn42NT7OFAnttnupGA=\r\n  allowed ips: 10.8.0.102/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: hSDPy+JWh+IYRK72AThfvET+N8B8/i7SBWvTq1m8Y1I=\r\n  allowed ips: 10.8.0.110/32\r\n  persistent keepalive: every 25 seconds\r\n\r\n";

		int peersInOutput = Regex.Matches(peersOutput, Regex.Escape(@"peer:")).Count;
		const int oneMillion = 1000 * 1000;// 1000k

		StringBuilder millionPeersBuilder = new(peersOutput.Length / peersInOutput * oneMillion);
		millionPeersBuilder.Append(interfaceOutput);
		for (int i = 0; i < oneMillion / peersInOutput; i++)
		{
			millionPeersBuilder.AppendLine(peersOutput);
		}
		var beforeCreate = GC.GetTotalMemory(true);
		var resultOutput = millionPeersBuilder.ToString();
		var afterCreate = GC.GetTotalMemory(true);
		double outputStrSize = Math.Round((afterCreate - beforeCreate) / 1024d / 1024d, 2);

		var stopwatch = new Stopwatch();
		prepStopwatch.Stop();

		stopwatch.Start();
		beforeCreate = GC.GetTotalMemory(true);
		var result = WgStatusParser.ParsePeersFromWgShow(resultOutput, out _);
		afterCreate = GC.GetTotalMemory(true);
		double resultArrSize = Math.Round((afterCreate - beforeCreate) / 1024d / 1024d, 2);
		stopwatch.Stop();

		Assert.NotNull(result);
		Assert.NotEmpty(result);
		Assert.NotNull(result.First());
		Assert.NotNull(result.First().Interface);

		Assert.Equal("wg0", result.First().Interface!.Name);
		Assert.Equal("Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=", result.First().Interface!.PublicKey);

		var first = result.First();
		Assert.Equal("zO2jr59gppRHrPlaVshrm/AH8YUzVGToGDvoeS8j4CI=", first.PublicKey);
		Assert.Equal("31.173.84.131:21260", first.Endpoint);
		Assert.Equal("10.8.0.101/32", first.AllowedIps);
		Assert.Equal("9 hours, 18 minutes, 25 seconds ago", first.LatestHandshake);
		Assert.Equal("20.19 MiB received, 683.31 MiB sent", first.Transfer);
		Assert.Equal("every 25 seconds", first.PersistentKeepalive);

		var last = result.Last();
		Assert.Equal("hSDPy+JWh+IYRK72AThfvET+N8B8/i7SBWvTq1m8Y1I=", last.PublicKey);
		Assert.Null(last.Endpoint);
		Assert.Equal("10.8.0.110/32", last.AllowedIps);
		Assert.Null(last.LatestHandshake);
		Assert.Null(last.Transfer);
		Assert.Equal("every 25 seconds", last.PersistentKeepalive);

		_output.WriteLine($"Output string length was: {resultOutput.Length} chars.");
		_output.WriteLine($"Output string size was (pre-calc): " +
			$"{Math.Round(resultOutput.Length * 2 / 1024d / 1024d, 2)} MiB.");
		_output.WriteLine($"Output string size was (GC-calc): " +
			$"{outputStrSize} MiB.");

		_output.WriteLine($"Result array length was: {result.Count} elements.");
		_output.WriteLine($"Result approx. array memory usage was (pre-calc): " +
			$"{Math.Round(resultOutput.Length*2/1024d/1024d,2)} MiB.");
		_output.WriteLine($"Result approx. array memory usage was (GC-calc): " +
			$"{Math.Round(resultArrSize)} MiB.");

		_output.WriteLine($"Time taken: {stopwatch.ElapsedMilliseconds / 1000} seconds.");
		_output.WriteLine($"Perparation time: {prepStopwatch.ElapsedMilliseconds / 1000} seconds.");
	}

	[Fact]
	public void CanParseInterfaceWithoutPeers()
	{
		var interfaceOutput = "interface: wg0\r\n  public key: Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=\r\n  private key: (hidden)\r\n  listening port: 51820\r\n\r\n";

		var result = WgStatusParser.ParsePeersFromWgShow(interfaceOutput, out var resultInterfaces);

		Assert.Empty(result);
		Assert.Single(resultInterfaces);

		Assert.Equal("wg0", resultInterfaces.First().Name);
		Assert.Equal("Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=", resultInterfaces.First().PublicKey);
	}

	[Fact]
	public void CanParseHandshake()
	{
		var days = Random.Shared.Next(1, 365);
		var hours = Random.Shared.Next(1, 24);
		var minutes = Random.Shared.Next(11, 60);
		var seconds = Random.Shared.Next(1, 60);

		var expected = new TimeSpan(days, hours, minutes, seconds).TotalSeconds;

		var generated = $"{days} days, {hours} hours, {minutes} minutes, {seconds} seconds ago";
		_output.WriteLine(generated);

		var actual = WgStatusParser.HandshakeToSecond(generated);

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void CanParsePeersShortly()
	{
		var output = "interface: wg0\r\n  public key: Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=\r\n  private key: (hidden)\r\n  listening port: 51820\r\n\r\npeer: zO2jr59gppRHrPlaVshrm/AH8YUzVGToGDvoeS8j4CI=\r\n  endpoint: 31.173.84.131:21260\r\n  allowed ips: 10.8.0.101/32\r\n  latest handshake: 9 hours, 18 minutes, 25 seconds ago\r\n  transfer: 20.19 MiB received, 683.31 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 3Sg1yyugDmxoYFnEu+1i280QZpgD8tAHHHXUZPWDSk4=\r\n  endpoint: 5.188.98.224:60796\r\n  allowed ips: 10.8.0.115/32\r\n  latest handshake: 1 day, 7 hours, 7 minutes, 11 seconds ago\r\n  transfer: 116.43 MiB received, 2.32 GiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: MTy1q7mVlyuUkCnvz0ZrXPCTSzhbjNMbhzfJU/gSsF8=\r\n  endpoint: 31.173.82.117:25566\r\n  allowed ips: 10.8.0.100/32\r\n  latest handshake: 1 day, 13 hours, 13 minutes, 53 seconds ago\r\n  transfer: 13.19 MiB received, 176.12 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: V1evhjZQhhygsdrtriAb5AzuUuE8SkQNHJ4YAYdxGQs=\r\n  allowed ips: 10.0.0.0/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: HTqjO7TgQ1Mke2PKPtC2XGrAAK1INyH6j9ke7cn8cQU=\r\n  allowed ips: 10.1.1.1/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: LwkXubXSXLOzQPK0a6PQp1DWz08lsfk+Oyp7s1056H8=\r\n  allowed ips: 10.6.6.6/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 9c0dwlfFnTPuVon4au2l3mx94jme2czT4CSkd8ZbODM=\r\n  allowed ips: 10.64.64.64/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: /T3Yzw1oFJYYLDsC2bVG1UE2q2fuUPppSI+O3tr18Ek=\r\n  allowed ips: 10.255.255.255/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: rLb4WX7XDfOIs69hO1CGUrQuUqn42NT7OFAnttnupGA=\r\n  allowed ips: 10.8.0.102/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: hSDPy+JWh+IYRK72AThfvET+N8B8/i7SBWvTq1m8Y1I=\r\n  allowed ips: 10.8.0.110/32\r\n  persistent keepalive: every 25 seconds";
		var result = WgStatusParser.ParsePeersFromWgShowShortly(output);
		var fullResult = WgStatusParser.ParsePeersFromWgShow(output, out _);

		Assert.NotNull(result);
		Assert.NotEmpty(result);

		var first = result.First();
		var fullFirst = fullResult.First();
		Assert.Equal("zO2jr59gppRHrPlaVshrm/AH8YUzVGToGDvoeS8j4CI=", first.PublicKey);
		Assert.Equal(WgStatusParser.HandshakeToSecond(fullFirst.LatestHandshake!), first.HandshakeSecondsAgo);

		var last = result.Last();
		Assert.Equal("hSDPy+JWh+IYRK72AThfvET+N8B8/i7SBWvTq1m8Y1I=", last.PublicKey);
		Assert.Equal(int.MaxValue, last.HandshakeSecondsAgo);
	}

	[Fact]
	public void CanParseOneMillionPeersShortly()
	{
		var interfaceOutput = "interface: wg0\r\n  public key: Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=\r\n  private key: (hidden)\r\n  listening port: 51820\r\n\r\n";
		var peersOutput = "peer: zO2jr59gppRHrPlaVshrm/AH8YUzVGToGDvoeS8j4CI=\r\n  endpoint: 31.173.84.131:21260\r\n  allowed ips: 10.8.0.101/32\r\n  latest handshake: 9 hours, 18 minutes, 25 seconds ago\r\n  transfer: 20.19 MiB received, 683.31 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 3Sg1yyugDmxoYFnEu+1i280QZpgD8tAHHHXUZPWDSk4=\r\n  endpoint: 5.188.98.224:60796\r\n  allowed ips: 10.8.0.115/32\r\n  latest handshake: 1 day, 7 hours, 7 minutes, 11 seconds ago\r\n  transfer: 116.43 MiB received, 2.32 GiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: MTy1q7mVlyuUkCnvz0ZrXPCTSzhbjNMbhzfJU/gSsF8=\r\n  endpoint: 31.173.82.117:25566\r\n  allowed ips: 10.8.0.100/32\r\n  latest handshake: 1 day, 13 hours, 13 minutes, 53 seconds ago\r\n  transfer: 13.19 MiB received, 176.12 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: V1evhjZQhhygsdrtriAb5AzuUuE8SkQNHJ4YAYdxGQs=\r\n  allowed ips: 10.0.0.0/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: HTqjO7TgQ1Mke2PKPtC2XGrAAK1INyH6j9ke7cn8cQU=\r\n  allowed ips: 10.1.1.1/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: LwkXubXSXLOzQPK0a6PQp1DWz08lsfk+Oyp7s1056H8=\r\n  allowed ips: 10.6.6.6/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 9c0dwlfFnTPuVon4au2l3mx94jme2czT4CSkd8ZbODM=\r\n  allowed ips: 10.64.64.64/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: /T3Yzw1oFJYYLDsC2bVG1UE2q2fuUPppSI+O3tr18Ek=\r\n  allowed ips: 10.255.255.255/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: rLb4WX7XDfOIs69hO1CGUrQuUqn42NT7OFAnttnupGA=\r\n  allowed ips: 10.8.0.102/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: hSDPy+JWh+IYRK72AThfvET+N8B8/i7SBWvTq1m8Y1I=\r\n  allowed ips: 10.8.0.110/32\r\n  persistent keepalive: every 25 seconds\r\n\r\n";

		int peersInOutput = Regex.Matches(peersOutput, Regex.Escape(@"peer:")).Count;
		const int oneMillion = 1000 * 1000;// 1000k

		StringBuilder millionPeersBuilder = new(peersOutput.Length / peersInOutput * oneMillion);
		millionPeersBuilder.Append(interfaceOutput);
		for (int i = 0; i < oneMillion / peersInOutput; i++)
		{
			millionPeersBuilder.AppendLine(peersOutput);
		}

		var resultOutput = millionPeersBuilder.ToString();

		Stopwatch stopwatch = new();

		stopwatch.Restart();
		var parsed2 = WgStatusParser.ParsePeersFromWgShow(resultOutput, out _)
			.Select(WgShortPeerInfo.FromFullInfo);
		stopwatch.Stop();
		_output.WriteLine($"ParseThenSelect took " +
			$"{Math.Round(stopwatch.ElapsedMilliseconds / 1000d, 2)} seconds.");

		stopwatch.Restart();
		var parsed = WgStatusParser.ParsePeersFromWgShowShortly(resultOutput);
		stopwatch.Stop();
		_output.WriteLine($"ParseShortly took " +
			$"{Math.Round(stopwatch.ElapsedMilliseconds / 1000d, 2)} seconds.");

		Assert.NotNull(parsed);
		Assert.NotEmpty(parsed);

		var first = parsed.First(); ;
		Assert.Equal("zO2jr59gppRHrPlaVshrm/AH8YUzVGToGDvoeS8j4CI=", first.PublicKey);

		var last = parsed.Last();
		Assert.Equal("hSDPy+JWh+IYRK72AThfvET+N8B8/i7SBWvTq1m8Y1I=", last.PublicKey);
	}
}

