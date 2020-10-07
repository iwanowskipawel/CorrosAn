namespace CorrosAn
{
    public class PixelElement
    {
        private byte _r, _g, _b;
        private double _rgb;
        public PixelElement( byte R, byte G, byte B)
        {
            _r = R;
            _g = G;
            _b = B;
            _rgb = CalcFunction(_r, _g, _b);
        }
        public double RGB
        {
            get { return _rgb; }
            set { _rgb = value; }
        }
        public byte R
        {
            get { return _r; }
        }
        public byte G
        {
            get { return _g; }
        }
        public byte B
        {
            get { return _b; }
        }

        public double CalcFunction(byte R, byte G, byte B)
        {
            //return Math.Sqrt(Math.Pow(R,2) + Math.Pow(G,2) + Math.Pow(B,2));
            return (R * 77 + G * 150 + B * 29) / 256;
        }
    }
}