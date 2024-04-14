using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using WinRT;

namespace WinRTLibrary
{
    public sealed partial class FadeVideoEffect : IBasicVideoEffect
    {
        #region Fields
        private IPropertySet configuration;
        private VideoEncodingProperties encodingProperties;
        #endregion

        #region Properties
        public bool IsReadOnly => true;

        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                var encodingProperties = new VideoEncodingProperties
                {
                    Subtype = "ARGB32"
                };
                return new List<VideoEncodingProperties>() { encodingProperties };
            }
        }

        public MediaMemoryTypes SupportedMemoryTypes => MediaMemoryTypes.Cpu;

        public bool TimeIndependent => true;

        public double FadeValue
        {
            get
            {
                if (configuration != null &&
                    configuration.TryGetValue("FadeValue", out object val) &&
                    val is double fadeValue)
                {
                    return fadeValue;
                }
                return .5;
            }
        }
        #endregion

        public void SetProperties(IPropertySet configuration)
        {
            this.configuration = configuration;
        }

        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
            this.encodingProperties = encodingProperties;
        }

        public void DiscardQueuedFrames()
        {
            // Initialize effect resources
        }

        public unsafe void ProcessFrame(ProcessVideoFrameContext context)
        {
            using (BitmapBuffer buffer = context.InputFrame.SoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Read))
            using (BitmapBuffer targetBuffer = context.OutputFrame.SoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Read))
            using (var reference = buffer.CreateReference())
            using (var targetReference = targetBuffer.CreateReference())
            {
                reference.As<IMemoryBufferByteAccess>().GetBuffer(out byte* dataInBytes, out uint capacity);
                targetReference.As<IMemoryBufferByteAccess>().GetBuffer(out byte* targetDataInBytes, out uint targetCapacity);

                var fadeValue = FadeValue;

                // Fill-in the BGRA plane
                BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);
                for (int i = 0; i < bufferLayout.Height; i++)
                {
                    for (int j = 0; j < bufferLayout.Width; j++)
                    {
                        byte value = (byte)((float)j / bufferLayout.Width * 255);

                        int bytesPerPixel = 4;
                        if (encodingProperties.Subtype != "ARGB32")
                        {
                            // If you support other encodings, adjust index into the buffer accordingly
                        }

                        int idx = bufferLayout.StartIndex + bufferLayout.Stride * i + bytesPerPixel * j;

                        targetDataInBytes[idx + 0] = (byte)(fadeValue * (float)dataInBytes[idx + 0]);
                        targetDataInBytes[idx + 1] = (byte)(fadeValue * (float)dataInBytes[idx + 1]);
                        targetDataInBytes[idx + 2] = (byte)(fadeValue * (float)dataInBytes[idx + 2]);
                        targetDataInBytes[idx + 3] = dataInBytes[idx + 3];
                    }
                }
            }
        }

        public void Close(MediaEffectClosedReason reason)
        {
            // Dispose of effect resources
        }
    }
}
