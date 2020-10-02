using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenH264Lib.Encoder;
using Encoder = OpenH264Lib.Encoder;

namespace SipSorceryH264Example.Codec
{
    public class H264Codec
    {
        private Encoder H264Encoder;


        public H264Codec(int width, int height, int fps, OnEncodeCallback onEncode)
        {
            this.H264Encoder = new Encoder("openh264-2.1.1-win64.dll");
            this.H264Encoder.Setup(width, height, 1920 * 1080 * 3 * 8, fps, 2.0F, onEncode);
        }

        public void EncodeImage(byte[] yuvFrameBuffer)
        {
            this.H264Encoder.Encode(yuvFrameBuffer);
        }
    }
}
