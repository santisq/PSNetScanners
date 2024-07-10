// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Net.NetworkInformation;
// using System.Threading;
// using System.Threading.Tasks;

// namespace PSNetScanners;

// public sealed class PingAsync
// {
//     private readonly Ping _ping;

//     private readonly PingOptions _options;

//     private readonly int _timeOut;

//     private readonly byte[] _buffer;

//     private readonly string _target;

//     internal PingAsync(
//         PingOptions options,
//         string target,
//         int timeOut,
//         byte[] buffer)
//     {
//         _ping = new Ping();
//         _options = options;
//         _target = target;
//         _timeOut = timeOut;
//         _buffer = buffer;
//     }

//     internal Task Run() => Task.Run(async () =>
//     {

//         CancellationTokenSource source = new();
//         Task myCancellationTask = Task.Run(async () =>
//         {
//             while (true)
//             {
//                 source.Token.ThrowIfCancellationRequested();
//                 await Task.Delay(200);
//             }
//         });
//         Task<IPHostEntry> entryTask = Dns.GetHostEntryAsync(_target);

//         await Task.WhenAny(myCancellationTask, entryTask);

//         Task<PingReply> pingTask = _ping.SendPingAsync(_target, _timeOut, _buffer, _options);
//         Task done = await Task.WhenAny(dnsTask, pingTask);
//     });
// }