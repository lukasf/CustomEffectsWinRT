using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using System.Collections.Generic;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;

namespace WinRTLibrary
{
    public sealed class BlurVideoEffect : IBasicVideoEffect
    {

        #region Fields
        private IPropertySet configuration;
        private CanvasDevice canvasDevice;
        #endregion

        #region Properties
        public bool IsReadOnly => false;

        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                var encodingProperties = new VideoEncodingProperties();
                encodingProperties.Subtype = "ARGB32";
                return new List<VideoEncodingProperties>() { encodingProperties };
            }
        }

        public MediaMemoryTypes SupportedMemoryTypes => MediaMemoryTypes.Gpu;

        public bool TimeIndependent => true;

        public double BlurAmount
        {
            get
            {
                if (configuration != null &&
                    configuration.TryGetValue("BlurAmount", out object val) &&
                    val is double fadeValue)
                {
                    return fadeValue;
                }
                return 3;
            }
        }
        #endregion

        public void SetProperties(IPropertySet configuration)
        {
            this.configuration = configuration;
        }

        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
            this.canvasDevice = CanvasDevice.CreateFromDirect3D11Device(device);
        }

        public void DiscardQueuedFrames()
        {
            // Initialize effect resources
        }

        public void ProcessFrame(ProcessVideoFrameContext context)
        {
            using (CanvasBitmap inputBitmap = CanvasBitmap.CreateFromDirect3D11Surface(canvasDevice, context.InputFrame.Direct3DSurface))
            using (CanvasRenderTarget renderTarget = CanvasRenderTarget.CreateFromDirect3D11Surface(canvasDevice, context.OutputFrame.Direct3DSurface))
            using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
            {
                var gaussianBlurEffect = new GaussianBlurEffect
                {
                    Source = inputBitmap,
                    BlurAmount = (float)BlurAmount,
                    Optimization = EffectOptimization.Speed
                };

                ds.DrawImage(gaussianBlurEffect);
            }
        }

        public void Close(MediaEffectClosedReason reason)
        {
            // Dispose of effect resources
            canvasDevice?.Dispose();
        }
    }
}