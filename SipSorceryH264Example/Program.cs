using SIPSorcery.Net;
using SipSorceryH264Example.Codec;
using SipSorceryH264Example.Source.SkinHost.Services;
using SIPSorceryMedia.Abstractions.V1;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketSharp.Server;
using static OpenH264Lib.Encoder;

namespace SipSorceryH264Example
{
    class Program
    {
        private const int WIDTH = 640;
        private const int HEIGHT = 420;
        private const int FPS = 24;
        private const bool USE_BUFFER = false;

        // We only support one connection, it's just an example so it doesn't matter
        private static RTCPeerConnection ConnectedPeer;

        private H264Codec Codec;

        // Send buffer containing images so we match timings, this is ugly as well
        private ConcurrentQueue<byte[]> SendBuffer = new ConcurrentQueue<byte[]>();

        private void Pc_OnVideoFormatsNegotiated(List<SDPMediaFormat> obj)
        {
            if (obj.Count == 0)
                Console.WriteLine("Video Format negotiation failed.");
            else
                Console.WriteLine("Video formats negotiated, got {0} results; [0]={1}", obj.Count, obj[0].Name);
        }

        public void SendFrame(uint duration, byte[] data)
        {
            if (ConnectedPeer == null || ConnectedPeer.connectionState != RTCPeerConnectionState.connected)
                return;
            ConnectedPeer.SendVideo(duration, data);
        }

        private RTCPeerConnection CreatePeerConnection()
        {
            RTCConfiguration config = new RTCConfiguration();
            RTCPeerConnection pc = new RTCPeerConnection(config);
            MediaStreamTrack videoTrack = new MediaStreamTrack(new List<VideoCodecsEnum>() { VideoCodecsEnum.H264 });

            pc.addTrack(videoTrack);
            pc.OnVideoFormatsNegotiated += Pc_OnVideoFormatsNegotiated;
            ConnectedPeer = pc;
            return pc;
        }

        private void OnImageEncoded(byte[] encodedData, int length, FrameType frameType)
        {
            if (frameType == FrameType.IDR)
                Console.WriteLine("Sending IDR Frame...");

            if (USE_BUFFER)
                SendBuffer.Enqueue(encodedData);
            else
            {
                SendFrame(90000 / FPS, encodedData);
                Thread.Sleep(1000 / FPS);
            }
        }

        public Program()
        {
            WebSocketServer webSocketServer = new WebSocketServer(IPAddress.Any, 8081);
            TestCard testCard = new TestCard(WIDTH, HEIGHT, FPS); 
            Timer sendTimer = new Timer((state) =>
            {
                if (!SendBuffer.IsEmpty && ConnectedPeer != null && ConnectedPeer.connectionState == RTCPeerConnectionState.connected)
                {
                    byte[] imageData;

                    if (!SendBuffer.TryDequeue(out imageData))
                        return;
                    SendFrame(90000 / FPS, imageData);
                }
            }, null, 0, 33);

            webSocketServer.AddWebSocketService<WebRTCWebSocketPeer>("/", (peer) => peer.CreatePeerConnection = CreatePeerConnection);
            webSocketServer.Start();
            Console.WriteLine("Started WebRTC Server.");
            Codec = new H264Codec(WIDTH, HEIGHT, FPS, OnImageEncoded);
            Console.WriteLine("Initialized Codec.");
            testCard.ReceivedYUVFrame += TestCard_ReceivedYUVFrame;
            Console.WriteLine("TestCard is now sending frames.");
            Console.ReadKey();
        }

        private void TestCard_ReceivedYUVFrame(uint timestamp, int width, int height, byte[] data)
        {
            if (ConnectedPeer != null && ConnectedPeer.connectionState == RTCPeerConnectionState.connected)
                Codec.EncodeImage(data);    
        }

        static void Main(string[] args)
        {
            new Program();
        }
    }
}
